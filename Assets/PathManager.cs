using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathManager : MonoBehaviour {

	public class PathfindingTile {
		public TileManager.Tile tile;
		public PathfindingTile cameFrom;
		public float cost;

		public PathfindingTile(TileManager.Tile tile,PathfindingTile cameFrom,float cost) {
			this.tile = tile;
			this.cameFrom = cameFrom;
			this.cost = cost;
		}
	}

	public List<TileManager.Tile> FindPathToTile(TileManager.Tile startTile, TileManager.Tile endTile) {

		if (!endTile.walkable) {
			return new List<TileManager.Tile>();
		}

		PathfindingTile currentTile = new PathfindingTile(startTile,null,0);

		List<PathfindingTile> checkedTiles = new List<PathfindingTile>();
		checkedTiles.Add(currentTile);
		List<PathfindingTile> frontier = new List<PathfindingTile>();
		frontier.Add(currentTile);

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

			foreach (TileManager.Tile nTile in (currentTile.tile.surroundingTiles.Find(o => o != null && !o.walkable) != null ? currentTile.tile.horizontalSurroundingTiles : currentTile.tile.surroundingTiles)) {
				if (nTile != null && checkedTiles.Find(o => o.tile == nTile) == null && (walkable ? nTile.walkable : true)) {
					float cost = Vector2.Distance(nTile.obj.transform.position,endTile.obj.transform.position) - (nTile.walkSpeed * 10f);
					PathfindingTile pTile = new PathfindingTile(nTile,currentTile,cost);
					frontier.Add(pTile);
					checkedTiles.Add(pTile);
				}
			}
			frontier = frontier.OrderBy(o => o.cost).ToList();
		}
		return new List<TileManager.Tile>();
	}
}
