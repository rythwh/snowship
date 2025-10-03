using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;
using Snowship.NCamera;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NMap;
using Snowship.NMap.Models.Structure;
using Snowship.NResource;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NColonist {

	public class ColonistManager : Manager
	{
		public IEnumerable<Colonist> Colonists => HumanM.GetHumans<Colonist>();
		public int ColonistCount => HumanM.CountHumans<Colonist>();

		private MapManager MapM => GameManager.Get<MapManager>();
		private CameraManager CameraM => GameManager.Get<CameraManager>();
		private ColonyManager ColonyM => GameManager.Get<ColonyManager>();
		private TimeManager TimeM => GameManager.Get<TimeManager>();
		private ResourceManager ResourceM => GameManager.Get<ResourceManager>();
		private HumanManager HumanM => GameManager.Get<HumanManager>();

		private void SetInitialRegionVisibility() {
			foreach (Region region in MapM.Map.regions) {
				region.SetVisible(IsRegionVisibleToAnyColonist(region), false, false);
			}
		}

		public override void OnUpdate() {
			UpdateColonists();
		}

		private void UpdateColonists() {
			foreach (Colonist colonist in Colonists) {
				colonist.Update();
			}
		}

		public void SpawnStartColonists(int amount) {
			SpawnColonists(amount);


			Vector2 averageColonistPosition = new Vector2(0, 0);
			foreach (Colonist colonist in Colonists) {
				averageColonistPosition += colonist.Position;
			}
			averageColonistPosition /= ColonistCount;
			CameraM.SetCameraPosition(averageColonistPosition);

			if (ColonistCount <= 0) {
				Debug.LogError("Unable to spawn starting colonists");
				return;
			}

			// TODO TEMPORARY COLONIST TESTING STUFF
			Colonists
				.ElementAt(Random.Range(0, ColonistCount))
				.Inventory
				.ChangeResourceAmount(
					Resource.GetResourceByEnum(EResource.WheatSeed),
					Random.Range(5, 11),
					false
				);
			Colonists
				.ElementAt(Random.Range(0, ColonistCount))
				.Inventory
				.ChangeResourceAmount(
					Resource.GetResourceByEnum(EResource.Potato),
					Random.Range(5, 11),
					false
				);
			Colonists
				.ElementAt(Random.Range(0, ColonistCount))
				.Inventory
				.ChangeResourceAmount(
					Resource.GetResourceByEnum(EResource.CottonSeed),
					Random.Range(5, 11),
					false
				);
		}

		public void SpawnColonists(int amount) {
			if (amount <= 0) {
				return;
			}

			int mapSize = MapM.Map.MapData.mapSize;
			for (int i = 0; i < amount; i++) {
				List<Tile> walkableTilesByDistanceToCentre = MapM.Map.tiles.Where(o => o.walkable && o.buildable && Colonists.ToList().Find(c => c.Tile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))).ToList();
				if (walkableTilesByDistanceToCentre.Count <= 0) {
					foreach (Tile tile in MapM.Map.tiles.Where(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
						tile.SetTileType(tile.biome.tileTypes[TileTypeGroup.TypeEnum.Ground], true, true, true);
					}
					walkableTilesByDistanceToCentre = MapM.Map.tiles.Where(o => o.walkable && Colonists.ToList().Find(c => c.Tile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))).ToList();
				}

				List<Tile> validSpawnTiles = new List<Tile>();
				Tile currentTile = walkableTilesByDistanceToCentre[0];
				float minimumDistance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
				foreach (Tile tile in walkableTilesByDistanceToCentre) {
					float distance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
					if (distance < minimumDistance) {
						currentTile = tile;
						minimumDistance = distance;
					}
				}
				List<Tile> frontier = new List<Tile>() { currentTile };
				List<Tile> checkedTiles = new List<Tile>();
				while (frontier.Count > 0) {
					currentTile = frontier[0];
					frontier.RemoveAt(0);
					checkedTiles.Add(currentTile);
					validSpawnTiles.Add(currentTile);
					if (validSpawnTiles.Count > 100) {
						break;
					}
					foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
						if (walkableTilesByDistanceToCentre.Contains(nTile) && !checkedTiles.Contains(nTile)) {
							frontier.Add(nTile);
						}
					}
				}
				Tile colonistSpawnTile = validSpawnTiles.Count >= amount ? validSpawnTiles[Random.Range(0, validSpawnTiles.Count)] : walkableTilesByDistanceToCentre[Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

				HumanM.CreateHuman<Colonist, ColonistViewModule>(colonistSpawnTile, new HumanData());
			}

			// MapM.Map.RedrawTiles(MapM.Map.tiles, true, true);
			// MapM.Map.UpdateGlobalLighting(TimeM.Time.TileBrightnessTime, true);
		}

		public bool IsRegionVisibleToAnyColonist(Region region) {
			return Colonists.Any(c => c.Tile.region == region);
		}

		public bool IsTileVisibleToAnyColonist(Tile tile) {
			if (tile.walkable) {
				foreach (Colonist colonist in Colonists) {
					if (colonist.Tile.walkable) {
						if (colonist.Tile.region == tile.region) {
							return true;
						}
					} else {
						foreach (Tile hTile in colonist.Tile.horizontalSurroundingTiles) {
							if (hTile != null && hTile.visible) {
								if (hTile.region == tile.region) {
									return true;
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < tile.surroundingTiles.Count; i++) {
				Tile surroundingTile = tile.surroundingTiles[i];
				if (surroundingTile != null && surroundingTile.walkable) {
					if (Map.diagonalCheckMap.ContainsKey(i)) {
						bool skip = true;
						foreach (int horizontalTileIndex in Map.diagonalCheckMap[i]) {
							Tile horizontalTile = surroundingTile.surroundingTiles[horizontalTileIndex];
							if (horizontalTile != null && horizontalTile.walkable) {
								skip = false;
								break;
							}
						}
						if (skip) {
							continue;
						}
					}
					foreach (Colonist colonist in Colonists) {
						if (colonist.Tile.region == surroundingTile.region) {
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
