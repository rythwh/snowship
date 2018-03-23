using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour {

	private UIManager uiM;
	private CameraManager cameraM;
	private ResourceManager resourceM;
	private ColonistManager colonistM;

	void Awake() {
		uiM = GetComponent<UIManager>();
		cameraM = GetComponent<CameraManager>();
		resourceM = GetComponent<ResourceManager>();
		colonistM = GetComponent<ColonistManager>();
	}

	void Start() {
		PreInitialize();
	}

	public enum TileTypes {
		Dirt, DirtWater, Mud, DirtGrass, DirtThinGrass, DirtDryGrass, DirtHole, Grass, GrassWater, ThickGrass, ColdGrass, ColdGrassWater, DryGrass, DryGrassWater, Sand, SandWater,
		SandHole, Snow, SnowIce, SnowStone, SnowHole, Stone, StoneIce, StoneWater, StoneThinGrass, StoneSand, StoneSnow, StoneHole, Granite, Limestone, Marble, Sandstone, Slate, Clay, ClayWater
	};

	List<TileTypes> WaterEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.SnowIce, TileTypes.StoneIce, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater, TileTypes.ClayWater
	};
	public List<TileTypes> GetWaterEquivalentTileTypes() {
		return WaterEquivalentTileTypes;
	}

	List<TileTypes> LiquidWaterEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater, TileTypes.ClayWater
	};
	public List<TileTypes> GetLiquidWaterEquivalentTileTypes() {
		return LiquidWaterEquivalentTileTypes;
	}

	List<TileTypes> StoneEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.Stone, TileTypes.Granite, TileTypes.Limestone, TileTypes.Marble, TileTypes.Sandstone, TileTypes.Slate
	};
	public List<TileTypes> GetStoneEquivalentTileTypes() {
		return StoneEquivalentTileTypes;
	}

	List<TileTypes> PlantableTileTypes = new List<TileTypes>() {
		TileTypes.Dirt, TileTypes.Mud, TileTypes.DirtGrass, TileTypes.DirtThinGrass, TileTypes.DirtDryGrass, TileTypes.Grass, TileTypes.ThickGrass,
		TileTypes.DryGrass, TileTypes.ColdGrass, TileTypes.Sand, TileTypes.Snow, TileTypes.SnowStone, TileTypes.StoneThinGrass, TileTypes.StoneSand, TileTypes.StoneSnow
	};
	public List<TileTypes> GetPlantableTileTypes() {
		return PlantableTileTypes;
	}

	List<TileTypes> BitmaskingTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.SnowIce, TileTypes.StoneIce, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater, TileTypes.Stone,
		TileTypes.DirtHole, TileTypes.StoneHole, TileTypes.SnowHole, TileTypes.SandHole, TileTypes.ClayWater
	};
	public List<TileTypes> GetBitmaskingTileTypes() {
		return BitmaskingTileTypes;
	}

	List<TileTypes> HoleTileTypes = new List<TileTypes>() {
		TileTypes.DirtHole, TileTypes.StoneHole, TileTypes.SandHole, TileTypes.SnowHole
	};
	public List<TileTypes> GetHoleTileTypes() {
		return HoleTileTypes;
	}

	List<TileTypes> ResourceTileTypes = new List<TileTypes>() {
		TileTypes.Clay, TileTypes.ClayWater
	};
	public List<TileTypes> GetResourceTileTypes() {
		return ResourceTileTypes;
	}

	Dictionary<TileTypes, TileTypes> GroundToWaterResourceMap = new Dictionary<TileTypes, TileTypes>() {
		{ TileTypes.Clay, TileTypes.ClayWater }
	};
	public Dictionary<TileTypes, TileTypes> GetGroundToWaterResourceMap() {
		return GroundToWaterResourceMap;
	}

	Dictionary<TileTypes, TileTypes> WaterToGroundResourceMap = new Dictionary<TileTypes, TileTypes>() {
		{ TileTypes.ClayWater, TileTypes.Clay }
	};
	public Dictionary<TileTypes, TileTypes> GetWaterToGroundResourceMap() {
		return WaterToGroundResourceMap;
	}

	public List<TileType> tileTypes = new List<TileType>();

	public class TileType {
		public TileTypes type;
		public string name;

		public float walkSpeed;

		public bool walkable;
		public bool buildable;

		public List<Sprite> baseSprites = new List<Sprite>();
		public List<Sprite> bitmaskSprites = new List<Sprite>();
		public List<Sprite> riverSprites = new List<Sprite>();

		public TileType(List<string> tileTypeData, TileManager tm) {
			type = (TileTypes)System.Enum.Parse(typeof(TileTypes), tileTypeData[0]);
			name = type.ToString();

			walkSpeed = float.Parse(tileTypeData[1]);

			walkable = bool.Parse(tileTypeData[2]);
			buildable = bool.Parse(tileTypeData[3]);

			baseSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + name + "/" + name + "-base").ToList();
			bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + name + "/" + name + "-bitmask").ToList();

			if (tm.LiquidWaterEquivalentTileTypes.Contains(type)) {
				riverSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + name + "/" + name + "-river").ToList();
			}
		}
	}

	public void CreateTileTypes() {
		List<string> stringTileTypes = Resources.Load<TextAsset>(@"Data/tiletypes").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('`').ToList();
		foreach (string stringTileType in stringTileTypes) {
			List<string> stringTileTypeData = stringTileType.Split('/').ToList();
			tileTypes.Add(new TileType(stringTileTypeData, this));
		}
		foreach (TileType tileType in tileTypes) {
			tileType.name = uiM.SplitByCapitals(tileType.name);
		}
	}

	public TileType GetTileTypeByEnum(TileTypes find) {
		return tileTypes.Find(tileType => tileType.type == find);
	}

	public Dictionary<TileTypes, System.Func<Tile, bool>> resourceVeinValidTileFunctions = new Dictionary<TileTypes, System.Func<Tile, bool>>();

	public void InitializeResourceVeinValidTileFunctions() {
		resourceVeinValidTileFunctions.Add(TileTypes.Clay, delegate (Tile tile) {
			if (((WaterEquivalentTileTypes.Contains(tile.tileType.type) && tile.horizontalSurroundingTiles.Find(t => t != null && !WaterEquivalentTileTypes.Contains(t.tileType.type)) != null) || (!WaterEquivalentTileTypes.Contains(tile.tileType.type))) && (!StoneEquivalentTileTypes.Contains(tile.tileType.type))) {
				if (tile.temperature >= -30) {
					return true;
				}
			}
			return false;
		});
	}

	public enum BiomeTypes {
		None,
		PolarDesert, IceCap, Tundra, WetTundra, PolarWetlands, CoolDesert, TemperateDesert, Steppe, BorealForest, TemperateWoodlands, TemperateForest,
		TemperateWetForest, TemperateWetlands, ExtremeDesert, Desert, SubtropicalScrub, TropicalScrub, SubtropicalWoodlands, TropicalWoodlands,
		Mediterranean, SubtropicalDryForest, TropicalDryForest, SubtropicalForest, SubtropicalWetForest, SubtropicalWetlands, TropicalWetForest, TropicalWetlands
	};

	public List<Biome> biomes = new List<Biome>();

	public class Biome {
		public BiomeTypes type;
		public string name;

		public Dictionary<ResourceManager.PlantGroupsEnum, float> vegetationChances = new Dictionary<ResourceManager.PlantGroupsEnum, float>();

		public Color colour;

		public TileType tileType;
		public TileType waterType;
		public TileType holeType;

		public ResourceManager.Resource groundResource;

		public Biome(List<string> biomeData, TileManager tileM, ResourceManager resourceM, UIManager uiM) {
			type = (BiomeTypes)System.Enum.Parse(typeof(BiomeTypes), biomeData[0]);
			name = type.ToString();

			List<string> tileTypeData = biomeData[1].Split(',').ToList();
			tileType = tileM.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes), tileTypeData[0]));
			waterType = tileM.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes), tileTypeData[1]));
			holeType = tileM.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes), tileTypeData[2]));

			groundResource = resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum), biomeData[2]));

			if (float.Parse(biomeData[3].Split(',')[0]) != 0) {
				int vegetationIndex = 0;
				foreach (string vegetationChance in biomeData[3].Split(',').ToList()) {
					vegetationChances.Add((ResourceManager.PlantGroupsEnum)System.Enum.Parse(typeof(ResourceManager.PlantGroupsEnum), biomeData[4].Split(',').ToList()[vegetationIndex]), float.Parse(vegetationChance));
					vegetationIndex += 1;
				}
			}

			colour = uiM.HexToColor(biomeData[5]);
		}
	}

	public void CreateBiomes() {
		List<string> stringBiomeTypes = Resources.Load<TextAsset>(@"Data/biomes").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('`').ToList();
		foreach (string stringBiomeType in stringBiomeTypes) {
			List<string> stringBiomeData = stringBiomeType.Split('/').ToList();
			biomes.Add(new Biome(stringBiomeData, this, resourceM, uiM));
		}
		foreach (Biome biome in biomes) {
			biome.name = uiM.SplitByCapitals(biome.name);
		}
	}

	public class PrecipitationRange {

		public float min = 0;
		public float max = 0;

		public List<TemperatureRange> temperatureRanges = new List<TemperatureRange>();

		public PrecipitationRange(string dataString, TileManager tileM) {
			List<string> precipitationRangeData = dataString.Split(':').ToList();

			min = float.Parse(precipitationRangeData[0].Split(',')[0]);
			max = float.Parse(precipitationRangeData[0].Split(',')[1]);

			if (Mathf.RoundToInt(min) == -1) {
				min = float.MinValue;
			}
			if (Mathf.RoundToInt(max) == 2) {
				max = float.MaxValue;
			}

			foreach (string temperatureRangeString in precipitationRangeData[1].Split('`')) {
				temperatureRanges.Add(new TemperatureRange(temperatureRangeString, this, tileM));
			}
		}

		public class TemperatureRange {

			public PrecipitationRange precipitationRange;

			public int min = 0;
			public int max = 0;

			public Biome biome;

			public TemperatureRange(string dataString, PrecipitationRange precipitationRange, TileManager tileM) {

				this.precipitationRange = precipitationRange;

				List<string> temperatureRangeData = dataString.Split('/').ToList();

				min = int.Parse(temperatureRangeData[0].Split(',')[0]);
				max = int.Parse(temperatureRangeData[0].Split(',')[1]);

				if (min == -1000) {
					min = int.MinValue;
				}
				if (max == 1000) {
					max = int.MaxValue;
				}

				biome = tileM.biomes.Find(b => b.type == (BiomeTypes)System.Enum.Parse(typeof(BiomeTypes), temperatureRangeData[1]));
			}
		}
	}

	public List<PrecipitationRange> biomeRanges = new List<PrecipitationRange>();

	public void CreateBiomeRanges() {
		List<string> biomeRangeStrings = Resources.Load<TextAsset>(@"Data/biomeRanges").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('~').ToList();
		foreach (string biomeRangeString in biomeRangeStrings) {
			biomeRanges.Add(new PrecipitationRange(biomeRangeString, this));
		}
	}

	public enum TileResourceTypes { Copper, Silver, Gold, Iron, Steel, Diamond };

	Dictionary<int, List<List<int>>> nonWalkableSurroundingTilesComparatorMap = new Dictionary<int, List<List<int>>>() {
		{0, new List<List<int>>() { new List<int>() { 4,1,5,2 },new List<int>() { 7,3,6,2 } } },
		{1, new List<List<int>>() { new List<int>() { 4,0,7,3},new List<int>() { 5,2,6,3 } } },
		{2, new List<List<int>>() { new List<int>() { 5,1,4,0 },new List<int>() { 6,3,7,0 } } },
		{3, new List<List<int>>() { new List<int>() { 6,2,5,1 },new List<int>() { 7,0,4,1 } } }
	};

	public class Tile {

		private TileManager tileM;
		private TimeManager timeM;

		public SpriteRenderer sr;

		public Map map;

		public GameObject obj;
		public Vector2 position;

		public List<Tile> horizontalSurroundingTiles = new List<Tile>();
		public List<Tile> diagonalSurroundingTiles = new List<Tile>();
		public List<Tile> surroundingTiles = new List<Tile>();

		public float height;

		public TileType tileType;

		public Map.Region region;
		public Map.Region drainageBasin;
		public Map.RegionBlock regionBlock;
		public Map.RegionBlock squareRegionBlock;

		public Biome biome;
		public ResourceManager.Plant plant;
		public ResourceManager.Farm farm;

		private float precipitation = 0;
		public float temperature = 0;

		public bool walkable = false;
		public float walkSpeed = 0;

		public bool roof = false;

		public float brightness = 0;
		public Dictionary<int, float> brightnessAtHour = new Dictionary<int, float>();
		public Dictionary<int, Dictionary<Tile, float>> shadowsFrom = new Dictionary<int, Dictionary<Tile, float>>(); // Tiles that affect the shadow on this tile
		public Dictionary<int, List<Tile>> shadowsTo = new Dictionary<int, List<Tile>>(); // Tiles that have shadows due to this tile
		public Dictionary<int, List<Tile>> blockingShadowsFrom = new Dictionary<int, List<Tile>>(); // Tiles that have shadows that were cut short because this tile was in the way

		public Dictionary<ResourceManager.LightSource, float> lightSourceBrightnesses = new Dictionary<ResourceManager.LightSource, float>();
		public ResourceManager.LightSource primaryLightSource;
		public float lightSourceBrightness;

		public Dictionary<int, ResourceManager.TileObjectInstance> objectInstances = new Dictionary<int, ResourceManager.TileObjectInstance>();

		public bool dugPreviously;

		public Tile(Map map, Vector2 position, float height, TileManager tileM, TimeManager timeM, bool normalTile) {

			this.tileM = tileM;
			this.timeM = timeM;

			this.map = map;

			this.position = position;

			if (normalTile) {
				obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"), new Vector2(position.x + 0.5f, position.y + 0.5f), Quaternion.identity);
				obj.transform.SetParent(GameObject.Find("TileParent").transform, true);
				obj.name = "Tile: " + position;

				sr = obj.GetComponent<SpriteRenderer>();

				SetTileHeight(height);

				SetBrightness(1f, 12);
			}
		}

		public void SetTileHeight(float height) {
			this.height = height;
			SetTileTypeBasedOnHeight();
		}

		public void SetTileType(TileType tileType, bool bitmask, bool resetRegion, bool removeFromOldRegion, bool setBiomeTileType) {
			TileType oldTileType = this.tileType;
			this.tileType = tileType;
			if (setBiomeTileType && biome != null) {
				SetBiome(biome);
			}
			walkable = tileType.walkable;
			if (bitmask) {
				map.Bitmasking(new List<Tile>() { this }.Concat(surroundingTiles).ToList());
			}
			if (plant != null && !tileM.PlantableTileTypes.Contains(tileType.type)) {
				map.smallPlants.Remove(plant);
				Destroy(plant.obj);
				plant = null;
			}
			if (resetRegion) {
				ResetRegion(oldTileType, removeFromOldRegion);
			}
			SetWalkSpeed();
		}

		public void ResetRegion(TileType oldTileType, bool removeFromOldRegion) {
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
							List<List<int>> orderedIndexesToCheckList = tileM.nonWalkableSurroundingTilesComparatorMap[tileIndex];
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
						print("Independent tiles");
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
									print("Separate tiles part of the same group");
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
							Map.Region groupRegion = new Map.Region(nonWalkableTileGroup[0].tileType, map.currentRegionID);
							map.currentRegionID += 1;
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
						ChangeRegion(new Map.Region(tileType, map.currentRegionID), false, false);
						map.currentRegionID += 1;
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
		}

		public void SetTileTypeBasedOnHeight() {
			if (height < map.mapData.terrainTypeHeights[TileTypes.GrassWater]) {
				SetTileType(tileM.GetTileTypeByEnum(TileTypes.GrassWater), false, false, false, false);
			} else if (height > map.mapData.terrainTypeHeights[TileTypes.Stone]) {
				SetTileType(tileM.GetTileTypeByEnum(TileTypes.Stone), false, false, false, false);
			} else {
				SetTileType(tileM.GetTileTypeByEnum(TileTypes.Grass), false, false, false, false);
			}
		}

		public void ChangeRegion(Map.Region newRegion, bool changeTileTypeToRegionType, bool bitmask) {
			region = newRegion;
			region.tiles.Add(this);
			if (!map.regions.Contains(region)) {
				map.regions.Add(region);
			}
			if (changeTileTypeToRegionType) {
				SetTileType(region.tileType, bitmask, false, false, true);
			}
		}

		public void SetBiome(Biome biome) {
			this.biome = biome;
			if (!tileM.StoneEquivalentTileTypes.Contains(tileType.type)) {
				if (!tileM.WaterEquivalentTileTypes.Contains(tileType.type)) {
					SetTileType(biome.tileType, false, false, false, false);
				} else {
					SetTileType(biome.waterType, false, false, false, false);
				}
			}
			if (tileM.PlantableTileTypes.Contains(tileType.type)) {
				SetPlant(false, null);
			}
		}

		public void SetPlant(bool removePlant, ResourceManager.Plant specificPlant) {
			if (plant != null) {
				map.smallPlants.Remove(plant);
				Destroy(plant.obj);
				plant = null;
			}
			if (!removePlant) {
				if (specificPlant == null) {
					ResourceManager.PlantGroup biomePlantGroup = tileM.resourceM.GetPlantGroupByBiome(biome, false);
					if (biomePlantGroup != null) {
						plant = new ResourceManager.Plant(biomePlantGroup, this, true, false, map.smallPlants, true, null, tileM.resourceM);
					}
				} else {
					plant = specificPlant;
				}
			}
			SetWalkSpeed();
		}

		public void RemoveTileObjectAtLayer(int layer) {
			if (objectInstances.ContainsKey(layer)) {
				if (objectInstances[layer] != null) {
					Destroy(objectInstances[layer].obj);
					objectInstances[layer] = null;
				}
			}
			PostChangeTileObject();
		}

		public void SetTileObject(ResourceManager.TileObjectPrefab tileObjectPrefab, int rotationIndex) {
			bool farm = tileObjectPrefab.tileObjectPrefabSubGroup.type == ResourceManager.TileObjectPrefabSubGroupsEnum.PlantFarm;
			if (objectInstances.ContainsKey(tileObjectPrefab.layer)) {
				if (objectInstances[tileObjectPrefab.layer] != null) {
					if (tileObjectPrefab != null) {
						Debug.LogError("Trying to add object where one already exists at " + obj.transform.position);
					} else {
						objectInstances[tileObjectPrefab.layer] = null;
					}
				} else {
					if (farm) {
						ResourceManager.Farm newFarm = new ResourceManager.Farm(tileObjectPrefab, this);
						objectInstances[tileObjectPrefab.layer] = newFarm;
						SetFarm(newFarm);
					} else {
						objectInstances[tileObjectPrefab.layer] = new ResourceManager.TileObjectInstance(tileObjectPrefab, this, rotationIndex);
					}
				}
			} else {
				if (farm) {
					ResourceManager.Farm newFarm = new ResourceManager.Farm(tileObjectPrefab, this);
					objectInstances.Add(tileObjectPrefab.layer, newFarm);
					SetFarm(newFarm);
				} else {
					objectInstances.Add(tileObjectPrefab.layer, new ResourceManager.TileObjectInstance(tileObjectPrefab, this, rotationIndex));
				}
			}
			if (farm) {

			}
			PostChangeTileObject();
			tileM.resourceM.AddTileObjectInstance(objectInstances[tileObjectPrefab.layer]);
		}

		public void PostChangeTileObject() {
			walkable = tileType.walkable;
			foreach (KeyValuePair<int, ResourceManager.TileObjectInstance> kvp in objectInstances) {
				if (kvp.Value != null && !kvp.Value.prefab.walkable) {
					walkable = false;
					map.RecalculateRegionsAtTile(this);

					map.DetermineShadowTiles(new List<Tile>() { this }, true);

					break;
				}
			}
			SetWalkSpeed();
		}

		public ResourceManager.TileObjectInstance GetObjectInstanceAtLayer(int layer) {
			if (objectInstances.ContainsKey(layer)) {
				return objectInstances[layer];
			}
			return null;
		}

		public List<ResourceManager.TileObjectInstance> GetAllObjectInstances() {
			List<ResourceManager.TileObjectInstance> allObjectInstances = new List<ResourceManager.TileObjectInstance>();
			foreach (KeyValuePair<int, ResourceManager.TileObjectInstance> kvp in objectInstances) {
				if (kvp.Value != null) {
					allObjectInstances.Add(kvp.Value);
				}
			}
			return allObjectInstances;
		}

		public void SetWalkSpeed() {
			walkSpeed = tileType.walkSpeed;
			if (plant != null && walkSpeed > 0.6f) {
				walkSpeed = 0.6f;
			}
			float minObjectWalkSpeed = float.MaxValue; // Arbitrary value
			foreach (KeyValuePair<int, ResourceManager.TileObjectInstance> kvp in objectInstances) {
				if (kvp.Value != null && kvp.Value.prefab.walkSpeed <= minObjectWalkSpeed) {
					minObjectWalkSpeed = kvp.Value.prefab.walkSpeed;
				}
			}
			if (minObjectWalkSpeed < walkSpeed) {
				walkSpeed = minObjectWalkSpeed;
			}
		}

		public void SetFarm(ResourceManager.Farm farm) {
			if (farm == null) {
				tileM.resourceM.farms.Remove(this.farm);
				this.farm = null;
			} else {
				this.farm = farm;
				tileM.resourceM.farms.Add(farm);
			}
		}

		public void SetColour(Color newColour, int hour, bool printInfo = false) {
			float currentHourBrightness = Mathf.Max((brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f), lightSourceBrightness);
			int nextHour = (hour == 23 ? 0 : hour + 1);
			float nextHourBrightness = Mathf.Max((brightnessAtHour.ContainsKey(nextHour) ? brightnessAtHour[nextHour] : 1f), lightSourceBrightness);

			if (primaryLightSource != null) {
				sr.color = Color.Lerp(newColour, primaryLightSource.parentObject.prefab.lightColour + (newColour * (brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f) * 0.8f), lightSourceBrightness);
			} else {
				sr.color = newColour;
			}
			float colourBrightnessMultiplier = Mathf.Lerp(currentHourBrightness, nextHourBrightness, (timeM.GetTileBrightnessTime() - hour));
			sr.color = new Color(sr.color.r * colourBrightnessMultiplier, sr.color.g * colourBrightnessMultiplier, sr.color.b * colourBrightnessMultiplier, 1f);

			if (plant != null) {
				plant.obj.GetComponent<SpriteRenderer>().color = sr.color;
			}
			foreach (ResourceManager.TileObjectInstance instance in GetAllObjectInstances()) {
				instance.SetColour(sr.color);
			}
			brightness = colourBrightnessMultiplier;
		}

		public void SetBrightness(float newBrightness, int hour) {
			brightness = newBrightness;
			SetColour(sr.color, hour);
		}

		public void AddLightSourceBrightness(ResourceManager.LightSource lightSource, float brightness) {
			lightSourceBrightnesses.Add(lightSource, brightness);
			lightSourceBrightness = lightSourceBrightnesses.Max(kvp => kvp.Value);
			primaryLightSource = lightSourceBrightnesses.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		public void RemoveLightSourceBrightness(ResourceManager.LightSource lightSource) {
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
	}

	public bool generating;
	public bool generated;

	void Update() {
		if (generated) {
			map.DetermineVisibleRegionBlocks();
			map.GrowPlants();
		}
	}

	public class MapData {
		public int mapSeed;
		public int mapSize;
		public bool actualMap;

		public float equatorOffset;
		public bool planetTemperature;
		public int temperatureRange;
		public float planetDistance;
		public float temperatureOffset;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileTypes, float> terrainTypeHeights;
		public List<int> surroundingPlanetTileHeightDirections;
		public Vector2 planetTilePosition;

		public bool preventEdgeTouching;

		public int primaryWindDirection = -1;

		public string mapRegenerationCode = string.Empty;

		public MapData(
			MapData planetMapData,
			int mapSeed,
			int mapSize,
			bool actualMap,
			float equatorOffset,
			bool planetTemperature,
			int temperatureRange,
			float planetDistance,
			float temperatureOffset,
			float averageTemperature,
			float averagePrecipitation,
			Dictionary<TileTypes, float> terrainTypeHeights,
			List<int> surroundingPlanetTileHeightDirections,
			bool preventEdgeTouching,
			int primaryWindDirection,
			Vector2 planetTilePosition
			) {

			if (mapSeed < 0) {
				mapSeed = Random.Range(0, int.MaxValue);
			}
			Random.InitState(mapSeed);
			this.mapSeed = mapSeed;

			this.mapSize = mapSize;
			this.actualMap = actualMap;

			this.equatorOffset = equatorOffset;
			this.planetTemperature = planetTemperature;
			this.temperatureRange = temperatureRange;
			this.planetDistance = planetDistance;
			this.temperatureOffset = temperatureOffset;
			this.averageTemperature = averageTemperature;
			this.averagePrecipitation = averagePrecipitation;
			this.terrainTypeHeights = terrainTypeHeights;
			this.surroundingPlanetTileHeightDirections = surroundingPlanetTileHeightDirections;
			this.preventEdgeTouching = preventEdgeTouching;
			this.primaryWindDirection = primaryWindDirection;
			this.planetTilePosition = planetTilePosition;

			if (planetMapData != null) {
				mapRegenerationCode = planetMapData.mapSeed + "~" + planetMapData.mapSize + "~" + planetMapData.temperatureRange + "~" + planetMapData.planetDistance + "~" + planetMapData.primaryWindDirection + "~" + planetTilePosition.x + "~" + planetTilePosition.y + "~" + mapSize + "~" + mapSeed;
			}
		}
	}

	public void PreInitialize() {
		resourceM.CreateResources();
		resourceM.CreateTileObjectPrefabs();
		//resourceM.SetManufacturableResourcesData();
		resourceM.CreatePlantGroups();

		CreateTileTypes();
		CreateBiomes();
		CreateBiomeRanges();

		InitializeResourceVeinValidTileFunctions();
	}

	public void Initialize(MapData mapData) {
		generating = true;
		StartCoroutine(InitializeMap(mapData));
		StartCoroutine(PostInitializeMap());
	}

	public Map map;

	public IEnumerator InitializeMap(MapData mapData) {
		map = new Map(mapData, false);

		cameraM.SetCameraPosition(new Vector2(mapData.mapSize / 2f, mapData.mapSize / 2f));
		cameraM.SetCameraZoom(20);

		yield return null;
	}

	public IEnumerator PostInitializeMap() {
		while (!map.createdMap) {
			yield return null;
		}

		uiM.ToggleGameUI(true);

		colonistM.SpawnStartColonists(3);

		generated = true;
		generating = false;

		uiM.InitializeGameUI();
	}

	public class Map {

		private TileManager tileM;
		private TimeManager timeM;
		private PathManager pathM;
		private CameraManager cameraM;
		private ColonistManager colonistM;
		private ResourceManager resourceM;
		private UIManager uiM;

		public bool createdMap;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			timeM = GM.GetComponent<TimeManager>();
			pathM = GM.GetComponent<PathManager>();
			cameraM = GM.GetComponent<CameraManager>();
			colonistM = GM.GetComponent<ColonistManager>();
			resourceM = GM.GetComponent<ResourceManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public MapData mapData;
		public Map(MapData mapData, bool loadMap) {

			GetScriptReferences();

			this.mapData = mapData;

			createdMap = false;
			if (loadMap) {
				CreateTiles();
				SetMapEdgeTiles();
				SetSortedMapEdgeTiles();
				SmoothHeightWithSurroundingPlanetTiles();
				SetTileRegions(true);
				ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 5f), new List<TileTypes>() { TileTypes.GrassWater, TileTypes.Stone, TileTypes.Grass });
				ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 2f), new List<TileTypes>() { TileTypes.GrassWater });
				SetTileRegions(false);
				DetermineDrainageBasins();
				CreateRivers();
				CalculateTemperature();
				CalculatePrecipitation();
				SetBiomes();
				CreateRegionBlocks();
				SetRoofs();
				SetResourceVeins();
				DetermineShadowDirectionsAtHour();
				DetermineShadowTiles(tiles, false);
				SetTileBrightness(timeM.GetTileBrightnessTime());
				DetermineVisibleRegionBlocks();
				Bitmasking(tiles);
				createdMap = true;
			} else {
				uiM.StartCoroutine(CreateMap());
			}
		}

		public List<Tile> tiles = new List<Tile>();
		public List<List<Tile>> sortedTiles = new List<List<Tile>>();
		public List<Tile> edgeTiles = new List<Tile>();
		public Dictionary<int, List<Tile>> sortedEdgeTiles = new Dictionary<int, List<Tile>>();

		public IEnumerator CreateMap() {
			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Map", "Creating Tiles"); yield return null; }
			CreateTiles();
			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Map", "Rendering"); yield return null; }
			Bitmasking(tiles);

			if (mapData.preventEdgeTouching) {
				PreventEdgeTouching();
			}

			if (mapData.actualMap) {
				uiM.UpdateLoadingStateText("Map", "Determining Map Edges"); yield return null;
				SetMapEdgeTiles();
				uiM.UpdateLoadingStateText("Map", "Determining Sorted Map Edges"); yield return null;
				SetSortedMapEdgeTiles();
				uiM.UpdateLoadingStateText("Terrain", "Merging Terrain with Planet"); yield return null;
				SmoothHeightWithSurroundingPlanetTiles();
				uiM.UpdateLoadingStateText("Terrain", "Rendering"); yield return null;
				Bitmasking(tiles);
			}

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Terrain", "Determining Regions by Tile Type"); yield return null; }
			SetTileRegions(true);

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Terrain", ("Reducing Terrain Noise - " + Mathf.RoundToInt(mapData.mapSize / 5f))); yield return null; }
			ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 5f), new List<TileTypes>() { TileTypes.GrassWater, TileTypes.Stone, TileTypes.Grass });
			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Terrain", ("Reducing Terrain Noise - " + Mathf.RoundToInt(mapData.mapSize / 2f))); yield return null; }
			ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 2f), new List<TileTypes>() { TileTypes.GrassWater });
			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Terrain", "Determining Regions by Walkability"); yield return null; }
			SetTileRegions(false);
			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Terrain", "Rendering"); yield return null; }
			Bitmasking(tiles);

			if (mapData.actualMap) {
				uiM.UpdateLoadingStateText("Rivers", "Determining Drainage Basins"); yield return null;
				DetermineDrainageBasins();
				uiM.UpdateLoadingStateText("Rivers", "Determining River Path"); yield return null;
				CreateRivers();
				uiM.UpdateLoadingStateText("Rivers", "Rendering"); yield return null;
				Bitmasking(tiles);
			}

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Biomes", "Calculating Temperature"); yield return null; }
			CalculateTemperature();

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Biomes", "Calculating Precipitation"); yield return null; }
			CalculatePrecipitation();
			mapData.primaryWindDirection = primaryWindDirection;

			/*
			foreach (Tile tile in tiles) {
				tile.SetTileHeight(0.5f);
				tile.SetPrecipitation(tile.position.x / mapData.mapSize);
				tile.temperature = ((1 - (tile.position.y / mapData.mapSize)) * 140) - 50;
			}
			*/

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Biomes", "Setting Biomes"); yield return null; }
			SetBiomes();
			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Biomes", "Rendering"); yield return null; }
			Bitmasking(tiles);

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Region Blocks", "Determining Region Blocks"); yield return null; }
			CreateRegionBlocks();

			if (mapData.actualMap) {
				uiM.UpdateLoadingStateText("Roofs", "Determining Roofs"); yield return null;
				SetRoofs();

				uiM.UpdateLoadingStateText("Resources", "Creating Resource Veins"); yield return null;
				SetResourceVeins();
				uiM.UpdateLoadingStateText("Resources", "Rendering"); yield return null;
				Bitmasking(tiles);

				uiM.UpdateLoadingStateText("Lighting", "Determining Hourly Shadow Directions"); yield return null;
				DetermineShadowDirectionsAtHour();
				uiM.UpdateLoadingStateText("Lighting", "Calculating Shadows"); yield return null;
				DetermineShadowTiles(tiles, false);
				uiM.UpdateLoadingStateText("Lighting", "Applying Shadows"); yield return null;
				SetTileBrightness(timeM.GetTileBrightnessTime());
				uiM.UpdateLoadingStateText("Lighting", "Determining Visible Region Blocks"); yield return null;
				DetermineVisibleRegionBlocks();
			}

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Lighting", "Rendering"); yield return null; }
			Bitmasking(tiles);

			if (mapData.actualMap) { uiM.UpdateLoadingStateText("Finalizing", string.Empty); yield return null; }
			createdMap = true;
		}

		void CreateTiles() {
			for (int y = 0; y < mapData.mapSize; y++) {
				List<Tile> innerTiles = new List<Tile>();
				for (int x = 0; x < mapData.mapSize; x++) {

					float height = Random.Range(0f, 1f);

					Vector2 position = new Vector2(x, y);

					Tile tile = new Tile(this, position, height, tileM, timeM, true);

					innerTiles.Add(tile);
					tiles.Add(tile);
				}
				sortedTiles.Add(innerTiles);
			}

			SetSurroundingTiles();
			GenerateTerrain();
			AverageTileHeights();
		}

		void SetSurroundingTiles() {
			for (int y = 0; y < mapData.mapSize; y++) {
				for (int x = 0; x < mapData.mapSize; x++) {
					/* Horizontal */
					if (y + 1 < mapData.mapSize) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y + 1][x]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x + 1 < mapData.mapSize) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y][x + 1]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y - 1][x]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y][x - 1]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}

					/* Diagonal */
					if (x + 1 < mapData.mapSize && y + 1 < mapData.mapSize) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y + 1][x + 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0 && x + 1 < mapData.mapSize) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x + 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0 && y - 1 >= 0) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x - 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y + 1 < mapData.mapSize && x - 1 >= 0) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y + 1][x - 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}

					sortedTiles[y][x].surroundingTiles.AddRange(sortedTiles[y][x].horizontalSurroundingTiles);
					sortedTiles[y][x].surroundingTiles.AddRange(sortedTiles[y][x].diagonalSurroundingTiles);
				}
			}
		}

		void GenerateTerrain() {
			int lastSize = mapData.mapSize;
			for (int halves = 0; halves < Mathf.CeilToInt(Mathf.Log(mapData.mapSize, 2)); halves++) {
				int size = Mathf.CeilToInt(lastSize / 2f);
				for (int sectionY = 0; sectionY < mapData.mapSize; sectionY += size) {
					for (int sectionX = 0; sectionX < mapData.mapSize; sectionX += size) {
						float sectionAverage = 0;
						for (int y = sectionY; (y < sectionY + size && y < mapData.mapSize); y++) {
							for (int x = sectionX; (x < sectionX + size && x < mapData.mapSize); x++) {
								sectionAverage += sortedTiles[y][x].height;
							}
						}
						sectionAverage /= (size * size);
						float maxDeviationSize = -(((float)(size - mapData.mapSize)) / (4 * mapData.mapSize));
						sectionAverage += Random.Range(-maxDeviationSize, maxDeviationSize);
						for (int y = sectionY; (y < sectionY + size && y < mapData.mapSize); y++) {
							for (int x = sectionX; (x < sectionX + size && x < mapData.mapSize); x++) {
								sortedTiles[y][x].height = sectionAverage;
							}
						}
					}
				}
				lastSize = size;
			}

			foreach (Tile tile in tiles) {
				tile.SetTileHeight(tile.height);
			}
		}

		void AverageTileHeights() {
			for (int i = 0; i < 3; i++) { // 3
				List<float> averageTileHeights = new List<float>();

				foreach (Tile tile in tiles) {
					float averageHeight = tile.height;
					float numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						float multiplicationValue = 1f; // Reduces the weight of horizontal tiles by 50% to help prevent visible edges/corners on the map
						if (nTile != null) {
							if (i > 3) {
								numValidTiles += 1f;
							} else {
								numValidTiles += 0.5f;
								multiplicationValue = 0.5f;
							}
							averageHeight += nTile.height * multiplicationValue;
						}
					}
					averageHeight /= numValidTiles;
					averageTileHeights.Add(averageHeight);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].height = averageTileHeights[k];
					tiles[k].SetTileTypeBasedOnHeight();
				}
			}
		}

		void PreventEdgeTouching() {
			foreach (Tile tile in tiles) {
				float edgeDistance = (mapData.mapSize - (Vector2.Distance(tile.obj.transform.position, new Vector2(mapData.mapSize / 2f, mapData.mapSize / 2f)))) / mapData.mapSize;
				tile.SetTileHeight(tile.height * Mathf.Clamp(-Mathf.Pow(edgeDistance - 1.5f, 10) + 1, 0f, 1f));
			}
		}

		public List<Region> regions = new List<Region>();
		public int currentRegionID = 0;

		public class Region {
			public TileType tileType;
			public List<Tile> tiles = new List<Tile>();
			public int id;

			public List<Region> connectedRegions = new List<Region>();

			public Color colour;

			public Region(TileType regionTileType, int regionID) {
				tileType = regionTileType;
				id = regionID;

				colour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
			}

			public void ColourRegion() {
				Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
				foreach (Tile tile in this.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = colour;
				}
			}
		}

		void SmoothHeightWithSurroundingPlanetTiles() {
			for (int i = 0; i < mapData.surroundingPlanetTileHeightDirections.Count; i++) {
				if (mapData.surroundingPlanetTileHeightDirections[i] != 0) {
					foreach (Tile tile in tiles) {
						float closestEdgeDistance = sortedEdgeTiles[i].Min(edgeTile => Vector2.Distance(edgeTile.obj.transform.position, tile.obj.transform.position)) / (mapData.mapSize);
						float heightMultiplier = mapData.surroundingPlanetTileHeightDirections[i] * Mathf.Pow(closestEdgeDistance - 1f, 10f) + 1f;
						float newHeight = Mathf.Clamp(tile.height * heightMultiplier, 0f, 1f);
						tile.SetTileHeight(newHeight);
					}
				}
			}
		}

		public void SetTileRegions(bool splitByTileType) {
			regions.Clear();

			EstablishInitialRegions(splitByTileType);
			FindConnectedRegions(splitByTileType);
			MergeConnectedRegions(splitByTileType);

			RemoveEmptyRegions();
		}

		void EstablishInitialRegions(bool splitByTileType) {
			foreach (Tile tile in tiles) { // Go through all tiles
				List<Region> foundRegions = new List<Region>(); // For each tile, store a list of the regions around them
				for (int i = 0; i < tile.surroundingTiles.Count; i++) { // Go through the tiles around each tile
					Tile nTile = tile.surroundingTiles[i];
					if (nTile != null && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable)) && (i == 2 || i == 3 /*|| i == 5 || i == 6 */)) { // Uncomment indexes 5 and 6 to enable 8-connectivity connected-component labeling -- If the tiles have the same type
						if (nTile.region != null && !foundRegions.Contains(nTile.region)) { // If the tiles have a region and it hasn't already been looked at
							foundRegions.Add(nTile.region); // Add the surrounding tile's region to the regions found around the original tile
						}
					}
				}
				if (foundRegions.Count <= 0) { // If there weren't any tiles with the same region/tiletype found around them, make a new region for this tile
					tile.ChangeRegion(new Region(tile.tileType, currentRegionID), false, false);
					currentRegionID += 1;
				} else if (foundRegions.Count == 1) { // If there was a single region found around them, give them that region
					tile.ChangeRegion(foundRegions[0], false, false);
				} else if (foundRegions.Count > 1) { // If there was more than one around found around them, give them the region with the lowest ID
					tile.ChangeRegion(FindLowestRegion(foundRegions), false, false);
				}
			}
		}

		void FindConnectedRegions(bool splitByTileType) {
			foreach (Region region in regions) {
				foreach (Tile tile in region.tiles) {
					foreach (Tile nTile in tile.horizontalSurroundingTiles) {
						if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains(nTile.region) && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable))) {
							region.connectedRegions.Add(nTile.region);
						}
					}
				}
			}
		}

		void MergeConnectedRegions(bool splitByTileType) {
			while (regions.Where(region => region.connectedRegions.Count > 0).ToList().Count > 0) { // While there are regions that have connected regions
				foreach (Region region in regions) { // Go through each region
					if (region.connectedRegions.Count > 0) { // If this region has connected regions
						Region lowestRegion = FindLowestRegion(region.connectedRegions); // Find the lowest ID region from the connected regions
						if (region != lowestRegion) { // If this region is not the lowest region
							foreach (Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
								tile.ChangeRegion(lowestRegion, false, false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
						foreach (Region connectedRegion in region.connectedRegions) { // Set each tile's region in the connected regions that aren't the lowest region to the lowest region
							if (connectedRegion != lowestRegion) {
								foreach (Tile tile in connectedRegion.tiles) {
									tile.ChangeRegion(lowestRegion, false, false);
								}
								connectedRegion.tiles.Clear();
							}
						}
					}
					region.connectedRegions.Clear(); // Clear the connected regions from this region
				}
				FindConnectedRegions(splitByTileType); // Find the new connected regions
			}
		}

		public List<RegionBlock> regionBlocks = new List<RegionBlock>();

		public class RegionBlock : Region {
			public Vector2 averagePosition = new Vector2(0, 0);
			public List<RegionBlock> surroundingRegionBlocks = new List<RegionBlock>();
			public List<RegionBlock> horizontalSurroundingRegionBlocks = new List<RegionBlock>();
			public RegionBlock(TileType regionTileType, int regionID) : base(regionTileType, regionID) {

			}
		}

		private int regionBlockSize = 10;
		public List<RegionBlock> squareRegionBlocks = new List<RegionBlock>();
		public void CreateRegionBlocks() {

			regionBlocks.Clear();
			squareRegionBlocks.Clear();

			int size = regionBlockSize;
			int regionIndex = 0;
			for (int sectionY = 0; sectionY < mapData.mapSize; sectionY += size) {
				for (int sectionX = 0; sectionX < mapData.mapSize; sectionX += size) {
					RegionBlock regionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Grass), regionIndex);
					RegionBlock squareRegionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Grass), regionIndex);
					for (int y = sectionY; (y < sectionY + size && y < mapData.mapSize); y++) {
						for (int x = sectionX; (x < sectionX + size && x < mapData.mapSize); x++) {
							regionBlock.tiles.Add(sortedTiles[y][x]);
							squareRegionBlock.tiles.Add(sortedTiles[y][x]);
							sortedTiles[y][x].squareRegionBlock = squareRegionBlock;
						}
					}
					regionIndex += 1;
					regionBlocks.Add(regionBlock);
					squareRegionBlocks.Add(squareRegionBlock);
				}
			}
			foreach (RegionBlock squareRegionBlock in squareRegionBlocks) {
				foreach (Tile tile in squareRegionBlock.tiles) {
					foreach (Tile nTile in tile.surroundingTiles) {
						if (nTile != null && nTile.squareRegionBlock != tile.squareRegionBlock && nTile.squareRegionBlock != null && !squareRegionBlock.surroundingRegionBlocks.Contains(nTile.squareRegionBlock)) {
							squareRegionBlock.surroundingRegionBlocks.Add(nTile.squareRegionBlock);
						}
					}
					squareRegionBlock.averagePosition = new Vector2(squareRegionBlock.averagePosition.x + tile.obj.transform.position.x, squareRegionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				squareRegionBlock.averagePosition = new Vector2(squareRegionBlock.averagePosition.x / squareRegionBlock.tiles.Count, squareRegionBlock.averagePosition.y / squareRegionBlock.tiles.Count);
			}
			regionIndex += 1;
			List<RegionBlock> removeRegionBlocks = new List<RegionBlock>();
			List<RegionBlock> newRegionBlocks = new List<RegionBlock>();
			foreach (RegionBlock regionBlock in regionBlocks) {
				if (regionBlock.tiles.Find(tile => !tile.walkable) != null) {
					removeRegionBlocks.Add(regionBlock);
					List<Tile> unwalkableTiles = new List<Tile>();
					List<Tile> walkableTiles = new List<Tile>();
					foreach (Tile tile in regionBlock.tiles) {
						if (tile.walkable) {
							walkableTiles.Add(tile);
						} else {
							unwalkableTiles.Add(tile);
						}
					}
					regionBlock.tiles.Clear();
					foreach (Tile unwalkableTile in unwalkableTiles) {
						if (unwalkableTile.regionBlock == null) {
							RegionBlock unwalkableRegionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Stone), regionIndex);
							regionIndex += 1;
							Tile currentTile = unwalkableTile;
							List<Tile> frontier = new List<Tile>() { currentTile };
							List<Tile> checkedTiles = new List<Tile>() { currentTile };
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								unwalkableRegionBlock.tiles.Add(currentTile);
								currentTile.regionBlock = unwalkableRegionBlock;
								foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && !nTile.walkable && !checkedTiles.Contains(nTile) && unwalkableTiles.Contains(nTile) && nTile.regionBlock == null) {
										frontier.Add(nTile);
									}
									checkedTiles.Add(nTile);
								}
							}
							newRegionBlocks.Add(unwalkableRegionBlock);
						}
					}
					foreach (Tile walkableTile in walkableTiles) {
						if (walkableTile.regionBlock == null) {
							RegionBlock walkableRegionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Grass), regionIndex);
							regionIndex += 1;
							Tile currentTile = walkableTile;
							List<Tile> frontier = new List<Tile>() { currentTile };
							List<Tile> checkedTiles = new List<Tile>() { currentTile };
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								walkableRegionBlock.tiles.Add(currentTile);
								currentTile.regionBlock = walkableRegionBlock;
								foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && nTile.walkable && !checkedTiles.Contains(nTile) && walkableTiles.Contains(nTile) && nTile.regionBlock == null) {
										frontier.Add(nTile);
									}
									checkedTiles.Add(nTile);
								}
							}
							newRegionBlocks.Add(walkableRegionBlock);
						}
					}
				} else {
					foreach (Tile tile in regionBlock.tiles) {
						tile.regionBlock = regionBlock;
					}
				}
			}
			foreach (RegionBlock regionBlock in removeRegionBlocks) {
				regionBlocks.Remove(regionBlock);
			}
			removeRegionBlocks.Clear();
			regionBlocks.AddRange(newRegionBlocks);
			foreach (RegionBlock regionBlock in regionBlocks) {
				foreach (Tile tile in regionBlock.tiles) {
					foreach (Tile nTile in tile.horizontalSurroundingTiles) {
						if (nTile != null && nTile.regionBlock != tile.regionBlock && nTile.regionBlock != null && !regionBlock.horizontalSurroundingRegionBlocks.Contains(nTile.regionBlock)) {
							regionBlock.horizontalSurroundingRegionBlocks.Add(nTile.regionBlock);
						}
					}
					foreach (Tile nTile in tile.surroundingTiles) {
						if (nTile != null && nTile.regionBlock != tile.regionBlock && nTile.regionBlock != null && !regionBlock.surroundingRegionBlocks.Contains(nTile.regionBlock)) {
							regionBlock.surroundingRegionBlocks.Add(nTile.regionBlock);
						}
					}
					regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x + tile.obj.transform.position.x, regionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x / regionBlock.tiles.Count, regionBlock.averagePosition.y / regionBlock.tiles.Count);
			}
		}

		Region FindLowestRegion(List<Region> searchRegions) {
			Region lowestRegion = searchRegions[0];
			foreach (Region region in searchRegions) {
				if (region.id < lowestRegion.id) {
					lowestRegion = region;
				}
			}
			return lowestRegion;
		}

		void RemoveEmptyRegions() {
			for (int i = 0; i < regions.Count; i++) {
				if (regions[i].tiles.Count <= 0) {
					regions.RemoveAt(i);
					i -= 1;
				}
			}

			for (int i = 0; i < regions.Count; i++) {
				regions[i].id = i;
			}
		}

		public void RecalculateRegionsAtTile(Tile tile) {
			if (!tile.walkable) {
				List<Tile> orderedSurroundingTiles = new List<Tile>() {
				tile.surroundingTiles[0],tile.surroundingTiles[4],tile.surroundingTiles[1],tile.surroundingTiles[5],
				tile.surroundingTiles[2],tile.surroundingTiles[6],tile.surroundingTiles[3],tile.surroundingTiles[7]
			};
				List<List<Tile>> separateTileGroups = new List<List<Tile>>();
				int groupIndex = 0;
				for (int i = 0; i < orderedSurroundingTiles.Count; i++) {
					if (groupIndex == separateTileGroups.Count) {
						separateTileGroups.Add(new List<Tile>());
					}
					if (orderedSurroundingTiles[i] != null && orderedSurroundingTiles[i].walkable) {
						separateTileGroups[groupIndex].Add(orderedSurroundingTiles[i]);
						if (i == orderedSurroundingTiles.Count - 1 && groupIndex != 0) {
							if (orderedSurroundingTiles[i] != null && orderedSurroundingTiles[i].walkable && orderedSurroundingTiles[0] != null && orderedSurroundingTiles[0].walkable) {
								separateTileGroups[0].AddRange(separateTileGroups[groupIndex]);
								separateTileGroups.RemoveAt(groupIndex);
							}
						}
					} else {
						if (separateTileGroups[groupIndex].Count > 0) {
							groupIndex += 1;
						}
					}
				}
				List<Tile> horizontalGroups = new List<Tile>();
				foreach (List<Tile> tileGroup in separateTileGroups) {
					List<Tile> horizontalTilesInGroup = tileGroup.Where(groupTile => tile.horizontalSurroundingTiles.Contains(groupTile)).ToList();
					if (horizontalTilesInGroup.Count > 0) {
						horizontalGroups.Add(horizontalTilesInGroup[0]);
					}
				}
				if (horizontalGroups.Count > 1) {
					List<Tile> removeTiles = new List<Tile>();
					foreach (Tile startTile in horizontalGroups) {
						if (!removeTiles.Contains(startTile)) {
							foreach (Tile endTile in horizontalGroups) {
								if (!removeTiles.Contains(endTile) && startTile != endTile) {
									if (pathM.PathExists(startTile, endTile, true, mapData.mapSize, PathManager.WalkableSetting.Walkable, PathManager.DirectionSetting.Horizontal)) {
										removeTiles.Add(endTile);
									}
								}
							}
						}
					}
					foreach (Tile removeTile in removeTiles) {
						horizontalGroups.Remove(removeTile);
					}
					if (horizontalGroups.Count > 1) {
						SetTileRegions(false);
					}
				}
			}
		}

		void ReduceNoise(int removeRegionsBelowSize, List<TileTypes> typesToRemove) {
			foreach (Region region in regions) {
				if (typesToRemove.Contains(region.tileType.type)) {
					if (region.tiles.Count < removeRegionsBelowSize) {
						/* --- This code is essentially copied from FindConnectedRegions() */
						foreach (Tile tile in region.tiles) {
							foreach (Tile nTile in tile.horizontalSurroundingTiles) {
								if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains(nTile.region)) {
									region.connectedRegions.Add(nTile.region);
								}
							}
						}
						/* --- This code is essentially copied from MergeConnectedRegions() */
						if (region.connectedRegions.Count > 0) {
							Region lowestRegion = FindLowestRegion(region.connectedRegions);
							foreach (Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
								tile.ChangeRegion(lowestRegion, true, false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
					}
				}
			}
			RemoveEmptyRegions();
		}

		public List<River> rivers = new List<River>();

		public Dictionary<Region, Tile> drainageBasins = new Dictionary<Region, Tile>();
		public int drainageBasinID = 0;

		void DetermineDrainageBasins() {
			List<Tile> tilesByHeight = tiles.OrderBy(tile => tile.height).ToList();
			foreach (Tile tile in tilesByHeight) {
				if (!tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type) && tile.drainageBasin == null) {
					Region drainageBasin = new Region(null, drainageBasinID);
					drainageBasinID += 1;

					Tile currentTile = tile;

					List<Tile> checkedTiles = new List<Tile>();
					checkedTiles.Add(currentTile);
					List<Tile> frontier = new List<Tile>();
					frontier.Add(currentTile);

					while (frontier.Count > 0) {
						currentTile = frontier[0];
						frontier.RemoveAt(0);

						drainageBasin.tiles.Add(currentTile);
						currentTile.drainageBasin = drainageBasin;

						foreach (Tile nTile in currentTile.surroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && !tileM.StoneEquivalentTileTypes.Contains(nTile.tileType.type) && nTile.drainageBasin == null) {
								if (nTile.height >= currentTile.height) {
									frontier.Add(nTile);
									checkedTiles.Add(nTile);
								}
							}
						}
					}
					drainageBasins.Add(drainageBasin, tile);
				}
			}
		}

		public class River {
			public Tile startTile;
			public Tile endTile;
			public List<Tile> tiles;

			public River(Tile startTile, Tile endTile, List<Tile> tiles) {
				this.startTile = startTile;
				this.endTile = endTile;
				this.tiles = tiles;
			}
		}

		void CreateRivers() {
			Dictionary<Tile, Tile> riverStartTiles = new Dictionary<Tile, Tile>();
			foreach (KeyValuePair<Region, Tile> kvp in drainageBasins) {
				Region drainageBasin = kvp.Key;
				if (drainageBasin.tiles.Find(o => tileM.WaterEquivalentTileTypes.Contains(o.tileType.type)) != null && drainageBasin.tiles.Find(o => o.horizontalSurroundingTiles.Find(o2 => o2 != null && tileM.StoneEquivalentTileTypes.Contains(o2.tileType.type)) != null) != null) {
					foreach (Tile tile in drainageBasin.tiles) {
						if (tile.walkable && !tileM.WaterEquivalentTileTypes.Contains(tile.tileType.type) && tile.horizontalSurroundingTiles.Find(o => o != null && tileM.StoneEquivalentTileTypes.Contains(o.tileType.type)) != null) {
							riverStartTiles.Add(tile, kvp.Value);
						}
					}
				}
			}
			for (int i = 0; i < mapData.mapSize / 10f && i < riverStartTiles.Count; i++) {
				Tile riverStartTile = Enumerable.ToList(riverStartTiles.Keys)[Random.Range(0, riverStartTiles.Count)];
				Tile riverEndTile = riverStartTiles[riverStartTile];
				List<Tile> removeTiles = new List<Tile>();
				foreach (KeyValuePair<Tile, Tile> kvp in riverStartTiles) {
					if (Vector2.Distance(kvp.Key.obj.transform.position, riverStartTile.obj.transform.position) < 5f) {
						removeTiles.Add(kvp.Key);
					}
				}
				foreach (Tile removeTile in removeTiles) {
					riverStartTiles.Remove(removeTile);
				}
				removeTiles.Clear();

				List<Tile> riverTiles = RiverPathfinding(riverStartTile, riverEndTile);

				rivers.Add(new River(riverStartTile, riverEndTile, riverTiles));
			}
		}

		public List<Tile> RiverPathfinding(Tile riverStartTile, Tile riverEndTile) {
			PathManager.PathfindingTile currentTile = new PathManager.PathfindingTile(riverStartTile, null, 0);

			List<PathManager.PathfindingTile> checkedTiles = new List<PathManager.PathfindingTile>();
			checkedTiles.Add(currentTile);
			List<PathManager.PathfindingTile> frontier = new List<PathManager.PathfindingTile>();
			frontier.Add(currentTile);

			List<Tile> river = new List<Tile>();

			while (frontier.Count > 0) {
				currentTile = frontier[0];
				frontier.RemoveAt(0);

				if (tileM.WaterEquivalentTileTypes.Contains(currentTile.tile.tileType.type) || (currentTile.tile.horizontalSurroundingTiles.Find(tile => tile != null && tileM.WaterEquivalentTileTypes.Contains(tile.tileType.type) && RiversContainTile(tile).Key == null) != null)) {
					Tile foundOtherRiverAtTile = null;
					River foundOtherRiver = null;
					bool expandRiver = true; // false: SET TO FALSE TO ENABLE RIVER EXPANSION
					while (currentTile != null) {
						river.Add(currentTile.tile);
						currentTile.tile.SetTileType(tileM.GetTileTypeByEnum(TileTypes.GrassWater), false, false, false, true);
						if (!expandRiver) {
							KeyValuePair<Tile, River> kvp = RiversContainTile(currentTile.tile);
							if (kvp.Key != null) {
								foundOtherRiverAtTile = kvp.Key;
								foundOtherRiver = kvp.Value;
								expandRiver = true;
								print("Expanding river at " + foundOtherRiverAtTile.obj.transform.position);
							}
						}
						currentTile = currentTile.cameFrom;
					}
					if (foundOtherRiver != null && foundOtherRiver.tiles.Count > 1) {
						int riverTileIndex = 1;
						while (expandRiver) {
							Tile riverTile = foundOtherRiver.tiles[riverTileIndex];
							if (riverTile == foundOtherRiverAtTile) {
								break;
							}
							int maxExpandRadius = 1;
							List<Tile> expandFrontier = new List<Tile>();
							expandFrontier.Add(riverTile);
							List<Tile> checkedExpandTiles = new List<Tile>();
							checkedExpandTiles.Add(riverTile);
							while (expandFrontier.Count > 0) {
								Tile expandTile = expandFrontier[0];
								expandFrontier.RemoveAt(0);
								expandTile.SetTileType(tileM.GetTileTypeByEnum(TileTypes.GrassWater), false, false, false, true);
								foreach (Tile nTile in expandTile.surroundingTiles) {
									if (nTile != null && !checkedExpandTiles.Contains(nTile) && !tileM.StoneEquivalentTileTypes.Contains(nTile.tileType.type) && Vector2.Distance(nTile.obj.transform.position, riverTile.obj.transform.position) <= maxExpandRadius) {
										expandFrontier.Add(nTile);
										checkedExpandTiles.Add(nTile);
									}
								}
							}
							riverTileIndex += 1;
						}
					}
					break;
				}

				foreach (Tile nTile in currentTile.tile.horizontalSurroundingTiles) {
					if (nTile != null && checkedTiles.Find(checkedTile => checkedTile.tile == nTile) == null && !tileM.StoneEquivalentTileTypes.Contains(nTile.tileType.type)) {
						if (rivers.Find(otherRiver => otherRiver.tiles.Find(riverTile => nTile == riverTile) != null) != null) {
							frontier.Clear();
							frontier.Add(new PathManager.PathfindingTile(nTile, currentTile, 0));
							nTile.SetTileType(tileM.GetTileTypeByEnum(TileTypes.GrassWater), false, false, false, true);
							break;
						}
						float cost = Vector2.Distance(nTile.obj.transform.position, riverEndTile.obj.transform.position) + (nTile.height * (mapData.mapSize / 10f)) + Random.Range(0, 10);
						PathManager.PathfindingTile pTile = new PathManager.PathfindingTile(nTile, currentTile, cost);
						frontier.Add(pTile);
						checkedTiles.Add(pTile);
					}
				}
				frontier = frontier.OrderBy(frontierTile => frontierTile.cost).ToList();
			}
			return river;
		}

		public KeyValuePair<Tile, River> RiversContainTile(Tile tile) {
			foreach (River river in rivers) {
				foreach (Tile riverTile in river.tiles) {
					if (riverTile == tile) {
						return new KeyValuePair<Tile, River>(riverTile, river);
					}
				}
			}
			return new KeyValuePair<Tile, River>(null, null);
		}

		public float TemperatureFromMapLatitude(float yPos, float temperatureRange, float temperatureOffset, int mapSize) {
			return ((-2 * Mathf.Abs((yPos - (mapSize / 2f)) / ((mapSize / 100f) / (temperatureRange / 50f)))) + temperatureRange) + temperatureOffset;
		}

		void CalculateTemperature() {
			foreach (Tile tile in tiles) {
				if (mapData.planetTemperature) {
					tile.temperature = TemperatureFromMapLatitude(tile.position.y, mapData.temperatureRange, mapData.temperatureOffset, mapData.mapSize);
				} else {
					tile.temperature = mapData.averageTemperature;
				}
				tile.temperature += -(100f * Mathf.Pow(tile.height - 0.5f, 3));
			}

			AverageTileTemperatures();
		}

		void AverageTileTemperatures() {
			for (int i = 0; i < 3; i++) {
				List<float> averageTileTemperatures = new List<float>();

				foreach (Tile tile in tiles) {
					float averageTemperature = tile.temperature;
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						if (nTile != null) {
							numValidTiles += 1;
							averageTemperature += nTile.temperature;
						}
					}
					averageTemperature /= numValidTiles;
					averageTileTemperatures.Add(averageTemperature);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].temperature = averageTileTemperatures[k];
				}
			}
		}

		private Dictionary<int, int> oppositeDirectionTileMap = new Dictionary<int, int>() { { 0, 2 }, { 1, 3 }, { 2, 0 }, { 3, 1 }, { 4, 6 }, { 5, 7 }, { 6, 4 }, { 7, 5 } };
		private Dictionary<int, List<float>> windStrengthMap = new Dictionary<int, List<float>>() {
			{ 0,new List<float>(){ 1.0f,0.6f,0.1f,0.6f,0.8f,0.2f,0.2f,0.8f } },
			{ 1,new List<float>(){ 0.6f,1.0f,0.6f,0.1f,0.8f,0.8f,0.2f,0.2f } },
			{ 2,new List<float>(){ 0.1f,0.6f,1.0f,0.6f,0.2f,0.8f,0.8f,0.2f } },
			{ 3,new List<float>(){ 0.6f,0.1f,0.6f,1.0f,0.2f,0.2f,0.8f,0.8f } },
			{ 4,new List<float>(){ 0.8f,0.8f,0.2f,0.2f,1.0f,0.6f,0.1f,0.6f } },
			{ 5,new List<float>(){ 0.2f,0.8f,0.8f,0.2f,0.6f,1.0f,0.6f,0.1f } },
			{ 6,new List<float>(){ 0.2f,0.2f,0.8f,0.8f,0.1f,0.6f,1.0f,0.6f } },
			{ 7,new List<float>(){ 0.8f,0.2f,0.2f,0.8f,0.6f,0.1f,0.6f,1.0f } }
		};

		public int primaryWindDirection = -1;
		private void CalculatePrecipitation() {
			int windDirectionMin = 0;
			int windDirectionMax = 7;

			List<List<float>> directionPrecipitations = new List<List<float>>();
			for (int i = 0; i < windDirectionMin; i++) {
				directionPrecipitations.Add(new List<float>());
			}
			for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) { // 0 - up, 1 - right, 2 - down, 3 - left, 4 - up/right, 5 - down/right, 6 - down-left, 7 - up/left
				int windDirection = i;
				if (windDirection <= 3) { // Wind is going horizontally/vertically
					bool yStartAtTop = (windDirection == 2);
					bool xStartAtRight = (windDirection == 3);

					for (int y = (yStartAtTop ? mapData.mapSize - 1 : 0); (yStartAtTop ? y >= 0 : y < mapData.mapSize); y += (yStartAtTop ? -1 : 1)) {
						for (int x = (xStartAtRight ? mapData.mapSize - 1 : 0); (xStartAtRight ? x >= 0 : x < mapData.mapSize); x += (xStartAtRight ? -1 : 1)) {
							Tile tile = sortedTiles[y][x];
							Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
							SetTilePrecipitation(tile, previousTile, mapData.planetTemperature);
						}
					}
				} else { // Wind is going diagonally
					bool up = (windDirection == 4 || windDirection == 7);
					bool left = (windDirection == 6 || windDirection == 7);
					int mapSize2x = mapData.mapSize * 2;
					for (int k = (up ? 0 : mapSize2x); (up ? k < mapSize2x : k >= 0); k += (up ? 1 : -1)) {
						for (int x = (left ? k : 0); (left ? x >= 0 : x <= k); x += (left ? -1 : 1)) {
							int y = k - x;
							if (y < mapData.mapSize && x < mapData.mapSize) {
								Tile tile = sortedTiles[y][x];
								Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
								SetTilePrecipitation(tile, previousTile, mapData.planetTemperature);
							}
						}
					}
				}
				List<float> singleDirectionPrecipitations = new List<float>();
				foreach (Tile tile in tiles) {
					singleDirectionPrecipitations.Add(tile.GetPrecipitation());
					tile.SetPrecipitation(0);
				}
				directionPrecipitations.Add(singleDirectionPrecipitations);
			}

			if (mapData.primaryWindDirection == -1) {
				primaryWindDirection = Random.Range(windDirectionMin, (windDirectionMax + 1));
			} else {
				primaryWindDirection = mapData.primaryWindDirection;
			}

			float windStrengthMapSum = 0;
			for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) {
				windStrengthMapSum += windStrengthMap[primaryWindDirection][i];
			}

			for (int t = 0; t < tiles.Count; t++) {
				Tile tile = tiles[t];
				tile.SetPrecipitation(0);
				for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) {
					tile.SetPrecipitation(tile.GetPrecipitation() + (directionPrecipitations[i][t] * windStrengthMap[primaryWindDirection][i]));
				}
				tile.SetPrecipitation(tile.GetPrecipitation() / windStrengthMapSum);
			}

			AverageTilePrecipitations();

			foreach (Tile tile in tiles) {
				if (Mathf.RoundToInt(mapData.averagePrecipitation) != -1) {
					tile.SetPrecipitation((tile.GetPrecipitation() + mapData.averagePrecipitation) / 2f);
				}
				tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
			}
		}

		private void SetTilePrecipitation(Tile tile, Tile previousTile, bool planet) {
			if (planet) {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation(((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier) * (mapData.mapSize / 5f));
					} else if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.25f);
					} else {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.9f);
					}
				} else {
					if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation(1f);
					} else if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation(0f);
					} else {
						tile.SetPrecipitation(0.1f);
					}
				}
			} else {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
						float waterMultiplier = (mapData.mapSize / 5f);
						if (RiversContainTile(tile).Value != null) {
							waterMultiplier *= 5;
						}
						tile.SetPrecipitation(((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier) * waterMultiplier);
					} else if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.5f);
					} else {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.95f);
					}
				} else {
					if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation(1f);
					} else if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
						tile.SetPrecipitation(0f);
					} else {
						tile.SetPrecipitation(mapData.averagePrecipitation);
					}
				}
			}
			tile.SetPrecipitation(ChangePrecipitationByTemperature(tile.GetPrecipitation(), tile.temperature));
			tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
		}

		private float ChangePrecipitationByTemperature(float precipitation, float temperature) {
			return precipitation * (Mathf.Clamp(-Mathf.Pow((temperature - 30) / (90 - 30), 3) + 1, 0f, 1f)); // Less precipitation as the temperature gets higher
		}

		void AverageTilePrecipitations() {
			int numPasses = 2;
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTilePrecipitations = new List<float>();

				foreach (Tile tile in tiles) {
					float averagePrecipitation = tile.GetPrecipitation();
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						if (nTile != null) {
							numValidTiles += 1;
							averagePrecipitation += nTile.GetPrecipitation();
						}
					}
					averagePrecipitation /= numValidTiles;
					averageTilePrecipitations.Add(averagePrecipitation);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].SetPrecipitation(averageTilePrecipitations[k]);
				}
			}
		}

		void SetBiomes() {
			foreach (Tile tile in tiles) {
				bool next = false;
				foreach (PrecipitationRange precipitationRange in tileM.biomeRanges) {
					if (tile.GetPrecipitation() >= precipitationRange.min && tile.GetPrecipitation() < precipitationRange.max) {
						foreach (PrecipitationRange.TemperatureRange temperatureRange in precipitationRange.temperatureRanges) {
							if (tile.temperature >= temperatureRange.min && tile.temperature < temperatureRange.max) {
								tile.SetBiome(temperatureRange.biome);
								if (tile.plant != null && tile.plant.small) {
									tile.plant.growthProgress = Random.Range(0f, 5760f);
								}
								next = true;
								break;
							}
						}
					}
					if (next) {
						break;
					}
				}
			}
		}

		void SetMapEdgeTiles() {
			for (int i = 1; i < mapData.mapSize - 1; i++) {
				edgeTiles.Add(sortedTiles[0][i]);
				edgeTiles.Add(sortedTiles[mapData.mapSize - 1][i]);
				edgeTiles.Add(sortedTiles[i][0]);
				edgeTiles.Add(sortedTiles[i][mapData.mapSize - 1]);
			}
			edgeTiles.Add(sortedTiles[0][0]);
			edgeTiles.Add(sortedTiles[0][mapData.mapSize - 1]);
			edgeTiles.Add(sortedTiles[mapData.mapSize - 1][0]);
			edgeTiles.Add(sortedTiles[mapData.mapSize - 1][mapData.mapSize - 1]);
		}

		void SetSortedMapEdgeTiles() {
			int sideNum = -1;
			List<Tile> tilesOnThisEdge = null;
			for (int i = 0; i <= mapData.mapSize; i++) {
				i %= mapData.mapSize;
				if (i == 0) {
					sideNum += 1;
					sortedEdgeTiles.Add(sideNum, new List<Tile>());
					tilesOnThisEdge = sortedEdgeTiles[sideNum];
				}
				if (sideNum == 0) {
					tilesOnThisEdge.Add(sortedTiles[mapData.mapSize - 1][i]);
				} else if (sideNum == 1) {
					tilesOnThisEdge.Add(sortedTiles[i][mapData.mapSize - 1]);
				} else if (sideNum == 2) {
					tilesOnThisEdge.Add(sortedTiles[0][i]);
				} else if (sideNum == 3) {
					tilesOnThisEdge.Add(sortedTiles[i][0]);
				} else {
					break;
				}
			}
		}

		public void SetRoofs() {
			float roofHeightMultiplier = 1.25f;
			foreach (Tile tile in tiles) {
				if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type) && tile.height >= mapData.terrainTypeHeights[TileTypes.Stone] * roofHeightMultiplier) {
					tile.roof = true;
				} else {
					tile.roof = false;
				}
			}
		}

		public class ResourceVeinData {
			public TileTypes tileType;
			public int numVeins = 0;
			public int veinDistance = 0;
			public int veinSize = 0;
			public int veinSizeRange = 0;

			public ResourceVeinData(TileTypes tileType, int numVeins, int veinDistance, int veinSize, int veinSizeRange) {
				this.tileType = tileType;
				this.numVeins = numVeins;
				this.veinDistance = veinDistance;
				this.veinSize = veinSize;
				this.veinSizeRange = veinSizeRange;
			}
		}

		public void SetResourceVeins() {
			List<ResourceVeinData> stoneVeins = new List<ResourceVeinData>() {

			};
			List<Tile> stoneTiles = new List<Tile>();
			foreach (Tile tile in tiles) {
				if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
					stoneTiles.Add(tile);
				}
			}
			if (stoneTiles.Count > 0) {
				foreach (ResourceVeinData resourceVeinData in stoneVeins) {
					PlaceResourceVeins(resourceVeinData, stoneTiles);
				}
			}

			List<ResourceVeinData> coastVeins = new List<ResourceVeinData>() {
				new ResourceVeinData(TileTypes.Clay,Mathf.RoundToInt(mapData.mapSize / 10f),5,10,5)
			};
			List<Tile> coastTiles = new List<Tile>();
			foreach (Tile tile in tiles) {
				if (tileM.WaterEquivalentTileTypes.Contains(tile.tileType.type) && tile.surroundingTiles.Find(t => t != null && !tileM.WaterEquivalentTileTypes.Contains(t.tileType.type)) != null) {
					coastTiles.Add(tile);
				}
			}
			if (coastTiles.Count > 0) {
				foreach (ResourceVeinData resourceVeinData in coastVeins) {
					PlaceResourceVeins(resourceVeinData, coastTiles);
				}
			}
		}

		void PlaceResourceVeins(ResourceVeinData resourceVeinData, List<Tile> mediumTiles) {
			List<Tile> previousVeinStartTiles = new List<Tile>();
			for (int i = 0; i < resourceVeinData.numVeins; i++) {
				List<Tile> validVeinStartTiles = mediumTiles.Where(tile => !tileM.ResourceTileTypes.Contains(tile.tileType.type) && tileM.resourceVeinValidTileFunctions[resourceVeinData.tileType](tile)).ToList();
				foreach (Tile previousVeinStartTile in previousVeinStartTiles) {
					List<Tile> removeTiles = new List<Tile>();
					foreach (Tile validVeinStartTile in validVeinStartTiles) {
						if (Vector2.Distance(validVeinStartTile.obj.transform.position, previousVeinStartTile.obj.transform.position) < resourceVeinData.veinDistance) {
							removeTiles.Add(validVeinStartTile);
						}
					}
					foreach (Tile removeTile in removeTiles) {
						validVeinStartTiles.Remove(removeTile);
					}
				}
				if (validVeinStartTiles.Count > 0) {

					int veinSizeMax = resourceVeinData.veinSize + Random.Range(-resourceVeinData.veinSizeRange, resourceVeinData.veinSizeRange);

					Tile veinStartTile = validVeinStartTiles[Random.Range(0, validVeinStartTiles.Count)];
					previousVeinStartTiles.Add(veinStartTile);

					List<Tile> frontier = new List<Tile>() { veinStartTile };
					List<Tile> checkedTiles = new List<Tile>();
					Tile currentTile = veinStartTile;

					int veinSize = 0;

					while (frontier.Count > 0) {
						currentTile = frontier[Random.Range(0, frontier.Count)];
						frontier.RemoveAt(0);
						checkedTiles.Add(currentTile);

						if (tileM.WaterEquivalentTileTypes.Contains(currentTile.tileType.type)) {
							currentTile.SetTileType(tileM.GetTileTypeByEnum(tileM.GroundToWaterResourceMap[resourceVeinData.tileType]), true, false, false, false);
						} else {
							currentTile.SetTileType(tileM.GetTileTypeByEnum(resourceVeinData.tileType), true, false, false, false);
						}

						foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && !tileM.ResourceTileTypes.Contains(nTile.tileType.type)) {
								if (tileM.resourceVeinValidTileFunctions[resourceVeinData.tileType](nTile)) {
									frontier.Add(nTile);
								}
							}
						}

						veinSize += 1;

						if (veinSize >= veinSizeMax) {
							break;
						}
					}
				}
			}
		}

		Dictionary<int, int> bitmaskMap = new Dictionary<int, int>() {
		{ 19,16 },{ 23,17 },{ 27,18 },{ 31,19 },{ 38,20 },{ 39,21 },{ 46,22 },
		{ 47,23 },{ 55,24 },{ 63,25 },{ 76,26 },{ 77,27 },{ 78,28 },{ 79,29 },
		{ 95,30 },{ 110,31 },{ 111,32 },{ 127,33 },{ 137,34 },{ 139,35 },{ 141,36 },
		{ 143,37 },{ 155,38 },{ 159,39 },{ 175,40 },{ 191,41 },{ 205,42 },{ 207,43 },
		{ 223,44 },{ 239,45 },{ 255,46 }
	};
		Dictionary<int, List<int>> diagonalCheckMap = new Dictionary<int, List<int>>() {
		{4,new List<int>() {0,1 } },
		{5,new List<int>() {1,2 } },
		{6,new List<int>() {2,3 } },
		{7,new List<int>() {3,0 } }
	};

		int BitSum(List<TileTypes> compareTileTypes, List<Tile> tilesToSum, bool includeMapEdge) {
			int sum = 0;
			for (int i = 0; i < tilesToSum.Count; i++) {
				if (tilesToSum[i] != null) {
					if (compareTileTypes.Contains(tilesToSum[i].tileType.type)) {
						bool ignoreTile = false;
						if (compareTileTypes.Contains(tilesToSum[i].tileType.type) && diagonalCheckMap.ContainsKey(i)) {
							List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
							List<Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && compareTileTypes.Contains(tile.tileType.type)).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							sum += Mathf.RoundToInt(Mathf.Pow(2, i));
						}
					}
				} else if (includeMapEdge) {
					if (tilesToSum.Find(tile => tile != null && tilesToSum.IndexOf(tile) <= 3 && !compareTileTypes.Contains(tile.tileType.type)) == null) {
						sum += Mathf.RoundToInt(Mathf.Pow(2, i));
					} else {
						if (i <= 3) {
							sum += Mathf.RoundToInt(Mathf.Pow(2, i));
						} else {
							List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)) == null) {
								sum += Mathf.RoundToInt(Mathf.Pow(2, i));
							}
						}
					}
				}
			}
			return sum;
		}

		void BitmaskTile(Tile tile, bool includeDiagonalSurroundingTiles, bool customBitSumInputs, List<TileTypes> customCompareTileTypes, bool includeMapEdge) {
			int sum = 0;
			List<Tile> surroundingTilesToUse = (includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles);
			if (customBitSumInputs) {
				sum = BitSum(customCompareTileTypes, surroundingTilesToUse, includeMapEdge);
			} else {
				if (RiversContainTile(tile).Key != null) {
					sum = BitSum(tileM.WaterEquivalentTileTypes, surroundingTilesToUse, false);
				} else if (tileM.WaterEquivalentTileTypes.Contains(tile.tileType.type)) {
					sum = BitSum(tileM.WaterEquivalentTileTypes, surroundingTilesToUse, includeMapEdge);
				} else if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
					sum = BitSum(tileM.StoneEquivalentTileTypes, surroundingTilesToUse, includeMapEdge);
				} else if (tileM.HoleTileTypes.Contains(tile.tileType.type)) {
					sum = BitSum(tileM.HoleTileTypes, surroundingTilesToUse, false);
				} else {
					sum = BitSum(new List<TileTypes>() { tile.tileType.type }, surroundingTilesToUse, includeMapEdge);
				}
			}
			if ((sum < 16) || (bitmaskMap[sum] != 46)) {
				if (sum >= 16) {
					sum = bitmaskMap[sum];
				}
				if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type) && RiversContainTile(tile).Key != null) {
					tile.sr.sprite = tile.tileType.riverSprites[sum];
				} else {
					tile.sr.sprite = tile.tileType.bitmaskSprites[sum];
				}
			} else {
				if (tile.tileType.baseSprites.Count > 0 && !tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
					tile.sr.sprite = tile.tileType.baseSprites[Random.Range(0, tile.tileType.baseSprites.Count)];
				}
			}
		}

		public void Bitmasking(List<Tile> tilesToBitmask) {
			foreach (Tile tile in tilesToBitmask) {
				if (tile != null) {
					if (tileM.BitmaskingTileTypes.Contains(tile.tileType.type)) {
						BitmaskTile(tile, true, false, null, true);
					} else {
						if (!tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
							tile.sr.sprite = tile.tileType.baseSprites[Random.Range(0, tile.tileType.baseSprites.Count)];
						}
					}
				}
			}
			BitmaskRiverStartTiles();
		}

		void BitmaskRiverStartTiles() {
			foreach (River river in rivers) {
				List<TileTypes> compareTileTypes = new List<TileTypes>();
				compareTileTypes.AddRange(tileM.WaterEquivalentTileTypes);
				compareTileTypes.AddRange(tileM.StoneEquivalentTileTypes);
				BitmaskTile(river.startTile, false, true, compareTileTypes, false);
			}
		}

		private List<RegionBlock> visibleRegionBlocks = new List<RegionBlock>();
		private RegionBlock centreRegionBlock;
		private int lastOrthographicSize = -1;

		public void DetermineVisibleRegionBlocks() {
			RegionBlock newCentreRegionBlock = GetTileFromPosition(cameraM.cameraGO.transform.position).squareRegionBlock;
			if (newCentreRegionBlock != centreRegionBlock || Mathf.RoundToInt(cameraM.cameraComponent.orthographicSize) != lastOrthographicSize) {
				visibleRegionBlocks.Clear();
				lastOrthographicSize = Mathf.RoundToInt(cameraM.cameraComponent.orthographicSize);
				centreRegionBlock = newCentreRegionBlock;
				List<RegionBlock> frontier = new List<RegionBlock>() { centreRegionBlock };
				List<RegionBlock> checkedBlocks = new List<RegionBlock>() { centreRegionBlock };
				while (frontier.Count > 0) {
					RegionBlock currentRegionBlock = frontier[0];
					frontier.RemoveAt(0);
					visibleRegionBlocks.Add(currentRegionBlock);
					foreach (RegionBlock nBlock in currentRegionBlock.surroundingRegionBlocks) {
						if (Vector2.Distance(currentRegionBlock.averagePosition, cameraM.cameraGO.transform.position) <= cameraM.cameraComponent.orthographicSize * ((float)Screen.width / Screen.height)) {
							if (!checkedBlocks.Contains(nBlock)) {
								frontier.Add(nBlock);
								checkedBlocks.Add(nBlock);
							}
						} else {
							if (!checkedBlocks.Contains(nBlock)) {
								visibleRegionBlocks.Add(nBlock);
								checkedBlocks.Add(nBlock);
							}
						}
					}
				}
				SetTileBrightness(timeM.GetTileBrightnessTime());
			}
		}

		public void SetTileBrightness(float time) {
			Color newColour = GetTileColourAtHour(time);
			foreach (RegionBlock visibleRegionBlock in visibleRegionBlocks) {
				foreach (Tile tile in visibleRegionBlock.tiles) {
					tile.SetColour(newColour, Mathf.FloorToInt(time));
				}
			}
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				colonist.SetColour(colonist.overTile.sr.color);
			}
			cameraM.cameraComponent.backgroundColor = newColour * 0.5f;
		}

		public float CalculateBrightnessLevelAtHour(float time) {
			return ((-(1f / 144f)) * Mathf.Pow(((1 + (24 - (1 - time))) % 24) - 12, 2) + 1.2f);
		}

		public Color GetTileColourAtHour(float time) {
			float r = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.4f * time + 7.2f), 10)) / 5f, 0f, 1f);
			float g = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.5f * time + 6), 10)) / 5f - 0.2f, 0f, 1f);
			float b = Mathf.Clamp((-1.5f * Mathf.Pow(Mathf.Cos((CalculateBrightnessLevelAtHour(2 * time + 12)) / 1.5f), 3) + 1.65f * (CalculateBrightnessLevelAtHour(time) / 2f)) + 0.7f, 0f, 1f);
			return new Color(r, g, b, 1f);
		}

		public bool TileBlocksLight(Tile tile) {
			if (tile.GetAllObjectInstances().Find(toi => resourceM.GetLightBlockingTileObjects().Contains(toi.prefab.type)) != null) {
				return true;
			}
			if (tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) {
				return true;
			}
			return false;
		}

		public bool TileCanShadowTiles(Tile tile) {
			return (TileBlocksLight(tile) && tile.surroundingTiles.Find(nTile => nTile != null && !TileBlocksLight(nTile)) != null) || (tile.roof && !tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type));
		}

		public bool TileCanBeShadowed(Tile tile) {
			return (!TileBlocksLight(tile) && tile.GetAllObjectInstances().Find(instance => !resourceM.GetLightBlockingTileObjects().Contains(instance.prefab.type)) != null ? true : !TileBlocksLight(tile));
		}

		private Dictionary<int, Vector2> shadowDirectionAtHour = new Dictionary<int, Vector2>();
		private void DetermineShadowDirectionsAtHour() {
			for (int h = 0; h < 24; h++) {
				float hShadow = (2f * ((h - 12f) / 24f)) * (1f - Mathf.Pow(mapData.equatorOffset, 2f));
				float vShadow = Mathf.Pow(2f * ((h - 12f) / 24f), 2f) * mapData.equatorOffset + (mapData.equatorOffset / 2f);
				shadowDirectionAtHour.Add(h, new Vector2(hShadow, vShadow) * 5f);
			}
		}

		public void DetermineShadowTiles(List<Tile> tilesToInclude, bool setBrightnessAtEnd) {
			List<Tile> shadowStartTiles = new List<Tile>();
			foreach (Tile tile in tilesToInclude) {
				if (TileCanShadowTiles(tile)) {
					shadowStartTiles.Add(tile);
				}
			}
			for (int h = 0; h < 24; h++) {
				Vector2 hourDirection = shadowDirectionAtHour[h];

				foreach (Tile tile in shadowStartTiles) {
					Vector2 tilePosition = tile.obj.transform.position;

					float oppositeTileMaxHeight = 0;
					float oppositeDistance = 0;
					Tile oppositeTile = tile;
					while (oppositeTile != null && !TileBlocksLight(oppositeTile)) {
						if (oppositeTile.height >= oppositeTileMaxHeight) {
							oppositeTileMaxHeight = oppositeTile.height;
						}
						Tile newOppositeTile = oppositeTile;
						int sameCounter = 0;
						while (newOppositeTile == oppositeTile) {
							oppositeDistance += 0.1f;
							newOppositeTile = GetTileFromPosition(tilePosition + ((-hourDirection) * oppositeDistance));
							if (newOppositeTile == oppositeTile) {
								if (sameCounter >= 4) {
									break;
								}
								sameCounter += 1;
							} else {
								oppositeTile = newOppositeTile;
								break;
							}
						}
						if (sameCounter >= 4) {
							break;
						}
					}
					float heightModifer = (1 + (oppositeTileMaxHeight - mapData.terrainTypeHeights[TileTypes.Stone]));
					float maxDistance = hourDirection.magnitude * heightModifer * 5f + (Mathf.Pow(h - 12, 2) / 6f);

					List<Tile> shadowTiles = new List<Tile>();
					for (float distance = 0; distance <= maxDistance; distance += 0.1f) {
						Vector2 nextTilePosition = tilePosition + (hourDirection * distance);
						if (nextTilePosition.x < 0 || nextTilePosition.x >= mapData.mapSize || nextTilePosition.y < 0 || nextTilePosition.y >= mapData.mapSize) {
							break;
						}
						Tile shadowTile = GetTileFromPosition(nextTilePosition);
						if (shadowTiles.Contains(shadowTile)) {
							distance += 0.1f;
							continue;
						}
						if (shadowTile != tile) {
							float newBrightness = 1;
							if (TileCanBeShadowed(shadowTile)) {
								newBrightness = Mathf.Clamp((1 - (0.6f * CalculateBrightnessLevelAtHour(h)) + 0.3f) /* / (1 - ((distance - (maxDistance / 2f)) / (maxDistance <= 0 ? 1 : maxDistance))) */, 0, 1);
								if (shadowTile.brightnessAtHour.ContainsKey(h)) {
									shadowTile.brightnessAtHour[h] = Mathf.Min(shadowTile.brightnessAtHour[h], newBrightness);
								} else {
									shadowTile.brightnessAtHour.Add(h, newBrightness);
								}
								shadowTiles.Add(shadowTile);
							} else {
								if (shadowTile.blockingShadowsFrom.ContainsKey(h)) {
									shadowTile.blockingShadowsFrom[h].Add(tile);
								} else {
									shadowTile.blockingShadowsFrom.Add(h, new List<Tile>() { tile });
								}
								shadowTile.blockingShadowsFrom[h] = shadowTile.blockingShadowsFrom[h].Distinct().ToList();
							}
							if (shadowTile.shadowsFrom.ContainsKey(h)) {
								if (shadowTile.shadowsFrom[h].ContainsKey(tile)) {
									shadowTile.shadowsFrom[h][tile] = newBrightness;
								} else {
									shadowTile.shadowsFrom[h].Add(tile, newBrightness);
								}
							} else {
								shadowTile.shadowsFrom.Add(h, new Dictionary<Tile, float>() { { tile, newBrightness } });
							}
						}
					}
					if (tile.shadowsTo.ContainsKey(h)) {
						tile.shadowsTo[h].AddRange(shadowTiles);
					} else {
						tile.shadowsTo.Add(h, shadowTiles);
					}
					tile.shadowsTo[h] = tile.shadowsTo[h].Distinct().ToList();
				}
			}
			if (setBrightnessAtEnd) {
				SetTileBrightness(timeM.GetTileBrightnessTime());
			}
		}

		public void RemoveTileBrightnessEffect(Tile tile) {
			List<Tile> tilesToRecalculateShadowsFor = new List<Tile>();
			for (int h = 0; h < 24; h++) {
				if (tile.shadowsTo.ContainsKey(h)) {
					foreach (Tile nTile in tile.shadowsTo[h]) {
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

			DetermineShadowTiles(tilesToRecalculateShadowsFor.Distinct().ToList(), true);
		}

		public List<ResourceManager.Plant> smallPlants = new List<ResourceManager.Plant>();

		public void GrowPlants() {
			if (!timeM.GetPaused() && timeM.minuteChanged) {
				List<ResourceManager.Plant> growPlants = new List<ResourceManager.Plant>();
				foreach (ResourceManager.Plant plant in smallPlants) {
					plant.growthProgress += 1 * timeM.deltaTime;
					if (plant.growthProgress > 5760) { // 5760 = 4 in-game days in seconds
						if (Random.Range(0, 100) < (0.01 * (plant.growthProgress / 5760))) {
							growPlants.Add(plant);
						}
					}
				}
				foreach (ResourceManager.Plant plant in growPlants) {
					plant.Grow(smallPlants);
				}
				growPlants.Clear();
			}
		}

		public Tile GetTileFromPosition(Vector2 position) {
			position = new Vector2(Mathf.Clamp(position.x, 0, mapData.mapSize - 1), Mathf.Clamp(position.y, 0, mapData.mapSize - 1));
			return sortedTiles[Mathf.FloorToInt(position.y)][Mathf.FloorToInt(position.x)];
		}
	}
}