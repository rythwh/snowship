using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NPath;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NColonist {
	public class Colonist : Human
	{
		public bool playerMoved;

		public override string Title => "Colonist";
		public override ILocation OriginLocation => GameManager.Get<ColonyManager>().colony;

		public Colonist(int id, Tile spawnTile, HumanData data) : base(id, spawnTile, data) {
		}

		public override void Update() {

			if (playerMoved && !IsMoving) {
				playerMoved = false;
			}

			base.Update();

			if (IsDead) {
				return;
			}

			if (Jobs.ActiveJob == null) {
				if (!IsMoving) {
					Wander();
				} else {
					WanderTimer = Random.Range(10f, 20f);
				}
			}
		}

		public List<Container> FindValidContainersToEmptyInventory() {
			return Container.GetContainersInRegion(Tile.region).Where(container => container.Inventory.UsedWeight() < container.Inventory.maxWeight && container.Inventory.UsedVolume() < container.Inventory.maxVolume).ToList();
		}

		public void EmptyInventory(List<Container> validContainers) {
			if (Inventory.UsedWeight() > 0 && Inventory.UsedVolume() > 0 && validContainers.Count > 0) {
				Container closestContainer = validContainers.OrderBy(container => Path.RegionBlockDistance(container.tile.regionBlock, Tile.regionBlock, true, true, false)).ToList()[0];
				Jobs.ForceJob(new EmptyInventoryJob(closestContainer));
			}
		}

		/*public void SetJob(ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			Job = colonistJob;
			Job.resourcesColonistHas = colonistJob.resourcesColonistHas;
			Job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && Job.containerPickups != null && Job.containerPickups.Count > 0) {
				foreach (ContainerPickup containerPickup in Job.containerPickups) {
					containerPickup.container.Inventory.ReserveResources(containerPickup.resourcesToPickup, this);
				}
			}
			if (Job.transferResources != null && Job.transferResources.Count > 0) {
				Container collectContainer = (Container)Job.Job.Tile.GetAllObjectInstances().Find(oi => oi is Container);
				if (collectContainer != null) {
					collectContainer.Inventory.ReserveResources(Job.transferResources, this);
				}
			}
			Job.SetColonist(this);
			MoveToTile(Job.tile, !Job.tile.walkable);

			if (colonistJob.Job.objectPrefab.type == ObjectPrefab.ObjectEnum.Sleep) {
				Debug.Log($"Sleeping: {colonistJob.Colonist.Name} at {GameManager.Get<TimeManager>().Time.DateString} {GameManager.Get<TimeManager>().Time.TimeString}");
			}
		}*/

		/*public void SetEatJob() {

			// Find a chair (ideally next to a table) for the colonist to sit at to eat
			List<ObjectInstance> chairs = new();
			foreach (ObjectPrefab chairPrefab in ObjectPrefabSubGroup.GetObjectPrefabSubGroupByEnum(ObjectPrefabSubGroup.ObjectSubGroupEnum.Chairs).prefabs) {
				List<ObjectInstance> chairsFromPrefab = ObjectInstance.GetObjectInstancesByPrefab(chairPrefab);
				if (chairsFromPrefab != null) {
					chairs.AddRange(chairsFromPrefab);
				}
			}
			chairs.Select(chair => chair.tile.region == overTile.region);
			chairs.OrderBy(chair => PathManager.RegionBlockDistance(overTile.regionBlock, chair.tile.regionBlock, true, true, false));
			chairs.OrderByDescending(chair => chair.tile.surroundingTiles.Find(surroundingTile => {
				ObjectInstance tableNextToChair = surroundingTile.GetObjectInstanceAtLayer(2);
				if (tableNextToChair != null) {
					return tableNextToChair.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Tables;
				}
				return false;
			}) != null);

			SetJob(
				new ColonistJob(
					this,
					new JobInstance(
						JobPrefab.GetJobPrefabByName("Eat"),
						chairs.Count > 0 ? chairs[0].tile : overTile,
						ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.Eat),
						null,
						0
					),
					null,
					null
				)
			);
		}*/

		/*public void StartJob() {
			Job.started = true;

			Job.jobProgress *= 1 + (1 - GetJobSkillMultiplier(Job.objectPrefab.jobType));

			if (Job.prefab.name == "Eat") {
				Job.jobProgress += needs.Find(need => need.prefab.type == ENeed.Food).GetValue();
			}
			if (Job.prefab.name == "Sleep") {
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				Job.jobProgress += restNeed.GetValue() / restNeed.prefab.baseIncreaseRate / restNeed.prefab.decreaseRateMultiplier / SimulationDateTime.PermanentTimerMultiplier;
				Job.jobProgress += Random.Range(Job.jobProgress * 0.1f, Job.jobProgress * 0.3f);
			}

			Job.colonistBuildTime = Job.jobProgress;

			// GameManager.Get<UIManagerOld>().SetJobElements();
		}*/

		/*public void WorkJob() {

			if (Job.prefab.name == "HarvestFarm" && Job.tile.farm == null) {
				Job.Remove();
				Job = null;
				return;
			}

			if (
				Job.prefab.name == "EmptyInventory" ||
				Job.prefab.name == "CollectFood" ||
				Job.prefab.name == "PickupResources"
			) {

				Container containerOnTile = Container.containers.Find(container => container.tile == Job.tile);
				if (containerOnTile == null) {
					Job.Remove();
					Job = null;
					return;
				}
			} else if (Job.prefab.name == "Sleep") { // TODO: Check that this still works - (removed timeM.minuteChanged, but added the * DeltaTime instead)
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				restNeed.ChangeValue(-restNeed.prefab.baseIncreaseRate * restNeed.prefab.decreaseRateMultiplier /* * GameManager.Get<TimeManager>().Time.DeltaTime#1#);
			}

			if (Job.activeObject != null) {
				Job.activeObject.SetActiveSprite(Job, true);
			}

			Job.jobProgress -= 1 * GameManager.Get<TimeManager>().Time.DeltaTime;

			if (Job.jobProgress <= 0 || Mathf.Approximately(Job.jobProgress, 0)) {
				Job.jobProgress = 0;
				FinishJob();
				return;
			}

			Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (Job.colonistBuildTime - Job.jobProgress) / Job.colonistBuildTime);
		}*/

		/*public void FinishJob() {
			Job finishedJob = Job;
			Job = null;

			MonoBehaviour.Destroy(finishedJob.jobPreview);

			SkillInstance skill = GetSkillFromJobType(finishedJob.objectPrefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJob.objectPrefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			// TODO finishedJob.prefab.RunFinishJobActions(finishedJob, this);

			if (finishedJob.activeObject != null) {
				finishedJob.activeObject.SetActiveSprite(finishedJob, false);
			}

			// ColonistJob.UpdateSingleColonistJobs(this);
			// GameManager.Get<UIManagerOld>().SetJobElements();
			// GameManager.Get<UIManagerOld>().UpdateSelectedColonistInformation(); // TODO Job Finished, Skill Updated
			// GameManager.Get<UIManagerOld>().UpdateSelectedContainerInfo();
			// GameManager.Get<UIManagerOld>().UpdateSelectedTradingPostInfo();
			// GameManager.Get<UIManagerOld>().UpdateTileInformation();

			// if (StoredJob != null) {
			// 	Update();
			// }
		}*/

		/*public void ReturnJob() {
			foreach (Container container in Container.containers) {
				container.Inventory.ReleaseReservedResources(this);
			}
			if (StoredJob != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(StoredJob.objectPrefab.jobType) || !StoredJob.prefab.returnable) {
					StoredJob.Remove();
					StoredJob = null;
					if (Job != null) {
						if (NeedUtilities.jobToNeedMap.ContainsKey(Job.objectPrefab.jobType) || !Job.prefab.returnable) {
							Job.Remove();
						}
						Job = null;
					}
					return;
				}
				GameManager.Get<JobManager>().AddExistingJob(StoredJob);
				if (StoredJob.jobPreview != null) {
					StoredJob.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				StoredJob = null;
				if (Job != null) {
					if (Job.jobPreview != null) {
						Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
					}
					Job = null;
				}
			} else if (Job != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(Job.objectPrefab.jobType) || !Job.prefab.returnable) {
					Job.Remove();
					Job = null;
					return;
				}
				GameManager.Get<JobManager>().AddExistingJob(Job);
				if (Job.jobPreview != null) {
					Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				Job = null;
			}
		}*/

		public void PlayerMoveToTile(Tile tile) {
			playerMoved = true;
			Jobs.ReturnJob();
			MoveToTile(tile, false);
		}

		public override void ChangeClothing(BodySection bodySection, Clothing clothing) {
			if (clothing == null || Inventory.resources.Find(ra => ra.Resource == clothing) != null) {
				base.ChangeClothing(bodySection, clothing);
			} else {
				Container container = NeedUtilities.FindClosestResourceAmountInContainers(this, new ResourceAmount(clothing, 1));
				if (container == null) {
					return;
				}

				ResourceAmount clothingToPickup = new(clothing, 1);
				container.Inventory.ReserveResources(new List<ResourceAmount> { clothingToPickup }, this);
				Jobs.ForceJob(new WearClothesJob(container.tile, container, clothing));
			}
		}
	}
}
