using System.Collections.Generic;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private void CreateRegionBlocks(MapGenContext context) {
			const int regionBlockSize = 10;

			context.Map.regionBlocks.Clear();
			context.Map.squareRegionBlocks.Clear();

			for (int sectionY = 0; sectionY < context.Data.mapSize; sectionY += regionBlockSize) {
				for (int sectionX = 0; sectionX < context.Data.mapSize; sectionX += regionBlockSize) {
					RegionBlock regionBlock = new RegionBlock(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass));
					RegionBlock squareRegionBlock = new RegionBlock(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass));
					for (int y = sectionY; y < sectionY + regionBlockSize && y < context.Data.mapSize; y++) {
						for (int x = sectionX; x < sectionX + regionBlockSize && x < context.Data.mapSize; x++) {
							regionBlock.tiles.Add(context.Map.sortedTiles[y][x]);
							squareRegionBlock.tiles.Add(context.Map.sortedTiles[y][x]);
							context.Map.sortedTiles[y][x].squareRegionBlock = squareRegionBlock;
						}
					}
					context.Map.regionBlocks.Add(regionBlock);
					context.Map.squareRegionBlocks.Add(squareRegionBlock);
				}
			}
			foreach (RegionBlock squareRegionBlock in context.Map.squareRegionBlocks) {
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
			List<RegionBlock> removeRegionBlocks = new List<RegionBlock>();
			List<RegionBlock> newRegionBlocks = new List<RegionBlock>();
			foreach (RegionBlock regionBlock in context.Map.regionBlocks) {
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
							RegionBlock unwalkableRegionBlock = new RegionBlock(unwalkableTile.tileType);
							Tile currentTile = unwalkableTile;
							List<Tile> frontier = new List<Tile> { currentTile };
							List<Tile> checkedTiles = new List<Tile> { currentTile };
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
							RegionBlock walkableRegionBlock = new RegionBlock(walkableTile.tileType);
							Tile currentTile = walkableTile;
							List<Tile> frontier = new List<Tile> { currentTile };
							List<Tile> checkedTiles = new List<Tile> { currentTile };
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
				context.Map.regionBlocks.Remove(regionBlock);
			}
			removeRegionBlocks.Clear();
			context.Map.regionBlocks.AddRange(newRegionBlocks);
			foreach (RegionBlock regionBlock in context.Map.regionBlocks) {
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
	}
}
