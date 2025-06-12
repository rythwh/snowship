using System.Collections.Generic;
using Snowship.NPlanet;
using UnityEngine;

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
	public Dictionary<TileTypeGroup.TypeEnum, float> terrainTypeHeights;
	public List<int> surroundingPlanetTileHeightDirections;
	public bool isRiver;
	public List<int> surroundingPlanetTileRivers;
	public Vector2 planetTilePosition;

	public bool preventEdgeTouching;

	public int primaryWindDirection = -1;

	public string mapRegenerationCode = string.Empty;

	public MapData(
		MapData planetMapData,
		int mapSeed,
		int mapSize,
		bool actualMap,
		bool planetTemperature,
		int temperatureRange,
		float planetDistance,
		float averageTemperature,
		float averagePrecipitation,
		Dictionary<TileTypeGroup.TypeEnum, float> terrainTypeHeights,
		List<int> surroundingPlanetTileHeightDirections,
		bool isRiver,
		List<int> surroundingPlanetTileRivers,
		bool preventEdgeTouching,
		int primaryWindDirection,
		Vector2 planetTilePosition
	) {
		this.mapSeed = mapSeed;
		UnityEngine.Random.InitState(mapSeed);

		this.mapSize = mapSize;
		this.actualMap = actualMap;

		this.planetTemperature = planetTemperature;
		this.temperatureRange = temperatureRange;
		this.planetDistance = planetDistance;
		temperatureOffset = CreatePlanetData.CalculatePlanetTemperature(planetDistance);
		this.averageTemperature = averageTemperature;
		this.averagePrecipitation = averagePrecipitation;
		this.terrainTypeHeights = terrainTypeHeights;
		this.surroundingPlanetTileHeightDirections = surroundingPlanetTileHeightDirections;
		this.isRiver = isRiver;
		this.surroundingPlanetTileRivers = surroundingPlanetTileRivers;
		this.preventEdgeTouching = preventEdgeTouching;
		this.primaryWindDirection = primaryWindDirection;
		this.planetTilePosition = planetTilePosition;

		equatorOffset = ((planetTilePosition.y - (mapSize / 2f)) * 2) / mapSize;

		if (planetMapData != null) {
			mapRegenerationCode = planetMapData.mapSeed + "~" + planetMapData.mapSize + "~" + planetMapData.temperatureRange + "~" + planetMapData.planetDistance + "~" + planetMapData.primaryWindDirection + "~" + planetTilePosition.x + "~" + planetTilePosition.y + "~" + mapSize + "~" + mapSeed;
		}
	}
}