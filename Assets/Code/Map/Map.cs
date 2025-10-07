using System;
using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NMap
{
	public partial class Map
	{

		private ColonistManager ColonistM => GameManager.Get<ColonistManager>();

		public bool Created = false;

		public MapData MapData { get; }
		public MapContext MapContext { get; }

		public Map(MapData mapData, MapContext mapContext) {
			MapData = mapData;
			MapContext = mapContext;
		}

		public List<Tile> tiles = new List<Tile>();
		public List<List<Tile>> sortedTiles = new List<List<Tile>>();
		public List<Tile> edgeTiles = new List<Tile>();
		public Dictionary<int, List<Tile>> sortedEdgeTiles = new Dictionary<int, List<Tile>>();

		public HashSet<Region> regions = new();
		public List<RegionBlock> regionBlocks = new List<RegionBlock>();
		public List<RegionBlock> squareRegionBlocks = new List<RegionBlock>();

		public List<DrainageBasin> drainageBasins = new();
		public List<River> rivers = new List<River>();
		public List<River> largeRivers = new List<River>();

		public event Action<Color> LightingUpdated;

		public void OnCameraPositionChanged(Vector2 position, float zoom) {
			DetermineVisibleRegionBlocks();
		}

		public void OnCameraZoomChanged(float zoom, Vector2 position) {
			DetermineVisibleRegionBlocks();
		}

		public Tile GetTileFromPosition(Vector2 position) {
			return GetTileFromPosition(position.x, position.y);
		}

		public Tile GetTileFromPosition(float x, float y) {
			return GetTileFromPosition(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
		}

		public Tile GetTileFromPosition(int x, int y) {
			return sortedTiles[Mathf.Clamp(y, 0, MapData.mapSize - 1)][Mathf.Clamp(x, 0, MapData.mapSize - 1)];
		}

		public static int GetRandomMapSeed() {
			return Random.Range(int.MinValue, int.MaxValue);
		}
	}
}
