using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		internal static void SetTileRegions(MapGenContext context, bool splitByTileType, bool removeNonWalkableRegions) {
			context.Map.regions.Clear();

			EstablishInitialRegions(context, splitByTileType);
			FindConnectedRegions(context, splitByTileType);
			MergeConnectedRegions(context, splitByTileType);

			RemoveEmptyRegions(context);

			if (removeNonWalkableRegions) {
				RemoveNonWalkableRegions(context);
			}
		}

		private static void EstablishInitialRegions(MapGenContext context, bool splitByTileType) {
			foreach (Tile tile in context.Map.tiles) { // Go through all tiles
				List<Region> foundRegions = new List<Region>(); // For each tile, store a list of the regions around them
				for (int i = 0; i < tile.SurroundingTiles[EGridConnectivity.EightWay].Count; i++) { // Go through the tiles around each tile
					Tile nTile = tile.SurroundingTiles[EGridConnectivity.EightWay][i];
					if (nTile != null && (splitByTileType ? tile.tileType == nTile.tileType : tile.walkable == nTile.walkable) && (i == 2 || i == 3 /*|| i == 5 || i == 6 */)) { // Uncomment indexes 5 and 6 to enable 8-connectivity connected-component labeling -- If the tiles have the same type
						if (nTile.region != null && !foundRegions.Contains(nTile.region)) { // If the tiles have a region, and it hasn't already been looked at
							foundRegions.Add(nTile.region); // Add the surrounding tile's region to the regions found around the original tile
						}
					}
				}
				if (foundRegions.Count <= 0) { // If there weren't any tiles with the same region/tiletype found around them, make a new region for this tile
					tile.ChangeRegion(new Region(tile.tileType), false, false);
				} else if (foundRegions.Count == 1) { // If there was a single region found around them, give them that region
					tile.ChangeRegion(foundRegions[0], false, false);
				} else if (foundRegions.Count > 1) { // If there was more than one region found around them, give them the region with the lowest ID
					tile.ChangeRegion(FindLargestRegion(foundRegions), false, false);
				}
			}
		}

		private static void FindConnectedRegions(MapGenContext context, bool splitByTileType) {
			foreach (Region region in context.Map.regions) {
				foreach (Tile tile in region.tiles) {
					foreach (Tile nTile in tile.SurroundingTiles[EGridConnectivity.FourWay]) {
						if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains(nTile.region) && (splitByTileType ? tile.tileType == nTile.tileType : tile.walkable == nTile.walkable)) {
							region.connectedRegions.Add(nTile.region);
						}
					}
				}
			}
		}

		private static void MergeConnectedRegions(MapGenContext context, bool splitByTileType) {
			while (context.Map.regions.Where(region => region.connectedRegions.Count > 0).ToList().Count > 0) { // While there are regions that have connected regions
				foreach (Region region in context.Map.regions) { // Go through each region
					if (region.connectedRegions.Count > 0) { // If this region has connected regions
						Region lowestRegion = FindLargestRegion(region.connectedRegions); // Find the lowest ID region from the connected regions
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
				FindConnectedRegions(context, splitByTileType); // Find the new connected regions
			}
		}

		private static Region FindLargestRegion(List<Region> searchRegions) {
			if (searchRegions is { Count: <= 0 }) {
				return null;
			}
			Region largestRegion = searchRegions[0];
			foreach (Region region in searchRegions) {
				if (region.tiles.Count > largestRegion.tiles.Count) {
					largestRegion = region;
				}
			}
			return largestRegion;
		}

		private static void RemoveEmptyRegions(MapGenContext context) {
			List<Region> regionsToRemove = null;
			foreach (Region region in context.Map.regions) {
				if (region.tiles.Count > 0) {
					continue;
				}
				regionsToRemove ??= new List<Region>();
				regionsToRemove.Add(region);
			}
			if (regionsToRemove == null) {
				return;
			}
			foreach (Region region in regionsToRemove) {
				context.Map.regions.Remove(region);
			}
		}

		private static void RemoveNonWalkableRegions(MapGenContext context) {
			List<Region> regionsToRemove = null;
			foreach (Region region in context.Map.regions) {
				if (region.tileType.walkable) {
					continue;
				}
				foreach (Tile tile in region.tiles) {
					tile.ChangeRegion(null, false, false);
				}
				regionsToRemove ??= new List<Region>();
				regionsToRemove.Add(region);
			}
			if (regionsToRemove == null) {
				return;
			}
			foreach (Region region in regionsToRemove) {
				context.Map.regions.Remove(region);
			}
		}

		private void ReduceNoise(MapGenContext context) {
			ReduceNoise(context, Mathf.RoundToInt(context.Data.mapSize / 5f), new List<TileTypeGroup.TypeEnum>() { TileTypeGroup.TypeEnum.Water, TileTypeGroup.TypeEnum.Stone, TileTypeGroup.TypeEnum.Ground });
			ReduceNoise(context, Mathf.RoundToInt(context.Data.mapSize / 2f), new List<TileTypeGroup.TypeEnum>() { TileTypeGroup.TypeEnum.Water });
		}

		private void ReduceNoise(MapGenContext context, int removeRegionsBelowSize, List<TileTypeGroup.TypeEnum> tileTypeGroupsToRemove) {
			foreach (Region region in context.Map.regions) {
				if (tileTypeGroupsToRemove.Contains(region.tileType.groupType)) {
					if (region.tiles.Count < removeRegionsBelowSize) {
						/* --- This code is essentially copied from FindConnectedRegions() */
						foreach (Tile tile in region.tiles) {
							foreach (Tile nTile in tile.SurroundingTiles[EGridConnectivity.FourWay]) {
								if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains(nTile.region)) {
									region.connectedRegions.Add(nTile.region);
								}
							}
						}
						/* --- This code is essentially copied from MergeConnectedRegions() */
						if (region.connectedRegions.Count > 0) {
							Region lowestRegion = FindLargestRegion(region.connectedRegions);
							foreach (Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
								tile.ChangeRegion(lowestRegion, true, false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
					}
				}
			}
			RemoveEmptyRegions(context);
		}
	}
}
