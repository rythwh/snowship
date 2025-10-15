using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NMap.NTile;
using Snowship.NCamera;
using Snowship.NHuman;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NMap.Models.Structure;
using Snowship.NResource;
using Snowship.NState;
using UnityEngine;
using VContainer.Unity;

namespace Snowship.NColonist
{
	[UsedImplicitly]
	public class ColonistManager : ITickable
	{
		private readonly IColonistQuery colonistQuery;
		private readonly IMapQuery mapQuery;
		private readonly ICameraWrite cameraWrite;
		private readonly HumanManager humanManager;
		private readonly IResourceQuery resourceQuery;
		private readonly IStateQuery stateQuery;

		private IEnumerable<Colonist> Colonists => colonistQuery.Colonists;
		private int ColonistCount => colonistQuery.ColonistCount;

		public ColonistManager(
			IColonistQuery colonistQuery,
			IMapQuery mapQuery,
			ICameraWrite cameraWrite,
			HumanManager humanManager,
			IResourceQuery resourceQuery,
			IStateQuery stateQuery
		) {
			this.colonistQuery = colonistQuery;
			this.mapQuery = mapQuery;
			this.cameraWrite = cameraWrite;
			this.humanManager = humanManager;
			this.resourceQuery = resourceQuery;
			this.stateQuery = stateQuery;
		}

		public void Tick() {
			if (stateQuery.State == EState.Simulation) {
				UpdateColonists();
			}
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
			cameraWrite.SetPosition(averageColonistPosition, CameraConstants.StartMoveTweenDuration);
			cameraWrite.SetZoom(CameraConstants.ZoomMin * 2, CameraConstants.StartZoomTweenDuration);

			if (ColonistCount <= 0) {
				Debug.LogError("Unable to spawn starting colonists");
				return;
			}

			// TODO TEMPORARY COLONIST TESTING STUFF
			Colonists
				.ElementAt(Random.Range(0, ColonistCount))
				.Inventory
				.ChangeResourceAmount(
					resourceQuery.GetResourceByEnum(EResource.WheatSeed),
					Random.Range(5, 11),
					false
				);
			Colonists
				.ElementAt(Random.Range(0, ColonistCount))
				.Inventory
				.ChangeResourceAmount(
					resourceQuery.GetResourceByEnum(EResource.Potato),
					Random.Range(5, 11),
					false
				);
			Colonists
				.ElementAt(Random.Range(0, ColonistCount))
				.Inventory
				.ChangeResourceAmount(
					resourceQuery.GetResourceByEnum(EResource.CottonSeed),
					Random.Range(5, 11),
					false
				);
		}

		public void SpawnColonists(int amount) {
			if (amount <= 0) {
				return;
			}

			int mapSize = mapQuery.Map.MapData.mapSize;
			for (int i = 0; i < amount; i++) {
				List<Tile> walkableTilesByDistanceToCentre = mapQuery.Map.tiles.Where(o => o.walkable && o.buildable && Colonists.ToList().Find(c => c.Tile == o) == null).OrderBy(o => Vector2.Distance(o.PositionGrid, new Vector2(mapSize / 2f, mapSize / 2f))).ToList();
				if (walkableTilesByDistanceToCentre.Count <= 0) {
					foreach (Tile tile in mapQuery.Map.tiles.Where(o => Vector2.Distance(o.PositionGrid, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
						tile.SetTileType(tile.biome.tileTypes[TileTypeGroup.TypeEnum.Ground], true, true, true);
					}
					walkableTilesByDistanceToCentre = mapQuery.Map.tiles.Where(o => o.walkable && Colonists.ToList().Find(c => c.Tile == o) == null).OrderBy(o => Vector2.Distance(o.PositionGrid, new Vector2(mapSize / 2f, mapSize / 2f))).ToList();
				}

				List<Tile> validSpawnTiles = new List<Tile>();
				Tile currentTile = walkableTilesByDistanceToCentre[0];
				float minimumDistance = Vector2.Distance(currentTile.PositionGrid, new Vector2(mapSize / 2f, mapSize / 2f));
				foreach (Tile tile in walkableTilesByDistanceToCentre) {
					float distance = Vector2.Distance(currentTile.PositionGrid, new Vector2(mapSize / 2f, mapSize / 2f));
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
					foreach (Tile nTile in currentTile.SurroundingTiles[EGridConnectivity.FourWay]) {
						if (walkableTilesByDistanceToCentre.Contains(nTile) && !checkedTiles.Contains(nTile)) {
							frontier.Add(nTile);
						}
					}
				}
				Tile colonistSpawnTile = validSpawnTiles.Count >= amount
					? validSpawnTiles.RandomElement()
					: walkableTilesByDistanceToCentre[Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

				// TODO Finish HumanData implementation and use it
				Gender gender = Random.RandomElement<Gender>();
				HumanData data = new HumanData(
					humanManager.GetName(gender),
					gender,
					new List<(ESkill, float)>()
				);

				humanManager.CreateHuman<Colonist, ColonistViewModule>(colonistSpawnTile, data);
			}

			// MapM.Map.RedrawTiles(MapM.Map.tiles, true, true);
			// MapM.Map.UpdateGlobalLighting(TimeM.Time.TileBrightnessTime, true);
		}

		private void SetInitialRegionVisibility() {
			foreach (Region region in mapQuery.Map.regions) {
				region.SetVisible(IsRegionVisibleToAnyColonist(region), false, false);
			}
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
						foreach (Tile hTile in colonist.Tile.SurroundingTiles[EGridConnectivity.FourWay]) {
							if (hTile != null && hTile.visible) {
								if (hTile.region == tile.region) {
									return true;
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < tile.SurroundingTiles[EGridConnectivity.EightWay].Count; i++) {
				Tile surroundingTile = tile.SurroundingTiles[EGridConnectivity.EightWay][i];
				if (surroundingTile != null && surroundingTile.walkable) {
					if (Map.diagonalCheckMap.ContainsKey(i)) {
						bool skip = true;
						foreach (int horizontalTileIndex in Map.diagonalCheckMap[i]) {
							Tile horizontalTile = surroundingTile.SurroundingTiles[EGridConnectivity.EightWay][horizontalTileIndex];
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
