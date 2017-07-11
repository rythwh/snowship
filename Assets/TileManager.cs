using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager:MonoBehaviour {

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

	public enum PlantGroupsEnum { Cactus, ColourfulShrub, ColourfulTree, DeadTree, Shrub, SnowTree, ThinTree, WideTree };

	private Dictionary<PlantGroupsEnum,ResourceManager.ResourcesEnum> plantSeeds = new Dictionary<PlantGroupsEnum,ResourceManager.ResourcesEnum>() {
		{ PlantGroupsEnum.ColourfulShrub,ResourceManager.ResourcesEnum.ShrubSeeds },
		{ PlantGroupsEnum.ColourfulTree,ResourceManager.ResourcesEnum.TreeSeeds },
		{ PlantGroupsEnum.DeadTree,ResourceManager.ResourcesEnum.TreeSeeds },
		{ PlantGroupsEnum.Shrub,ResourceManager.ResourcesEnum.ShrubSeeds },
		{ PlantGroupsEnum.SnowTree,ResourceManager.ResourcesEnum.TreeSeeds },
		{ PlantGroupsEnum.ThinTree,ResourceManager.ResourcesEnum.TreeSeeds },
		{ PlantGroupsEnum.WideTree,ResourceManager.ResourcesEnum.TreeSeeds },
		{ PlantGroupsEnum.Cactus,ResourceManager.ResourcesEnum.CactusSeeds }
	};
	public Dictionary<PlantGroupsEnum,ResourceManager.ResourcesEnum> GetPlantSeeds() {
		return plantSeeds;
	}

	private Dictionary<PlantGroupsEnum,ResourceManager.TileObjectPrefabsEnum> plantPlantObjectPrefabs = new Dictionary<PlantGroupsEnum,ResourceManager.TileObjectPrefabsEnum>() {
		{ PlantGroupsEnum.ColourfulShrub,ResourceManager.TileObjectPrefabsEnum.PlantShrub },
		{ PlantGroupsEnum.ColourfulTree,ResourceManager.TileObjectPrefabsEnum.PlantTree },
		{ PlantGroupsEnum.DeadTree,ResourceManager.TileObjectPrefabsEnum.PlantTree },
		{ PlantGroupsEnum.Shrub,ResourceManager.TileObjectPrefabsEnum.PlantShrub },
		{ PlantGroupsEnum.SnowTree,ResourceManager.TileObjectPrefabsEnum.PlantTree },
		{ PlantGroupsEnum.ThinTree,ResourceManager.TileObjectPrefabsEnum.PlantTree },
		{ PlantGroupsEnum.WideTree,ResourceManager.TileObjectPrefabsEnum.PlantTree },
		{ PlantGroupsEnum.Cactus,ResourceManager.TileObjectPrefabsEnum.PlantCactus }
	};
	public Dictionary<PlantGroupsEnum,ResourceManager.TileObjectPrefabsEnum> GetPlantPlantObjectPrefabs() {
		return plantPlantObjectPrefabs;
	}

	private Dictionary<PlantGroupsEnum,List<ResourceManager.ResourceAmount>> plantResources = new Dictionary<PlantGroupsEnum,List<ResourceManager.ResourceAmount>>();
	public void SetPlantResources() {
		ResourceManager.Resource woodResource = resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Wood);
		plantResources.Add(PlantGroupsEnum.ColourfulShrub,	new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,2),
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.ColourfulShrub]),2)
		});
		plantResources.Add(PlantGroupsEnum.ColourfulTree,	new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,5),
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.ColourfulTree]),3)
		});
		plantResources.Add(PlantGroupsEnum.DeadTree,		new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,3)
		});
		plantResources.Add(PlantGroupsEnum.Shrub,			new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,2),
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.Shrub]),2)
		});
		plantResources.Add(PlantGroupsEnum.SnowTree,		new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,5),
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.SnowTree]),3)
		});
		plantResources.Add(PlantGroupsEnum.ThinTree,		new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,5),
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.ThinTree]),3)
		});
		plantResources.Add(PlantGroupsEnum.WideTree,		new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(woodResource,6),
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.WideTree]),3)
		});
		plantResources.Add(PlantGroupsEnum.Cactus,new List<ResourceManager.ResourceAmount>() {
			new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(plantSeeds[PlantGroupsEnum.Cactus]),3)
		});
	}

	public List<PlantGroup> plantGroups = new List<PlantGroup>();

	public class PlantGroup {
		public PlantGroupsEnum type;
		public string name;

		public List<Sprite> smallPlants = new List<Sprite>();
		public List<Sprite> fullPlants = new List<Sprite>();

		public List<ResourceManager.ResourceAmount> returnResources = new List<ResourceManager.ResourceAmount>();

		public PlantGroup(PlantGroupsEnum type,List<ResourceManager.ResourceAmount> returnResources) {
			this.type = type;
			name = type.ToString();

			smallPlants = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + name + "/" + name + "-small").ToList();
			fullPlants = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + name + "/" + name + "-full").ToList();

			this.returnResources = returnResources;
		}
	}

	public void CreatePlantGroups() {
		foreach (PlantGroupsEnum plantGroup in System.Enum.GetValues(typeof(PlantGroupsEnum))) {
			plantGroups.Add(new PlantGroup(plantGroup,(plantResources.ContainsKey(plantGroup) ? plantResources[plantGroup] : new List<ResourceManager.ResourceAmount>())));
		}
		foreach (PlantGroup plantGroup in plantGroups) {
			plantGroup.name = uiM.SplitByCapitals(plantGroup.name);
		}
	}

	public PlantGroup GetPlantGroupByEnum(PlantGroupsEnum plantGroup) {
		return plantGroups.Find(group => group.type == plantGroup);
	}

	public class Plant {
		public PlantGroup group;
		public Tile tile;
		public GameObject obj;

		bool small;

		public Plant(PlantGroup group, Tile tile, bool randomSmall, bool smallValue) {
			this.group = group;
			this.tile = tile;
			
			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform.position,Quaternion.identity);

			SpriteRenderer pSR = obj.GetComponent<SpriteRenderer>();

			small = (randomSmall ? Random.Range(0f,1f) < 0.1f : smallValue);
			pSR.sprite = (small ? group.smallPlants[Random.Range(0,group.smallPlants.Count)] : group.fullPlants[Random.Range(0,group.fullPlants.Count)]);
			pSR.sortingOrder = 1; // Plant Sprite

			obj.name = "PLANT " + pSR.sprite.name;
			obj.transform.parent = tile.obj.transform;
		}

		public List<ResourceManager.ResourceAmount> GetResources() {
			List<ResourceManager.ResourceAmount> resourcesToReturn = new List<ResourceManager.ResourceAmount>();
			foreach (ResourceManager.ResourceAmount resourceAmount in group.returnResources) {
				int amount = Mathf.Clamp(resourceAmount.amount + Random.Range(-2,2),1,int.MaxValue);
				if (small && amount > 0) {
					amount = Mathf.CeilToInt(amount / 2f);
				}
				resourcesToReturn.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,amount));
			}
			return resourcesToReturn;
		}
	}

	public PlantGroup GetPlantGroupByBiome(Biome biome, bool guaranteedTree) {
		if (guaranteedTree) {
			List<PlantGroupsEnum> biomePlantGroupsEnums = biome.vegetationChances.Keys.Where(group => group != PlantGroupsEnum.DeadTree).ToList();
			return GetPlantGroupByEnum(biomePlantGroupsEnums[Random.Range(0,biomePlantGroupsEnums.Count)]);
		} else {
			foreach (KeyValuePair<PlantGroupsEnum,float> kvp in biome.vegetationChances) {
				PlantGroupsEnum plantGroup = kvp.Key;
				if (Random.Range(0f,1f) < biome.vegetationChances[plantGroup]) {
					return GetPlantGroupByEnum(plantGroup);
				}
			}
		}
		return null;
	}

	public enum TileTypes { GrassWater, Ice, Dirt, DirtWater, Mud, DirtGrass, DirtThinGrass, DirtDryGrass, Grass, ThickGrass, ColdGrass, ColdGrassWater, DryGrass, DryGrassWater, Sand, SandWater,
		Snow, SnowIce, SnowStone, Stone, StoneIce, StoneWater, StoneThinGrass, StoneSand, StoneSnow, Granite, Limestone, Marble, Sandstone, Slate, Clay
	};
	List<TileTypes> WaterEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.SnowIce, TileTypes.StoneIce, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater
	};
	List<TileTypes> LiquidWaterEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater
	};
	List<TileTypes> StoneEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.Stone, TileTypes.Granite, TileTypes.Limestone, TileTypes.Marble, TileTypes.Sandstone, TileTypes.Slate, TileTypes.Clay
	};
	List<TileTypes> PlantableTileTypes = new List<TileTypes>() {
		TileTypes.Dirt, TileTypes.Mud, TileTypes.DirtGrass, TileTypes.DirtThinGrass, TileTypes.DirtDryGrass, TileTypes.Grass, TileTypes.ThickGrass,
		TileTypes.DryGrass, TileTypes.ColdGrass, TileTypes.Sand, TileTypes.Snow, TileTypes.SnowStone, TileTypes.StoneThinGrass, TileTypes.StoneSand, TileTypes.StoneSnow
	};
	List<TileTypes> BitmaskingTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.SnowIce, TileTypes.StoneIce, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater, TileTypes.Stone
	};

	public List<TileTypes> GetWaterEquivalentTileTypes() {
		return WaterEquivalentTileTypes;
	}
	public List<TileTypes> GetLiquidWaterEquivalentTileTypes() {
		return LiquidWaterEquivalentTileTypes;
	}
	public List<TileTypes> GetStoneEquivalentTileTypes() {
		return StoneEquivalentTileTypes;
	}
	public List<TileTypes> GetPlantableTileTypes() {
		return PlantableTileTypes;
	}
	public List<TileTypes> GetBitmaskingTileTypes() {
		return BitmaskingTileTypes;
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
			type = (TileTypes)System.Enum.Parse(typeof(TileTypes),tileTypeData[0]);
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
		List<string> stringTileTypes = Resources.Load<TextAsset>(@"Data/tiletypes").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringTileType in stringTileTypes) {
			List<string> stringTileTypeData = stringTileType.Split('/').ToList();
			tileTypes.Add(new TileType(stringTileTypeData,this));
		}
		foreach (TileType tileType in tileTypes) {
			tileType.name = uiM.SplitByCapitals(tileType.name);
		}
	}

	public TileType GetTileTypeByEnum(TileTypes find) {
		return tileTypes.Find(tileType => tileType.type == find);
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

		public Dictionary<PlantGroupsEnum,float> vegetationChances = new Dictionary<PlantGroupsEnum,float>();

		public Color colour;

		public TileType tileType;
		public TileType waterType;

		public Biome(List<string> biomeData, TileManager tileM) {
			type = (BiomeTypes)System.Enum.Parse(typeof(BiomeTypes),biomeData[0]);
			name = type.ToString();

			List<string> tileTypeData = biomeData[1].Split(',').ToList();
			tileType = tileM.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes),tileTypeData[0]));
			waterType = tileM.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes),tileTypeData[1]));

			if (float.Parse(biomeData[2].Split(',')[0]) != 0) {
				int vegetationIndex = 0;
				foreach (string vegetationChance in biomeData[2].Split(',').ToList()) {
					vegetationChances.Add((PlantGroupsEnum)System.Enum.Parse(typeof(PlantGroupsEnum),biomeData[3].Split(',').ToList()[vegetationIndex]),float.Parse(vegetationChance));
					vegetationIndex += 1;
				}
			}

			int r = int.Parse("" + biomeData[4][2] + biomeData[4][3],System.Globalization.NumberStyles.HexNumber);
			int g = int.Parse("" + biomeData[4][4] + biomeData[4][5],System.Globalization.NumberStyles.HexNumber);
			int b = int.Parse("" + biomeData[4][6] + biomeData[4][7],System.Globalization.NumberStyles.HexNumber);
			colour = new Color(r,g,b,255f) / 255f;

		}
	}

	public void CreateBiomes() {
		List<string> stringBiomeTypes = Resources.Load<TextAsset>(@"Data/biomes").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringBiomeType in stringBiomeTypes) {
			List<string> stringBiomeData = stringBiomeType.Split('/').ToList();
			biomes.Add(new Biome(stringBiomeData,this));
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
				temperatureRanges.Add(new TemperatureRange(temperatureRangeString,this,tileM));
			}
		}

		public class TemperatureRange {

			public PrecipitationRange precipitationRange;

			public int min = 0;
			public int max = 0;

			public Biome biome;

			public TemperatureRange(string dataString,PrecipitationRange precipitationRange, TileManager tileM) {

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

				biome = tileM.biomes.Find(b => b.type == (BiomeTypes)System.Enum.Parse(typeof(BiomeTypes),temperatureRangeData[1]));
			}
		}
	}

	public List<PrecipitationRange> biomeRanges = new List<PrecipitationRange>();

	public void CreateBiomeRanges() {
		List<string> biomeRangeStrings = Resources.Load<TextAsset>(@"Data/biomeRanges").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('~').ToList();
		foreach (string biomeRangeString in biomeRangeStrings) {
			biomeRanges.Add(new PrecipitationRange(biomeRangeString,this));
		}
	}

	public enum TileResourceTypes { Copper, Silver, Gold, Iron, Steel, Diamond };

	Dictionary<int,List<List<int>>> nonWalkableSurroundingTilesComparatorMap = new Dictionary<int,List<List<int>>>() {
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
		public TileResourceTypes tileResource;

		public Map.Region region;
		public Map.Region drainageBasin;
		public Map.RegionBlock regionBlock;
		public Map.RegionBlock squareRegionBlock;

		public Biome biome;
		public Plant plant;
		public ResourceManager.Farm farm;

		public float precipitation;
		public float temperature;

		public bool walkable;
		public float walkSpeed;

		public bool roof;

		public float brightness;
		public Dictionary<int,float> brightnessAtHour = new Dictionary<int,float>();
		public Dictionary<int,Dictionary<Tile,float>> tilesAffectingBrightnessOfThisTile = new Dictionary<int,Dictionary<Tile,float>>();
		public Dictionary<int,Dictionary<Tile,float>> tilesBrightnessBeingAffectedByThisTile = new Dictionary<int,Dictionary<Tile,float>>();

		public Dictionary<int,ResourceManager.TileObjectInstance> objectInstances = new Dictionary<int,ResourceManager.TileObjectInstance>();

		public Tile(Map map, Vector2 position,float height,TileManager tileM, TimeManager timeM, bool normalTile) {

			this.tileM = tileM;
			this.timeM = timeM;

			this.map = map;

			this.position = position;

			if (normalTile) {
				obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),new Vector2(position.x + 0.5f,position.y + 0.5f),Quaternion.identity);
				obj.transform.SetParent(GameObject.Find("TileParent").transform,true);

				sr = obj.GetComponent<SpriteRenderer>();

				SetTileHeight(height);

				SetBrightness(1f,12);
			}
		}

		public void SetTileHeight(float height) {
			this.height = height;
			SetTileTypeBasedOnHeight();
		}

		public void SetTileType(TileType tileType,bool bitmask,bool resetRegion,bool removeFromOldRegion, bool setBiomeTileType) {
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
				Destroy(plant.obj);
				plant = null;
			}
			if (resetRegion) {
				ResetRegion(oldTileType,removeFromOldRegion);
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
							Map.Region groupRegion = new Map.Region(nonWalkableTileGroup[0].tileType,map.currentRegionID);
							map.currentRegionID += 1;
							foreach (Tile tile in nonWalkableTileGroup) {
								tile.ChangeRegion(groupRegion,false,false);
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
						ChangeRegion(new Map.Region(tileType,map.currentRegionID),false,false);
						map.currentRegionID += 1;
					} else if (similarRegions.Count == 1) {
						region.tiles.Remove(this);
						ChangeRegion(similarRegions[0],false,false);
					} else {
						region.tiles.Remove(this);
						ChangeRegion(similarRegions.OrderByDescending(similarRegion => similarRegion.tiles.Count).ToList()[0],false,false);
						foreach (Map.Region similarRegion in similarRegions) {
							if (similarRegion != region) {
								foreach (Tile tile in similarRegion.tiles) {
									tile.ChangeRegion(region,false,false);
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
				SetTileType(tileM.GetTileTypeByEnum(TileTypes.GrassWater),false,false,false,false);
			} else if (height > map.mapData.terrainTypeHeights[TileTypes.Stone]) {
				SetTileType(tileM.GetTileTypeByEnum(TileTypes.Stone),false,false,false,false);
				if (height >= map.mapData.terrainTypeHeights[TileTypes.GrassWater] + 0.05f) {
					roof = true;
				}
			} else {
				SetTileType(tileM.GetTileTypeByEnum(TileTypes.Grass),false,false,false,false);
			}
		}

		public void ChangeRegion(Map.Region newRegion, bool changeTileTypeToRegionType, bool bitmask) {
			region = newRegion;
			region.tiles.Add(this);
			if (!map.regions.Contains(region)) {
				map.regions.Add(region);
			}
			if (changeTileTypeToRegionType) {
				SetTileType(region.tileType,bitmask,false,false,true);
			}
		}

		public void SetBiome(Biome biome) {
			this.biome = biome;
			if (!tileM.StoneEquivalentTileTypes.Contains(tileType.type)) {
				if (!tileM.WaterEquivalentTileTypes.Contains(tileType.type)) {
					SetTileType(biome.tileType,false,false,false,false);
				} else {
					SetTileType(biome.waterType,false,false,false,false);
				}
			}
			if (tileM.PlantableTileTypes.Contains(tileType.type)) {
				SetPlant(false,null);
			}
		}

		public void SetPlant(bool removePlant, Plant specificPlant) {
			if (plant != null) {
				Destroy(plant.obj);
				plant = null;
			}
			if (!removePlant) {
				if (specificPlant == null) {
					PlantGroup biomePlantGroup = tileM.GetPlantGroupByBiome(biome,false);
					if (biomePlantGroup != null) {
						plant = new Plant(biomePlantGroup,this,true,false);
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

		public void SetTileObject(ResourceManager.TileObjectPrefab tileObjectPrefab,int rotationIndex) {
			bool farm = tileM.resourceM.GetTileObjectPrefabByEnum(tileObjectPrefab.type).tileObjectPrefabSubGroup.type == ResourceManager.TileObjectPrefabSubGroupsEnum.PlantFarm;
			if (objectInstances.ContainsKey(tileObjectPrefab.layer)) {
				if (objectInstances[tileObjectPrefab.layer] != null) {
					if (tileObjectPrefab != null) {
						Debug.LogError("Trying to add object where one already exists at " + obj.transform.position);
					} else {
						objectInstances[tileObjectPrefab.layer] = null;
					}
				} else {
					if (farm) {
						ResourceManager.Farm newFarm = new ResourceManager.Farm(tileObjectPrefab,this);
						objectInstances[tileObjectPrefab.layer] = newFarm;
						SetFarm(newFarm);
					} else {
						objectInstances[tileObjectPrefab.layer] = new ResourceManager.TileObjectInstance(tileObjectPrefab,this,rotationIndex);
					}
				}
			} else {
				if (farm) {
					ResourceManager.Farm newFarm = new ResourceManager.Farm(tileObjectPrefab,this);
					objectInstances.Add(tileObjectPrefab.layer,newFarm);
					SetFarm(newFarm);
				} else {
					objectInstances.Add(tileObjectPrefab.layer,new ResourceManager.TileObjectInstance(tileObjectPrefab,this,rotationIndex));
				}
			}
			tileM.resourceM.AddTileObjectInstance(objectInstances[tileObjectPrefab.layer]);
			if (farm) {

			}
			PostChangeTileObject();
		}

		public void PostChangeTileObject() {
			walkable = tileType.walkable;
			foreach (KeyValuePair<int,ResourceManager.TileObjectInstance> kvp in objectInstances) {
				if (kvp.Value != null && !kvp.Value.prefab.walkable) {
					walkable = false;
					map.RecalculateRegionsAtTile(this);

					map.DetermineShadowTiles(new List<Tile>() { this });

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
			foreach (KeyValuePair<int,ResourceManager.TileObjectInstance> kvp in objectInstances) {
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
			foreach (KeyValuePair<int,ResourceManager.TileObjectInstance> kvp in objectInstances) {
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

		public void SetColour(Color newColour, int hour) {
			//sr.color = newColour * (brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f);
			
			float currentHourBrightness = (brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f);
			int nextHour = (hour == 23 ? 0 : hour + 1);
			float nextHourBrightness = (brightnessAtHour.ContainsKey(nextHour) ? brightnessAtHour[nextHour] : 1f);
			sr.color = newColour * Mathf.Lerp(currentHourBrightness,nextHourBrightness,(timeM.GetTileBrightnessTime() - hour));
			if (plant != null) {
				plant.obj.GetComponent<SpriteRenderer>().color = new Color(sr.color.r,sr.color.g,sr.color.b,1f); ;
			}
			foreach (ResourceManager.TileObjectInstance instance in GetAllObjectInstances()) {
				instance.SetColour(sr.color);
			}
		}

		public void SetBrightness(float newBrightness, int hour) {
			brightness = newBrightness;
			SetColour(sr.color, hour);
		}
	}

	public bool generated;

	public bool debugMode;
	private int viewRiverAtIndex = 0;

	void Update() {
		if (generated) {
			if (debugMode) {
				DebugFunctions();
			} else {
				map.DetermineVisibleRegionBlocks();
			}
		}
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote)) {
			debugMode = !debugMode;
		}
	}

	public void DebugFunctions() {
		if (Input.GetKeyDown(KeyCode.Z)) {
			foreach (Tile tile in map.tiles) {
				tile.sr.color = Color.white;
			}
			map.Bitmasking(map.tiles);
		}
		if (Input.GetKeyDown(KeyCode.X)) {
			foreach (Map.Region region in map.regions) {
				region.ColourRegion();
			}
		}
		if (Input.GetKeyDown(KeyCode.C)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile tile in map.tiles) {
				tile.sr.sprite = whiteSquare;
				tile.sr.color = new Color(tile.height,tile.height,tile.height,1f);
			}
		}
		if (Input.GetKeyDown(KeyCode.V)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile tile in map.tiles) {
				tile.sr.sprite = whiteSquare;
				tile.sr.color = new Color(tile.precipitation,tile.precipitation,tile.precipitation,1f);
			}
		}
		if (Input.GetKeyDown(KeyCode.B)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile tile in map.tiles) {
				tile.sr.sprite = whiteSquare;
				tile.sr.color = new Color((tile.temperature + 50f) / 100f,(tile.temperature + 50f) / 100f,(tile.temperature + 50f) / 100f,1f);
			}
		}
		if (Input.GetKeyDown(KeyCode.N)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile tile in map.tiles) {
				tile.sr.sprite = whiteSquare;
				if (tile.biome != null) {
					tile.sr.color = tile.biome.colour;
				} else {
					tile.sr.color = Color.black;
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.M)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (KeyValuePair<Map.Region,Tile> kvp in map.drainageBasins) {
				foreach (Tile tile in kvp.Key.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = kvp.Key.colour;
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Comma)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			/*
			foreach (List<Tile> river in rivers) {
				foreach (Tile tile in river) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = Color.blue;
				}
				river[0].sr.color = Color.red;
				river[river.Count - 1].sr.color = Color.green;
			}
			*/
			foreach (Tile tile in map.tiles) {
				tile.sr.color = Color.white;
			}
			map.Bitmasking(map.tiles);
			foreach (Tile tile in map.rivers[viewRiverAtIndex].tiles) {
				tile.sr.sprite = whiteSquare;
				tile.sr.color = Color.blue;
			}
			map.rivers[viewRiverAtIndex].tiles[0].sr.color = Color.red;
			map.rivers[viewRiverAtIndex].tiles[map.rivers[viewRiverAtIndex].tiles.Count - 1].sr.color = Color.green;
			viewRiverAtIndex += 1;
			if (viewRiverAtIndex == map.rivers.Count) {
				viewRiverAtIndex = 0;
			}
		}
		if (Input.GetKeyDown(KeyCode.Period)) {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile tile in map.tiles) {
				tile.sr.sprite = whiteSquare;
				tile.sr.color = new Color(tile.walkSpeed,tile.walkSpeed,tile.walkSpeed,1f);
			}
		}
		if (Input.GetKeyDown(KeyCode.Slash)) {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Potatoes),10);
			}
			/*
			foreach (ResourceManager.Container container in resourceM.containers) {
				container.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Wood),10);
			}
			*/
		}
		if (Input.GetKeyDown(KeyCode.L)) {
			foreach (Map.Region region in map.regionBlocks) {
				region.ColourRegion();
			}
		}
		if (Input.GetKeyDown(KeyCode.K)) {
			foreach (Map.Region region in map.squareRegionBlocks) {
				region.ColourRegion();
			}
		}
		if (Input.GetKeyDown(KeyCode.J)) {
			colonistM.colonists[0].inventory.ReserveResources(new List<ResourceManager.ResourceAmount>() { new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Wood),5) },colonistM.colonists[1]);
			/*
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				colonist.inventory.ReserveResources(new List<ResourceManager.ResourceAmount>() { new ResourceManager.ResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Wood),5) },colonist );
			}
			*/
		}
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetKeyDown(KeyCode.H)) {
			print("tilesAffectingBrightnessOfThisTile");
			map.SetTileBrightness(12);
			Tile tile = map.GetTileFromPosition(mousePosition);
			foreach (KeyValuePair<int,Dictionary<Tile,float>> hourKVP in tile.tilesAffectingBrightnessOfThisTile) {
				foreach (KeyValuePair<Tile,float> sTile in hourKVP.Value) {
					sTile.Key.SetColour(new Color(hourKVP.Key / 24f,1 - (hourKVP.Key / 24f),hourKVP.Key / 24f,1f),12);
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.G)) {
			print("tilesBrightnessBeingAffectedByThisTile");
			map.SetTileBrightness(12);
			Tile tile = map.GetTileFromPosition(mousePosition);
			foreach (KeyValuePair<int,Dictionary<Tile,float>> hourKVP in tile.tilesBrightnessBeingAffectedByThisTile) {
				foreach (KeyValuePair<Tile,float> sTile in hourKVP.Value) {
					sTile.Key.SetColour(new Color(hourKVP.Key / 24f,1 - (hourKVP.Key / 24f),hourKVP.Key / 24f,1f),12);
				}
			}
		}
		if (Input.GetMouseButtonDown(0)) {
			Tile tile = map.GetTileFromPosition(mousePosition);
			tile.SetTileType(GetTileTypeByEnum(TileTypes.Dirt),true,true,true,false);
			map.RemoveTileBrightnessEffect(tile);
			//print(tile.height);
			//print(tile.walkSpeed);
			/*
			ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == tile);
			if (container != null) {
				print("Found container");
			}
			*/
			/*pathM.RegionBlockDistance(GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,tile.regionBlock,true,true);*/
			/*
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile rTile in tile.squareRegionBlock.tiles) {
				rTile.sr.sprite = whiteSquare;
				rTile.sr.color = Color.black;
			}
			GetTileFromPosition(tile.squareRegionBlock.averagePosition).sr.sprite = whiteSquare;
			GetTileFromPosition(tile.squareRegionBlock.averagePosition).sr.color = Color.white;
			print(tile.squareRegionBlock.surroundingRegionBlocks.Count + " " + tile.squareRegionBlock.horizontalSurroundingRegionBlocks.Count);
			foreach (RegionBlock nRegionBlock in tile.squareRegionBlock.surroundingRegionBlocks) {
				Color colour = nRegionBlock.tileType.walkable ? Color.blue : Color.red;
				foreach (Tile rTile in nRegionBlock.tiles) {
					rTile.sr.sprite = whiteSquare;
					rTile.sr.color = colour;
				}
				GetTileFromPosition(nRegionBlock.averagePosition).sr.sprite = whiteSquare;
				GetTileFromPosition(nRegionBlock.averagePosition).sr.color = Color.white;
			}
			*/
			/*
			tile.SetTileType(GetTileTypeByEnum(TileTypes.Stone),true,true,true,true);
			RecalculateRegionsAtTile(tile);
			*/
			//SetTileRegions(false);
			//print(tile.region.tileType.walkable);
		}
		if (Input.GetMouseButtonDown(1)) {
			/*
			Tile tile = GetTileFromPosition(mousePosition);
			tile.SetTileType(GetTileTypeByEnum(TileTypes.Grass),true,true,true,true);
			RecalculateRegionsAtTile(tile);
			*/
			//tile.SetTileType(GetTileTypeByEnum(TileTypes.Grass),true);
			//print(tile.tileType.name);
		}
	}

	public class MapData {
		public int mapSeed;
		public int mapSize;
		public bool actualMap;

		public float equatorOffset;
		public bool planetTemperature;
		public float temperatureOffset;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileTypes,float> terrainTypeHeights;
		public bool coast;

		public bool preventEdgeTouching;

		public MapData(int mapSeed, int mapSize, bool actualMap, float equatorOffset, bool planetTemperature, float temperatureOffset, float averageTemperature, float averagePrecipitation, Dictionary<TileTypes,float> terrainTypeHeights, bool coast, bool preventEdgeTouching) {

			if (mapSeed < 0) {
				mapSeed = Random.Range(0,int.MaxValue);
			}
			Random.InitState(mapSeed);
			this.mapSeed = mapSeed;
			print("Map Seed: " + mapSeed);

			this.mapSize = mapSize;
			this.actualMap = actualMap;

			this.equatorOffset = equatorOffset;
			this.planetTemperature = planetTemperature;
			this.temperatureOffset = temperatureOffset;
			this.averageTemperature = averageTemperature;
			this.averagePrecipitation = averagePrecipitation;
			this.terrainTypeHeights = terrainTypeHeights;
			this.coast = coast;
			this.preventEdgeTouching = preventEdgeTouching;
		}
	}

	public void PreInitialize() {
		resourceM.CreateResources();
		resourceM.CreateTileObjectPrefabs();

		CreatePlantGroups();
		CreateTileTypes();
		CreateBiomes();
		CreateBiomeRanges();
	}

	public Map map;

	public void Initialize(MapData mapData) {
		map = new Map(mapData);

		cameraM.SetCameraPosition(new Vector2(mapData.mapSize / 2f,mapData.mapSize / 2f));
		cameraM.SetCameraZoom(20);

		colonistM.SpawnColonists(3);

		generated = true;

		uiM.SetSelectedColonistInformation();
		uiM.SetSelectedContainerInfo();
		uiM.SetJobElements();
		uiM.InitializeProfessionsList();
	}

	public class Map {

		private TileManager tileM;
		private TimeManager timeM;
		private PathManager pathM;
		private CameraManager cameraM;
		private ColonistManager colonistM;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			timeM = GM.GetComponent<TimeManager>();
			pathM = GM.GetComponent<PathManager>();
			cameraM = GM.GetComponent<CameraManager>();
			colonistM = GM.GetComponent<ColonistManager>();
		}

		public MapData mapData;
		public Map(MapData mapData) {

			GetScriptReferences();

			this.mapData = mapData;

			CreateMap();
		}

		public List<Tile> tiles = new List<Tile>();
		public List<List<Tile>> sortedTiles = new List<List<Tile>>();
		public List<Tile> edgeTiles = new List<Tile>();

		public void CreateMap() {

			CreateTiles();
			if (mapData.preventEdgeTouching) {
				PreventEdgeTouching();
			}

			SetTileRegions(true);

			ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 5f),new List<TileTypes>() { TileTypes.GrassWater,TileTypes.Stone,TileTypes.Grass });
			ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 2f),new List<TileTypes>() { TileTypes.GrassWater });

			CalculatePrecipitation();
			CalculateTemperature();

			/*
			foreach (Tile tile in tiles) {
				tile.SetTileHeight(0.5f);
				tile.precipitation = tile.position.x / mapData.mapSize;
				tile.temperature = ((1 - (tile.position.y / mapData.mapSize)) * 140) - 50;
			}
			*/

			SetTileRegions(false);
			SetBiomes();

			if (mapData.actualMap) {
				SetMapEdgeTiles();
				DetermineDrainageBasins();
				CreateRivers();
			}

			CreateRegionBlocks();

			Bitmasking(tiles);

			if (mapData.actualMap) {
				DetermineShadowTiles(tiles);
				SetTileBrightness(12);
			}
		}

		void CreateTiles() {
			for (int y = 0; y < mapData.mapSize; y++) {
				List<Tile> innerTiles = new List<Tile>();
				for (int x = 0; x < mapData.mapSize; x++) {

					float height = Random.Range(0f,1f);

					Vector2 position = new Vector2(x,y);

					Tile tile = new Tile(this,position,height,tileM,timeM,true);

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
			for (int halves = 0; halves < Mathf.CeilToInt(Mathf.Log(mapData.mapSize,2)); halves++) {
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
						sectionAverage += Random.Range(-maxDeviationSize,maxDeviationSize);
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
				float edgeDistance = (mapData.mapSize - (Vector2.Distance(tile.obj.transform.position,new Vector2(mapData.mapSize / 2f,mapData.mapSize / 2f)))) / mapData.mapSize;
				tile.SetTileHeight(tile.height * Mathf.Clamp(-Mathf.Pow(edgeDistance - 1.5f,10) + 1,0f,1f));
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

			public Region(TileType regionTileType,int regionID) {
				tileType = regionTileType;
				id = regionID;

				colour = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),1f);
			}

			public void ColourRegion() {
				Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
				foreach (Tile tile in this.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = colour;
				}
			}
		}

		void SetTileRegions(bool splitByTileType) {
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
					tile.ChangeRegion(new Region(tile.tileType,currentRegionID),false,false);
					currentRegionID += 1;
				} else if (foundRegions.Count == 1) { // If there was a single region found around them, give them that region
					tile.ChangeRegion(foundRegions[0],false,false);
				} else if (foundRegions.Count > 1) { // If there was more than one around found around them, give them the region with the lowest ID
					tile.ChangeRegion(FindLowestRegion(foundRegions),false,false);
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
								tile.ChangeRegion(lowestRegion,false,false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
						foreach (Region connectedRegion in region.connectedRegions) { // Set each tile's region in the connected regions that aren't the lowest region to the lowest region
							if (connectedRegion != lowestRegion) {
								foreach (Tile tile in connectedRegion.tiles) {
									tile.ChangeRegion(lowestRegion,false,false);
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
			public Vector2 averagePosition = new Vector2(0,0);
			public List<RegionBlock> surroundingRegionBlocks = new List<RegionBlock>();
			public List<RegionBlock> horizontalSurroundingRegionBlocks = new List<RegionBlock>();
			public RegionBlock(TileType regionTileType,int regionID) : base(regionTileType,regionID) {

			}
		}

		private int regionBlockSize = 10;
		public List<RegionBlock> squareRegionBlocks = new List<RegionBlock>();
		void CreateRegionBlocks() {

			regionBlocks.Clear();
			squareRegionBlocks.Clear();

			int size = regionBlockSize;
			int regionIndex = 0;
			for (int sectionY = 0; sectionY < mapData.mapSize; sectionY += size) {
				for (int sectionX = 0; sectionX < mapData.mapSize; sectionX += size) {
					RegionBlock regionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Grass),regionIndex);
					RegionBlock squareRegionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Grass),regionIndex);
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
					squareRegionBlock.averagePosition = new Vector2(squareRegionBlock.averagePosition.x + tile.obj.transform.position.x,squareRegionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				squareRegionBlock.averagePosition = new Vector2(squareRegionBlock.averagePosition.x / squareRegionBlock.tiles.Count,squareRegionBlock.averagePosition.y / squareRegionBlock.tiles.Count);
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
							RegionBlock unwalkableRegionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Stone),regionIndex);
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
							RegionBlock walkableRegionBlock = new RegionBlock(tileM.GetTileTypeByEnum(TileTypes.Grass),regionIndex);
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
					regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x + tile.obj.transform.position.x,regionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x / regionBlock.tiles.Count,regionBlock.averagePosition.y / regionBlock.tiles.Count);
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
									if (pathM.PathExists(startTile,endTile,true,mapData.mapSize,PathManager.WalkableSetting.Walkable,PathManager.DirectionSetting.Horizontal)) {
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

		void ReduceNoise(int removeRegionsBelowSize,List<TileTypes> typesToRemove) {
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
								tile.ChangeRegion(lowestRegion,true,false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
					}
				}
			}
			RemoveEmptyRegions();
		}

		Dictionary<int,int> oppositeDirectionTileMap = new Dictionary<int,int>() { { 0,2 },{ 1,3 },{ 2,0 },{ 3,1 },{ 4,6 },{ 5,7 },{ 6,4 },{ 7,5 } };

		private int windDirection = 0;
		void CalculatePrecipitation() {
			List<List<float>> precipitations = new List<List<float>>();
			for (int i = 0; i < 5; i++) { // 0 - up, 1 - right, 2 - down, 3 - left, 4 - up/right, 5 - down/right, 6 - down-left, 7 - up/left
				windDirection = i;
				if (windDirection <= 3) { // Wind is going horizontally/vertically
					bool yStartAtTop = (windDirection == 2);
					bool xStartAtRight = (windDirection == 3);

					for (int y = (yStartAtTop ? mapData.mapSize - 1 : 0); (yStartAtTop ? y >= 0 : y < mapData.mapSize); y += (yStartAtTop ? -1 : 1)) {
						for (int x = (xStartAtRight ? mapData.mapSize - 1 : 0); (xStartAtRight ? x >= 0 : x < mapData.mapSize); x += (xStartAtRight ? -1 : 1)) {
							Tile tile = sortedTiles[y][x];
							Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
							if (previousTile != null) {
								if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
									tile.precipitation = previousTile.precipitation + (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,(1 - previousTile.precipitation) / 5f) : 0f);
								} else {
									tile.precipitation = previousTile.precipitation - (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,previousTile.precipitation / 5f) * tile.height : 0f);//(tile.height * Random.Range(0.005f,0.03f));
								}
							} else {
								tile.precipitation = Random.Range(0.1f,0.5f) * (1 - tile.height);
							}
						}
					}
				} else { // Wind is going diagonally
					for (int k = 0; k < mapData.mapSize * 2; k++) {
						for (int x = 0; x <= k; x++) {
							int y = k - x;
							if (y < mapData.mapSize && x < mapData.mapSize) {
								Tile tile = sortedTiles[y][x];
								Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
								if (previousTile != null) {
									if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
										tile.precipitation = previousTile.precipitation + (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,(1 - previousTile.precipitation) / 5f) : 0f);
									} else {
										tile.precipitation = previousTile.precipitation - (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,previousTile.precipitation / 5f) * tile.height : 0f);
									}
								} else {
									tile.precipitation = Random.Range(0.1f,0.5f) * (1 - tile.height);
								}
							}
						}
					}
				}
				List<float> directionPrecipitations = new List<float>();
				foreach (Tile tile in tiles) {
					directionPrecipitations.Add(tile.precipitation);
					tile.precipitation = 0;
				}
				precipitations.Add(directionPrecipitations);
			}
			int primaryDirection = Random.Range(0,3);
			int oppositeDirection = oppositeDirectionTileMap[primaryDirection];

			for (int t = 0; t < tiles.Count; t++) {
				tiles[t].precipitation = 0;
				for (int i = 0; i < 4; i++) {
					if (i == oppositeDirection) {
						tiles[t].precipitation += precipitations[i][t] * 0.1f; // 0.25f
					} else if (i != primaryDirection) {
						tiles[t].precipitation += precipitations[i][t] * 0.25f; // 0.5f
					} else {
						tiles[t].precipitation += precipitations[i][t];
					}
				}
				tiles[t].precipitation /= 1.35f; //1.3f; // 2.25f
			}
			AverageTilePrecipitations();

			foreach (Tile tile in tiles) {
				tile.precipitation = Mathf.Clamp((Mathf.RoundToInt(mapData.averagePrecipitation) != -1 ? (tile.precipitation + mapData.averagePrecipitation) / 2f : tile.precipitation),0f,1f);
			}
		}

		void AverageTilePrecipitations() {
			for (int i = 0; i < 3; i++) {
				List<float> averageTilePrecipitations = new List<float>();

				foreach (Tile tile in tiles) {
					float averagePrecipitation = tile.precipitation;
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						if (nTile != null) {
							numValidTiles += 1;
							averagePrecipitation += nTile.precipitation;
						}
					}
					averagePrecipitation /= numValidTiles;
					averageTilePrecipitations.Add(averagePrecipitation);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].precipitation = averageTilePrecipitations[k];
				}
			}
		}

		public float TemperatureFromMapLatitude(float yPos,float temperatureSteepness,float temperatureOffset,int mapSize) {
			return ((-2 * Mathf.Abs((yPos - (mapSize / 2f)) / ((mapSize / 100f) / (temperatureSteepness / 50f)))) + temperatureSteepness) + temperatureOffset;
		}

		void CalculateTemperature() {

			float temperatureSteepness = 70; // 50

			foreach (Tile tile in tiles) {
				if (mapData.planetTemperature) {
					tile.temperature = TemperatureFromMapLatitude(tile.position.y,temperatureSteepness,mapData.temperatureOffset,mapData.mapSize);
				} else {
					tile.temperature = mapData.averageTemperature;
				}
				tile.temperature += -(100f * Mathf.Pow(tile.height - 0.5f,3));
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

		void SetBiomes() {
			foreach (Tile tile in tiles) {
				bool next = false;
				foreach (PrecipitationRange precipitationRange in tileM.biomeRanges) {
					if (tile.precipitation >= precipitationRange.min && tile.precipitation < precipitationRange.max) {
						foreach (PrecipitationRange.TemperatureRange temperatureRange in precipitationRange.temperatureRanges) {
							if (tile.temperature >= temperatureRange.min && tile.temperature < temperatureRange.max) {
								tile.SetBiome(temperatureRange.biome);
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

		public List<River> rivers = new List<River>();

		public Dictionary<Region,Tile> drainageBasins = new Dictionary<Region,Tile>();
		public int drainageBasinID = 0;

		void DetermineDrainageBasins() {
			List<Tile> tilesByHeight = tiles.OrderBy(tile => tile.height).ToList();
			foreach (Tile tile in tilesByHeight) {
				if (!tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type) && tile.drainageBasin == null) {
					Region drainageBasin = new Region(null,drainageBasinID);
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
					drainageBasins.Add(drainageBasin,tile);
				}
			}
		}

		public class River {
			public Tile startTile;
			public Tile endTile;
			public List<Tile> tiles;

			public River(Tile startTile,Tile endTile,List<Tile> tiles) {
				this.startTile = startTile;
				this.endTile = endTile;
				this.tiles = tiles;
			}
		}

		void CreateRivers() {
			Dictionary<Tile,Tile> riverStartTiles = new Dictionary<Tile,Tile>();
			foreach (KeyValuePair<Region,Tile> kvp in drainageBasins) {
				Region drainageBasin = kvp.Key;
				if (drainageBasin.tiles.Find(o => tileM.WaterEquivalentTileTypes.Contains(o.tileType.type)) != null && drainageBasin.tiles.Find(o => o.horizontalSurroundingTiles.Find(o2 => o2 != null && tileM.StoneEquivalentTileTypes.Contains(o2.tileType.type)) != null) != null) {
					foreach (Tile tile in drainageBasin.tiles) {
						if (tile.walkable && !tileM.WaterEquivalentTileTypes.Contains(tile.tileType.type) && tile.horizontalSurroundingTiles.Find(o => o != null && tileM.StoneEquivalentTileTypes.Contains(o.tileType.type)) != null) {
							riverStartTiles.Add(tile,kvp.Value);
						}
					}
				}
			}
			for (int i = 0; i < mapData.mapSize / 10f && i < riverStartTiles.Count; i++) {
				Tile riverStartTile = Enumerable.ToList(riverStartTiles.Keys)[Random.Range(0,riverStartTiles.Count)];
				Tile riverEndTile = riverStartTiles[riverStartTile];
				List<Tile> removeTiles = new List<Tile>();
				foreach (KeyValuePair<Tile,Tile> kvp in riverStartTiles) {
					if (Vector2.Distance(kvp.Key.obj.transform.position,riverStartTile.obj.transform.position) < 5f) {
						removeTiles.Add(kvp.Key);
					}
				}
				foreach (Tile removeTile in removeTiles) {
					riverStartTiles.Remove(removeTile);
				}
				removeTiles.Clear();

				List<Tile> riverTiles = RiverPathfinding(riverStartTile,riverEndTile);

				rivers.Add(new River(riverStartTile,riverEndTile,riverTiles));
			}
		}

		public List<Tile> RiverPathfinding(Tile riverStartTile,Tile riverEndTile) {
			PathManager.PathfindingTile currentTile = new PathManager.PathfindingTile(riverStartTile,null,0);

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
						currentTile.tile.SetTileType(currentTile.tile.biome.waterType,false,false,false,true);
						if (!expandRiver) {
							KeyValuePair<Tile,River> kvp = RiversContainTile(currentTile.tile);
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
								expandTile.SetTileType(expandTile.biome.waterType,false,false,false,true);
								foreach (Tile nTile in expandTile.surroundingTiles) {
									if (nTile != null && !checkedExpandTiles.Contains(nTile) && !tileM.StoneEquivalentTileTypes.Contains(nTile.tileType.type) && Vector2.Distance(nTile.obj.transform.position,riverTile.obj.transform.position) <= maxExpandRadius) {
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
							frontier.Add(new PathManager.PathfindingTile(nTile,currentTile,0));
							nTile.SetTileType(nTile.biome.waterType,false,false,false,true);
							break;
						}
						float cost = Vector2.Distance(nTile.obj.transform.position,riverEndTile.obj.transform.position) + (nTile.height * (mapData.mapSize / 10f)) + Random.Range(0,10);
						PathManager.PathfindingTile pTile = new PathManager.PathfindingTile(nTile,currentTile,cost);
						frontier.Add(pTile);
						checkedTiles.Add(pTile);
					}
				}
				frontier = frontier.OrderBy(frontierTile => frontierTile.cost).ToList();
			}
			return river;
		}

		KeyValuePair<Tile,River> RiversContainTile(Tile tile) {
			foreach (River river in rivers) {
				foreach (Tile riverTile in river.tiles) {
					if (riverTile == tile) {
						return new KeyValuePair<Tile,River>(riverTile,river);
					}
				}
			}
			return new KeyValuePair<Tile,River>(null,null);
		}

		Dictionary<int,int> bitmaskMap = new Dictionary<int,int>() {
		{ 19,16 },{ 23,17 },{ 27,18 },{ 31,19 },{ 38,20 },{ 39,21 },{ 46,22 },
		{ 47,23 },{ 55,24 },{ 63,25 },{ 76,26 },{ 77,27 },{ 78,28 },{ 79,29 },
		{ 95,30 },{ 110,31 },{ 111,32 },{ 127,33 },{ 137,34 },{ 139,35 },{ 141,36 },
		{ 143,37 },{ 155,38 },{ 159,39 },{ 175,40 },{ 191,41 },{ 205,42 },{ 207,43 },
		{ 223,44 },{ 239,45 },{ 255,46 }
	};
		Dictionary<int,List<int>> diagonalCheckMap = new Dictionary<int,List<int>>() {
		{4,new List<int>() {0,1 } },
		{5,new List<int>() {1,2 } },
		{6,new List<int>() {2,3 } },
		{7,new List<int>() {3,0 } }
	};

		int BitSum(List<TileTypes> compareTileTypes,List<Tile> tilesToSum,bool includeMapEdge) {
			int sum = 0;
			for (int i = 0; i < tilesToSum.Count; i++) {
				if (tilesToSum[i] != null) {
					if (compareTileTypes.Contains(tilesToSum[i].tileType.type)) {
						bool ignoreTile = false;
						if (compareTileTypes.Contains(tilesToSum[i].tileType.type) && diagonalCheckMap.ContainsKey(i)) {
							List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]],tilesToSum[diagonalCheckMap[i][1]] };
							List<Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && compareTileTypes.Contains(tile.tileType.type)).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							sum += Mathf.RoundToInt(Mathf.Pow(2,i));
						}
					}
				} else if (includeMapEdge) {
					if (tilesToSum.Find(tile => tile != null && tilesToSum.IndexOf(tile) <= 3 && !compareTileTypes.Contains(tile.tileType.type)) == null) {
						sum += Mathf.RoundToInt(Mathf.Pow(2,i));
					} else {
						if (i <= 3) {
							sum += Mathf.RoundToInt(Mathf.Pow(2,i));
						} else {
							List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]],tilesToSum[diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)) == null) {
								sum += Mathf.RoundToInt(Mathf.Pow(2,i));
							}
						}
					}
				}
			}
			return sum;
		}

		void BitmaskTile(Tile tile,bool includeDiagonalSurroundingTiles,bool customBitSumInputs,List<TileTypes> customCompareTileTypes,bool includeMapEdge) {
			int sum = 0;
			if (customBitSumInputs) {
				sum = BitSum(customCompareTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles),includeMapEdge);
			} else {
				if (RiversContainTile(tile).Key != null) {
					sum = BitSum(tileM.WaterEquivalentTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles),false);
				} else if (tileM.WaterEquivalentTileTypes.Contains(tile.tileType.type)) {
					sum = BitSum(tileM.WaterEquivalentTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles),includeMapEdge);
				} else if (tileM.StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
					sum = BitSum(tileM.StoneEquivalentTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles),includeMapEdge);
				} else {
					sum = BitSum(new List<TileTypes>() { tile.tileType.type },(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles),includeMapEdge);
				}
			}
			if ((sum < 16) || (bitmaskMap[sum] != 46)) {
				if (sum >= 16) {
					if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type) && RiversContainTile(tile).Key != null) {
						tile.sr.sprite = tile.tileType.riverSprites[bitmaskMap[sum]];
					} else {
						tile.sr.sprite = tile.tileType.bitmaskSprites[bitmaskMap[sum]];
					}
				} else {
					if (tileM.LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type) && RiversContainTile(tile).Key != null) {
						tile.sr.sprite = tile.tileType.riverSprites[sum];
					} else {
						tile.sr.sprite = tile.tileType.bitmaskSprites[sum];
					}
				}
			} else {
				if (!tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
					tile.sr.sprite = tile.tileType.baseSprites[Random.Range(0,tile.tileType.baseSprites.Count)];
				}
			}
		}

		public void Bitmasking(List<Tile> tilesToBitmask) {
			foreach (Tile tile in tilesToBitmask) {
				if (tile != null) {
					if (tileM.BitmaskingTileTypes.Contains(tile.tileType.type)) {
						BitmaskTile(tile,true,false,null,true);
					} else {
						if (!tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
							tile.sr.sprite = tile.tileType.baseSprites[Random.Range(0,tile.tileType.baseSprites.Count)];
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
				BitmaskTile(river.tiles[river.tiles.Count - 1],false,true,compareTileTypes,false);
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
						if (Vector2.Distance(currentRegionBlock.averagePosition,cameraM.cameraGO.transform.position) <= cameraM.cameraComponent.orthographicSize * ((float)Screen.width / Screen.height)) {
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
					tile.SetColour(newColour,Mathf.FloorToInt(time));
				}
			}
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				colonist.SetColour(colonist.overTile.sr.color);
			}
		}

		public Color GetTileColourAtHour(float time) {
			float r = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.4f * time + 7.2f),10)) / 5f,0f,1f);
			float g = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.5f * time + 6),10)) / 5f - 0.2f,0f,1f);
			float b = Mathf.Clamp((-1.5f * Mathf.Pow(Mathf.Cos((CalculateBrightnessLevelAtHour(2 * time + 12)) / 1.5f),3) + 1.65f * (CalculateBrightnessLevelAtHour(time) / 2f)) + 0.7f,0f,1f);
			return new Color(r,g,b,1f);
		}

		public float CalculateBrightnessLevelAtHour(float time) {
			return ((-(1f / 144f)) * Mathf.Pow(((1 + (24 - (1 - time))) % 24) - 12,2) + 1.2f);
		}

		public void DetermineShadowTiles(List<Tile> tilesToInclude) {
			
			List<Tile> shadowStartTiles = new List<Tile>();
			foreach (Tile tile in tilesToInclude) {
				if (!tile.walkable && tile.surroundingTiles.Find(nTile => nTile != null && nTile.walkable) != null) {
					shadowStartTiles.Add(tile);
				}
			}
			for (int h = 0; h < 24; h++) {
				foreach (Tile tile in shadowStartTiles) {
					tile.tilesAffectingBrightnessOfThisTile.Clear();
					tile.tilesBrightnessBeingAffectedByThisTile.Clear();
				}
			}
			List<Vector2> hourDirections = new List<Vector2>();
			for (int h = 0; h < 24; h++) {
				float hShadow = -Mathf.Abs(mapData.equatorOffset) * (-(h / 12f) + 1);
				float vShadow = -(mapData.equatorOffset / 144f) * Mathf.Pow(h - 12,2) + mapData.equatorOffset;
				Vector2 hourDirection = new Vector2(hShadow,vShadow);
				hourDirections.Add(hourDirection);

				foreach (Tile tile in shadowStartTiles) {

					Vector2 tilePosition = tile.obj.transform.position;

					float oppositeTileMaxHeight = 0;
					float oppositeDistance = 0;
					Tile oppositeTile = tile;
					while (oppositeTile != null && !oppositeTile.walkable) {
						if (oppositeTile.height >= oppositeTileMaxHeight) {
							oppositeTileMaxHeight = oppositeTile.height;
						}
						Tile newOppositeTile = oppositeTile;
						int sameCounter = 0;
						while (newOppositeTile == oppositeTile) {
							oppositeDistance += 0.25f;
							newOppositeTile = GetTileFromPosition(tilePosition + ((-hourDirection.normalized) * oppositeDistance));
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
					float maxDistance = hourDirection.magnitude * heightModifer * 5f + (Mathf.Pow(h - 12,2) / 6f);

					List<Tile> shadowTiles = new List<Tile>();
					for (float distance = 0; distance <= maxDistance; distance += 0.25f) {
						Vector2 nextTilePosition = tilePosition + (hourDirection.normalized * distance);
						if (nextTilePosition.x < 0 || nextTilePosition.x >= mapData.mapSize || nextTilePosition.y < 0 || nextTilePosition.y >= mapData.mapSize) {
							break;
						}
						Tile shadowTile = GetTileFromPosition(nextTilePosition);
						if (shadowTiles.Contains(shadowTile)) {
							distance += 0.25f;
							continue;
						}
						if (shadowTile != tile) {
							float newBrightness = 1;
							if ((shadowTile.tileType.walkable && shadowTile.GetAllObjectInstances().Find(instance => !instance.prefab.walkable) != null ? true : shadowTile.walkable)) {
								newBrightness = Mathf.Clamp((1 - (0.6f * CalculateBrightnessLevelAtHour(h)) + 0.3f),0,1);
								if (shadowTile.brightnessAtHour.ContainsKey(h)) {
									shadowTile.brightnessAtHour[h] = Mathf.Min(shadowTile.brightnessAtHour[h],newBrightness);
								} else {
									shadowTile.brightnessAtHour.Add(h,newBrightness);
								}
								shadowTiles.Add(shadowTile);
							}
							if (shadowTile.tilesAffectingBrightnessOfThisTile.ContainsKey(h)) {
								if (shadowTile.tilesAffectingBrightnessOfThisTile[h].ContainsKey(tile)) {
									shadowTile.tilesAffectingBrightnessOfThisTile[h][tile] = newBrightness;
								} else {
									shadowTile.tilesAffectingBrightnessOfThisTile[h].Add(tile,newBrightness);
								}
							} else {
								shadowTile.tilesAffectingBrightnessOfThisTile.Add(h,new Dictionary<Tile,float>() { { tile,newBrightness } });
							}
							if (tile.tilesBrightnessBeingAffectedByThisTile.ContainsKey(h)) {
								if (tile.tilesBrightnessBeingAffectedByThisTile[h].ContainsKey(shadowTile)) {
									tile.tilesBrightnessBeingAffectedByThisTile[h][shadowTile] = newBrightness;
								} else {
									tile.tilesBrightnessBeingAffectedByThisTile[h].Add(shadowTile,newBrightness);
								}
							} else {
								tile.tilesBrightnessBeingAffectedByThisTile.Add(h,new Dictionary<Tile,float>() { { shadowTile,newBrightness } });
							}
						}
					}
				}
			}
			SetTileBrightness(timeM.GetTileBrightnessTime());
		}

		public void RemoveTileBrightnessEffect(Tile tile) {
			print("A " + tile.tilesBrightnessBeingAffectedByThisTile.Count);
			foreach (KeyValuePair<int,Dictionary<Tile,float>> affectingTilesKVP in tile.tilesBrightnessBeingAffectedByThisTile) {
				int h = affectingTilesKVP.Key;
				print("B " + h + " " + affectingTilesKVP.Value.Keys.Count);
				foreach (Tile affectingTile in affectingTilesKVP.Value.Keys) {
					print("C " + affectingTile.tilesAffectingBrightnessOfThisTile.Count);
					foreach (KeyValuePair<int,Dictionary<Tile,float>> brightnessFromOtherTilesKVP in affectingTile.tilesAffectingBrightnessOfThisTile) {
						float minBrightness = 1f;
						List<KeyValuePair<Tile,float>> validTiles = affectingTilesKVP.Value.Where(tileBrightness => tileBrightness.Key != tile).ToList();
						if (validTiles.Count > 0) {
							minBrightness = validTiles.Min(tileBrightness => tileBrightness.Value);
						}
						if (affectingTile.brightnessAtHour.ContainsKey(h)) {
							affectingTile.brightnessAtHour[h] = minBrightness;
						} else {
							affectingTile.brightnessAtHour.Add(h,minBrightness);
						}
						if (affectingTile.tilesAffectingBrightnessOfThisTile.ContainsKey(h)) {
							affectingTile.tilesAffectingBrightnessOfThisTile[h].Remove(tile);
						}
						affectingTile.SetColour(Color.black,12);
					}
				}
			}
			foreach (KeyValuePair<int,Dictionary<Tile,float>> tilesAffectingBrightnessOfTile in tile.tilesAffectingBrightnessOfThisTile) {
				DetermineShadowTiles(tilesAffectingBrightnessOfTile.Value.Keys.ToList());
			}
			//DetermineShadowTiles(tile.surroundingTiles);
		}

		public Tile GetTileFromPosition(Vector2 position) {
			position = new Vector2(Mathf.Clamp(position.x,0,mapData.mapSize - 1),Mathf.Clamp(position.y,0,mapData.mapSize - 1));
			return sortedTiles[Mathf.FloorToInt(position.y)][Mathf.FloorToInt(position.x)];
		}
	}
}