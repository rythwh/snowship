using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Geography;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private void CreateResourceVeins(MapGenContext context) {

			List<Tile> stoneTiles = new List<Tile>();
			foreach (RegionBlock regionBlock in context.Map.regionBlocks) {
				if (regionBlock.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					stoneTiles.AddRange(regionBlock.tiles);
				}
			}
			if (stoneTiles.Count > 0) {
				foreach (ResourceVein resourceVein in ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Stone)) {
					PlaceResourceVeins(context, resourceVein, stoneTiles);
				}
			}

			List<Tile> coastTiles = new List<Tile>();
			foreach (RegionBlock regionBlock in context.Map.regionBlocks) {
				if (regionBlock.tileType.groupType != TileTypeGroup.TypeEnum.Water) {
					continue;
				}
				foreach (Tile tile in regionBlock.tiles) {
					if (tile.SurroundingTiles[EGridConnectivity.EightWay].Find(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) != null) {
						coastTiles.Add(tile);
					}
				}
			}
			if (coastTiles.Count <= 0) {
				return;
			}
			foreach (ResourceVein resourceVein in ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Coast)) {
				PlaceResourceVeins(context, resourceVein, coastTiles);
			}
		}

		// TODO Add resources to ground tiles, so they can be in both the Stone and the Ground, making the valid start tiles unnecessary
		private void PlaceResourceVeins(MapGenContext context, ResourceVein resourceVeinData, List<Tile> mediumTiles)
		{
			List<Tile> validVeinStartTiles = mediumTiles
				.Where(tile => resourceVeinData.tileTypes.ContainsKey(tile.tileType.groupType))
				.Where(tile => !resourceVeinData.tileTypes.ContainsValue(tile.tileType.type))
				.Where(tile => ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](tile))
				.ToList();
			if (validVeinStartTiles.Count <= 0) {
				return;
			}

			int numVeins = Mathf.CeilToInt(context.Data.mapSize / (float)resourceVeinData.numVeinsByMapSize);
			for (int i = 0; i < numVeins; i++) {

				int veinSizeMax = resourceVeinData.veinSize + Random.Range(-resourceVeinData.veinSizeRange, resourceVeinData.veinSizeRange);

				if (validVeinStartTiles.Count <= 0) {
					return;
				}
				Tile veinStartTile = validVeinStartTiles.RandomElement();

				List<Tile> removeTiles = new();
				foreach (Tile validStartTiles in validVeinStartTiles) {
					if (Vector2.Distance(validStartTiles.PositionGrid, veinStartTile.PositionGrid) < resourceVeinData.veinDistance) {
						removeTiles.Add(validStartTiles);
					}
				}
				foreach (Tile removeTile in removeTiles) {
					validVeinStartTiles.Remove(removeTile);
				}
				removeTiles.Clear();

				List<Tile> frontier = new() { veinStartTile };
				HashSet<Tile> checkedTiles = new();

				int veinSize = 0;

				while (frontier.Count > 0) {

					Tile currentTile = frontier.RandomElement();

					frontier.RemoveAt(0);
					checkedTiles.Add(currentTile);

					currentTile.SetTileType(TileType.GetTileTypeByEnum(resourceVeinData.tileTypes[currentTile.tileType.groupType]), false, false, false);

					foreach (Tile nTile in currentTile.SurroundingTiles[EGridConnectivity.FourWay]) {
						if (nTile == null || checkedTiles.Contains(nTile) || resourceVeinData.tileTypes.Values.Contains(nTile.tileType.type)) {
							continue;
						}
						if (resourceVeinData.tileTypes.ContainsKey(nTile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](nTile)) {
							frontier.Add(nTile);
						}
					}

					veinSize += 1;

					if (veinSize >= veinSizeMax) {
						break;
					}
				}
			}
		}
	}
}
