using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NLife;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.Tile;
using Snowship.NResource;
using Snowship.NState;
using Snowship.NTime;
using Snowship.NUI;
using UnityEngine;

namespace Snowship.NMap
{
	public class Map {

		private ColonistManager ColonistM => GameManager.Get<ColonistManager>();

		public bool Created = false;

		public MapData MapData;

		public Map(MapData mapData) {

			MapData = mapData;

			DetermineShadowDirectionsAtHour(mapData.equatorOffset);

			CreateMap().Forget();
		}

		public Map() {
		}

		public List<Tile.Tile> tiles = new List<Tile.Tile>();
		public List<List<Tile.Tile>> sortedTiles = new List<List<Tile.Tile>>();
		public List<Tile.Tile> edgeTiles = new List<Tile.Tile>();
		public Dictionary<int, List<Tile.Tile>> sortedEdgeTiles = new Dictionary<int, List<Tile.Tile>>();

		public async UniTask CreateMap() {

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Map", "Creating Tiles");
				await UniTask.NextFrame();
			}
			CreateTiles();
			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Map", "Validating");
				await UniTask.NextFrame();
				Bitmasking(tiles, false, false);
			}

			if (MapData.preventEdgeTouching) {
				PreventEdgeTouching();
			}

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Map", "Determining Map Edges");
				await UniTask.NextFrame();
				SetMapEdgeTiles();
				UIEvents.UpdateLoadingScreenText("Map", "Determining Sorted Map Edges");
				await UniTask.NextFrame();
				SetSortedMapEdgeTiles();
				UIEvents.UpdateLoadingScreenText("Terrain", "Merging Terrain with Planet");
				await UniTask.NextFrame();
				SmoothHeightWithSurroundingPlanetTiles();
				UIEvents.UpdateLoadingScreenText("Terrain", "Validating");
				await UniTask.NextFrame();
				Bitmasking(tiles, false, false);
			}

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Terrain", "Determining Regions by Tile Type");
				await UniTask.NextFrame();
			}
			SetTileRegions(true, false);

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Terrain", "Reducing Terrain Noise");
				await UniTask.NextFrame();
			}
			ReduceNoise();

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Rivers", "Determining Large River Paths");
				await UniTask.NextFrame();
				CreateLargeRivers();
				UIEvents.UpdateLoadingScreenText("Terrain", "Determining Regions by Walkability");
				await UniTask.NextFrame();
				SetTileRegions(false, false);
				UIEvents.UpdateLoadingScreenText("Terrain", "Reducing Terrain Noise");
				await UniTask.NextFrame();
				ReduceNoise();
			}
			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Terrain", "Determining Regions by Walkability");
				await UniTask.NextFrame();
			}
			SetTileRegions(false, true);
			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Terrain", "Validating");
				await UniTask.NextFrame();
				Bitmasking(tiles, false, false);
			}

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Rivers", "Determining Drainage Basins");
				await UniTask.NextFrame();
			}
			DetermineDrainageBasins();
			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Rivers", "Determining River Paths");
				await UniTask.NextFrame();
			}
			CreateRivers();
			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Rivers", "Validating");
				await UniTask.NextFrame();
				Bitmasking(tiles, false, false);
			}

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Biomes", "Calculating Temperature");
				await UniTask.NextFrame();
			}
			CalculateTemperature();

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Biomes", "Calculating Precipitation");
				await UniTask.NextFrame();
			}
			CalculatePrecipitation();
			MapData.primaryWindDirection = primaryWindDirection;

			/*
			foreach (Tile tile in tiles) {
				tile.SetTileHeight(0.5f);
				tile.SetPrecipitation(tile.position.x / mapData.mapSize);
				tile.temperature = ((1 - (tile.position.y / mapData.mapSize)) * 140) - 50;
			}
			*/

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Biomes", "Setting Biomes");
				await UniTask.NextFrame();
			}
			SetBiomes(MapData.actualMap);
			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Biomes", "Validating");
				await UniTask.NextFrame();
				Bitmasking(tiles, false, false);
			}

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Region Blocks", "Determining Region Blocks");
				await UniTask.NextFrame();
			}
			CreateRegionBlocks();

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Roofs", "Determining Roofs");
				await UniTask.NextFrame();
				SetRoofs();

				UIEvents.UpdateLoadingScreenText("Water", "Determining Coastal Water");
				await UniTask.NextFrame();
				SetCoastalWater();

				UIEvents.UpdateLoadingScreenText("Resources", "Creating Resource Veins");
				await UniTask.NextFrame();
				SetResourceVeins();
				UIEvents.UpdateLoadingScreenText("Resources", "Validating");
				await UniTask.NextFrame();
				Bitmasking(tiles, false, false);

				UIEvents.UpdateLoadingScreenText("Lighting", "Calculating Shadows");
				await UniTask.NextFrame();
				RecalculateLighting(tiles, false);
				UIEvents.UpdateLoadingScreenText("Lighting", "Determining Visible Region Blocks");
				await UniTask.NextFrame();
				DetermineVisibleRegionBlocks();
				UIEvents.UpdateLoadingScreenText("Lighting", "Applying Shadows");
				await UniTask.NextFrame();
				SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
			}

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Lighting", "Validating");
				await UniTask.NextFrame();
			}
			Bitmasking(tiles, false, false);

			if (MapData.actualMap) {
				UIEvents.UpdateLoadingScreenText("Finalizing", string.Empty);
				await UniTask.NextFrame();
			}
			Created = true;

			if (MapData.actualMap) {
				await GameManager.Get<StateManager>().TransitionToState(EState.Simulation);
			}
		}

		public void OnCameraPositionChanged(Vector2 position) {
			DetermineVisibleRegionBlocks();
		}

		public void OnCameraZoomChanged(float zoom) {
			DetermineVisibleRegionBlocks();
		}

		void CreateTiles() {
			for (int y = 0; y < MapData.mapSize; y++) {
				List<Tile.Tile> innerTiles = new List<Tile.Tile>();
				for (int x = 0; x < MapData.mapSize; x++) {

					float height = UnityEngine.Random.Range(0f, 1f);

					Vector2 position = new Vector2(x, y);

					Tile.Tile tile = new Tile.Tile(this, position, height);

					innerTiles.Add(tile);
					tiles.Add(tile);
				}
				sortedTiles.Add(innerTiles);
			}

			SetSurroundingTiles();
			GenerateTerrain();
			AverageTileHeights();
		}

		public void SetSurroundingTiles() {
			for (int y = 0; y < MapData.mapSize; y++) {
				for (int x = 0; x < MapData.mapSize; x++) {
					/* Horizontal */
					if (y + 1 < MapData.mapSize) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y + 1][x]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x + 1 < MapData.mapSize) {
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
					if (x + 1 < MapData.mapSize && y + 1 < MapData.mapSize) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y + 1][x + 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0 && x + 1 < MapData.mapSize) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x + 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0 && y - 1 >= 0) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x - 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y + 1 < MapData.mapSize && x - 1 >= 0) {
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
			int lastSize = MapData.mapSize;
			for (int halves = 0; halves < Mathf.CeilToInt(Mathf.Log(MapData.mapSize, 2)); halves++) {
				int size = Mathf.CeilToInt(lastSize / 2f);
				for (int sectionY = 0; sectionY < MapData.mapSize; sectionY += size) {
					for (int sectionX = 0; sectionX < MapData.mapSize; sectionX += size) {
						float sectionAverage = 0;
						for (int y = sectionY; (y < sectionY + size && y < MapData.mapSize); y++) {
							for (int x = sectionX; (x < sectionX + size && x < MapData.mapSize); x++) {
								sectionAverage += sortedTiles[y][x].height;
							}
						}
						sectionAverage /= (size * size);
						float maxDeviationSize = -(((float)(size - MapData.mapSize)) / (4 * MapData.mapSize));
						sectionAverage += UnityEngine.Random.Range(-maxDeviationSize, maxDeviationSize);
						for (int y = sectionY; (y < sectionY + size && y < MapData.mapSize); y++) {
							for (int x = sectionX; (x < sectionX + size && x < MapData.mapSize); x++) {
								sortedTiles[y][x].height = sectionAverage;
							}
						}
					}
				}
				lastSize = size;
			}

			foreach (Tile.Tile tile in tiles) {
				tile.SetTileHeight(tile.height);
			}
		}

		void AverageTileHeights() {
			for (int i = 0; i < 3; i++) { // 3
				List<float> averageTileHeights = new List<float>();

				foreach (Tile.Tile tile in tiles) {
					float averageHeight = tile.height;
					float numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile.Tile nTile = tile.surroundingTiles[t];
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
					tiles[k].SetTileTypeByHeight();
				}
			}
		}

		void PreventEdgeTouching() {
			foreach (Tile.Tile tile in tiles) {
				float edgeDistance = (MapData.mapSize - (Vector2.Distance(tile.obj.transform.position, new Vector2(MapData.mapSize / 2f, MapData.mapSize / 2f)))) / MapData.mapSize;
				tile.SetTileHeight(tile.height * Mathf.Clamp(-Mathf.Pow(edgeDistance - 1.5f, 10) + 1, 0f, 1f));
			}
		}

		public List<Region> regions = new List<Region>();
		public int currentRegionID = 0;

		void SmoothHeightWithSurroundingPlanetTiles() {
			for (int i = 0; i < MapData.surroundingPlanetTileHeightDirections.Count; i++) {
				if (MapData.surroundingPlanetTileHeightDirections[i] != 0) {
					foreach (Tile.Tile tile in tiles) {
						float closestEdgeDistance = sortedEdgeTiles[i].Min(edgeTile => Vector2.Distance(edgeTile.obj.transform.position, tile.obj.transform.position)) / (MapData.mapSize);
						float heightMultiplier = MapData.surroundingPlanetTileHeightDirections[i] * Mathf.Pow(closestEdgeDistance - 1f, 10f) + 1f;
						float newHeight = Mathf.Clamp(tile.height * heightMultiplier, 0f, 1f);
						tile.SetTileHeight(newHeight);
					}
				}
			}
		}

		public void SetTileRegions(bool splitByTileType, bool removeNonWalkableRegions) {
			regions.Clear();

			EstablishInitialRegions(splitByTileType);
			FindConnectedRegions(splitByTileType);
			MergeConnectedRegions(splitByTileType);

			RemoveEmptyRegions();

			if (removeNonWalkableRegions) {
				RemoveNonWalkableRegions();
			}
		}

		private void EstablishInitialRegions(bool splitByTileType) {
			foreach (Tile.Tile tile in tiles) { // Go through all tiles
				List<Region> foundRegions = new List<Region>(); // For each tile, store a list of the regions around them
				for (int i = 0; i < tile.surroundingTiles.Count; i++) { // Go through the tiles around each tile
					Tile.Tile nTile = tile.surroundingTiles[i];
					if (nTile != null && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable)) && (i == 2 || i == 3 /*|| i == 5 || i == 6 */)) { // Uncomment indexes 5 and 6 to enable 8-connectivity connected-component labeling -- If the tiles have the same type
						if (nTile.region != null && !foundRegions.Contains((Region)nTile.region)) { // If the tiles have a region and it hasn't already been looked at
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

		private void FindConnectedRegions(bool splitByTileType) {
			foreach (Region region in regions) {
				foreach (Tile.Tile tile in region.tiles) {
					foreach (Tile.Tile nTile in tile.horizontalSurroundingTiles) {
						if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains((Region)nTile.region) && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable))) {
							region.connectedRegions.Add(nTile.region);
						}
					}
				}
			}
		}

		private void MergeConnectedRegions(bool splitByTileType) {
			while (regions.Where<Region>(region => region.connectedRegions.Count > 0).ToList().Count > 0) { // While there are regions that have connected regions
				foreach (Region region in regions) { // Go through each region
					if (region.connectedRegions.Count > 0) { // If this region has connected regions
						Region lowestRegion = FindLowestRegion(region.connectedRegions); // Find the lowest ID region from the connected regions
						if (region != lowestRegion) { // If this region is not the lowest region
							foreach (Tile.Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
								tile.ChangeRegion(lowestRegion, false, false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
						foreach (Region connectedRegion in region.connectedRegions) { // Set each tile's region in the connected regions that aren't the lowest region to the lowest region
							if (connectedRegion != lowestRegion) {
								foreach (Tile.Tile tile in connectedRegion.tiles) {
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

		public List<RegionBlock> squareRegionBlocks = new List<RegionBlock>();
		public void CreateRegionBlocks() {
			int regionBlockSize = 10;/*Mathf.RoundToInt(mapData.mapSize / 10f);*/

			regionBlocks.Clear();
			squareRegionBlocks.Clear();

			int size = regionBlockSize;
			int regionIndex = 0;
			for (int sectionY = 0; sectionY < MapData.mapSize; sectionY += size) {
				for (int sectionX = 0; sectionX < MapData.mapSize; sectionX += size) {
					RegionBlock regionBlock = new RegionBlock(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass), regionIndex);
					RegionBlock squareRegionBlock = new RegionBlock(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass), regionIndex);
					for (int y = sectionY; (y < sectionY + size && y < MapData.mapSize); y++) {
						for (int x = sectionX; (x < sectionX + size && x < MapData.mapSize); x++) {
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
				foreach (Tile.Tile tile in squareRegionBlock.tiles) {
					foreach (Tile.Tile nTile in tile.surroundingTiles) {
						if (nTile != null && nTile.squareRegionBlock != tile.squareRegionBlock && nTile.squareRegionBlock != null && !squareRegionBlock.surroundingRegionBlocks.Contains((RegionBlock)nTile.squareRegionBlock)) {
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
					List<Tile.Tile> unwalkableTiles = new List<Tile.Tile>();
					List<Tile.Tile> walkableTiles = new List<Tile.Tile>();
					foreach (Tile.Tile tile in regionBlock.tiles) {
						if (tile.walkable) {
							walkableTiles.Add(tile);
						} else {
							unwalkableTiles.Add(tile);
						}
					}
					regionBlock.tiles.Clear();
					foreach (Tile.Tile unwalkableTile in unwalkableTiles) {
						if (unwalkableTile.regionBlock == null) {
							RegionBlock unwalkableRegionBlock = new RegionBlock(unwalkableTile.tileType, regionIndex);
							regionIndex += 1;
							Tile.Tile currentTile = unwalkableTile;
							List<Tile.Tile> frontier = new List<Tile.Tile>() { currentTile };
							List<Tile.Tile> checkedTiles = new List<Tile.Tile>() { currentTile };
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								unwalkableRegionBlock.tiles.Add(currentTile);
								currentTile.regionBlock = unwalkableRegionBlock;
								foreach (Tile.Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && !nTile.walkable && !checkedTiles.Contains(nTile) && unwalkableTiles.Contains(nTile) && nTile.regionBlock == null) {
										frontier.Add(nTile);
									}
									checkedTiles.Add(nTile);
								}
							}
							newRegionBlocks.Add(unwalkableRegionBlock);
						}
					}
					foreach (Tile.Tile walkableTile in walkableTiles) {
						if (walkableTile.regionBlock == null) {
							RegionBlock walkableRegionBlock = new RegionBlock(walkableTile.tileType, regionIndex);
							regionIndex += 1;
							Tile.Tile currentTile = walkableTile;
							List<Tile.Tile> frontier = new List<Tile.Tile>() { currentTile };
							List<Tile.Tile> checkedTiles = new List<Tile.Tile>() { currentTile };
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								walkableRegionBlock.tiles.Add(currentTile);
								currentTile.regionBlock = walkableRegionBlock;
								foreach (Tile.Tile nTile in currentTile.horizontalSurroundingTiles) {
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
					foreach (Tile.Tile tile in regionBlock.tiles) {
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
				foreach (Tile.Tile tile in regionBlock.tiles) {
					foreach (Tile.Tile nTile in tile.horizontalSurroundingTiles) {
						if (nTile != null && nTile.regionBlock != tile.regionBlock && nTile.regionBlock != null && !regionBlock.horizontalSurroundingRegionBlocks.Contains((RegionBlock)nTile.regionBlock)) {
							regionBlock.horizontalSurroundingRegionBlocks.Add(nTile.regionBlock);
						}
					}
					foreach (Tile.Tile nTile in tile.surroundingTiles) {
						if (nTile != null && nTile.regionBlock != tile.regionBlock && nTile.regionBlock != null && !regionBlock.surroundingRegionBlocks.Contains((RegionBlock)nTile.regionBlock)) {
							regionBlock.surroundingRegionBlocks.Add(nTile.regionBlock);
						}
					}
					regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x + tile.obj.transform.position.x, regionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x / regionBlock.tiles.Count, regionBlock.averagePosition.y / regionBlock.tiles.Count);
			}
		}

		private Region FindLowestRegion(List<Region> searchRegions) {
			Region lowestRegion = searchRegions[0];
			foreach (Region region in searchRegions) {
				if (region.id < lowestRegion.id) {
					lowestRegion = region;
				}
			}
			return lowestRegion;
		}

		private void RemoveEmptyRegions() {
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

		private void RemoveNonWalkableRegions() {
			List<Region> removeRegions = new List<Region>();
			foreach (Region region in regions) {
				if (!region.tileType.walkable) {
					foreach (Tile.Tile tile in region.tiles) {
						tile.ChangeRegion(null, false, false);
					}
					removeRegions.Add(region);
				}
			}
			foreach (Region region in removeRegions) {
				regions.Remove(region);
			}
		}

		public void RecalculateRegionsAtTile(Tile.Tile tile) {
			if (!tile.walkable) {
				List<Tile.Tile> orderedSurroundingTiles = new List<Tile.Tile>() {
					tile.surroundingTiles[0], tile.surroundingTiles[4], tile.surroundingTiles[1], tile.surroundingTiles[5],
					tile.surroundingTiles[2], tile.surroundingTiles[6], tile.surroundingTiles[3], tile.surroundingTiles[7]
				};
				List<List<Tile.Tile>> separateTileGroups = new List<List<Tile.Tile>>();
				int groupIndex = 0;
				for (int i = 0; i < orderedSurroundingTiles.Count; i++) {
					if (groupIndex == separateTileGroups.Count) {
						separateTileGroups.Add(new List<Tile.Tile>());
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
				List<Tile.Tile> horizontalGroups = new List<Tile.Tile>();
				foreach (List<Tile.Tile> tileGroup in separateTileGroups) {
					List<Tile.Tile> horizontalTilesInGroup = tileGroup.Where(groupTile => tile.horizontalSurroundingTiles.Contains(groupTile)).ToList();
					if (horizontalTilesInGroup.Count > 0) {
						horizontalGroups.Add(horizontalTilesInGroup[0]);
					}
				}
				if (horizontalGroups.Count > 1) {
					List<Tile.Tile> removeTiles = new List<Tile.Tile>();
					foreach (Tile.Tile startTile in horizontalGroups) {
						if (!removeTiles.Contains(startTile)) {
							foreach (Tile.Tile endTile in horizontalGroups) {
								if (!removeTiles.Contains(endTile) && startTile != endTile) {
									if (PathManager.PathExists(startTile, endTile, true, MapData.mapSize, PathManager.WalkableSetting.Walkable, PathManager.DirectionSetting.Horizontal)) {
										removeTiles.Add(endTile);
									}
								}
							}
						}
					}
					foreach (Tile.Tile removeTile in removeTiles) {
						horizontalGroups.Remove(removeTile);
					}
					if (horizontalGroups.Count > 1) {
						SetTileRegions(false, true);
					}
				}
			}
		}

		public void ReduceNoise() {
			ReduceNoise(Mathf.RoundToInt(MapData.mapSize / 5f), new List<TileTypeGroup.TypeEnum>() { TileTypeGroup.TypeEnum.Water, TileTypeGroup.TypeEnum.Stone, TileTypeGroup.TypeEnum.Ground });
			ReduceNoise(Mathf.RoundToInt(MapData.mapSize / 2f), new List<TileTypeGroup.TypeEnum>() { TileTypeGroup.TypeEnum.Water });
		}

		private void ReduceNoise(int removeRegionsBelowSize, List<TileTypeGroup.TypeEnum> tileTypeGroupsToRemove) {
			foreach (Region region in regions) {
				if (tileTypeGroupsToRemove.Contains(region.tileType.groupType)) {
					if (region.tiles.Count < removeRegionsBelowSize) {
						/* --- This code is essentially copied from FindConnectedRegions() */
						foreach (Tile.Tile tile in region.tiles) {
							foreach (Tile.Tile nTile in tile.horizontalSurroundingTiles) {
								if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains((Region)nTile.region)) {
									region.connectedRegions.Add(nTile.region);
								}
							}
						}
						/* --- This code is essentially copied from MergeConnectedRegions() */
						if (region.connectedRegions.Count > 0) {
							Region lowestRegion = FindLowestRegion(region.connectedRegions);
							foreach (Tile.Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
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
		public List<River> largeRivers = new List<River>();

		public Dictionary<Region, Tile.Tile> drainageBasins = new Dictionary<Region, Tile.Tile>();
		public int drainageBasinID = 0;

		public void DetermineDrainageBasins() {
			drainageBasins.Clear();
			drainageBasinID = 0;

			List<Tile.Tile> tilesByHeight = tiles.OrderBy(tile => tile.height).ToList();
			foreach (Tile.Tile tile in tilesByHeight) {
				if (tile.tileType.groupType != TileTypeGroup.TypeEnum.Stone && tile.drainageBasin == null) {
					Region drainageBasin = new Region(null, drainageBasinID);
					drainageBasinID += 1;

					Tile.Tile currentTile = tile;

					List<Tile.Tile> checkedTiles = new List<Tile.Tile> { currentTile };
					List<Tile.Tile> frontier = new List<Tile.Tile>() { currentTile };

					while (frontier.Count > 0) {
						currentTile = frontier[0];
						frontier.RemoveAt(0);

						drainageBasin.tiles.Add(currentTile);
						currentTile.drainageBasin = drainageBasin;

						foreach (Tile.Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone && nTile.drainageBasin == null) {
								if (nTile.height * 1.2f >= currentTile.height) {
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

		void CreateLargeRivers() {
			largeRivers.Clear();
			if (MapData.isRiver) {
				int riverEndRiverIndex = MapData.surroundingPlanetTileRivers.OrderByDescending(i => i).ToList()[0];
				int riverEndListIndex = MapData.surroundingPlanetTileRivers.IndexOf(riverEndRiverIndex);

				List<Tile.Tile> validEndTiles = sortedEdgeTiles[riverEndListIndex].Where(tile => Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverEndListIndex][0].obj.transform.position) >= 10 && Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverEndListIndex][sortedEdgeTiles[riverEndListIndex].Count - 1].obj.transform.position) >= 10).ToList();
				Tile.Tile riverEndTile = validEndTiles[UnityEngine.Random.Range(0, validEndTiles.Count)];

				int riverStartListIndex = 0;
				foreach (int riverStartRiverIndex in MapData.surroundingPlanetTileRivers) {
					if (riverStartRiverIndex != -1 && riverStartRiverIndex != riverEndRiverIndex) {
						int expandRadius = UnityEngine.Random.Range(1, 3) * Mathf.CeilToInt(MapData.mapSize / 100f);
						List<Tile.Tile> validStartTiles = sortedEdgeTiles[riverStartListIndex].Where(tile => Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverStartListIndex][0].obj.transform.position) >= 10 && Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverStartListIndex][sortedEdgeTiles[riverStartListIndex].Count - 1].obj.transform.position) >= 10).ToList();
						Tile.Tile riverStartTile = validStartTiles[UnityEngine.Random.Range(0, validStartTiles.Count)];
						List<Tile.Tile> possibleCentreTiles = tiles.Where(t => Vector2.Distance(new Vector2(MapData.mapSize / 2f, MapData.mapSize / 2f), t.obj.transform.position) < MapData.mapSize / 5f).ToList();
						River river = new River(riverStartTile, possibleCentreTiles[UnityEngine.Random.Range(0, possibleCentreTiles.Count)], riverEndTile, expandRadius, true, this, true);
						if (river.tiles.Count > 0) {
							largeRivers.Add(river);
						} else {
							Debug.LogWarning("Large River has no tiles. startTile: " + riverStartTile.obj.transform.position + " endTile: " + riverEndTile.obj.transform.position);
						}
					}
					riverStartListIndex += 1;
				}
			}
		}

		void CreateRivers() {
			rivers.Clear();
			Dictionary<Tile.Tile, Tile.Tile> riverStartTiles = new Dictionary<Tile.Tile, Tile.Tile>();
			foreach (KeyValuePair<Region, Tile.Tile> kvp in drainageBasins) {
				Region drainageBasin = kvp.Key;
				if (drainageBasin.tiles.Find(o => o.tileType.groupType == TileTypeGroup.TypeEnum.Water) != null && drainageBasin.tiles.Find(o => o.horizontalSurroundingTiles.Find(o2 => o2 != null && o2.tileType.groupType == TileTypeGroup.TypeEnum.Stone) != null) != null) {
					foreach (Tile.Tile tile in drainageBasin.tiles) {
						if (tile.walkable && tile.tileType.groupType != TileTypeGroup.TypeEnum.Water && tile.horizontalSurroundingTiles.Find(o => o != null && o.tileType.groupType == TileTypeGroup.TypeEnum.Stone) != null) {
							riverStartTiles.Add(tile, kvp.Value);
						}
					}
				}
			}
			for (int i = 0; i < MapData.mapSize / 10f && i < riverStartTiles.Count; i++) {
				Tile.Tile riverStartTile = Enumerable.ToList(riverStartTiles.Keys)[UnityEngine.Random.Range(0, riverStartTiles.Count)];
				Tile.Tile riverEndTile = riverStartTiles[riverStartTile];
				List<Tile.Tile> removeTiles = new List<Tile.Tile>();
				foreach (KeyValuePair<Tile.Tile, Tile.Tile> kvp in riverStartTiles) {
					if (Vector2.Distance(kvp.Key.obj.transform.position, riverStartTile.obj.transform.position) < 5f) {
						removeTiles.Add(kvp.Key);
					}
				}
				foreach (Tile.Tile removeTile in removeTiles) {
					riverStartTiles.Remove(removeTile);
				}
				removeTiles.Clear();

				River river = new River(riverStartTile, null, riverEndTile, 0, false, this, true);
				if (river.tiles.Count > 0) {
					rivers.Add(river);
				} else {
					Debug.LogWarning("River has no tiles. startTile: " + riverStartTile.obj.transform.position + " endTile: " + riverEndTile.obj.transform.position);
				}
			}
		}

		public List<Tile.Tile> RiverPathfinding(Tile.Tile riverStartTile, Tile.Tile riverEndTile, int expandRadius, bool ignoreStone) {
			PathManager.PathfindingTile currentTile = new PathManager.PathfindingTile(riverStartTile, null, 0);

			List<PathManager.PathfindingTile> checkedTiles = new List<PathManager.PathfindingTile>() { currentTile };
			List<PathManager.PathfindingTile> frontier = new List<PathManager.PathfindingTile>() { currentTile };

			List<Tile.Tile> river = new List<Tile.Tile>();

			while (frontier.Count > 0) {
				currentTile = frontier[0];
				frontier.RemoveAt(0);

				if (currentTile.tile == riverEndTile || (expandRadius == 0 && (currentTile.tile.tileType.groupType == TileTypeGroup.TypeEnum.Water || (currentTile.tile.horizontalSurroundingTiles.Find(tile => tile != null && tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && RiversContainTile(tile, true).Key == null) != null)))) {
					while (currentTile != null) {
						river.Add(currentTile.tile);
						currentTile.tile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.GrassWater), true, false, false);
						currentTile = currentTile.cameFrom;
					}
					break;
				}

				foreach (Tile.Tile nTile in currentTile.tile.horizontalSurroundingTiles) {
					if (nTile != null && checkedTiles.Find(checkedTile => checkedTile.tile == nTile) == null && (ignoreStone || nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
						if (rivers.Find(otherRiver => otherRiver.tiles.Find(riverTile => nTile == riverTile) != null) != null) {
							frontier.Clear();
							frontier.Add(new PathManager.PathfindingTile(nTile, currentTile, 0));
							nTile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.GrassWater), true, false, false);
							break;
						}
						float cost = Vector2.Distance(nTile.obj.transform.position, riverEndTile.obj.transform.position) + (nTile.height * (MapData.mapSize / 10f)) + UnityEngine.Random.Range(0, 10);
						PathManager.PathfindingTile pTile = new PathManager.PathfindingTile(nTile, currentTile, cost);
						frontier.Add(pTile);
						checkedTiles.Add(pTile);
					}
				}
				frontier = frontier.OrderBy(frontierTile => frontierTile.cost).ToList();
			}

			if (river.Count == 0) {
				return river;
			}

			if (expandRadius > 0) {
				float expandedExpandRadius = expandRadius * UnityEngine.Random.Range(2f, 4f);
				List<Tile.Tile> riverAdditions = new List<Tile.Tile>();
				riverAdditions.AddRange(river);
				foreach (Tile.Tile riverTile in river) {
					riverTile.SetTileHeight(CalculateLargeRiverTileHeight(expandRadius, 0));

					List<Tile.Tile> expandFrontier = new List<Tile.Tile>() { riverTile };
					List<Tile.Tile> checkedExpandTiles = new List<Tile.Tile>() { riverTile };
					while (expandFrontier.Count > 0) {
						Tile.Tile expandTile = expandFrontier[0];
						expandFrontier.RemoveAt(0);
						float distanceExpandTileRiverTile = Vector2.Distance(expandTile.obj.transform.position, riverTile.obj.transform.position);
						float newRiverHeight = CalculateLargeRiverTileHeight(expandRadius, distanceExpandTileRiverTile);
						float newRiverBankHeight = CalculateLargeRiverBankTileHeight(expandRadius, distanceExpandTileRiverTile);
						if (distanceExpandTileRiverTile <= expandRadius) {
							if (!riverAdditions.Contains(expandTile)) {
								riverAdditions.Add(expandTile);
								expandTile.SetTileHeight(newRiverHeight);
							}
						} else if (!riverAdditions.Contains(expandTile) && expandTile.height > newRiverBankHeight) {
							expandTile.SetTileHeight(newRiverBankHeight);
						}
						foreach (Tile.Tile nTile in expandTile.surroundingTiles) {
							if (nTile != null && !checkedExpandTiles.Contains(nTile) && (ignoreStone || nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
								if (Vector2.Distance(nTile.obj.transform.position, riverTile.obj.transform.position) <= expandedExpandRadius) {
									expandFrontier.Add(nTile);
									checkedExpandTiles.Add(nTile);
								}
							}
						}
					}
				}
				river.AddRange(riverAdditions);
			}

			return river;
		}

		private float CalculateLargeRiverTileHeight(int expandRadius, float distanceExpandTileRiverTile) {
			float height = (MapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Water] / expandRadius) * distanceExpandTileRiverTile;//(2 * mapData.terrainTypeHeights[TileTypes.GrassWater]) * (distanceExpandTileRiverTile / expandedExpandRadius);
			height -= 0.01f;
			return Mathf.Clamp(height, 0f, 1f);
		}

		private float CalculateLargeRiverBankTileHeight(int expandRadius, float distanceExpandTileRiverTile) {
			float height = CalculateLargeRiverTileHeight(expandRadius, distanceExpandTileRiverTile / 2f);
			height += (MapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Water] / 2f);
			return Mathf.Clamp(height, 0f, 1f);
		}

		public KeyValuePair<Tile.Tile, River> RiversContainTile(Tile.Tile tile, bool includeLargeRivers) {
			foreach (River river in includeLargeRivers ? rivers.Concat<River>(largeRivers) : rivers) {
				foreach (Tile.Tile riverTile in river.tiles) {
					if (riverTile == tile) {
						return new KeyValuePair<Tile.Tile, River>(riverTile, river);
					}
				}
			}
			return new KeyValuePair<Tile.Tile, River>(null, null);
		}

		public float TemperatureFromMapLatitude(float yPos, float temperatureRange, float temperatureOffset, int mapSize) {
			return ((-2 * Mathf.Abs((yPos - (mapSize / 2f)) / ((mapSize / 100f) / (temperatureRange / 50f)))) + temperatureRange) + temperatureOffset + (UnityEngine.Random.Range(-50f, 50f));
		}

		public void CalculateTemperature() {
			foreach (Tile.Tile tile in tiles) {
				if (MapData.planetTemperature) {
					tile.temperature = TemperatureFromMapLatitude(tile.position.y, MapData.temperatureRange, MapData.temperatureOffset, MapData.mapSize);
				} else {
					tile.temperature = MapData.averageTemperature;
				}
				tile.temperature += -(50f * Mathf.Pow(tile.height - 0.5f, 3));
			}

			AverageTileTemperatures();
		}

		void AverageTileTemperatures() {
			int numPasses = 3; // 3
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTileTemperatures = new List<float>();

				foreach (Tile.Tile tile in tiles) {
					float averageTemperature = tile.temperature;
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile.Tile nTile = tile.surroundingTiles[t];
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

		private static readonly List<int> oppositeDirectionTileMap = new List<int>() { 2, 3, 0, 1, 6, 7, 4, 5 };
		private static readonly List<List<float>> windStrengthMap = new List<List<float>>() {
			new List<float>(){ 1.0f,0.6f,0.1f,0.6f,0.8f,0.2f,0.2f,0.8f },
			new List<float>(){ 0.6f,1.0f,0.6f,0.1f,0.8f,0.8f,0.2f,0.2f },
			new List<float>(){ 0.1f,0.6f,1.0f,0.6f,0.2f,0.8f,0.8f,0.2f },
			new List<float>(){ 0.6f,0.1f,0.6f,1.0f,0.2f,0.2f,0.8f,0.8f },
			new List<float>(){ 0.8f,0.8f,0.2f,0.2f,1.0f,0.6f,0.1f,0.6f },
			new List<float>(){ 0.2f,0.8f,0.8f,0.2f,0.6f,1.0f,0.6f,0.1f },
			new List<float>(){ 0.2f,0.2f,0.8f,0.8f,0.1f,0.6f,1.0f,0.6f },
			new List<float>(){ 0.8f,0.2f,0.2f,0.8f,0.6f,0.1f,0.6f,1.0f }
		};

		public int primaryWindDirection = -1;
		public void CalculatePrecipitation() {
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

					for (int y = (yStartAtTop ? MapData.mapSize - 1 : 0); (yStartAtTop ? y >= 0 : y < MapData.mapSize); y += (yStartAtTop ? -1 : 1)) {
						for (int x = (xStartAtRight ? MapData.mapSize - 1 : 0); (xStartAtRight ? x >= 0 : x < MapData.mapSize); x += (xStartAtRight ? -1 : 1)) {
							Tile.Tile tile = sortedTiles[y][x];
							Tile.Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
							SetTilePrecipitation(tile, previousTile, MapData.planetTemperature);
						}
					}
				} else { // Wind is going diagonally
					bool up = (windDirection == 4 || windDirection == 7);
					bool left = (windDirection == 6 || windDirection == 7);
					int mapSize2x = MapData.mapSize * 2;
					for (int k = (up ? 0 : mapSize2x); (up ? k < mapSize2x : k >= 0); k += (up ? 1 : -1)) {
						for (int x = (left ? k : 0); (left ? x >= 0 : x <= k); x += (left ? -1 : 1)) {
							int y = k - x;
							if (y < MapData.mapSize && x < MapData.mapSize) {
								Tile.Tile tile = sortedTiles[y][x];
								Tile.Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
								SetTilePrecipitation(tile, previousTile, MapData.planetTemperature);
							}
						}
					}
				}
				List<float> singleDirectionPrecipitations = new List<float>();
				foreach (Tile.Tile tile in tiles) {
					singleDirectionPrecipitations.Add(tile.GetPrecipitation());
					tile.SetPrecipitation(0);
				}
				directionPrecipitations.Add(singleDirectionPrecipitations);
			}

			if (MapData.primaryWindDirection == -1) {
				primaryWindDirection = UnityEngine.Random.Range(windDirectionMin, (windDirectionMax + 1));
			} else {
				primaryWindDirection = MapData.primaryWindDirection;
			}

			float windStrengthMapSum = 0;
			for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) {
				windStrengthMapSum += windStrengthMap[primaryWindDirection][i];
			}

			for (int t = 0; t < tiles.Count; t++) {
				Tile.Tile tile = tiles[t];
				tile.SetPrecipitation(0);
				for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) {
					tile.SetPrecipitation(tile.GetPrecipitation() + (directionPrecipitations[i][t] * windStrengthMap[primaryWindDirection][i]));
				}
				tile.SetPrecipitation(tile.GetPrecipitation() / windStrengthMapSum);
			}

			AverageTilePrecipitations();

			foreach (Tile.Tile tile in tiles) {
				if (Mathf.RoundToInt(MapData.averagePrecipitation) != -1) {
					tile.SetPrecipitation((tile.GetPrecipitation() + MapData.averagePrecipitation) / 2f);
				}
				tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
			}
		}

		private void SetTilePrecipitation(Tile.Tile tile, Tile.Tile previousTile, bool planet) {
			if (planet) {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation(((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier) * (MapData.mapSize / 5f));
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.9f);
					} else {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.95f);
					}
				} else {
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation(1f);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(1f);
					} else {
						tile.SetPrecipitation(0.1f);
					}
				}
			} else {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						float waterMultiplier = (MapData.mapSize / 5f);
						if (RiversContainTile(tile, true).Value != null) {
							waterMultiplier *= 5;
						}
						tile.SetPrecipitation(((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier) * waterMultiplier);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * UnityEngine.Random.Range(0.95f, 0.99f));
					} else {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * UnityEngine.Random.Range(0.98f, 1f));
					}
				} else {
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation(1f);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(1f);
					} else {
						tile.SetPrecipitation(MapData.averagePrecipitation);
					}
				}
			}
			tile.SetPrecipitation(ChangePrecipitationByTemperature(tile.GetPrecipitation(), tile.temperature));
			tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
		}

		private float ChangePrecipitationByTemperature(float precipitation, float temperature) {
			return precipitation * (Mathf.Clamp(-Mathf.Pow((temperature - 30) / (90 - 30), 3) + 1, 0f, 1f)); // Less precipitation as the temperature gets higher
		}

		public void AverageTilePrecipitations() {
			int numPasses = 5;
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTilePrecipitations = new List<float>();

				foreach (Tile.Tile tile in tiles) {
					float averagePrecipitation = tile.GetPrecipitation();
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile.Tile nTile = tile.surroundingTiles[t];
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

		public void SetBiomes(bool setPlant) {

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

			foreach (Tile.Tile tile in tiles) {
				foreach (Biome biome in Biome.biomes) {
					foreach (Biome.Range range in biome.ranges) {
						if (range.IsInRange(tile.GetPrecipitation(), tile.temperature)) {
							tile.SetBiome(biome, setPlant);
							if (tile.plant != null && tile.plant.small) {
								tile.plant.growthProgress = UnityEngine.Random.Range(0, SimulationDateTime.DayLengthSeconds * 4);
							}
						}
					}
				}
			}
		}

		public void SetMapEdgeTiles() {
			edgeTiles.Clear();
			for (int i = 1; i < MapData.mapSize - 1; i++) {
				edgeTiles.Add(sortedTiles[0][i]);
				edgeTiles.Add(sortedTiles[MapData.mapSize - 1][i]);
				edgeTiles.Add(sortedTiles[i][0]);
				edgeTiles.Add(sortedTiles[i][MapData.mapSize - 1]);
			}
			edgeTiles.Add(sortedTiles[0][0]);
			edgeTiles.Add(sortedTiles[0][MapData.mapSize - 1]);
			edgeTiles.Add(sortedTiles[MapData.mapSize - 1][0]);
			edgeTiles.Add(sortedTiles[MapData.mapSize - 1][MapData.mapSize - 1]);
		}

		public void SetSortedMapEdgeTiles() {
			sortedEdgeTiles.Clear();

			int sideNum = -1;
			List<Tile.Tile> tilesOnThisEdge = null;
			for (int i = 0; i <= MapData.mapSize; i++) {
				i %= MapData.mapSize;
				if (i == 0) {
					sideNum += 1;
					sortedEdgeTiles.Add(sideNum, new List<Tile.Tile>());
					tilesOnThisEdge = sortedEdgeTiles[sideNum];
				}
				if (sideNum == 0) {
					tilesOnThisEdge.Add(sortedTiles[MapData.mapSize - 1][i]);
				} else if (sideNum == 1) {
					tilesOnThisEdge.Add(sortedTiles[i][MapData.mapSize - 1]);
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
			foreach (Tile.Tile tile in tiles) {
				tile.SetRoof(tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone && tile.height >= MapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Stone] * roofHeightMultiplier);
			}
		}

		private void SetCoastalWater() {
			foreach (Tile.Tile tile in tiles) {
				tile.CoastalWater = tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && tile.surroundingTiles.Count(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) > 0;
			}
		}

		public void SetResourceVeins() {

			List<Tile.Tile> stoneTiles = new List<Tile.Tile>();
			foreach (RegionBlock regionBlock in regionBlocks) {
				if (regionBlock.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					stoneTiles.AddRange(regionBlock.tiles);
				}
			}
			if (stoneTiles.Count > 0) {
				foreach (ResourceVein resourceVein in ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Stone)) {
					PlaceResourceVeins(resourceVein, stoneTiles);
				}
			}

			List<Tile.Tile> coastTiles = new List<Tile.Tile>();
			foreach (RegionBlock regionBlock in regionBlocks) {
				if (regionBlock.tileType.groupType == TileTypeGroup.TypeEnum.Water) {
					foreach (Tile.Tile tile in regionBlock.tiles) {
						if (tile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) != null) {
							coastTiles.Add(tile);
						}
					}
				}
			}
			if (coastTiles.Count > 0) {
				foreach (ResourceVein resourceVein in ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Coast)) {
					PlaceResourceVeins(resourceVein, coastTiles);
				}
			}
		}

		void PlaceResourceVeins(ResourceVein resourceVeinData, List<Tile.Tile> mediumTiles) {
			List<Tile.Tile> previousVeinStartTiles = new List<Tile.Tile>();
			for (int i = 0; i < Mathf.CeilToInt(MapData.mapSize / (float)resourceVeinData.numVeinsByMapSize); i++) {
				List<Tile.Tile> validVeinStartTiles = mediumTiles.Where(tile => !resourceVeinData.tileTypes.ContainsValue(tile.tileType.type) && resourceVeinData.tileTypes.ContainsKey(tile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](tile)).ToList();
				foreach (Tile.Tile previousVeinStartTile in previousVeinStartTiles) {
					List<Tile.Tile> removeTiles = new List<Tile.Tile>();
					foreach (Tile.Tile validVeinStartTile in validVeinStartTiles) {
						if (Vector2.Distance(validVeinStartTile.obj.transform.position, previousVeinStartTile.obj.transform.position) < resourceVeinData.veinDistance) {
							removeTiles.Add(validVeinStartTile);
						}
					}
					foreach (Tile.Tile removeTile in removeTiles) {
						validVeinStartTiles.Remove(removeTile);
					}
				}
				if (validVeinStartTiles.Count > 0) {

					int veinSizeMax = resourceVeinData.veinSize + UnityEngine.Random.Range(-resourceVeinData.veinSizeRange, resourceVeinData.veinSizeRange);

					Tile.Tile veinStartTile = validVeinStartTiles[UnityEngine.Random.Range(0, validVeinStartTiles.Count)];
					previousVeinStartTiles.Add(veinStartTile);

					List<Tile.Tile> frontier = new List<Tile.Tile>() { veinStartTile };
					List<Tile.Tile> checkedTiles = new List<Tile.Tile>();
					Tile.Tile currentTile = veinStartTile;

					int veinSize = 0;

					while (frontier.Count > 0) {
						currentTile = frontier[UnityEngine.Random.Range(0, frontier.Count)];
						frontier.RemoveAt(0);
						checkedTiles.Add(currentTile);

						currentTile.SetTileType(TileType.GetTileTypeByEnum(resourceVeinData.tileTypes[currentTile.tileType.groupType]), false, true, false);

						foreach (Tile.Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && !resourceVeinData.tileTypes.Values.Contains(nTile.tileType.type)) {
								if (resourceVeinData.tileTypes.ContainsKey(nTile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](nTile)) {
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

		public static readonly Dictionary<int, int> bitmaskMap = new Dictionary<int, int>() {
			{ 19, 16 },
			{ 23, 17 },
			{ 27, 18 },
			{ 31, 19 },
			{ 38, 20 },
			{ 39, 21 },
			{ 46, 22 },
			{ 47, 23 },
			{ 55, 24 },
			{ 63, 25 },
			{ 76, 26 },
			{ 77, 27 },
			{ 78, 28 },
			{ 79, 29 },
			{ 95, 30 },
			{ 110, 31 },
			{ 111, 32 },
			{ 127, 33 },
			{ 137, 34 },
			{ 139, 35 },
			{ 141, 36 },
			{ 143, 37 },
			{ 155, 38 },
			{ 159, 39 },
			{ 175, 40 },
			{ 191, 41 },
			{ 205, 42 },
			{ 207, 43 },
			{ 223, 44 },
			{ 239, 45 },
			{ 255, 46 }
		};
		public static readonly Dictionary<int, List<int>> diagonalCheckMap = new Dictionary<int, List<int>>() {
			{ 4, new List<int>() { 0, 1 } },
			{ 5, new List<int>() { 1, 2 } },
			{ 6, new List<int>() { 2, 3 } },
			{ 7, new List<int>() { 3, 0 } }
		};

		public int BitSum(
			List<TileType.TypeEnum> compareTileTypes,
			List<ObjectPrefab.ObjectEnum> compareObjectTypes,
			List<Tile.Tile> tilesToSum,
			bool includeMapEdge
		) {
			//if (compareObjectTypes == null) {
			//	compareObjectTypes = new List<ResourceManager.ObjectEnum>();
			//}

			int sum = 0;
			for (int i = 0; i < tilesToSum.Count; i++) {
				if (tilesToSum[i] != null) {
					if (compareTileTypes.Contains(tilesToSum[i].tileType.type)
						//|| compareObjectTypes.Intersect(tilesToSum[i].objectInstances.Values.Select(obj => obj.prefab.type)).ToList().Count > 0
					) {
						bool ignoreTile = false;
						if (diagonalCheckMap.ContainsKey(i)) {
							List<Tile.Tile> surroundingHorizontalTiles = new List<Tile.Tile>() { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
							List<Tile.Tile> similarTiles = surroundingHorizontalTiles.Where(tile =>
								tile != null
								&& (compareTileTypes.Contains(tile.tileType.type)
									/*|| compareObjectTypes.Intersect(tile.objectInstances.Values.Select(obj => obj.prefab.type)).ToList().Count > 0*/)
							).ToList();
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
							List<Tile.Tile> surroundingHorizontalTiles = new List<Tile.Tile>() { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)) == null) {
								sum += Mathf.RoundToInt(Mathf.Pow(2, i));
							}
						}
					}
				}
			}
			return sum;
		}

		void BitmaskTile(Tile.Tile tile, bool includeDiagonalSurroundingTiles, bool customBitSumInputs, List<TileType.TypeEnum> customCompareTileTypes, bool includeMapEdge) {
			int sum = 0;
			List<Tile.Tile> surroundingTilesToUse = includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles;
			if (customBitSumInputs) {
				sum = BitSum(customCompareTileTypes, null, surroundingTilesToUse, includeMapEdge);
			} else {
				if (RiversContainTile(tile, false).Key != null) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, false);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Water) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, includeMapEdge);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, includeMapEdge);
					// Not-fully-working implementation of walls and stone connecting
					//sum += GameManager.Get<ResourceManager>().BitSumObjects(
					//	GameManager.Get<ResourceManager>().GetObjectPrefabSubGroupByEnum(ResourceManager.ObjectSubGroupEnum.Walls).prefabs.Select(prefab => prefab.type).ToList(),
					//	surroundingTilesToUse
					//);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Hole) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Hole).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, false);
				} else {
					sum = BitSum(new List<TileType.TypeEnum>() { tile.tileType.type }, null, surroundingTilesToUse, includeMapEdge);
				}
			}
			if ((sum < 16) || (bitmaskMap[sum] != 46)) {
				if (sum >= 16) {
					sum = bitmaskMap[sum];
				}
				if (tile.tileType.classes[TileType.ClassEnum.LiquidWater] && RiversContainTile(tile, false).Key != null) {
					tile.sr.sprite = tile.tileType.riverSprites[sum];
				} else {
					try {
						tile.sr.sprite = tile.tileType.bitmaskSprites[sum];
					} catch (ArgumentOutOfRangeException) {
						Debug.LogWarning("BitmaskTile Error: Index " + sum + " does not exist in bitmaskSprites. " + tile.obj.transform.position + " " + tile.tileType.type + " " + tile.tileType.bitmaskSprites.Count);
					}
				}
			} else {
				if (tile.tileType.baseSprites.Count > 0 && !tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
					tile.sr.sprite = tile.tileType.baseSprites[UnityEngine.Random.Range(0, tile.tileType.baseSprites.Count)];
				}
				if (ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Stone).Find(rvd => rvd.tileTypes.ContainsValue(tile.tileType.type)) != null) {
					TileType biomeTileType = tile.biome.tileTypes[tile.tileType.groupType];
					tile.sr.sprite = biomeTileType.baseSprites[UnityEngine.Random.Range(0, biomeTileType.baseSprites.Count)];
				}
			}
		}

		public void Bitmasking(List<Tile.Tile> tilesToBitmask, bool careAboutColonistVisibility, bool recalculateLighting) {
			foreach (Tile.Tile tile in tilesToBitmask) {
				if (tile != null) {
					if (!careAboutColonistVisibility || ColonistM.IsTileVisibleToAnyColonist(tile)) {
						tile.SetVisible(true); // "true" on "recalculateBitmasking" would cause stack overflow
						if (tile.tileType.bitmasking) {
							BitmaskTile(tile, true, false, null, true);
						} else {
							if (!tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
								tile.sr.sprite = tile.tileType.baseSprites[UnityEngine.Random.Range(0, tile.tileType.baseSprites.Count)];
							}
						}
					} else {
						tile.SetVisible(false); // "true" on "recalculateBitmasking" would cause stack overflow
					}
				}
			}
			BitmaskRiverStartTiles();
			if (recalculateLighting) {
				RecalculateLighting(tilesToBitmask, true);
			}
		}

		void BitmaskRiverStartTiles() {
			foreach (River river in rivers) {
				List<TileType.TypeEnum> compareTileTypes = new List<TileType.TypeEnum>();
				compareTileTypes.AddRange(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList());
				compareTileTypes.AddRange(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tt => tt.type).ToList());
				BitmaskTile(river.startTile, false, true, compareTileTypes, false/*river.expandRadius > 0*/);
			}
		}

		private readonly List<RegionBlock> visibleRegionBlocks = new List<RegionBlock>();
		private RegionBlock centreRegionBlock;
		private int lastOrthographicSize = -1;

		public void DetermineVisibleRegionBlocks() {
			Camera camera = GameManager.Get<CameraManager>().camera;
			RegionBlock newCentreRegionBlock = GetTileFromPosition(camera.transform.position).squareRegionBlock;
			if (newCentreRegionBlock != centreRegionBlock || Mathf.RoundToInt(camera.orthographicSize) != lastOrthographicSize) {
				visibleRegionBlocks.Clear();
				lastOrthographicSize = Mathf.RoundToInt(camera.orthographicSize);
				centreRegionBlock = newCentreRegionBlock;
				float maxVisibleRegionBlockDistance = camera.orthographicSize * ((float)Screen.width / Screen.height);
				List<RegionBlock> frontier = new List<RegionBlock>() { centreRegionBlock };
				List<RegionBlock> checkedBlocks = new List<RegionBlock>() { centreRegionBlock };
				while (frontier.Count > 0) {
					RegionBlock currentRegionBlock = frontier[0];
					frontier.RemoveAt(0);
					visibleRegionBlocks.Add(currentRegionBlock);
					float currentRegionBlockCameraDistance = Vector2.Distance(currentRegionBlock.averagePosition, camera.transform.position);
					foreach (RegionBlock nBlock in currentRegionBlock.surroundingRegionBlocks) {
						if (currentRegionBlockCameraDistance <= maxVisibleRegionBlockDistance) {
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
				SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, false);
			}
		}

		public void SetTileBrightness(float time, bool forceUpdate) {
			Color newColour = GetTileColourAtHour(time);
			foreach (RegionBlock visibleRegionBlock in visibleRegionBlocks) {
				if (forceUpdate || !Mathf.Approximately(visibleRegionBlock.lastBrightnessUpdate, time)) {
					visibleRegionBlock.lastBrightnessUpdate = time;
					foreach (Tile.Tile tile in visibleRegionBlock.tiles) {
						tile.SetColour(newColour, Mathf.FloorToInt(time));
					}
				}
			}
			foreach (Life life in GameManager.Get<LifeManager>().life) {
				life.SetColour(life.Tile.sr.color);
			}
			GameManager.Get<CameraManager>().camera.backgroundColor = newColour * 0.5f;
		}

		private readonly Dictionary<int, Vector2> shadowDirectionAtHour = new Dictionary<int, Vector2>();
		private bool shadowDirectionsCalculated = false;
		public void DetermineShadowDirectionsAtHour(float equatorOffset) {
			for (int h = 0; h < 24; h++) {
				float hShadow = (2f * ((h - 12f) / 24f)) * (1f - Mathf.Pow(equatorOffset, 2f));
				float vShadow = Mathf.Pow(2f * ((h - 12f) / 24f), 2f) * equatorOffset + (equatorOffset / 2f);
				shadowDirectionAtHour.Add(h, new Vector2(hShadow, vShadow) * 5f);
			}
			shadowDirectionsCalculated = true;
		}

		public float CalculateBrightnessLevelAtHour(float time) {
			return ((-(1f / 144f)) * Mathf.Pow(((1 + (24 - (1 - time))) % 24) - 12, 2) + 1.2f);
		}

		public Color GetTileColourAtHour(float time) {
			float r = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.4f * time + 7.2f), 10)) / 5f, 0f, 1f);
			float g = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.5f * time + 6), 10)) / 5f - 0.2f, 0f, 1f);
			float b = Mathf.Clamp((-1.5f * Mathf.Pow(Mathf.Cos(CalculateBrightnessLevelAtHour(2 * time + 12) / 1.5f), 3) + 1.65f * (CalculateBrightnessLevelAtHour(time) / 2f)) + 0.7f, 0f, 1f);
			return new Color(r, g, b, 1f);
		}

		public bool TileCanShadowTiles(Tile.Tile tile) {
			return tile.surroundingTiles.Any(nTile => nTile != null && !nTile.blocksLight) && (tile.blocksLight || tile.HasRoof());
		}

		public bool TileCanBeShadowed(Tile.Tile tile) {
			return !tile.blocksLight || (!tile.blocksLight && tile.HasRoof());
		}

		public void RecalculateLighting(List<Tile.Tile> tilesToRecalculate, bool setBrightnessAtEnd, bool forceBrightnessUpdate = false) {
			List<Tile.Tile> shadowSourceTiles = DetermineShadowSourceTiles(tilesToRecalculate);
			DetermineShadowTiles(shadowSourceTiles, setBrightnessAtEnd, forceBrightnessUpdate);
		}

		public List<Tile.Tile> DetermineShadowSourceTiles(List<Tile.Tile> tilesToRecalculate) {
			List<Tile.Tile> shadowSourceTiles = new List<Tile.Tile>();
			foreach (Tile.Tile tile in tilesToRecalculate) {
				if (tile != null && TileCanShadowTiles(tile)) {
					shadowSourceTiles.Add(tile);
				}
			}
			return shadowSourceTiles;
		}

		private static readonly float distanceIncreaseAmount = 0.1f; // 0.1f
		private void DetermineShadowTiles(List<Tile.Tile> shadowSourceTiles, bool setBrightnessAtEnd, bool forceBrightnessUpdate) {
			if (!shadowDirectionsCalculated) {
				DetermineShadowDirectionsAtHour(GameManager.Get<MapManager>().Map.MapData.equatorOffset);
			}
			for (int h = 0; h < 24; h++) {
				Vector2 hourDirection = shadowDirectionAtHour[h];
				float maxShadowDistanceAtHour = hourDirection.magnitude * 5f + (Mathf.Pow(h - 12, 2) / 6f);
				float shadowedBrightnessAtHour = Mathf.Clamp(1 - (0.6f * CalculateBrightnessLevelAtHour(h)) + 0.3f, 0, 1);

				foreach (Tile.Tile shadowSourceTile in shadowSourceTiles) {
					Vector2 shadowSourceTilePosition = shadowSourceTile.obj.transform.position;
					bool shadowedAnyTile = false;

					List<Tile.Tile> shadowTiles = new List<Tile.Tile>();
					for (float distance = 0; distance <= maxShadowDistanceAtHour; distance += distanceIncreaseAmount) {
						Vector2 nextTilePosition = shadowSourceTilePosition + (hourDirection * distance);
						if (nextTilePosition.x < 0 || nextTilePosition.x >= MapData.mapSize || nextTilePosition.y < 0 || nextTilePosition.y >= MapData.mapSize) {
							break;
						}
						Tile.Tile tileToShadow = GetTileFromPosition(nextTilePosition);
						if (shadowTiles.Contains(tileToShadow)) {
							distance += distanceIncreaseAmount;
							continue;
						}
						if (tileToShadow != shadowSourceTile) {
							float newBrightness = 1;
							if (TileCanBeShadowed(tileToShadow)) {
								shadowedAnyTile = true;
								newBrightness = shadowedBrightnessAtHour;
								if (tileToShadow.brightnessAtHour.ContainsKey(h)) {
									tileToShadow.brightnessAtHour[h] = Mathf.Min(tileToShadow.brightnessAtHour[h], newBrightness);
								} else {
									tileToShadow.brightnessAtHour.Add(h, newBrightness);
								}
								shadowTiles.Add(tileToShadow);
							} else {
								if (shadowedAnyTile || Vector2.Distance(tileToShadow.position, shadowSourceTile.position) > maxShadowDistanceAtHour) {
									if (tileToShadow.blockingShadowsFrom.ContainsKey(h)) {
										tileToShadow.blockingShadowsFrom[h].Add(shadowSourceTile);
									} else {
										tileToShadow.blockingShadowsFrom.Add(h, new List<Tile.Tile>() { shadowSourceTile });
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
								tileToShadow.shadowsFrom.Add(h, new Dictionary<Tile.Tile, float>() { { shadowSourceTile, newBrightness } });
							}
						}
					}
					if (shadowSourceTile.shadowsTo.ContainsKey(h)) {
						shadowSourceTile.shadowsTo[h].AddRange(shadowTiles);
					} else {
						shadowSourceTile.shadowsTo.Add(h, shadowTiles);
					}
					shadowSourceTile.shadowsTo[h] = shadowSourceTile.shadowsTo[h].Distinct().ToList();
				}
			}
			if (setBrightnessAtEnd) {
				SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, forceBrightnessUpdate);
			}
		}

		public void RemoveTileBrightnessEffect(Tile.Tile tile) {
			List<Tile.Tile> tilesToRecalculateShadowsFor = new List<Tile.Tile>();
			for (int h = 0; h < 24; h++) {
				if (tile.shadowsTo.ContainsKey(h)) {
					foreach (Tile.Tile nTile in tile.shadowsTo[h]) {
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

		public Tile.Tile GetTileFromPosition(Vector2 position) {
			return GetTileFromPosition(position.x, position.y);
		}

		public Tile.Tile GetTileFromPosition(float x, float y) {
			return GetTileFromPosition(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
		}

		public Tile.Tile GetTileFromPosition(int x, int y) {
			return sortedTiles[Mathf.Clamp(y, 0, MapData.mapSize - 1)][Mathf.Clamp(x, 0, MapData.mapSize - 1)];
		}

		public static int GetRandomMapSeed() {
			return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
	}
}
