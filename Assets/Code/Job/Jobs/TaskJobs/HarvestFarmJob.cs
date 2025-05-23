﻿using System.Collections.Generic;
using System.Linq;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Harvest Farm")]
	public class HarvestFarmJob : Job
	{
		protected HarvestFarmJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			TargetName = Tile.farm.name;
			Description = $"Harvesting a farm of {Tile.farm.name}.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (Tile.farm != null) {
				// TODO Set the amount of seeds returned from a data file
				Worker.GetInventory().ChangeResourceAmount(Tile.farm.prefab.seedResource, Random.Range(1, 3), false);
				foreach (ResourceRange harvestResourceRange in Tile.farm.prefab.harvestResources) {
					Worker.GetInventory().ChangeResourceAmount(harvestResourceRange.resource, Random.Range(harvestResourceRange.min, harvestResourceRange.max), false);
				}

				GameManager.Get<JobManager>()
					.CreateJob(
						new JobInstance(
							JobPrefab.GetJobPrefabByName("PlantFarm"),
							Tile,
							Tile.farm.prefab,
							Tile.farm.variation,
							0
						)
					);

				// TODO ObjectInstance.RemoveObjectInstance should probably be called inside of Tile.RemoveObjectAtLayer?
				int layer = Tile.farm.prefab.layer; // Required because RemoveObjectInstance sets Tile.farm = null but must happen before RemoveObjectAtLayer
				ObjectInstance.RemoveObjectInstance(Tile.farm);
				Tile.RemoveObjectAtLayer(layer);
			}
			GameManager.Get<ResourceManager>().Bitmask(new List<TileManager.Tile> { Tile }.Concat(Tile.surroundingTiles).ToList());
		}
	}
}