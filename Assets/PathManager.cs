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
		PathfindingTile currentTile = new PathfindingTile(startTile,null,0);

		List<PathfindingTile> checkedTiles = new List<PathfindingTile>();
		checkedTiles.Add(currentTile);
		List<PathfindingTile> frontier = new List<PathfindingTile>();
		frontier.Add(currentTile);

		List<TileManager.Tile> path = new List<TileManager.Tile>();

		while (frontier.Count > 0) {
			currentTile = frontier[0];
			frontier.RemoveAt(0);

			if (currentTile.tile == endTile) {
				while (currentTile != null) {
					currentTile.tile.obj.GetComponent<SpriteRenderer>().color = Color.black;
					path.Add(currentTile.tile);
					currentTile = currentTile.cameFrom;
				}
				//startTile.obj.GetComponent<SpriteRenderer>().color = Color.blue;
				path.Reverse();
				path[0].obj.GetComponent<SpriteRenderer>().color = Color.blue;
				return path;
			}

			foreach (TileManager.Tile nTile in currentTile.tile.horizontalSurroundingTiles) {
				if (nTile != null && checkedTiles.Find(o => o.tile == nTile) == null && nTile.walkable) {
					float cost = Vector2.Distance(nTile.obj.transform.position,endTile.obj.transform.position) - (nTile.walkSpeed * 10f);
					PathfindingTile pTile = new PathfindingTile(nTile,currentTile,cost);
					frontier.Add(pTile);
					checkedTiles.Add(pTile);
					nTile.obj.GetComponent<SpriteRenderer>().color = Color.red;
				}
			}
			frontier = frontier.OrderBy(o => o.cost).ToList();
		}
		return new List<TileManager.Tile>();
	}
}
