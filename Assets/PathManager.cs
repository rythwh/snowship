﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathManager : MonoBehaviour {

	public class PathfindingTile {
		public TileManager.Tile tile;
		public PathfindingTile cameFrom;
		public int pathDistance = 0;
		public float cost;

		public PathfindingTile(TileManager.Tile tile,PathfindingTile cameFrom,float cost) {
			this.tile = tile;
			this.cameFrom = cameFrom;
			this.cost = cost;
			if (cameFrom != null) {
				pathDistance = cameFrom.pathDistance + 1;
			} else {
				pathDistance = 0;
			}
		}
	}

	public List<TileManager.Tile> FindPathToTile(TileManager.Tile startTile, TileManager.Tile endTile, bool allowEndTileNonWalkable) {

		bool stop = false;
		if (!allowEndTileNonWalkable && (!endTile.walkable || startTile.region != endTile.region)) {
			stop = true;
		}
		if (!startTile.walkable && endTile.walkable) {
			stop = false;
		}
		if (stop) {
			return new List<TileManager.Tile>();
		}

		PathfindingTile currentTile = new PathfindingTile(startTile,null,0);

		List<PathfindingTile> frontier = new List<PathfindingTile>() { currentTile };
		List<PathfindingTile> checkedTiles = new List<PathfindingTile>() { currentTile };

		List<TileManager.Tile> path = new List<TileManager.Tile>();

		bool walkable = startTile.walkable;

		while (frontier.Count > 0) {
			currentTile = frontier[0];
			frontier.RemoveAt(0);

			if (!walkable && currentTile.tile.walkable) {
				walkable = true;
			}

			if (currentTile.tile == endTile) {
				while (currentTile.cameFrom != null) {
					path.Add(currentTile.tile);
					currentTile = currentTile.cameFrom;
				}
				path.Reverse();
				return path;
			}

			foreach (TileManager.Tile nTile in (currentTile.tile.surroundingTiles.Find(tile => tile != null && !tile.walkable) != null ? currentTile.tile.horizontalSurroundingTiles : currentTile.tile.surroundingTiles)) {
				if (nTile != null && checkedTiles.Find(checkedTile => checkedTile.tile == nTile) == null && (allowEndTileNonWalkable && nTile == endTile ? true : (walkable ? nTile.walkable : true))) {
					float cost = (Vector2.Distance(nTile.obj.transform.position,endTile.obj.transform.position) * 1) + (RegionBlockDistance(nTile.regionBlock,endTile.regionBlock,true,true,true) * 10) + (currentTile.pathDistance * 5) - (nTile.walkSpeed * 15);
					PathfindingTile pTile = new PathfindingTile(nTile,currentTile,cost);
					frontier.Add(pTile);
					checkedTiles.Add(pTile);
				}
			}
			frontier = frontier.OrderBy(frontierTile => frontierTile.cost).ToList();
		}
		return new List<TileManager.Tile>();
	}

	public enum WalkableSetting { Walkable, NonWalkable, Both };
	public enum DirectionSetting { Horizontal, Diagonal, Both };

	public bool PathExists(TileManager.Tile startTile,TileManager.Tile endTile, bool breakTooLong,int breakAfterTiles, WalkableSetting walkableSetting,DirectionSetting directionSetting) {

		PathfindingTile currentTile = new PathfindingTile(startTile,null,0);

		List<PathfindingTile> frontier = new List<PathfindingTile>() { currentTile };
		List<PathfindingTile> checkedTiles = new List<PathfindingTile>() { currentTile };

		int breakCounter = 0;

		while (frontier.Count > 0) {
			currentTile = frontier[0];
			frontier.RemoveAt(0);

			if (breakTooLong) {
				if (breakCounter >= breakAfterTiles) {
					break;
				}
				breakCounter += 1;
			}

			if (currentTile.tile == endTile) {
				return true;
			}

			foreach (TileManager.Tile nTile in (directionSetting == DirectionSetting.Horizontal ? currentTile.tile.horizontalSurroundingTiles : (directionSetting == DirectionSetting.Diagonal ? currentTile.tile.diagonalSurroundingTiles : currentTile.tile.surroundingTiles))) {
				if (nTile != null && checkedTiles.Find(o => o.tile == nTile) == null && (walkableSetting == WalkableSetting.Walkable ? nTile.walkable : (walkableSetting == WalkableSetting.NonWalkable ? !nTile.walkable : true))) {
					PathfindingTile pTile = new PathfindingTile(nTile,null,Vector2.Distance(nTile.obj.transform.position,endTile.obj.transform.position));
					frontier.Add(pTile);
					checkedTiles.Add(pTile);
				}
			}
			frontier = frontier.OrderBy(o => o.cost).ToList();
		}
		return false;
	}

	public class PathfindingRegionBlock {
		public TileManager.RegionBlock regionBlock;
		public PathfindingRegionBlock cameFrom;
		public float cost;

		public PathfindingRegionBlock(TileManager.RegionBlock regionBlock,PathfindingRegionBlock cameFrom, float cost) {
			this.regionBlock = regionBlock;
			this.cameFrom = cameFrom;
			this.cost = cost;
		}
	}

	public float RegionBlockDistance(TileManager.RegionBlock startRegionBlock,TileManager.RegionBlock endRegionBlock,bool careAboutWalkability,bool walkable, bool allowEndTileNonWalkable) {

		if (!allowEndTileNonWalkable && (careAboutWalkability && (startRegionBlock.tileType.walkable != endRegionBlock.tileType.walkable || startRegionBlock.tiles[0].region != endRegionBlock.tiles[0].region))) {
			return Mathf.Infinity;
		}

		PathfindingRegionBlock currentRegionBlock = new PathfindingRegionBlock(startRegionBlock,null,0);
		List<PathfindingRegionBlock> frontier = new List<PathfindingRegionBlock>() { currentRegionBlock };
		List<TileManager.RegionBlock> checkedBlocks = new List<TileManager.RegionBlock>() { startRegionBlock };
		while (frontier.Count > 0) {
			currentRegionBlock = frontier[0];
			if (currentRegionBlock.regionBlock == endRegionBlock) {
				float distance = 0;
				while (currentRegionBlock.cameFrom != null) {
					distance += Vector2.Distance(currentRegionBlock.regionBlock.averagePosition,currentRegionBlock.cameFrom.regionBlock.averagePosition);
					currentRegionBlock = currentRegionBlock.cameFrom;
				}
				return distance;
			}
			frontier.RemoveAt(0);
			foreach (TileManager.RegionBlock regionBlock in currentRegionBlock.regionBlock.horizontalSurroundingRegionBlocks) {
				if ((allowEndTileNonWalkable && regionBlock == endRegionBlock ? true : (careAboutWalkability ? regionBlock.tileType.walkable == walkable : true)) && !checkedBlocks.Contains(regionBlock)) {
					PathfindingRegionBlock pRegionBlock = new PathfindingRegionBlock(regionBlock,currentRegionBlock,Vector2.Distance(regionBlock.averagePosition,endRegionBlock.averagePosition));
					frontier.Add(pRegionBlock);
					checkedBlocks.Add(regionBlock);
				}
			}
			frontier = frontier.OrderBy(regionBlock => regionBlock.cost).ToList();
		}
		return Mathf.Infinity;
	}
}