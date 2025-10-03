using System.Collections.Generic;
using System.Linq;
using Snowship.NCamera;
using Snowship.NMap.Generation;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NMap
{
	public partial class Map
	{
		internal void UpdateGlobalLighting(float time, bool forceUpdate) {
			Color newColour = MapGenerator.GetTileColourAtHour(time);
			foreach (RegionBlock visibleRegionBlock in visibleRegionBlocks) {
				if (!forceUpdate && Mathf.Approximately(visibleRegionBlock.lastBrightnessUpdate, time)) {
					continue;
				}
				visibleRegionBlock.lastBrightnessUpdate = time;
				foreach (Tile tile in visibleRegionBlock.tiles) {
					tile.SetColour(newColour, Mathf.FloorToInt(time));
				}
			}

			LightingUpdated?.Invoke();
			GameManager.Get<CameraManager>().camera.backgroundColor = newColour * 0.5f;
		}

		internal void RecalculateLighting(List<Tile> tilesToRecalculate, bool setBrightnessAtEnd, bool forceBrightnessUpdate = false) {
			List<Tile> shadowSourceTiles = DetermineShadowSourceTiles(tilesToRecalculate);
			DetermineShadowTiles(shadowSourceTiles, setBrightnessAtEnd, forceBrightnessUpdate);
		}

		private static List<Tile> DetermineShadowSourceTiles(List<Tile> tilesToRecalculate) {
			List<Tile> shadowSourceTiles = new List<Tile>();
			foreach (Tile tile in tilesToRecalculate) {
				if (tile != null && TileCanShadowTiles(tile)) {
					shadowSourceTiles.Add(tile);
				}
			}
			return shadowSourceTiles;
		}

		private const float DistanceIncreaseAmount = 0.1f;

		// TODO Convert to proper raycast system, or configure to use Unity's system (if it works for 2D?)
		private void DetermineShadowTiles(List<Tile> shadowSourceTiles, bool setBrightnessAtEnd, bool forceBrightnessUpdate) {
			if (!MapGenerator.ShadowDirectionsCalculated) {
				MapGenerator.DetermineShadowDirectionsAtHour(MapData.equatorOffset);
			}
			for (int h = 0; h < 24; h++) {
				Vector2 hourDirection = MapGenerator.CachedShadowDirectionsAtTime[h];
				float maxShadowDistanceAtHour = hourDirection.magnitude * 5f + Mathf.Pow(h - 12, 2) / 6f;
				float shadowedBrightnessAtHour = Mathf.Clamp(1 - 0.6f * MapGenerator.CalculateBrightnessLevelAtHour(h) + 0.3f, 0, 1);

				foreach (Tile shadowSourceTile in shadowSourceTiles) {
					Vector2 shadowSourceTilePosition = shadowSourceTile.obj.transform.position;
					bool shadowedAnyTile = false;

					List<Tile> shadowTiles = new List<Tile>();
					for (float distance = 0; distance <= maxShadowDistanceAtHour; distance += DistanceIncreaseAmount) {

						Vector2 nextTilePosition = shadowSourceTilePosition + hourDirection * distance;

						if (nextTilePosition.x < 0 || nextTilePosition.x >= MapData.mapSize || nextTilePosition.y < 0 || nextTilePosition.y >= MapData.mapSize) {
							break;
						}

						Tile tileToShadow = GetTileFromPosition(nextTilePosition);

						if (shadowTiles.Contains(tileToShadow)) {
							distance += DistanceIncreaseAmount;
							continue;
						}

						if (tileToShadow == shadowSourceTile) {
							continue;
						}

						float newBrightness = 1;

						if (TileCanBeShadowed(tileToShadow)) {
							shadowedAnyTile = true;
							newBrightness = shadowedBrightnessAtHour;
							if (!tileToShadow.brightnessAtHour.TryAdd(h, newBrightness)) {
								tileToShadow.brightnessAtHour[h] = Mathf.Min(tileToShadow.brightnessAtHour[h], newBrightness);
							}
							shadowTiles.Add(tileToShadow);
						} else {
							if (shadowedAnyTile || Vector2.Distance(tileToShadow.PositionGrid, shadowSourceTile.PositionGrid) > maxShadowDistanceAtHour) {
								if (tileToShadow.blockingShadowsFrom.ContainsKey(h)) {
									tileToShadow.blockingShadowsFrom[h].Add(shadowSourceTile);
								} else {
									tileToShadow.blockingShadowsFrom.Add(h, new List<Tile> { shadowSourceTile });
								}
								tileToShadow.blockingShadowsFrom[h] = tileToShadow.blockingShadowsFrom[h].Distinct().ToList();
								break;
							}
						}

						if (tileToShadow.shadowsFrom.ContainsKey(h)) {
							if (tileToShadow.shadowsFrom[h].ContainsKey(shadowSourceTile)) {
								tileToShadow.shadowsFrom[h][shadowSourceTile] = newBrightness;
							} else {
								tileToShadow.shadowsFrom[h].Add(shadowSourceTile, newBrightness);
							}
						} else {
							tileToShadow.shadowsFrom.Add(h, new Dictionary<Tile, float> { { shadowSourceTile, newBrightness } });
						}
					}
					if (!shadowSourceTile.shadowsTo.TryAdd(h, shadowTiles)) {
						shadowSourceTile.shadowsTo[h].AddRange(shadowTiles);
					}
					shadowSourceTile.shadowsTo[h] = shadowSourceTile.shadowsTo[h].Distinct().ToList();
				}
			}
			if (setBrightnessAtEnd) {
				UpdateGlobalLighting(GameManager.Get<TimeManager>().Time.TileBrightnessTime, forceBrightnessUpdate);
			}
		}

		public void RemoveTileBrightnessEffect(Tile tile) {
			List<Tile> tilesToRecalculateShadowsFor = new List<Tile>();
			for (int h = 0; h < 24; h++) {
				if (tile.shadowsTo.TryGetValue(h, out List<Tile> shadowedTiles)) {
					foreach (Tile nTile in shadowedTiles) {
						float darkestBrightnessAtHour = 1f;
						if (nTile.shadowsFrom.ContainsKey(h)) {
							nTile.shadowsFrom[h].Remove(tile);
							if (nTile.shadowsFrom[h].Count > 0) {
								darkestBrightnessAtHour = nTile.shadowsFrom[h].Min(shadowFromTile => shadowFromTile.Value);
							}
						}
						if (nTile.brightnessAtHour.ContainsKey(h)) {
							nTile.brightnessAtHour[h] = darkestBrightnessAtHour;
						}
						nTile.SetBrightness(darkestBrightnessAtHour, 12);
					}
				}
				if (tile.shadowsFrom.ContainsKey(h)) {
					tilesToRecalculateShadowsFor.AddRange(tile.shadowsFrom[h].Keys);
				}
				if (tile.blockingShadowsFrom.ContainsKey(h)) {
					tilesToRecalculateShadowsFor.AddRange(tile.blockingShadowsFrom[h]);
				}
			}
			tilesToRecalculateShadowsFor.AddRange(tile.surroundingTiles.Where(nTile => nTile != null));

			tile.shadowsFrom.Clear();
			tile.shadowsTo.Clear();
			tile.blockingShadowsFrom.Clear();

			RecalculateLighting(tilesToRecalculateShadowsFor.Distinct().ToList(), true);
		}

		private static bool TileCanShadowTiles(Tile tile) {
			return tile.surroundingTiles.Any(nTile => nTile is { blocksLight: false }) && (tile.blocksLight || tile.HasRoof());
		}

		private static bool TileCanBeShadowed(Tile tile) {
			return !tile.blocksLight || (!tile.blocksLight && tile.HasRoof());
		}
	}
}
