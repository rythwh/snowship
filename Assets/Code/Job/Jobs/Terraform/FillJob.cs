﻿using System;
using System.Collections.Generic;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Fill")]
	public class FillJobDefinition : JobDefinition<FillJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Fillable,
			Selectable.SelectionConditions.NoObjects,
			Selectable.SelectionConditions.NoPlant,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public override List<ResourceAmount> BaseRequiredResources { get; } = new() {
			new ResourceAmount(Resource.GetResourceByEnum(EResource.Dirt), 4)
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