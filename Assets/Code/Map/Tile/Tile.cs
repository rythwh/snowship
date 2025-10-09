using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.Models.Structure;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NMap.NTile
{
	public class Tile {

		public readonly Map map;

		public GameObject obj;
		public SpriteRenderer sr;

		public Vector2Int PositionGrid { get; }
		public Vector2 PositionWorld => obj.transform.position;

		public enum EGridConnectivity
		{
			FourWay,
			EightWay
		}

		public Dictionary<EGridConnectivity, List<Tile>> SurroundingTiles { get; } = new();

		public List<Tile> horizontalSurroundingTiles => SurroundingTiles[EGridConnectivity.FourWay];
		public List<Tile> surroundingTiles => SurroundingTiles[EGridConnectivity.EightWay];

		public float height;

		public TileType tileType;

		public Region region;
		public Region drainageBasin;
		public RegionBlock regionBlock;
		public RegionBlock squareRegionBlock;

		public Biome biome;
		public Plant plant;
		public Farm farm;

		private float precipitation = 0;
		public float temperature = 0;

		public bool walkable = false;
		public float walkSpeed = 0;

		public bool buildable = false;

		public bool blocksLight = false;

		private bool roof = false;
		public bool CoastalWater { get; set; } = false;

		public float brightness = 0;
		public Dictionary<int, float> brightnessAtHour = new Dictionary<int, float>();
		public Dictionary<int, Dictionary<Tile, float>> shadowsFrom = new Dictionary<int, Dictionary<Tile, float>>(); // Tiles that affect the shadow on this tile
		public Dictionary<int, List<Tile>> shadowsTo = new Dictionary<int, List<Tile>>(); // Tiles that have shadows due to this tile
		public Dictionary<int, List<Tile>> blockingShadowsFrom = new Dictionary<int, List<Tile>>(); // Tiles that have shadows that were cut short because this tile was in the way

		public Dictionary<LightSource, float> lightSourceBrightnesses = new();
		public LightSource primaryLightSource;
		public float lightSourceBrightness;

		public Dictionary<int, ObjectInstance> objectInstances = new();

		public bool dugPreviously;

		public bool visible;

		public Tile(GameObject tilePrefab, Map map, Vector2Int positionGrid, float height) {
			this.map = map;

			this.PositionGrid = positionGrid;

			// TODO Move somewhere else to allow creating a "headless" map
			obj = Object.Instantiate(
				tilePrefab,
				new Vector2(positionGrid.x + 0.5f, positionGrid.y + 0.5f),
				Quaternion.identity
			);
			obj.transform.SetParent(GameManager.Get<SharedReferences>().TileParent, true);
			obj.name = $"Tile-{positionGrid}";

			sr = obj.GetComponent<SpriteRenderer>();
			sr.sortingOrder = (int)SortingOrder.Tile + (Mathf.RoundToInt(positionGrid.y) * 2);

			SetTileHeight(height);

			SetBrightness(1f, 12);
		}

		public void SetTileHeight(float height) {
			this.height = height;
			SetTileTypeByHeight();
		}

		public void SetTileType(TileType tileType, bool setBiomeTileType, bool bitmask, bool redetermineRegion) {
			TileType oldTileType = this.tileType;
			this.tileType = tileType;

			if (setBiomeTileType && biome != null) {
				SetBiome(biome, true);
			}

			walkable = tileType.walkable;
			buildable = tileType.buildable;
			blocksLight = tileType.blocksLight;

			if (bitmask) {
				map.RedrawTiles(new List<Tile>() { this }.Concat(surroundingTiles).ToList(), true, !redetermineRegion); // Lighting automatically recalculated in RedetermineRegion()
			}

			if (plant != null && !tileType.classes[TileType.ClassEnum.Plantable]) {
				plant.Remove();
				plant = null;
			}

			if (redetermineRegion) {
				RedetermineRegion(oldTileType);
			}

			SetWalkSpeed();
		}

		public void RedetermineRegion(TileType oldTileType) {
			if (walkable != oldTileType.walkable) { // Difference in walkability
				if (region != null) {
					region.tiles.Remove(this);
				}
				if (walkable && !oldTileType.walkable) { // Type is walkable, old type wasn't (e.g. stone mined, now ground)
					List<Region> surroundingRegions = new List<Region>();
					bool anyVisible = false;
					foreach (Tile tile in horizontalSurroundingTiles) {
						if (tile != null && tile.region != null && !surroundingRegions.Contains(tile.region)) {
							surroundingRegions.Add(tile.region);
							if (tile.visible) {
								anyVisible = true;
							}
						}
					}
					if (surroundingRegions.Count > 0) {
						Region largestRegion = surroundingRegions.OrderByDescending(r => r.tiles.Count).FirstOrDefault();
						ChangeRegion(largestRegion, false, false);
						surroundingRegions.Remove(largestRegion);
						foreach (Region surroundingRegion in surroundingRegions) {
							if (surroundingRegion.Visible != anyVisible) {
								surroundingRegion.SetVisible(anyVisible, false, true);
							}
							foreach (Tile tile in surroundingRegion.tiles) {
								tile.ChangeRegion(largestRegion, false, false);
							}
							surroundingRegion.tiles.Clear();
							GameManager.Get<IMapQuery>().Map.regions.Remove(surroundingRegion);
						}
						region.SetVisible(anyVisible, true, false);
					} else {
						ChangeRegion(new Region(tileType), false, false);
					}
				} else { // Type is not walkable, old type was walkable (e.g. was ground, now stone)
					ChangeRegion(null, false, false);
				}
			}
			/*
			if (oldTileType.walkable != walkable && region != null) {
				bool setParentTileRegion = false;
				if (!oldTileType.walkable && walkable) { // If a non-walkable tile became a walkable tile (splits two non-walkable regions)
					setParentTileRegion = true;

					List<Tile> nonWalkableSurroundingTiles = new List<Tile>();
					foreach (Tile tile in horizontalSurroundingTiles) {
						if (tile != null && !tile.walkable) {
							nonWalkableSurroundingTiles.Add(tile);
						}
					}
					List<Tile> removeFromNonWalkableSurroundingTiles = new List<Tile>();
					foreach (Tile tile in nonWalkableSurroundingTiles) {
						if (!removeFromNonWalkableSurroundingTiles.Contains(tile)) {
							int tileIndex = surroundingTiles.IndexOf(tile);
							List<List<int>> orderedIndexesToCheckList = nonWalkableSurroundingTilesComparatorMap[tileIndex];
							bool removedOppositeTile = false;
							foreach (List<int> orderedIndexesToCheck in orderedIndexesToCheckList) {
								if (surroundingTiles[orderedIndexesToCheck[0]] != null && !surroundingTiles[orderedIndexesToCheck[0]].walkable) {
									if (nonWalkableSurroundingTiles.Contains(surroundingTiles[orderedIndexesToCheck[1]])) {
										removeFromNonWalkableSurroundingTiles.Add(surroundingTiles[orderedIndexesToCheck[1]]);
										if (!removedOppositeTile && surroundingTiles[orderedIndexesToCheck[2]] != null && !surroundingTiles[orderedIndexesToCheck[2]].walkable) {
											if (nonWalkableSurroundingTiles.Contains(surroundingTiles[orderedIndexesToCheck[3]])) {
												removeFromNonWalkableSurroundingTiles.Add(surroundingTiles[orderedIndexesToCheck[3]]);
												removedOppositeTile = true;
											}
										}
									}
								}
							}
						}
					}
					foreach (Tile tile in removeFromNonWalkableSurroundingTiles) {
						nonWalkableSurroundingTiles.Remove(tile);
					}
					if (nonWalkableSurroundingTiles.Count > 1) {
						Debug.Log("Independent tiles");
						Map.Region oldRegion = region;
						oldRegion.tiles.Clear();
						map.regions.Remove(oldRegion);
						List<List<Tile>> nonWalkableTileGroups = new List<List<Tile>>();
						foreach (Tile nonWalkableTile in nonWalkableSurroundingTiles) {
							Tile currentTile = nonWalkableTile;
							List<Tile> frontier = new List<Tile>() { currentTile };
							List<Tile> checkedTiles = new List<Tile>() { currentTile };
							List<Tile> nonWalkableTiles = new List<Tile>();
							bool addGroup = true;
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								if (nonWalkableTileGroups.Find(group => group.Contains(currentTile)) != null) {
									Debug.Log("Separate tiles part of the same group");
									addGroup = false;
									break;
								}
								frontier.RemoveAt(0);
								nonWalkableTiles.Add(currentTile);
								foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && !checkedTiles.Contains(nTile) && !nTile.walkable) {
										frontier.Add(nTile);
										checkedTiles.Add(nTile);
									}
								}
							}
							if (addGroup) {
								nonWalkableTileGroups.Add(nonWalkableTiles);
							}
						}
						foreach (List<Tile> nonWalkableTileGroup in nonWalkableTileGroups) {
							Map.Region groupRegion = new Map.Region(nonWalkableTileGroup[0].tileType, map.largestCurrentRegionId);
							map.largestCurrentRegionId += 1;
							foreach (Tile tile in nonWalkableTileGroup) {
								tile.ChangeRegion(groupRegion, false, false);
							}
							map.regions.Add(groupRegion);
						}
					}
				}
				if (setParentTileRegion || (oldTileType.walkable && !walkable)) { // If a walkable tile became a non-walkable tile (add non-walkable tile to nearby non-walkable region if exists, if not create it)
					List<Map.Region> similarRegions = new List<Map.Region>();
					foreach (Tile tile in horizontalSurroundingTiles) {
						if (tile != null && tile.region != null && tile.walkable == walkable) {
							if (tile.region != region) {
								similarRegions.Add(tile.region);
							}
						}
					}
					if (similarRegions.Count == 0) {
						region.tiles.Remove(this);
						ChangeRegion(new Map.Region(tileType, map.largestCurrentRegionId), false, false);
						map.largestCurrentRegionId += 1;
					} else if (similarRegions.Count == 1) {
						region.tiles.Remove(this);
						ChangeRegion(similarRegions[0], false, false);
					} else {
						region.tiles.Remove(this);
						ChangeRegion(similarRegions.OrderByDescending(similarRegion => similarRegion.tiles.Count).ToList()[0], false, false);
						foreach (Map.Region similarRegion in similarRegions) {
							if (similarRegion != region) {
								foreach (Tile tile in similarRegion.tiles) {
									tile.ChangeRegion(region, false, false);
								}
								similarRegion.tiles.Clear();
								map.regions.Remove(similarRegion);
							}
						}
					}
				}
			}
			*/
		}

		public void ChangeRegion(Region region, bool changeTileTypeToRegionType, bool bitmask) {
			//if (this.region != null) {
			//	this.region.tiles.Remove(this);
			//}
			this.region = region;
			if (region != null) {
				region.tiles.Add(this);
				if (!map.regions.Contains(region)) {
					map.regions.Add(region);
				}
				if (changeTileTypeToRegionType) {
					SetTileType(region.tileType, true, bitmask, false);
				}
			}
		}

		public void SetTileTypeByHeight() {
			if (height < map.MapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Water]) {
				SetTileType(TileType.GetTileTypeByEnum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).defaultTileType), false, false, false);
			} else if (height > map.MapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Stone]) {
				SetTileType(TileType.GetTileTypeByEnum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).defaultTileType), false, false, false);
			} else {
				SetTileType(TileType.GetTileTypeByEnum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Ground).defaultTileType), false, false, false);
			}
		}

		public void SetBiome(Biome biome, bool setPlant) {
			this.biome = biome;
			SetTileType(biome.tileTypes[tileType.groupType], false, false, false);
			if (setPlant && tileType.classes[TileType.ClassEnum.Plantable]) {
				SetPlant(false, null);
			}
		}

		public void SetPlant(bool onlyRemovePlant, Plant specificPlant) {
			if (plant != null) {
				plant.Remove();
				plant = null;
			}
			if (!onlyRemovePlant) {
				if (specificPlant == null) {
					PlantPrefab biomePlantGroup = PlantPrefab.GetPlantPrefabByBiome(biome, false);
					if (biomePlantGroup != null) {
						plant = new Plant(biomePlantGroup, this, null, true, null);
					}
				} else {
					plant = specificPlant;
				}
			}
			SetWalkSpeed();
		}

		public void SetObject(ObjectInstance instance) {
			AddObjectInstanceToLayer(instance, instance.prefab.layer);
			PostChangeObject();
		}

		public void PostChangeObject() {
			walkable = tileType.walkable;
			buildable = tileType.buildable;
			blocksLight = tileType.blocksLight;

			bool recalculatedLighting = false;
			bool recalculatedRegion = false;

			foreach (KeyValuePair<int, ObjectInstance> layerToObjectInstance in objectInstances) {
				if (layerToObjectInstance.Value != null) {

					// Object Instances are iterated from lowest layer to highest layer (sorted in AddObjectInstaceToLayer),
					// therefore, the highest layer is the buildable value that should be applied
					buildable = layerToObjectInstance.Value.prefab.buildable;

					if (!recalculatedLighting && layerToObjectInstance.Value.prefab.blocksLight) {
						blocksLight = true;
						map.RecalculateLighting(new List<Tile>() { this }, true);

						recalculatedLighting = true;
					}

					if (!recalculatedRegion && !layerToObjectInstance.Value.prefab.walkable) {
						walkable = false;
						map.RecalculateRegionsAtTile(this);

						recalculatedRegion = true;
					}
				}
			}
			SetWalkSpeed();
		}

		private void AddObjectInstanceToLayer(ObjectInstance instance, int layer) {
			if (objectInstances.ContainsKey(layer)) { // If the layer exists
				if (objectInstances[layer] != null) { // If the object at the layer exists
					if (instance != null) { // If the object being added exists, throw error
						Debug.LogError("Trying to add object where one already exists at " + obj.transform.position);
					} else { // If the object being added is null, set this layer to null
						objectInstances[layer] = null;
					}
				} else { // If the object at the layer does not exist
					objectInstances[layer] = instance;
				}
			} else { // If the layer does not exist
				objectInstances.Add(layer, instance);
			}
			objectInstances.OrderBy(kvp => kvp.Key); // Sorted from lowest layer to highest layer for iterating
		}

		public void RemoveObjectAtLayer(int layer) {
			if (objectInstances.ContainsKey(layer)) {
				ObjectInstance instance = objectInstances[layer];
				if (instance != null) {
					MonoBehaviour.Destroy(instance.obj);
					foreach (Tile additionalTile in instance.additionalTiles) {
						additionalTile.objectInstances[layer] = null;
						additionalTile.PostChangeObject();
					}
					if (instance.prefab.instanceType == ObjectInstance.ObjectInstanceType.Farm) {
						farm = null;
					}
					objectInstances[layer] = null;
				}
			}
			PostChangeObject();
		}

		public void SetObjectInstanceReference(ObjectInstance objectInstanceReference) {
			if (objectInstances.ContainsKey(objectInstanceReference.prefab.layer)) {
				if (objectInstances[objectInstanceReference.prefab.layer] != null) {
					if (objectInstanceReference != null) {
						Debug.LogError("Trying to add object where one already exists at " + obj.transform.position);
					} else {
						objectInstances[objectInstanceReference.prefab.layer] = null;
					}
				} else {
					objectInstances[objectInstanceReference.prefab.layer] = objectInstanceReference;
				}
			} else {
				objectInstances.Add(objectInstanceReference.prefab.layer, objectInstanceReference);
			}
			PostChangeObject();
		}

		public ObjectInstance GetObjectInstanceAtLayer(int layer) {
			if (objectInstances.ContainsKey(layer)) {
				return objectInstances[layer];
			}
			return null;
		}

		public List<ObjectInstance> GetAllObjectInstances() {
			List<ObjectInstance> allObjectInstances = new();
			foreach (KeyValuePair<int, ObjectInstance> kvp in objectInstances) {
				if (kvp.Value != null) {
					allObjectInstances.Add(kvp.Value);
				}
			}
			return allObjectInstances;
		}

		public bool HasRoof() {
			return roof;
		}

		public void SetRoof(bool roof) {
			this.roof = roof;
		}

		public void SetWalkSpeed() {
			walkSpeed = tileType.walkSpeed;
			if (plant != null && walkSpeed > 0.6f) {
				walkSpeed = 0.6f;
			}
			ObjectInstance lowestWalkSpeedObject = objectInstances.Values.Where(o => o != null).OrderBy(o => o.prefab.walkSpeed).FirstOrDefault();
			if (lowestWalkSpeedObject != null) {
				walkSpeed = lowestWalkSpeedObject.prefab.walkSpeed;
			}
		}

		public void SetColour(Color newColour, float decimalHour) {
			int hour = Mathf.FloorToInt(decimalHour);
			float currentHourBrightness = Mathf.Max((brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f), lightSourceBrightness);
			int nextHour = (hour == 23 ? 0 : hour + 1);
			float nextHourBrightness = Mathf.Max((brightnessAtHour.ContainsKey(nextHour) ? brightnessAtHour[nextHour] : 1f), lightSourceBrightness);

			if (primaryLightSource != null) {
				sr.color = Color.Lerp(newColour, primaryLightSource.prefab.lightColour + (newColour * (brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f) * 0.8f), lightSourceBrightness);
			} else {
				sr.color = newColour;
			}
			float colourBrightnessMultiplier = Mathf.Lerp(currentHourBrightness, nextHourBrightness, decimalHour - hour);
			sr.color = new Color(sr.color.r * colourBrightnessMultiplier, sr.color.g * colourBrightnessMultiplier, sr.color.b * colourBrightnessMultiplier, 1f);

			if (plant != null) {
				plant.obj.GetComponent<SpriteRenderer>().color = sr.color;
			}
			foreach (ObjectInstance instance in GetAllObjectInstances()) {
				instance.SetColour(sr.color);
			}
			brightness = colourBrightnessMultiplier;
		}

		public void SetBrightness(float newBrightness, int hour) {
			brightness = newBrightness;
			SetColour(sr.color, hour);
		}

		public void AddLightSourceBrightness(LightSource lightSource, float brightness) {
			lightSourceBrightnesses.Add(lightSource, brightness);
			lightSourceBrightness = lightSourceBrightnesses.Max(kvp => kvp.Value);
			primaryLightSource = lightSourceBrightnesses.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		public void RemoveLightSourceBrightness(LightSource lightSource) {
			lightSourceBrightnesses.Remove(lightSource);
			if (lightSourceBrightnesses.Count > 0) {
				lightSourceBrightness = lightSourceBrightnesses.Max(kvp => kvp.Value);
				primaryLightSource = lightSourceBrightnesses.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
			} else {
				lightSourceBrightness = 0;
				primaryLightSource = null;
			}
		}

		public void SetPrecipitation(float precipitation) {
			this.precipitation = precipitation;
		}

		public float GetPrecipitation() {
			return precipitation;
		}



		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);

			if (plant != null) {
				plant.SetVisible(visible);
			}

			foreach (ObjectInstance objectInstance in GetAllObjectInstances()) {
				objectInstance.SetVisible(visible);
			}
		}
	}
}
