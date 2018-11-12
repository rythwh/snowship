﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : BaseManager {

	public static string GetRandomPlanetName() {
		return "Planet";
	}

	public static int GetRandomPlanetSeed() {
		return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}

	private static readonly List<int> planetSizes = new List<int>() { 10, 15, 20, 25, 30, 40, 50, 60, 75, 100, 120 }; // Some divisors of 600 (600px = width of planet preview)

	public static int GetPlanetSizeByIndex(int index) {
		return planetSizes[index];
	}

	public static int GetNumPlanetSizes() {
		return planetSizes.Count;
	}

	public static float GetPlanetDistanceByIndex(int index) {
		return (float)Math.Round(0.1f * (index + 6), 1);
	}

	public static int GetMinPlanetDistance() {
		return 1;
	}

	public static int GetMaxPlanetDistance() {
		return 7;
	}

	public static int GetTemperatureRangeByIndex(int index) {
		return index * 10;
	}

	private static readonly List<int> windCircularDirectionMap = new List<int>() { 0, 4, 1, 5, 2, 6, 3, 7 };

	public static int GetWindCircularDirectionByIndex(int index) {
		return windCircularDirectionMap[index];
	}

	private static readonly List<string> windCardinalDirectionMap = new List<string>() { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

	public static string GetWindCardinalDirectionByIndex(int index) {
		return windCardinalDirectionMap[index];
	}

	public static int GetNumWindDirections() {
		if (windCircularDirectionMap.Count != windCardinalDirectionMap.Count) {
			Debug.LogError("windCircularDirectionMap.Count != windCardinalDirectionMap.Count");
		}
		return windCircularDirectionMap.Count;
	}

	public static class StaticPlanetMapDataValues {
		public static bool actualMap = false;
		public static bool planetTemperature = true;
		public static float averageTemperature = -1;
		public static float averagePrecipitation = -1;
		public static readonly Dictionary<TileManager.TileTypes, float> terrainTypeHeights = new Dictionary<TileManager.TileTypes, float> {
			{ TileManager.TileTypes.GrassWater, 0.40f },
			{ TileManager.TileTypes.Stone, 0.75f }
		};
		public static List<int> surroundingPlanetTileHeightDirections = null;
		public static bool river = false;
		public static List<int> surroundingPlanetTileRivers = null;
		public static bool preventEdgeTouching = true;
		public static Vector2 planetTilePosition = Vector2.zero;
	}

	public static int CalculatePlanetTemperature(float distance) {

		float starMass = 1; // 1 (lower = colder)
		float albedo = 29; // 29 (higher = colder)
		float greenhouse = 0.4f; // 1 (lower = colder)

		float sigma = 5.6703f * Mathf.Pow(10, -5);
		float L = 3.846f * Mathf.Pow(10, 33) * Mathf.Pow(starMass, 3);
		float D = distance * 1.496f * Mathf.Pow(10, 13);
		float A = albedo / 100f;
		float T = greenhouse * 0.5841f;
		float X = Mathf.Sqrt((1 - A) * L / (16 * Mathf.PI * sigma));
		float T_eff = Mathf.Sqrt(X) * (1 / Mathf.Sqrt(D));
		float T_eq = (Mathf.Pow(T_eff, 4)) * (1 + (3 * T / 4));
		float T_sur = T_eq / 0.9f;
		float T_kel = Mathf.Round(Mathf.Sqrt(Mathf.Sqrt(T_sur)));
		int celsius = Mathf.RoundToInt(T_kel - 273);

		return celsius;
	}

	public Planet CreatePlanet(string name, int seed, int size, float distance, int temperatureRange, bool randomOffsets, int windDirection) {
		Planet planet = new Planet(
			name,
			new TileManager.MapData(
				null,
				seed,
				size,
				StaticPlanetMapDataValues.actualMap,
				StaticPlanetMapDataValues.planetTemperature,
				temperatureRange,
				distance,
				randomOffsets,
				StaticPlanetMapDataValues.averageTemperature,
				StaticPlanetMapDataValues.averagePrecipitation,
				StaticPlanetMapDataValues.terrainTypeHeights,
				StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
				StaticPlanetMapDataValues.river,
				StaticPlanetMapDataValues.surroundingPlanetTileRivers,
				StaticPlanetMapDataValues.preventEdgeTouching,
				windDirection,
				StaticPlanetMapDataValues.planetTilePosition
			)
		);

		this.planet = planet;

		return planet;
	}

	public Planet planet = null;

	public void SetPlanet(Planet planet) {
		this.planet = planet;
	}

	public List<Planet> planets = new List<Planet>();

	public class Planet : TileManager.Map {

		public string directory;
		public string lastSaveDateTime;

		public string name;
		public List<PlanetTile> planetTiles;

		public string regenerationCode;

		public Planet(string name, TileManager.MapData mapData) : base(mapData) {

			this.name = name;

			planetTiles = new List<PlanetTile>();
			foreach (TileManager.Tile tile in tiles) {
				planetTiles.Add(new PlanetTile(this, tile));
			}

			regenerationCode = string.Format(
				"{0}{1}{2}{3}{4}{5}",
				mapData.mapSeed.ToString().PadLeft(20, '0'),
				mapData.mapSize.ToString().PadLeft(3, '0'),
				mapData.planetDistance.ToString().PadLeft(2, '0'),
				mapData.temperatureRange.ToString().PadLeft(3, '0'),
				mapData.randomOffsets ? "1" : "0",
				mapData.primaryWindDirection.ToString().PadLeft(2, '0')
			);

			lastSaveDateTime = PersistenceManager.GenerateSaveDateTimeString();
		}

		public void SetDirectory(string directory) {
			this.directory = directory;
		}

		public void SetLastSaveDateTime(string lastSaveDateTime) {
			this.lastSaveDateTime = lastSaveDateTime;
		}

		public class PlanetTile {
			public Planet planet;
			public TileManager.Tile tile;

			public Sprite sprite;

			public float equatorOffset;
			public float averageTemperature;
			public bool isRiver;
			public Dictionary<TileManager.TileTypes, float> terrainTypeHeights;
			public List<int> surroundingPlanetTileHeightDirections = new List<int>();
			public List<int> surroundingPlanetTileRivers = new List<int>();

			public bool settleable = true;

			public string altitude;

			public PlanetTile(Planet planet, TileManager.Tile tile) {
				this.planet = planet;
				this.tile = tile;

				sprite = tile.sr.sprite;

				// Setup PlanetTile-specific Information
				equatorOffset = ((tile.position.y - (planet.mapData.mapSize / 2f)) * 2) / planet.mapData.mapSize;
				averageTemperature = tile.temperature + planet.mapData.temperatureOffset;

				River river = planet.RiversContainTile(tile, true).Value;
				isRiver = river != null;

				if (isRiver || !TileManager.waterEquivalentTileTypes.Contains(tile.tileType.type)) {
					foreach (TileManager.Tile sTile in tile.horizontalSurroundingTiles) {
						if (sTile != null) {
							if (planet.rivers.Find(r => r.tiles.Contains(sTile)) == null) {
								if (TileManager.waterEquivalentTileTypes.Contains(sTile.tileType.type)) {
									surroundingPlanetTileHeightDirections.Add(-2);
								} else if (TileManager.stoneEquivalentTileTypes.Contains(sTile.tileType.type)) {
									surroundingPlanetTileHeightDirections.Add(5);
								} else {
									surroundingPlanetTileHeightDirections.Add(0);
								}
							} else {
								surroundingPlanetTileHeightDirections.Add(0);
							}
							if (isRiver) {
								int nTileRiverIndex = river.tiles.IndexOf(sTile);
								if (nTileRiverIndex == -1) {
									foreach (River r in planet.rivers) {
										if (r != river) {
											if (r.tiles.Contains(sTile)) {
												nTileRiverIndex = r.tiles.IndexOf(sTile);
											}
										}
									}
								}
								if (nTileRiverIndex == -1) {
									if (river.startTile == tile && TileManager.stoneEquivalentTileTypes.Contains(sTile.tileType.type)) {
										nTileRiverIndex = 0;
									} else if (river.endTile == tile && TileManager.waterEquivalentTileTypes.Contains(sTile.tileType.type)) {
										nTileRiverIndex = int.MaxValue;
									}
								}
								surroundingPlanetTileRivers.Add(nTileRiverIndex);
							} else {
								surroundingPlanetTileRivers.Add(-1);
							}
						} else {
							surroundingPlanetTileHeightDirections.Add(0);
							surroundingPlanetTileRivers.Add(-1);
						}
					}
				} else {
					settleable = false;
				}

				terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>() {
					{ TileManager.TileTypes.GrassWater, 0.40f * tile.GetPrecipitation() * (1 - tile.height) },
					{ TileManager.TileTypes.Stone, 0.75f * (1 - (tile.height - (1 - 0.75f))) }
				};

				altitude = Mathf.RoundToInt((tile.height - terrainTypeHeights[TileManager.TileTypes.GrassWater]) * 5000f) + "m";

				// Remove Tile-specific Information
				MonoBehaviour.Destroy(tile.obj);
				tile.obj = null;
				tile.sr = null;
			}
		}
	}

	public Planet.PlanetTile selectedPlanetTile = null;

	public void SetSelectedPlanetTile(Planet.PlanetTile planetTile) {
		selectedPlanetTile = planetTile;
	}

	//public int planetTileSize = 0;
	//public float planetDistance = 0;
	//public int temperatureRange = 0;
	//public bool randomOffsets = false;
	//public int windDirection = 0;
	//public int mapSize = 0;

	//private bool createdPlanet = false;

	//void CreateNewGamePlanet() {
	//	createdPlanet = true;

	//	GeneratePlanet();
	//	GameManager.uiM.SetSelectedPlanetTileInfo();
	//}

	//public PlanetTile selectedPlanetTile;

	//public void SetSelectedPlanetTile(PlanetTile selectedPlanetTile) {
	//	if (selectedPlanetTile == null || planet.rivers.Find(river => river.tiles.Contains(selectedPlanetTile.tile)) != null || !TileManager.waterEquivalentTileTypes.Contains(selectedPlanetTile.tile.tileType.type)) {
	//		this.selectedPlanetTile = selectedPlanetTile;
	//		GameManager.uiM.SetSelectedPlanetTileInfo();
	//	}
	//}

	//public List<PlanetTile> planetTiles = new List<PlanetTile>();

	//public class PlanetTile {
	//	public TileManager.Tile tile;
	//	public GameObject obj;

	//	public Image image;

	//	public Vector2 position;

	//	public TileManager.MapData data;
	//	private int planetSize;
	//	private float planetTemperature;

	//	public float equatorOffset;
	//	public float averageTemperature;
	//	public float averagePrecipitation;
	//	public Dictionary<TileManager.TileTypes, float> terrainTypeHeights;
	//	public List<int> surroundingPlanetTileHeightDirections = new List<int>();
	//	public bool river;
	//	public List<int> surroundingPlanetTileRivers = new List<int>();

	//	public PlanetTile(TileManager.Tile tile, Transform parent, Vector2 position, int planetSize, float planetTemperature) {

	//		this.tile = tile;

	//		this.position = position;

	//		this.planetSize = planetSize;
	//		this.planetTemperature = planetTemperature;

	//		obj = MonoBehaviour.Instantiate(GameManager.resourceM.planetTilePrefab, parent, false);
	//		obj.name = "Planet Tile: " + position;
	//		image = obj.GetComponent<Image>();
	//		image.sprite = tile.obj.GetComponent<SpriteRenderer>().sprite;

	//		obj.GetComponent<Button>().onClick.AddListener(delegate {
	//			GameManager.planetM.SetSelectedPlanetTile(this);
	//		});

	//		SetMapData();

	//		MonoBehaviour.Destroy(tile.obj);
	//	}

	//	public void SetMapData() {
	//		equatorOffset = ((position.y - (planetSize / 2f)) * 2) / planetSize;
	//		averageTemperature = tile.temperature + planetTemperature;
	//		averagePrecipitation = tile.GetPrecipitation();

	//		TileManager.Map.River tileRiver = tile.map.rivers.Find(river => river.tiles.Contains(tile));
	//		river = tileRiver != null;

	//		if (river || !TileManager.waterEquivalentTileTypes.Contains(tile.tileType.type)) {
	//			foreach (TileManager.Tile nTile in tile.horizontalSurroundingTiles) {
	//				if (nTile != null) {
	//					if (tile.map.rivers.Find(river => river.tiles.Contains(nTile)) == null) {
	//						if (TileManager.waterEquivalentTileTypes.Contains(nTile.tileType.type)) {
	//							surroundingPlanetTileHeightDirections.Add(-2);
	//						} else if (TileManager.stoneEquivalentTileTypes.Contains(nTile.tileType.type)) {
	//							surroundingPlanetTileHeightDirections.Add(5);
	//						} else {
	//							surroundingPlanetTileHeightDirections.Add(0);
	//						}
	//					} else {
	//						surroundingPlanetTileHeightDirections.Add(0);
	//					}
	//					if (river) {
	//						int nTileRiverIndex = tileRiver.tiles.IndexOf(nTile);
	//						if (nTileRiverIndex == -1) {
	//							foreach (TileManager.Map.River river in tile.map.rivers) {
	//								if (river != tileRiver) {
	//									if (river.tiles.Contains(nTile)) {
	//										nTileRiverIndex = river.tiles.IndexOf(nTile);
	//									}
	//								}
	//							}
	//						}
	//						if (nTileRiverIndex == -1) {
	//							if (tileRiver.startTile == tile && TileManager.stoneEquivalentTileTypes.Contains(nTile.tileType.type)) {
	//								nTileRiverIndex = 0;
	//							} else if (tileRiver.endTile == tile && TileManager.waterEquivalentTileTypes.Contains(nTile.tileType.type)) {
	//								nTileRiverIndex = int.MaxValue;
	//							}
	//						}
	//						surroundingPlanetTileRivers.Add(nTileRiverIndex);
	//					}
	//				} else {
	//					surroundingPlanetTileHeightDirections.Add(0);
	//					surroundingPlanetTileRivers.Add(-1);
	//				}
	//			}
	//		} else {
	//			obj.GetComponent<Button>().interactable = false;
	//		}

	//		float waterThreshold = 0.40f;
	//		float stoneThreshold = 0.75f;
	//		waterThreshold = waterThreshold * tile.GetPrecipitation() * (1 - tile.height);
	//		stoneThreshold = stoneThreshold * (1 - (tile.height - (1 - stoneThreshold)));

	//		terrainTypeHeights = new Dictionary<TileManager.TileTypes, float>() {
	//			{ TileManager.TileTypes.GrassWater,waterThreshold},{ TileManager.TileTypes.Stone,stoneThreshold }
	//		};
	//	}
	//}

	//public int CalculatePlanetTemperature(float distance) {

	//	float starMass = 1; // 1 (lower = colder)
	//	float albedo = 29; // 29 (higher = colder)
	//	float greenhouse = 0.4f; // 1 (lower = colder)

	//	float sigma = 5.6703f * Mathf.Pow(10, -5);
	//	float L = 3.846f * Mathf.Pow(10, 33) * Mathf.Pow(starMass, 3);
	//	float D = distance * 1.496f * Mathf.Pow(10, 13);
	//	float A = albedo / 100f;
	//	float T = greenhouse * 0.5841f;
	//	float X = Mathf.Sqrt((1 - A) * L / (16 * Mathf.PI * sigma));
	//	float T_eff = Mathf.Sqrt(X) * (1 / Mathf.Sqrt(D));
	//	float T_eq = (Mathf.Pow(T_eff, 4)) * (1 + (3 * T / 4));
	//	float T_sur = T_eq / 0.9f;
	//	float T_kel = Mathf.Round(Mathf.Sqrt(Mathf.Sqrt(T_sur)));
	//	int celsius = Mathf.RoundToInt(T_kel - 273);

	//	return celsius;
	//}

	//public TileManager.Map planet;
	//private static readonly List<int> planetTileSizes = new List<int>() { 20, 15, 12, 10, 8, 6, 5 }; // Some divisors of 600

	//public static class StaticPlanetMapDataValues {
	//	public static bool actualMap = false;
	//	public static float equatorOffset = -1;
	//	public static bool planetTemperature = true;
	//	public static float averageTemperature = -1;
	//	public static float averagePrecipitation = -1;
	//	public static readonly Dictionary<TileManager.TileTypes, float> terrainTypeHeights = new Dictionary<TileManager.TileTypes, float> {
	//		{ TileManager.TileTypes.GrassWater, 0.40f },
	//		{ TileManager.TileTypes.Stone, 0.75f }
	//	};
	//	public static List<int> surroundingPlanetTileHeightDirections = null;
	//	public static bool river = false;
	//	public static List<int> surroundingPlanetTileRivers = null;
	//	public static bool preventEdgeTouching = true;
	//	public static Vector2 planetTilePosition = Vector2.zero;
	//}

	//public void GeneratePlanet() {
	//	SetSelectedPlanetTile(null);

	//	foreach (PlanetTile tile in planetTiles) {
	//		MonoBehaviour.Destroy(tile.obj);
	//	}
	//	planetTiles.Clear();

	//	int planetSeed = SeedParser(planetSeedInputField.text, planetSeedInputField);

	//	int planetSize = Mathf.FloorToInt(Mathf.FloorToInt(planetPreviewPanel.GetComponent<RectTransform>().sizeDelta.x) / planetTileSize);

	//	int planetTemperature = CalculatePlanetTemperature(planetDistance);

	//	planetPreviewPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(planetTileSize, planetTileSize);
	//	planetPreviewPanel.GetComponent<GridLayoutGroup>().constraintCount = planetSize;

	//	TileManager.MapData planetData = new TileManager.MapData(
	//		null,
	//		planetSeed,
	//		planetSize,
	//		StaticPlanetMapDataValues.actualMap,
	//		StaticPlanetMapDataValues.equatorOffset,
	//		StaticPlanetMapDataValues.planetTemperature,
	//		temperatureRange,
	//		planetDistance,
	//		planetTemperature,
	//		randomOffsets,
	//		StaticPlanetMapDataValues.averageTemperature,
	//		StaticPlanetMapDataValues.averagePrecipitation,
	//		StaticPlanetMapDataValues.terrainTypeHeights,
	//		StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
	//		StaticPlanetMapDataValues.river,
	//		StaticPlanetMapDataValues.surroundingPlanetTileRivers,
	//		StaticPlanetMapDataValues.preventEdgeTouching,
	//		windDirection,
	//		StaticPlanetMapDataValues.planetTilePosition
	//	);
	//	planet = new TileManager.Map(planetData, false);
	//	foreach (TileManager.Tile tile in planet.tiles) {
	//		planetTiles.Add(new PlanetTile(tile, planetPreviewPanel.transform, tile.position, planetSize, planetTemperature));
	//	}
	//}

	//private static readonly List<string> windCardinalDirectionMap = new List<string>() {
	//	"N",
	//	"E",
	//	"S",
	//	"W",
	//	"NE",
	//	"SE",
	//	"SW",
	//	"NW",
	//};
	//private static readonly List<int> windCircularDirectionMap = new List<int>() {
	//	0,
	//	4,
	//	1,
	//	5,
	//	2,
	//	6,
	//	3,
	//	7,
	//};

	//public void UpdatePlanetInfo() {
	//	planetTileSize = planetTileSizes[Mathf.RoundToInt(planetSizeSlider.value)];
	//	planetSizeText.text = Mathf.FloorToInt(Mathf.FloorToInt(planetPreviewPanel.GetComponent<RectTransform>().sizeDelta.x) / planetTileSize).ToString();

	//	planetDistance = (float)Math.Round(0.1f * (planetDistanceSlider.value + 6), 1);
	//	planetDistanceText.text = planetDistance + " AU";

	//	temperatureRange = Mathf.RoundToInt(temperatureRangeSlider.value * 10);
	//	temperatureRangeText.text = temperatureRange + "°C";

	//	randomOffsets = randomOffsetsToggle.isOn;

	//	windDirection = windCircularDirectionMap[Mathf.RoundToInt(windDirectionSlider.value)];
	//	windDirectionText.text = windCardinalDirectionMap[windDirection];
	//}
}