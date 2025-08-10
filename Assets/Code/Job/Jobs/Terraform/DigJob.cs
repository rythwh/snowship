using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Tile;
using Snowship.NResource;
using Snowship.NUtilities;
using Random = UnityEngine.Random;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Dig")]
	public class DigJobDefinition : JobDefinition<DigJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.CoastalWater,
			Selectable.SelectionConditions.NoHole,
			Selectable.SelectionConditions.NotDugPreviously,
			Selectable.SelectionConditions.NotStone,
			Selectable.SelectionConditions.NoObjects,
			Selectable.SelectionConditions.NoPlant,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public DigJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class DigJob : Job<DigJobDefinition>
	{
		public DigJob(Tile tile) : base(tile) {
			TargetName = Tile.tileType.name;
			Description = $"Digging {string.Join(" and ", Tile.tileType.resourceRanges.Select(rr => rr.resource.name).ToArray())}";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Tile.dugPreviously = true;
			foreach (ResourceRange resourceRange in Tile.tileType.resourceRanges) {
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, Random.Range(resourceRange.min, resourceRange.max + 1), false);
			}
			bool setToWater = Tile.tileType.groupType == TileTypeGroup.TypeEnum.Water;
			if (!setToWater) {
				foreach (Tile nTile in Tile.horizontalSurroundingTiles) {
					if (nTile != null && nTile.tileType.groupType == TileTypeGroup.TypeEnum.Water) {
						setToWater = true;
						break;
					}
				}
			}
			if (setToWater) {
				Tile.SetTileType(Tile.biome.tileTypes[TileTypeGroup.TypeEnum.Water], false, true, true);
				foreach (Tile nTile in Tile.horizontalSurroundingTiles) {
					if (nTile != null && nTile.tileType.groupType == TileTypeGroup.TypeEnum.Hole) {
						List<Tile> frontier = new() { nTile };
						List<Tile> checkedTiles = new() { };
						while (frontier.Count > 0) {
							Tile currentTile = frontier[0];
							frontier.RemoveAt(0);
							checkedTiles.Add(currentTile);
							currentTile.SetTileType(currentTile.biome.tileTypes[TileTypeGroup.TypeEnum.Water], true, true, true);
							foreach (Tile nTile2 in currentTile.horizontalSurroundingTiles) {
								if (nTile2 != null && nTile2.tileType.groupType == TileTypeGroup.TypeEnum.Hole && !checkedTiles.Contains(nTile2)) {
									frontier.Add(nTile2);
								}
							}
						}
					}
				}
			} else {
				Tile.SetTileType(Tile.biome.tileTypes[TileTypeGroup.TypeEnum.Hole], false, true, true);
			}
		}
	}
}