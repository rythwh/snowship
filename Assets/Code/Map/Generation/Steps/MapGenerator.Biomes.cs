using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.NTile;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private static float TemperatureFromMapLatitude(
			MapGenContext context,
			float yPos
		) {
			return -2 * Mathf.Abs((yPos - context.Data.mapSize / 2f) / (context.Data.mapSize / 100f / (context.Data.temperatureRange / 50f))) + context.Data.temperatureRange + context.Data.temperatureOffset + context.Random.NextFloat(-50f, 50f);
		}

		private static void CalculateTemperature(MapGenContext context) {
			foreach (Tile tile in context.Map.tiles) {
				tile.temperature = context.Data.planetTemperature
					? TemperatureFromMapLatitude(context, tile.PositionGrid.y)
					: context.Data.averageTemperature;
				tile.temperature += -(50f * Mathf.Pow(tile.height - 0.5f, 3));
			}
		}

		private void AverageTileTemperatures(MapGenContext context) {
			const int numPasses = 3;
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTileTemperatures = new List<float>();

				foreach (Tile tile in context.Map.tiles) {
					float averageTemperature = tile.temperature;
					int numValidTiles = 1;
					foreach (Tile nTile in tile.surroundingTiles) {
						if (nTile == null) {
							continue;
						}
						numValidTiles += 1;
						averageTemperature += nTile.temperature;
					}
					averageTemperature /= numValidTiles;
					averageTileTemperatures.Add(averageTemperature);
				}

				for (int k = 0; k < context.Map.tiles.Count; k++) {
					context.Map.tiles[k].temperature = averageTileTemperatures[k];
				}
			}
		}

		private static readonly int[] oppositeDirectionTileMap = { 2, 3, 0, 1, 6, 7, 4, 5 };
		private static readonly float[,] windStrengthMap = {
			{ 1.0f, 0.6f, 0.1f, 0.6f, 0.8f, 0.2f, 0.2f, 0.8f },
			{ 0.6f, 1.0f, 0.6f, 0.1f, 0.8f, 0.8f, 0.2f, 0.2f },
			{ 0.1f, 0.6f, 1.0f, 0.6f, 0.2f, 0.8f, 0.8f, 0.2f },
			{ 0.6f, 0.1f, 0.6f, 1.0f, 0.2f, 0.2f, 0.8f, 0.8f },
			{ 0.8f, 0.8f, 0.2f, 0.2f, 1.0f, 0.6f, 0.1f, 0.6f },
			{ 0.2f, 0.8f, 0.8f, 0.2f, 0.6f, 1.0f, 0.6f, 0.1f },
			{ 0.2f, 0.2f, 0.8f, 0.8f, 0.1f, 0.6f, 1.0f, 0.6f },
			{ 0.8f, 0.2f, 0.2f, 0.8f, 0.6f, 0.1f, 0.6f, 1.0f }
		};
		private readonly List<List<float>> directionPrecipitations = new();
		private const int WindDirectionMin = 0;
		private const int WindDirectionMax = 7;
		private float windStrengthMapSum = 0;
		private int primaryWindDirection;

		private void CalculateWindDirections(MapGenContext context) {

			// 0 - up, 1 - right, 2 - down, 3 - left, 4 - up/right, 5 - down/right, 6 - down-left, 7 - up/left
			for (int windDirection = WindDirectionMin; windDirection < WindDirectionMax + 1; windDirection++) {
				if (windDirection <= 3) { // Wind is going horizontally/vertically
					bool yStartAtTop = windDirection == 2;
					bool xStartAtRight = windDirection == 3;

					for (int y = yStartAtTop ? context.Data.mapSize - 1 : 0; yStartAtTop ? y >= 0 : y < context.Data.mapSize; y += yStartAtTop ? -1 : 1) {
						for (int x = xStartAtRight ? context.Data.mapSize - 1 : 0; xStartAtRight ? x >= 0 : x < context.Data.mapSize; x += xStartAtRight ? -1 : 1) {
							Tile tile = context.Map.sortedTiles[y][x];
							Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
							SetTilePrecipitation(context, tile, previousTile, context.Data.planetTemperature);
						}
					}
				} else { // Wind is going diagonally
					bool up = windDirection is 4 or 7;
					bool left = windDirection is 6 or 7;
					int mapSizeDoubled = context.Data.mapSize * 2;
					for (int k = up ? 0 : mapSizeDoubled; up ? k < mapSizeDoubled : k >= 0; k += up ? 1 : -1) {
						for (int x = left ? k : 0; left ? x >= 0 : x <= k; x += left ? -1 : 1) {
							int y = k - x;
							if (y >= context.Data.mapSize || x >= context.Data.mapSize) {
								continue;
							}
							Tile tile = context.Map.sortedTiles[y][x];
							Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
							SetTilePrecipitation(context, tile, previousTile, context.Data.planetTemperature);
						}
					}
				}
				directionPrecipitations.Add(new List<float>());
				for (int t = 0; t < context.Map.tiles.Count; t++) {
					Tile tile = context.Map.tiles[t];
					directionPrecipitations[windDirection].Add(tile.GetPrecipitation());
					tile.SetPrecipitation(0);
				}
			}

			primaryWindDirection = context.Data.primaryWindDirection == -1
				? context.Random.NextInt(WindDirectionMin, WindDirectionMax + 1)
				: context.Data.primaryWindDirection;

			for (int i = WindDirectionMin; i < WindDirectionMax + 1; i++) {
				windStrengthMapSum += windStrengthMap[primaryWindDirection, i];
			}
		}

		private void CalculatePrecipitation(MapGenContext context) {
			for (int t = 0; t < context.Map.tiles.Count; t++) {
				Tile tile = context.Map.tiles[t];
				tile.SetPrecipitation(0);
				for (int i = WindDirectionMin; i < WindDirectionMax + 1; i++) {
					tile.SetPrecipitation(tile.GetPrecipitation() + directionPrecipitations[i][t] * windStrengthMap[primaryWindDirection, i]);
				}
				tile.SetPrecipitation(tile.GetPrecipitation() / windStrengthMapSum);
			}

			AverageTilePrecipitations(context);

			foreach (Tile tile in context.Map.tiles) {
				if (Mathf.RoundToInt(context.Data.averagePrecipitation) != -1) {
					tile.SetPrecipitation((tile.GetPrecipitation() + context.Data.averagePrecipitation) / 2f);
				}
				tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
			}
		}

		private void SetTilePrecipitation(MapGenContext context, Tile tile, Tile previousTile, bool planet) {
			if (planet) {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier * (context.Data.mapSize / 5f));
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(previousTile.GetPrecipitation() * previousTileDistanceMultiplier * 0.9f);
					} else {
						tile.SetPrecipitation(previousTile.GetPrecipitation() * previousTileDistanceMultiplier * 0.95f);
					}
				} else {
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater] || tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(1f);
					} else {
						tile.SetPrecipitation(0.1f);
					}
				}
			} else {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						float waterMultiplier = context.Data.mapSize / 5f;
						if (River.DoAnyRiversContainTile(tile, context.Map.rivers, context.Map.largeRivers)) {
							waterMultiplier *= 5;
						}
						tile.SetPrecipitation((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier * waterMultiplier);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(previousTile.GetPrecipitation() * previousTileDistanceMultiplier * context.Random.NextFloat(0.95f, 0.99f));
					} else {
						tile.SetPrecipitation(previousTile.GetPrecipitation() * previousTileDistanceMultiplier * context.Random.NextFloat(0.98f, 1f));
					}
				} else {
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater] || tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(1f);
					} else {
						tile.SetPrecipitation(context.Data.averagePrecipitation);
					}
				}
			}
			tile.SetPrecipitation(ChangePrecipitationByTemperature(tile.GetPrecipitation(), tile.temperature));
			tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
		}

		private float ChangePrecipitationByTemperature(float precipitation, float temperature) {
			return precipitation * Mathf.Clamp(-Mathf.Pow((temperature - 30) / (90 - 30), 3) + 1, 0f, 1f); // Less precipitation as the temperature gets higher
		}

		private void AverageTilePrecipitations(MapGenContext context) {
			const int numPasses = 5;
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTilePrecipitations = new List<float>();

				foreach (Tile tile in context.Map.tiles) {
					float averagePrecipitation = tile.GetPrecipitation();
					int numValidTiles = 1;
					foreach (Tile nTile in tile.surroundingTiles) {
						if (nTile == null) {
							continue;
						}
						numValidTiles += 1;
						averagePrecipitation += nTile.GetPrecipitation();
					}
					averagePrecipitation /= numValidTiles;
					averageTilePrecipitations.Add(averagePrecipitation);
				}

				for (int k = 0; k < context.Map.tiles.Count; k++) {
					context.Map.tiles[k].SetPrecipitation(averageTilePrecipitations[k]);
				}
			}
		}

		private void SetBiomes(MapGenContext context) {

			/* Biome Testing
			for (int y = mapData.mapSize - 1; y >= 0; y--) {
				for (int x = 0; x < mapData.mapSize; x++) {
					Tile tile = sortedTiles[y][x];
					tile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass), false, false, false);
					tile.temperature = 2f * (mapData.temperatureRange * (y / (float)mapData.mapSize) - (mapData.temperatureRange / 2f));
					tile.SetPrecipitation(x / (float)mapData.mapSize);
				}
			}
			*/

			foreach (Tile tile in context.Map.tiles) {
				foreach (Biome biome in Biome.biomes) {
					foreach (Biome.Range range in biome.ranges) {
						if (!range.IsInRange(tile.GetPrecipitation(), tile.temperature)) {
							continue;
						}
						tile.SetBiome(biome, context.Data.actualMap);
						if (tile.plant is { small: true }) {
							tile.plant.growthProgress = context.Random.NextInt(0, SimulationDateTime.DayLengthSeconds * 4);
						}
					}
				}
			}
		}

		private void SetCoastalWater(MapGenContext context) {
			foreach (Tile tile in context.Map.tiles) {
				tile.CoastalWater = tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && tile.surroundingTiles.Count(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) > 0;
			}
		}
	}
}
