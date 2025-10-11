using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;

namespace Snowship
{
	public static class Search
	{
		public static List<Tile> FloodSearch(
			Tile originTile,
			EGridConnectivity connectivity,
			Func<Tile, bool> predicate
		) {

			List<Tile> output = new();
			Queue<Tile> frontier = new();
			HashSet<Tile> visited = new() { originTile };

			frontier.Enqueue(originTile);

			while (frontier.Count > 0) {

				Tile currentTile = frontier.Dequeue();

				output.Add(currentTile);

				foreach (Tile nTile in currentTile.SurroundingTiles[connectivity]) {
					if (nTile == null) {
						continue;
					}
					if (!visited.Add(nTile)) {
						continue;
					}
					if (predicate(nTile)) {
						frontier.Enqueue(nTile);
					}
				}
			}

			return output;
		}
	}
}
