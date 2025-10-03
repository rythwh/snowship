using System.Collections.Generic;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private void CreateTiles(MapGenContext context) {
			for (int y = 0; y < context.Data.mapSize; y++) {
				List<Tile> innerTiles = new List<Tile>();
				for (int x = 0; x < context.Data.mapSize; x++) {

					float height = context.Random.NextFloat(0f, 1f);

					Vector2Int position = new(x, y);

					Tile tile = new Tile(context.Map, position, height);

					innerTiles.Add(tile);
					context.Map.tiles.Add(tile);
				}
				context.Map.sortedTiles.Add(innerTiles);
			}
		}

		private void SetSurroundingTiles(MapGenContext context) {
			for (int y = 0; y < context.Data.mapSize; y++) {
				for (int x = 0; x < context.Data.mapSize; x++) {
					/* Horizontal */
					if (y + 1 < context.Data.mapSize) {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(context.Map.sortedTiles[y + 1][x]);
					} else {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x + 1 < context.Data.mapSize) {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(context.Map.sortedTiles[y][x + 1]);
					} else {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0) {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(context.Map.sortedTiles[y - 1][x]);
					} else {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0) {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(context.Map.sortedTiles[y][x - 1]);
					} else {
						context.Map.sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}

					/* Diagonal */
					if (x + 1 < context.Data.mapSize && y + 1 < context.Data.mapSize) {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(context.Map.sortedTiles[y + 1][x + 1]);
					} else {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0 && x + 1 < context.Data.mapSize) {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(context.Map.sortedTiles[y - 1][x + 1]);
					} else {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0 && y - 1 >= 0) {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(context.Map.sortedTiles[y - 1][x - 1]);
					} else {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y + 1 < context.Data.mapSize && x - 1 >= 0) {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(context.Map.sortedTiles[y + 1][x - 1]);
					} else {
						context.Map.sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}

					context.Map.sortedTiles[y][x].surroundingTiles.AddRange(context.Map.sortedTiles[y][x].horizontalSurroundingTiles);
					context.Map.sortedTiles[y][x].surroundingTiles.AddRange(context.Map.sortedTiles[y][x].diagonalSurroundingTiles);
				}
			}
		}

		private void SetMapEdgeTiles(MapGenContext context) {
			context.Map.edgeTiles.Clear();
			for (int i = 1; i < context.Data.mapSize - 1; i++) {
				context.Map.edgeTiles.Add(context.Map.sortedTiles[0][i]);
				context.Map.edgeTiles.Add(context.Map.sortedTiles[context.Data.mapSize - 1][i]);
				context.Map.edgeTiles.Add(context.Map.sortedTiles[i][0]);
				context.Map.edgeTiles.Add(context.Map.sortedTiles[i][context.Data.mapSize - 1]);
			}
			context.Map.edgeTiles.Add(context.Map.sortedTiles[0][0]);
			context.Map.edgeTiles.Add(context.Map.sortedTiles[0][context.Data.mapSize - 1]);
			context.Map.edgeTiles.Add(context.Map.sortedTiles[context.Data.mapSize - 1][0]);
			context.Map.edgeTiles.Add(context.Map.sortedTiles[context.Data.mapSize - 1][context.Data.mapSize - 1]);
		}

		private void SetSortedMapEdgeTiles(MapGenContext context) {
			context.Map.sortedEdgeTiles.Clear();

			int sideNum = -1;
			List<Tile> tilesOnThisEdge = null;
			for (int i = 0; i <= context.Data.mapSize; i++) {
				i %= context.Data.mapSize;
				if (i == 0) {
					sideNum += 1;
					context.Map.sortedEdgeTiles.Add(sideNum, new List<Tile>());
					tilesOnThisEdge = context.Map.sortedEdgeTiles[sideNum];
				}
				if (sideNum == 0) {
					tilesOnThisEdge.Add(context.Map.sortedTiles[context.Data.mapSize - 1][i]);
				} else if (sideNum == 1) {
					tilesOnThisEdge.Add(context.Map.sortedTiles[i][context.Data.mapSize - 1]);
				} else if (sideNum == 2) {
					tilesOnThisEdge.Add(context.Map.sortedTiles[0][i]);
				} else if (sideNum == 3) {
					tilesOnThisEdge.Add(context.Map.sortedTiles[i][0]);
				} else {
					break;
				}
			}
		}
	}
}
