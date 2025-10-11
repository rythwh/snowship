using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;
using Snowship.NPath;

namespace Snowship.NMap
{
	public partial class Map
	{
		public void RecalculateRegionsAtTile(Tile tile) {
			if (!tile.walkable) {
				List<Tile> orderedSurroundingTiles = new List<Tile> {
					tile.SurroundingTiles[EGridConnectivity.EightWay][0], tile.SurroundingTiles[EGridConnectivity.EightWay][4], tile.SurroundingTiles[EGridConnectivity.EightWay][1], tile.SurroundingTiles[EGridConnectivity.EightWay][5],
					tile.SurroundingTiles[EGridConnectivity.EightWay][2], tile.SurroundingTiles[EGridConnectivity.EightWay][6], tile.SurroundingTiles[EGridConnectivity.EightWay][3], tile.SurroundingTiles[EGridConnectivity.EightWay][7]
				};
				List<List<Tile>> separateTileGroups = new List<List<Tile>>();
				int groupIndex = 0;
				for (int i = 0; i < orderedSurroundingTiles.Count; i++) {
					if (groupIndex == separateTileGroups.Count) {
						separateTileGroups.Add(new List<Tile>());
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
				List<Tile> horizontalGroups = new List<Tile>();
				foreach (List<Tile> tileGroup in separateTileGroups) {
					List<Tile> horizontalTilesInGroup = tileGroup.Where(groupTile => tile.SurroundingTiles[EGridConnectivity.FourWay].Contains(groupTile)).ToList();
					if (horizontalTilesInGroup.Count > 0) {
						horizontalGroups.Add(horizontalTilesInGroup[0]);
					}
				}
				if (horizontalGroups.Count > 1) {
					List<Tile> removeTiles = new List<Tile>();
					foreach (Tile startTile in horizontalGroups) {
						if (!removeTiles.Contains(startTile)) {
							foreach (Tile endTile in horizontalGroups) {
								if (!removeTiles.Contains(endTile) && startTile != endTile) {
									if (Path.PathExists(startTile, endTile, true, MapData.mapSize, WalkableSetting.Walkable, EGridConnectivity.FourWay)) {
										removeTiles.Add(endTile);
									}
								}
							}
						}
					}
					foreach (Tile removeTile in removeTiles) {
						horizontalGroups.Remove(removeTile);
					}
					if (horizontalGroups.Count > 1) {
						// TODO Fix -- MapGenerator.SetTileRegions(false, true);
					}
				}
			}
		}
	}
}
