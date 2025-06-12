using System.Collections.Generic;

public class River {
	public Tile startTile;
	public Tile centreTile;
	public Tile endTile;
	public List<Tile> tiles = new List<Tile>();
	public int expandRadius;
	public bool ignoreStone;

	public River(Tile startTile, Tile centreTile, Tile endTile, int expandRadius, bool ignoreStone, Map map, bool performPathfinding) {
		this.startTile = startTile;
		this.centreTile = centreTile;
		this.endTile = endTile;
		this.expandRadius = expandRadius;
		this.ignoreStone = ignoreStone;

		if (performPathfinding) {
			if (centreTile != null) {
				tiles.AddRange(map.RiverPathfinding(startTile, centreTile, expandRadius, ignoreStone));
				tiles.AddRange(map.RiverPathfinding(centreTile, endTile, expandRadius, ignoreStone));
			} else {
				tiles = map.RiverPathfinding(startTile, endTile, expandRadius, ignoreStone);
			}
		}
	}

	protected River() {
	}
}