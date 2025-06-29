﻿using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Food", "CollectFood")]
	public class CollectFoodJobDefinition : JobDefinition<CollectFoodJob>
	{
		public CollectFoodJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
			Returnable = false;
		}
	}

	public class CollectFoodJob : Job<CollectFoodJobDefinition>
	{
		private readonly Container container;
		private List<Container> alreadyCheckedContainers;
		private float nutritionTarget;

		private Colonist colonist;

		public CollectFoodJob(
			Tile tile,
			Container container,
			List<Container> alreadyCheckedContainers,
			float nutritionTarget
		) : base(
			tile
		) {
			this.container = container;
			this.alreadyCheckedContainers = alreadyCheckedContainers ?? new List<Container>();
			this.nutritionTarget = nutritionTarget;

			Description = "Finding something to eat.";
		}

		protected override void OnJobTaken() {
			base.OnJobTaken();

			colonist = Worker as Colonist; // TODO Remove cast when Humans have Job ability
			if (colonist == null) {
				Close();
				return;
			}

			if (container == null) {
				List<ResourceAmount> currentFood = Worker.Inventory.GetResourcesByClass(Resource.ResourceClassEnum.Food).ToList();
				if (currentFood.Count > 0) {
					colonist.Jobs.SetJob(new EatJob(Worker.Tile));
				}
			} else {
				alreadyCheckedContainers.Add(container);

				List<ResourceAmount> foodInContainer = container.Inventory.GetResourcesByClass(Resource.ResourceClassEnum.Food).ToList();
				List<ResourceAmount> resourcesToEat = new();
				foreach (ResourceAmount food in foodInContainer.OrderBy(ra => ((Food)ra.Resource).nutrition)) {
					nutritionTarget -= ((Food)food.Resource).nutrition;
					resourcesToEat.Add(food);
					if (nutritionTarget <= 0) {
						break;
					}
				}
				if (resourcesToEat.Count > 0) {
					container.Inventory.ReserveResources(resourcesToEat, Worker);
				}
			}
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Worker.Inventory.TakeReservedResources(Worker);

			if (nutritionTarget <= 0) {
				colonist.Jobs.SetJob(new EatJob(Worker.Tile));
			} else {
				Container nextContainerToCheck = Container
					.GetContainersInRegion(Worker.Tile.region)
					.Where(c => !alreadyCheckedContainers.Contains(c))
					.OrderBy(c => PathManager.RegionBlockDistance(Worker.Tile.regionBlock, c.tile.regionBlock, true, true, false))
					.FirstOrDefault();
				if (nextContainerToCheck == null) {
					colonist.Jobs.SetJob(new CollectFoodJob(colonist.Tile, null, alreadyCheckedContainers, nutritionTarget));
					return;
				}
				alreadyCheckedContainers.Add(nextContainerToCheck);
				colonist.Jobs.SetJob(new CollectFoodJob(nextContainerToCheck.tile, nextContainerToCheck, alreadyCheckedContainers, nutritionTarget));
			}
		}
	}
}