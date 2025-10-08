using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.NTile;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NMap
{
	public partial class Map
	{
		public static readonly Dictionary<int, int> bitmaskMap = new Dictionary<int, int> {
			{ 19, 16 },
			{ 23, 17 },
			{ 27, 18 },
			{ 31, 19 },
			{ 38, 20 },
			{ 39, 21 },
			{ 46, 22 },
			{ 47, 23 },
			{ 55, 24 },
			{ 63, 25 },
			{ 76, 26 },
			{ 77, 27 },
			{ 78, 28 },
			{ 79, 29 },
			{ 95, 30 },
			{ 110, 31 },
			{ 111, 32 },
			{ 127, 33 },
			{ 137, 34 },
			{ 139, 35 },
			{ 141, 36 },
			{ 143, 37 },
			{ 155, 38 },
			{ 159, 39 },
			{ 175, 40 },
			{ 191, 41 },
			{ 205, 42 },
			{ 207, 43 },
			{ 223, 44 },
			{ 239, 45 },
			{ 255, 46 }
		};
		public static readonly Dictionary<int, int[]> diagonalCheckMap = new Dictionary<int, int[]> {
			{ 4, new[] { 0, 1 } },
			{ 5, new[] { 1, 2 } },
			{ 6, new[] { 2, 3 } },
			{ 7, new[] { 3, 0 } }
		};

		private int BitSum(
			List<TileType.TypeEnum> compareTileTypes,
			List<ObjectPrefab.ObjectEnum> compareObjectTypes,
			List<Tile> tilesToSum,
			bool includeMapEdge
		) {
			compareObjectTypes ??= new List<ObjectPrefab.ObjectEnum>();

			int sum = 0;
			for (int i = 0; i < tilesToSum.Count; i++) {
				if (tilesToSum[i] != null) {
					if (!compareTileTypes.Contains(tilesToSum[i].tileType.type) && compareObjectTypes.Intersect(tilesToSum[i].objectInstances.Values.Select(obj => obj.prefab.type)).ToList().Count <= 0) {
						continue;
					}
					bool ignoreTile = false;
					if (diagonalCheckMap.ContainsKey(i)) {
						List<Tile> surroundingHorizontalTiles = new List<Tile> { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
						List<Tile> similarTiles = surroundingHorizontalTiles
							.Where(tile => tile != null)
							.Where(tile => compareTileTypes.Contains(tile.tileType.type) || compareObjectTypes.Intersect(tile.objectInstances.Values.Select(obj => obj.prefab.type)).ToList().Count > 0)
							.ToList();
						if (similarTiles.Count < 2) {
							ignoreTile = true;
						}
					}
					if (!ignoreTile) {
						sum += Mathf.RoundToInt(Mathf.Pow(2, i));
					}
				} else if (includeMapEdge) {
					if (tilesToSum.Find(tile => tile != null && tilesToSum.IndexOf(tile) <= 3 && !compareTileTypes.Contains(tile.tileType.type)) == null || i <= 3) {
						sum += Mathf.RoundToInt(Mathf.Pow(2, i));
					} else {
						List<Tile> surroundingHorizontalTiles = new List<Tile> { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
						if (surroundingHorizontalTiles.Find(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)) == null) {
							sum += Mathf.RoundToInt(Mathf.Pow(2, i));
						}
					}
				}
			}

			return sum;
		}

		private void RedrawTile(Tile tile, bool includeDiagonalSurroundingTiles, bool customBitSumInputs, List<TileType.TypeEnum> customCompareTileTypes, bool includeMapEdge) {
			int sum = 0;
			List<Tile> surroundingTilesToUse = includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles;
			if (customBitSumInputs) {
				sum = BitSum(customCompareTileTypes, null, surroundingTilesToUse, includeMapEdge);
			} else {
				if (River.DoAnyRiversContainTile(tile, rivers)) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, false);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Water) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, includeMapEdge);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, includeMapEdge);
					// Not-fully-working implementation of walls and stone connecting
					//sum += GameManager.Get<ResourceManager>().BitSumObjects(
					//	GameManager.Get<ResourceManager>().GetObjectPrefabSubGroupByEnum(ResourceManager.ObjectSubGroupEnum.Walls).prefabs.Select(prefab => prefab.type).ToList(),
					//	surroundingTilesToUse
					//);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Hole) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Hole).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, false);
				} else {
					sum = BitSum(new List<TileType.TypeEnum> { tile.tileType.type }, null, surroundingTilesToUse, includeMapEdge);
				}
			}
			if (sum < 16 || bitmaskMap[sum] != 46) {
				if (sum >= 16) {
					sum = bitmaskMap[sum];
				}
				if (tile.tileType.classes[TileType.ClassEnum.LiquidWater] && River.DoAnyRiversContainTile(tile, rivers)) {
					tile.sr.sprite = tile.tileType.riverSprites[sum];
				} else {
					try {
						tile.sr.sprite = tile.tileType.bitmaskSprites[sum];
					} catch (ArgumentOutOfRangeException) {
						Debug.LogWarning("BitmaskTile Error: Index " + sum + " does not exist in bitmaskSprites. " + tile.obj.transform.position + " " + tile.tileType.type + " " + tile.tileType.bitmaskSprites.Count);
					}
				}
			} else {
				if (tile.tileType.baseSprites.Count > 0 && !tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
					tile.sr.sprite = tile.tileType.baseSprites[Random.Range(0, tile.tileType.baseSprites.Count)];
				}
				if (ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Stone).Find(rvd => rvd.tileTypes.ContainsValue(tile.tileType.type)) != null) {
					TileType biomeTileType = tile.biome.tileTypes[tile.tileType.groupType];
					tile.sr.sprite = biomeTileType.baseSprites[Random.Range(0, biomeTileType.baseSprites.Count)];
				}
			}
		}

		public void RedrawTiles(List<Tile> tilesToBitmask, bool careAboutColonistVisibility, bool recalculateLighting) {
			// TODO Optimise use of ColonistM.IsTileVisibleToAnyColonist, use Region instead
			//List<Region> uniqueRegions = tilesToBitmask.Select(t => t.region).Distinct().ToList();
			foreach (Tile tile in tilesToBitmask) {
				if (tile != null) {
					if (!careAboutColonistVisibility || ColonistM.IsTileVisibleToAnyColonist(tile)) {
						tile.SetVisible(true); // "true" on "recalculateBitmasking" would cause stack overflow
						if (tile.tileType.bitmasking) {
							RedrawTile(tile, true, false, null, true);
						} else {
							if (!tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
								tile.sr.sprite = tile.tileType.baseSprites[Random.Range(0, tile.tileType.baseSprites.Count)];
							}
						}
					} else {
						tile.SetVisible(false); // "true" on "recalculateBitmasking" would cause stack overflow
					}
				}
			}
			BitmaskRiverStartTiles();
			if (recalculateLighting) {
				RecalculateLighting(tilesToBitmask, true);
			}
		}

		private void BitmaskRiverStartTiles() {
			foreach (River river in rivers) {
				List<TileType.TypeEnum> compareTileTypes = new List<TileType.TypeEnum>();
				compareTileTypes.AddRange(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList());
				compareTileTypes.AddRange(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tt => tt.type).ToList());
				RedrawTile(river.startTile, false, true, compareTileTypes, false /*river.expandRadius > 0*/);
			}
		}
	}
}
