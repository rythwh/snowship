using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowship.NMap.Tile;
using Snowship.NColony;
using Snowship.NMap;
using Snowship.NPlanet;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PColony : Colony
	{
		private readonly PMap pMap = new PMap();
		private readonly PRiver pRiver = new PRiver();

		private enum ColonyProperty
		{
			LastSaveDateTime,
			LastSaveTimeChunk,
			Name,
			PlanetPosition,
			Seed,
			Size,
			AverageTemperature,
			AveragePrecipitation,
			TerrainTypeHeights,
			SurroundingPlanetTileHeights,
			OnRiver,
			SurroundingPlanetTileRivers
		}

		private void SaveColony(StreamWriter file, Colony colony, Map map) {
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.LastSaveDateTime, colony.lastSaveDateTime, 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.LastSaveTimeChunk, colony.lastSaveTimeChunk, 0));

			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.Name, colony.Name, 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.PlanetPosition, PU.FormatVector2ToString(map.MapData.planetTilePosition), 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.Seed, map.MapData.mapSeed, 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.Size, map.MapData.mapSize, 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.AverageTemperature, map.MapData.averageTemperature, 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.AveragePrecipitation, map.MapData.averagePrecipitation, 0));

			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.TerrainTypeHeights, string.Empty, 0));
			foreach (KeyValuePair<TileTypeGroup.TypeEnum, float> terrainTypeHeight in map.MapData.terrainTypeHeights) {
				file.WriteLine(PU.CreateKeyValueString(terrainTypeHeight.Key, terrainTypeHeight.Value, 1));
			}

			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.SurroundingPlanetTileHeights, string.Join(",", map.MapData.surroundingPlanetTileHeightDirections.Select(i => i.ToString()).ToArray()), 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.OnRiver, map.MapData.isRiver, 0));
			file.WriteLine(PU.CreateKeyValueString(ColonyProperty.SurroundingPlanetTileRivers, string.Join(",", map.MapData.surroundingPlanetTileRivers.Select(i => i.ToString()).ToArray()), 0));
		}

		public List<PersistenceColony> GetPersistenceColonies() {
			List<PersistenceColony> persistenceColonies = new List<PersistenceColony>();
			string coloniesPath = GameManager.Get<PlanetManager>().planet.directory + "/Colonies";
			if (Directory.Exists(coloniesPath)) {
				foreach (string colonyDirectoryPath in Directory.GetDirectories(coloniesPath)) {
					PersistenceColony persistenceColony = LoadColony(colonyDirectoryPath + "/colony.snowship");

					string savesDirectoryPath = colonyDirectoryPath + "/Saves";
					List<string> saveDirectories = Directory.GetDirectories(savesDirectoryPath).OrderByDescending(sd => sd).ToList();
					if (saveDirectories.Count > 0) {
						string screenshotPath = Directory.GetFiles(saveDirectories[0]).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
						if (screenshotPath != null) {
							persistenceColony.lastSaveImage = PU.LoadSpriteFromImageFile(screenshotPath);
						}
					}
					persistenceColonies.Add(persistenceColony);
				}
			}
			persistenceColonies = persistenceColonies.OrderByDescending(pc => pc.lastSaveTimeChunk).ToList();
			return persistenceColonies;
		}

		private PersistenceColony LoadColony(string path) {

			PersistenceColony persistenceColony = new PersistenceColony(path);

			foreach (KeyValuePair<string, object> property in PU.GetKeyValuePairsFromFile(path)) {
				switch ((ColonyProperty)Enum.Parse(typeof(ColonyProperty), property.Key)) {
					case ColonyProperty.LastSaveDateTime:
						persistenceColony.lastSaveDateTime = (string)property.Value;
						break;
					case ColonyProperty.LastSaveTimeChunk:
						persistenceColony.lastSaveTimeChunk = (string)property.Value;
						break;
					case ColonyProperty.Name:
						persistenceColony.name = (string)property.Value;
						break;
					case ColonyProperty.PlanetPosition:
						persistenceColony.planetPosition = new Vector2(float.Parse(((string)property.Value).Split(',')[0]), float.Parse(((string)property.Value).Split(',')[1]));
						break;
					case ColonyProperty.Seed:
						persistenceColony.seed = int.Parse((string)property.Value);
						break;
					case ColonyProperty.Size:
						persistenceColony.size = int.Parse((string)property.Value);
						break;
					case ColonyProperty.AverageTemperature:
						persistenceColony.averageTemperature = float.Parse((string)property.Value);
						break;
					case ColonyProperty.AveragePrecipitation:
						persistenceColony.averagePrecipitation = float.Parse((string)property.Value);
						break;
					case ColonyProperty.TerrainTypeHeights:
						foreach (KeyValuePair<string, object> terrainTypeHeightProperty in (List<KeyValuePair<string, object>>)property.Value) {
							TileTypeGroup.TypeEnum terrainTypeHeightPropertyKey = (TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), terrainTypeHeightProperty.Key);
							persistenceColony.terrainTypeHeights.Add(terrainTypeHeightPropertyKey, float.Parse((string)terrainTypeHeightProperty.Value));
						}
						break;
					case ColonyProperty.SurroundingPlanetTileHeights:
						foreach (string height in ((string)property.Value).Split(',')) {
							persistenceColony.surroundingPlanetTileHeights.Add(int.Parse(height));
						}
						break;
					case ColonyProperty.OnRiver:
						persistenceColony.onRiver = bool.Parse((string)property.Value);
						break;
					case ColonyProperty.SurroundingPlanetTileRivers:
						foreach (string riverIndex in ((string)property.Value).Split(',')) {
							persistenceColony.surroundingPlanetTileRivers.Add(int.Parse(riverIndex));
						}
						break;
					default:
						Debug.LogError("Unknown colony property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return persistenceColony;
		}

		public Colony ApplyLoadedColony(PersistenceColony persistenceColony) {
			Colony colony = GameManager.Get<ColonyManager>()
				.CreateColony(
				new CreateColonyData(
					persistenceColony.name,
					persistenceColony.seed,
					persistenceColony.size,
					GameManager.Get<PlanetManager>().selectedPlanetTile
				)
				);
			colony.SetLastSaveDateTime(persistenceColony.lastSaveDateTime, persistenceColony.lastSaveTimeChunk);
			colony.SetDirectory(Directory.GetParent(persistenceColony.path)?.FullName);

			GameManager.Get<ColonyManager>().SetColony(colony);

			return colony;
		}

		public void CreateColony(Colony colony, Map map) {
			if (colony == null) {
				Debug.LogError("Colony to be saved is null.");
				return;
			}

			if (GameManager.Get<PlanetManager>().planet == null) {
				Debug.LogError("Planet to save the colony to is null.");
				return;
			}

			if (string.IsNullOrEmpty(GameManager.Get<PlanetManager>().planet.directory)) {
				Debug.LogError("Planet directory is null or empty.");
				return;
			}

			string coloniesDirectoryPath = GameManager.Get<PlanetManager>().planet.directory + "/Colonies";
			string dateTimeString = PU.GenerateDateTimeString();
			string colonyDirectoryPath = coloniesDirectoryPath + "/Colony-" + dateTimeString;
			Directory.CreateDirectory(colonyDirectoryPath);
			colony.SetDirectory(colonyDirectoryPath);

			UpdateColonySave(colony, map);

			string mapDirectoryPath = colonyDirectoryPath + "/Map";
			Directory.CreateDirectory(mapDirectoryPath);

			StreamWriter tilesFile = PU.CreateFileAtDirectory(mapDirectoryPath, "tiles.snowship");
			pMap.SaveOriginalTiles(tilesFile);
			tilesFile.Close();

			StreamWriter riversFile = PU.CreateFileAtDirectory(mapDirectoryPath, "rivers.snowship");
			pRiver.SaveOriginalRivers(riversFile);
			riversFile.Close();

			string savesDirectoryPath = colonyDirectoryPath + "/Saves";
			Directory.CreateDirectory(savesDirectoryPath);
		}

		public void UpdateColonySave(Colony colony, Map map) {
			if (colony == null) {
				Debug.LogError("Colony to be saved is null.");
				return;
			}

			if (GameManager.Get<PlanetManager>().planet == null) {
				Debug.LogError("Planet to save the colony to is null.");
				return;
			}

			if (string.IsNullOrEmpty(GameManager.Get<PlanetManager>().planet.directory)) {
				Debug.LogError("Planet directory is null or empty.");
				return;
			}

			string colonyFilePath = colony.directory + "/colony.snowship";
			if (File.Exists(colonyFilePath)) {
				File.WriteAllText(colonyFilePath, string.Empty);
			} else {
				PU.CreateFileAtDirectory(colony.directory, "colony.snowship").Close();
			}
			StreamWriter colonyFile = new StreamWriter(colonyFilePath);
			SaveColony(colonyFile, colony, map);
			colonyFile.Close();
		}

	}
}
