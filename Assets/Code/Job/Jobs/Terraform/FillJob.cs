using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine.UI;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Fill")]
	public class FillJobDefinition : JobDefinition<FillJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			NSelection.SelectionConditions.Fillable,
			NSelection.SelectionConditions.NoObjects,
			NSelection.SelectionConditions.NoPlant,
			NSelection.SelectionConditions.NoSameLayerJobs
		};

		public override List<(EResource, int)> BaseRequiredResources { get; } = new() {
			(EResource.Dirt, 4)
		};

		public FillJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class FillJob : Job<FillJobDefinition>
	{
		public FillJob(Tile tile) : base(tile) {
			Description = $"Filling {Tile.tileType.groupType.ToString().ToLower()}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			TileType fillType = TileType.GetTileTypeByEnum(TileType.TypeEnum.Dirt);
			Tile.dugPreviously = false;
			foreach (ResourceRange resourceRange in fillType.resourceRanges) {
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, -(resourceRange.max + 1), true);
			}
			Tile.SetTileType(fillType, false, true, true);
		}
	}
}
