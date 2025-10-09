using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine.UI;

namespace Snowship.NJob
{
	[RegisterJob("Farm", "Harvest", "HarvestFarm")]
	public class HarvestFarmJobDefinition : JobDefinition<HarvestFarmJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			NSelection.SelectionConditions.Farm,
			NSelection.SelectionConditions.NoSameLayerJobs
		};

		public HarvestFarmJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class HarvestFarmJob : Job<HarvestFarmJobDefinition>
	{
		private Farm Farm { get; }

		public HarvestFarmJob(Tile tile) : base(tile) {
			Farm = tile.farm;

			TargetName = Farm.name;
			Description = $"Harvesting a farm of {Farm.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (Farm != null) {
				// TODO Set the amount of seeds returned from a data file
				Worker.Inventory.ChangeResourceAmount(Farm.prefab.seedResource, Random.Range(1, 3), false);
				foreach (ResourceRange harvestResourceRange in Farm.prefab.harvestResources) {
					Worker.Inventory.ChangeResourceAmount(harvestResourceRange.resource, Random.Range(harvestResourceRange.min, harvestResourceRange.max), false);
				}

				GameManager.Get<JobManager>().AddJob(new PlantFarmJob(Tile, new BuildJobParams(Farm.prefab))); // TODO Removed Farm.variation, maybe need to add it back

				// TODO ObjectInstance.RemoveObjectInstance should probably be called inside of Tile.RemoveObjectAtLayer?
				ObjectInstance.RemoveObjectInstance(Farm);
				Tile.RemoveObjectAtLayer(Farm.prefab.layer);
			}
			GameManager.Get<ResourceManager>().Bitmask(new List<Tile> { Tile }.Concat(Tile.surroundingTiles).ToList());
		}
	}
}
