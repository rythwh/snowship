using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using Snowship.NPath;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private void DetermineDrainageBasins(MapGenContext context) {
			context.Map.drainageBasins.Clear();

			List<Tile> tilesByHeight = context.Map.tiles.OrderBy(tile => tile.height).ToList();
			foreach (Tile highestTileInRegion in tilesByHeight) {

				if (highestTileInRegion.tileType.groupType == TileTypeGroup.TypeEnum.Stone || highestTileInRegion.drainageBasin != null) {
					continue;
				}

				Region region = new Region(null);

				Tile currentFloodFillPoint = highestTileInRegion;

				List<Tile> checkedTiles = new() { currentFloodFillPoint };
				List<Tile> frontier = new() { currentFloodFillPoint };

				while (frontier.Count > 0) {
					currentFloodFillPoint = frontier[0];
					frontier.RemoveAt(0);

					region.tiles.Add(currentFloodFillPoint);
					currentFloodFillPoint.drainageBasin = region;

					foreach (Tile nTile in currentFloodFillPoint.horizontalSurroundingTiles) {
						if (nTile != null && !checkedTiles.Contains(nTile) && nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone && nTile.drainageBasin == null) {
							if (nTile.height * 1.2f >= currentFloodFillPoint.height) {
								frontier.Add(nTile);
								checkedTiles.Add(nTile);
							}
						}
					}
				}

				context.Map.drainageBasins.Add(new DrainageBasin(highestTileInRegion, region));
			}
		}

		private void CreateLargeRivers(MapGenContext context) {

			const bool ignoreStone = true;

			context.Map.largeRivers.Clear();

			if (!context.Data.isRiver) {
				return;
			}

			int riverEndRiverIndex = context.Data.surroundingPlanetTileRivers.OrderByDescending(i => i).ToList()[0];
			int riverEndListIndex = context.Data.surroundingPlanetTileRivers.IndexOf(riverEndRiverIndex);

			List<Tile> validEndTiles = context.Map.sortedEdgeTiles[riverEndListIndex]
				.Where(tile => Vector2.Distance(tile.PositionGrid, context.Map.sortedEdgeTiles[riverEndListIndex][0].PositionGrid) >= 10)
				.Where(tile => Vector2.Distance(tile.PositionGrid, context.Map.sortedEdgeTiles[riverEndListIndex][context.Map.sortedEdgeTiles[riverEndListIndex].Count - 1].PositionGrid) >= 10)
				.ToList();
			Tile riverEndTile = validEndTiles.RandomElement();

			int riverStartListIndex = 0;
			foreach (int riverStartRiverIndex in context.Data.surroundingPlanetTileRivers) {
				if (riverStartRiverIndex != -1 && riverStartRiverIndex != riverEndRiverIndex) {

					int expandRadius = Random.Range(1, 3) * Mathf.CeilToInt(context.Data.mapSize / 100f);

					List<Tile> validStartTiles = context.Map.sortedEdgeTiles[riverStartListIndex]
						.Where(tile => Vector2.Distance(tile.PositionGrid, context.Map.sortedEdgeTiles[riverStartListIndex][0].PositionGrid) >= 10)
						.Where(tile => Vector2.Distance(tile.PositionGrid, context.Map.sortedEdgeTiles[riverStartListIndex][context.Map.sortedEdgeTiles[riverStartListIndex].Count - 1].PositionGrid) >= 10)
						.ToList();
					Tile riverStartTile = validStartTiles.RandomElement();

					List<Tile> possibleCentreTiles = context.Map.tiles.Where(t => Vector2.Distance(new Vector2(context.Data.mapSize / 2f, context.Data.mapSize / 2f), t.PositionGrid) < context.Data.mapSize / 5f).ToList();
					Tile centreTile = possibleCentreTiles.RandomElement();

					List<Tile> riverTiles = RiverPathfinding(context, riverStartTile, centreTile, expandRadius, ignoreStone);
					riverTiles.AddRange(RiverPathfinding(context, centreTile, riverEndTile, expandRadius, ignoreStone));

					River river = new River(riverTiles, expandRadius, ignoreStone);

					if (river.tiles.Count > 0) {
						context.Map.largeRivers.Add(river);
					} else {
						Debug.LogWarning($"Large River has no tiles. startTile: {riverStartTile.PositionGrid} -> endTile: {riverEndTile.PositionGrid}");
					}
				}
				riverStartListIndex += 1;
			}
		}

		private void CreateRivers(MapGenContext context) {

			int numRivers = Mathf.CeilToInt(context.Data.mapSize / 10f);

			context.Map.rivers.Clear();

			// Create a list of potential river start tiles
			List<(Tile startTile, Tile drainageBasinHighestTile)> riverStartTiles = new();
			foreach (DrainageBasin drainageBasin in context.Map.drainageBasins) {
				bool drainageBasinContainsWater = drainageBasin.Tiles.Find(tile => tile.tileType.groupType == TileTypeGroup.TypeEnum.Water) != null;
				bool drainageBasinNeighboursStone = drainageBasin.Tiles.Find(tile => tile.horizontalSurroundingTiles.Find(hTile => hTile != null && hTile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) != null) != null;
				if (!drainageBasinContainsWater || !drainageBasinNeighboursStone) {
					continue;
				}
				foreach (Tile tile in drainageBasin.Tiles) {
					if (tile.walkable && tile.tileType.groupType != TileTypeGroup.TypeEnum.Water && tile.horizontalSurroundingTiles.Find(o => o != null && o.tileType.groupType == TileTypeGroup.TypeEnum.Stone) != null) {
						riverStartTiles.Add((tile, drainageBasin.HighestTile));
					}
				}
			}

			// Create rivers
			for (int i = 0; i < numRivers && i < riverStartTiles.Count; i++) {
				(Tile startTile, Tile drainageBasinHighestTile) = riverStartTiles.RandomElement();
				List<Tile> removeTiles = new List<Tile>();
				foreach ((Tile otherStartTile, Tile _) in riverStartTiles) {
					if (Vector2.Distance(startTile.PositionGrid, otherStartTile.PositionGrid) < 5f) {
						removeTiles.Add(startTile);
					}
				}
				foreach (Tile removeTile in removeTiles) {
					riverStartTiles.Remove((removeTile, drainageBasinHighestTile));
				}
				removeTiles.Clear();

				const int expandRadius = 0;
				const bool ignoreStone = false;

				List<Tile> riverTiles = RiverPathfinding(context, startTile, drainageBasinHighestTile, expandRadius, ignoreStone);
				if (riverTiles.Count > 0) {
					River river = new River(riverTiles, expandRadius, ignoreStone);
					context.Map.rivers.Add(river);
				} else {
					Debug.LogWarning("River has no tiles. startTile: " + startTile.obj.transform.position + " endTile: " + drainageBasinHighestTile.obj.transform.position);
				}
			}
		}

		private List<Tile> RiverPathfinding(
			MapGenContext context,
			Tile riverStartTile,
			Tile riverEndTile,
			int expandRadius,
			bool ignoreStone
		) {
			PathfindingTile currentTile = new PathfindingTile(riverStartTile, null, 0);

			List<PathfindingTile> checkedTiles = new List<PathfindingTile> { currentTile };
			List<PathfindingTile> frontier = new List<PathfindingTile> { currentTile };

			List<Tile> river = new List<Tile>();

			while (frontier.Count > 0) {
				currentTile = frontier[0];
				frontier.RemoveAt(0);

				if (currentTile.tile == riverEndTile || (expandRadius == 0 && (currentTile.tile.tileType.groupType == TileTypeGroup.TypeEnum.Water || currentTile.tile.horizontalSurroundingTiles.Find(tile => tile != null && tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && !River.DoAnyRiversContainTile(tile, context.Map.rivers, context.Map.largeRivers)) != null))) {
					while (currentTile != null) {
						river.Add(currentTile.tile);
						currentTile.tile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.GrassWater), true, false, false);
						currentTile = currentTile.cameFrom;
					}
					break;
				}

				foreach (Tile nTile in currentTile.tile.horizontalSurroundingTiles) {
					if (nTile != null && checkedTiles.Find(checkedTile => checkedTile.tile == nTile) == null && (ignoreStone || nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
						if (context.Map.rivers.Find(otherRiver => otherRiver.tiles.Find(riverTile => nTile == riverTile) != null) != null) {
							frontier.Clear();
							frontier.Add(new PathfindingTile(nTile, currentTile, 0));
							nTile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.GrassWater), true, false, false);
							break;
						}
						float cost = Vector2.Distance(nTile.obj.transform.position, riverEndTile.obj.transform.position) + nTile.height * (context.Data.mapSize / 10f) + Random.Range(0, 10);
						PathfindingTile pTile = new PathfindingTile(nTile, currentTile, cost);
						frontier.Add(pTile);
						checkedTiles.Add(pTile);
					}
				}
				frontier = frontier.OrderBy(frontierTile => frontierTile.cost).ToList();
			}

			if (river.Count == 0 || expandRadius <= 0) {
				return river;
			}

			float expandedExpandRadius = expandRadius * Random.Range(2f, 4f);
			List<Tile> riverAdditions = new List<Tile>();
			riverAdditions.AddRange(river);
			foreach (Tile riverTile in river) {
				riverTile.SetTileHeight(CalculateLargeRiverTileHeight(context, expandRadius, 0));

				List<Tile> expandFrontier = new List<Tile> { riverTile };
				List<Tile> checkedExpandTiles = new List<Tile> { riverTile };
				while (expandFrontier.Count > 0) {
					Tile expandTile = expandFrontier[0];
					expandFrontier.RemoveAt(0);
					float distanceExpandTileRiverTile = Vector2.Distance(expandTile.obj.transform.position, riverTile.obj.transform.position);
					float newRiverHeight = CalculateLargeRiverTileHeight(context, expandRadius, distanceExpandTileRiverTile);
					float newRiverBankHeight = CalculateLargeRiverBankTileHeight(context, expandRadius, distanceExpandTileRiverTile);
					if (distanceExpandTileRiverTile <= expandRadius) {
						if (!riverAdditions.Contains(expandTile)) {
							riverAdditions.Add(expandTile);
							expandTile.SetTileHeight(newRiverHeight);
						}
					} else if (!riverAdditions.Contains(expandTile) && expandTile.height > newRiverBankHeight) {
						expandTile.SetTileHeight(newRiverBankHeight);
					}
					foreach (Tile nTile in expandTile.surroundingTiles) {
						if (nTile != null && !checkedExpandTiles.Contains(nTile) && (ignoreStone || nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
							if (Vector2.Distance(nTile.obj.transform.position, riverTile.obj.transform.position) <= expandedExpandRadius) {
								expandFrontier.Add(nTile);
								checkedExpandTiles.Add(nTile);
							}
						}
					}
				}
			}
			river.AddRange(riverAdditions);

			return river;
		}

		private float CalculateLargeRiverTileHeight(MapGenContext context, int expandRadius, float distanceExpandTileRiverTile) {
			float height = context.Data.terrainTypeHeights[TileTypeGroup.TypeEnum.Water] / expandRadius * distanceExpandTileRiverTile; //(2 * mapData.terrainTypeHeights[TileTypes.GrassWater]) * (distanceExpandTileRiverTile / expandedExpandRadius);
			height -= 0.01f;
			return Mathf.Clamp(height, 0f, 1f);
		}

		private float CalculateLargeRiverBankTileHeight(MapGenContext context, int expandRadius, float distanceExpandTileRiverTile) {
			float height = CalculateLargeRiverTileHeight(context, expandRadius, distanceExpandTileRiverTile / 2f);
			height += context.Data.terrainTypeHeights[TileTypeGroup.TypeEnum.Water] / 2f;
			return Mathf.Clamp(height, 0f, 1f);
		}
	}
}
