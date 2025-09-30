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
					if (tile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) != null) {
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

		// TODO Refactor placement to just be random neighbours, don't need flood-filling
		// TODO Add resources to ground tiles, so they can be in both the Stone and the Ground, making the valid start tiles unnecessary
		private void PlaceResourceVeins(MapGenContext context, ResourceVein resourceVeinData, List<Tile> mediumTiles) {
			List<Tile> previousVeinStartTiles = new List<Tile>();
			for (int i = 0; i < Mathf.CeilToInt(context.Data.mapSize / (float)resourceVeinData.numVeinsByMapSize); i++) {
				List<Tile> validVeinStartTiles = mediumTiles.Where(tile => !resourceVeinData.tileTypes.ContainsValue(tile.tileType.type) && resourceVeinData.tileTypes.ContainsKey(tile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](tile)).ToList();
				foreach (Tile previousVeinStartTile in previousVeinStartTiles) {
					List<Tile> removeTiles = new List<Tile>();
					foreach (Tile validVeinStartTile in validVeinStartTiles) {
						if (Vector2.Distance(validVeinStartTile.obj.transform.position, previousVeinStartTile.obj.transform.position) < resourceVeinData.veinDistance) {
							removeTiles.Add(validVeinStartTile);
						}
					}
					foreach (Tile removeTile in removeTiles) {
						validVeinStartTiles.Remove(removeTile);
					}
				}
				if (validVeinStartTiles.Count > 0) {

					int veinSizeMax = resourceVeinData.veinSize + context.Random.NextInt(-resourceVeinData.veinSizeRange, resourceVeinData.veinSizeRange);

					Tile veinStartTile = validVeinStartTiles[context.Random.NextInt(0, validVeinStartTiles.Count)];
					previousVeinStartTiles.Add(veinStartTile);

					List<Tile> frontier = new List<Tile>() { veinStartTile };
					List<Tile> checkedTiles = new List<Tile>();
					Tile currentTile = veinStartTile;

					int veinSize = 0;

					while (frontier.Count > 0) {
						currentTile = frontier[context.Random.NextInt(0, frontier.Count)];
						frontier.RemoveAt(0);
						checkedTiles.Add(currentTile);

						currentTile.SetTileType(TileType.GetTileTypeByEnum(resourceVeinData.tileTypes[currentTile.tileType.groupType]), false, false, false);

						foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && !resourceVeinData.tileTypes.Values.Contains(nTile.tileType.type)) {
								if (resourceVeinData.tileTypes.ContainsKey(nTile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](nTile)) {
									frontier.Add(nTile);
								}
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
}
