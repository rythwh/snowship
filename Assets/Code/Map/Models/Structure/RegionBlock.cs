using System.Collections.Generic;
using UnityEngine;

public class RegionBlock : Region {

	public Vector2 averagePosition = new Vector2(0, 0);
	public List<RegionBlock> surroundingRegionBlocks = new List<RegionBlock>();
	public List<RegionBlock> horizontalSurroundingRegionBlocks = new List<RegionBlock>();

	public float lastBrightnessUpdate;

	public RegionBlock(TileType regionTileType, int regionID) : base(regionTileType, regionID) {
		lastBrightnessUpdate = -1;
	}
}