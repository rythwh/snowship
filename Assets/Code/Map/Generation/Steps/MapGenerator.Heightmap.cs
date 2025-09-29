using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private void GenerateTerrain(MapGenContext context) {
			int lastSize = context.Data.mapSize;
			for (int halves = 0; halves < Mathf.CeilToInt(Mathf.Log(context.Data.mapSize, 2)); halves++) {
				int size = Mathf.CeilToInt(lastSize / 2f);
				float deviationSpan = Mathf.Abs((size - context.Data.mapSize) / (4f * context.Data.mapSize));
				for (int sectionY = 0; sectionY < context.Data.mapSize; sectionY += size) {
					for (int sectionX = 0; sectionX < context.Data.mapSize; sectionX += size) {
						float sectionAverage = 0;
						for (int y = sectionY; y < sectionY + size && y < context.Data.mapSize; y++) {
							for (int x = sectionX; x < sectionX + size && x < context.Data.mapSize; x++) {
								sectionAverage += context.Map.sortedTiles[y][x].height;
							}
						}
						sectionAverage /= size * size;
						float deviation = context.Random.NextFloat(-deviationSpan, deviationSpan);
						sectionAverage += deviation;
						for (int y = sectionY; y < sectionY + size && y < context.Data.mapSize; y++) {
							for (int x = sectionX; x < sectionX + size && x < context.Data.mapSize; x++) {
								context.Map.sortedTiles[y][x].height = sectionAverage;
							}
						}
					}
				}
				lastSize = size;
			}

			foreach (Tile tile in context.Map.tiles) {
				tile.SetTileHeight(tile.height);
			}
		}

		private void AverageTileHeights(MapGenContext context) {

			const int numPasses = 3;

			for (int i = 0; i < numPasses; i++) {
				List<float> averageTileHeights = new List<float>();

				foreach (Tile tile in context.Map.tiles) {
					float averageHeight = tile.height;
					float numValidTiles = 1;
					for (int nTileIndex = 0; nTileIndex < tile.surroundingTiles.Count; nTileIndex++) {
						Tile nTile = tile.surroundingTiles[nTileIndex];
						if (nTile == null) {
							continue;
						}
						float weight = 1f;
						if (nTileIndex > 3) { // If nTile is diagonal to tile
							numValidTiles += 1f;
						} else {
							numValidTiles += 0.5f;
							weight = 0.5f; // Reduces the weight of horizontal tiles by 50% to help prevent visible edges/corners on the map
						}
						averageHeight += nTile.height * weight;
					}
					averageHeight /= numValidTiles;
					averageTileHeights.Add(averageHeight);
				}

				// Apply after calculating averages to prevent processing over new values in previous step
				for (int k = 0; k < context.Map.tiles.Count; k++) {
					context.Map.tiles[k].height = averageTileHeights[k];
				}
			}

			foreach (Tile tile in context.Map.tiles) {
				tile.SetTileTypeByHeight();
			}
		}

		private void PreventEdgeTouching(MapGenContext context) {
			foreach (Tile tile in context.Map.tiles) {
				float edgeDistance = (context.Data.mapSize - Vector2.Distance(tile.obj.transform.position, new Vector2(context.Data.mapSize / 2f, context.Data.mapSize / 2f))) / context.Data.mapSize;
				tile.SetTileHeight(tile.height * Mathf.Clamp(-Mathf.Pow(edgeDistance - 1.5f, 10) + 1, 0f, 1f));
			}
		}

		private void SmoothHeightWithSurroundingPlanetTiles(MapGenContext context) {
			for (int i = 0; i < context.Data.surroundingPlanetTileHeightDirections.Count; i++) {
				if (context.Data.surroundingPlanetTileHeightDirections[i] != 0) {
					foreach (Tile tile in context.Map.tiles) {
						float closestEdgeDistance = context.Map.sortedEdgeTiles[i].Min(edgeTile => Vector2.Distance(edgeTile.obj.transform.position, tile.obj.transform.position)) / context.Data.mapSize;
						float heightMultiplier = context.Data.surroundingPlanetTileHeightDirections[i] * Mathf.Pow(closestEdgeDistance - 1f, 10f) + 1f;
						float newHeight = Mathf.Clamp(tile.height * heightMultiplier, 0f, 1f);
						tile.SetTileHeight(newHeight);
					}
				}
			}
		}
	}
}
