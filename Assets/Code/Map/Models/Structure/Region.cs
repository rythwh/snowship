using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NColony;

public class Region {
	public TileType tileType;
	public List<Tile> tiles = new List<Tile>();
	public int id;

	public List<Region> connectedRegions = new List<Region>();

	public bool visible;

	public Region(TileType regionTileType, int regionID) {
		tileType = regionTileType;
		id = regionID;
	}

	public bool IsVisibleToAColonist() {
		if (tileType.walkable) {
			foreach (Colonist colonist in Colonist.colonists) {
				if (colonist.Tile.region == this) {
					return true;
				}
			}
		}
		return false;
	}

	public void SetVisible(bool visible, bool bitmasking, bool recalculateLighting) {
		this.visible = visible;

		List<Tile> tilesToModify = new List<Tile>();
		foreach (Tile tile in tiles) {
			tile.SetVisible(this.visible);

			tilesToModify.Add(tile);
			tilesToModify.AddRange(tile.surroundingTiles);
		}

		tilesToModify = tilesToModify.Distinct().ToList();

		if (bitmasking) {
			GameManager.Get<ColonyManager>().colony.map.Bitmasking(tilesToModify, true, false);
		}

		if (recalculateLighting) {
			GameManager.Get<ColonyManager>().colony.map.RecalculateLighting(tilesToModify, true);
		}

	}
}