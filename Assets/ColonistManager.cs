using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColonistManager : BaseManager {

	public override void Update() {
		foreach (Colonist colonist in colonists) {
			colonist.Update();
			if (colonist.dead) {
				deadColonists.Add(colonist);
			}
		}
		foreach (Colonist deadColonist in deadColonists) {
			deadColonist.Die();
		}
		deadColonists.Clear();

		if (!GameManager.timeM.GetPaused()) {
			GameManager.jobM.GiveJobsToColonists();
		}
	}

	public enum SkillTypeEnum { Building, Mining, Digging, Farming, Forestry, Crafting };

	public class SkillPrefab {

		public SkillTypeEnum type;
		public string name;

		public Dictionary<JobManager.JobTypesEnum, float> affectedJobTypes = new Dictionary<JobManager.JobTypesEnum, float>();

		public SkillPrefab(List<string> data) {
			type = (SkillTypeEnum)Enum.Parse(typeof(SkillTypeEnum), data[0]);
			name = type.ToString();

			foreach (string affectedJobTypeString in data[1].Split(';')) {
				List<string> affectedJobTypeData = affectedJobTypeString.Split(',').ToList();
				affectedJobTypes.Add((JobManager.JobTypesEnum)Enum.Parse(typeof(JobManager.JobTypesEnum), affectedJobTypeData[0]), float.Parse(affectedJobTypeData[1]));
			}
		}
	}

	public SkillPrefab GetSkillPrefabFromString(string skillTypeString) {
		return skillPrefabs.Find(skillPrefab => skillPrefab.type == (SkillTypeEnum)Enum.Parse(typeof(SkillTypeEnum), skillTypeString));
	}

	public class SkillInstance {
		public Colonist colonist;
		public SkillPrefab prefab;

		public int level;
		public float currentExperience;
		public float nextLevelExperience;

		public SkillInstance(Colonist colonist, SkillPrefab prefab, bool randomStartingLevel, int startingLevel) {
			this.colonist = colonist;
			this.prefab = prefab;

			if (randomStartingLevel) {
				level = UnityEngine.Random.Range((colonist.profession.primarySkill != null && colonist.profession.primarySkill.type == prefab.type ? Mathf.RoundToInt(colonist.profession.skillRandomMaxValues[prefab] / 2f) : 0), colonist.profession.skillRandomMaxValues[prefab]);
			} else {
				level = startingLevel;
			}

			currentExperience = UnityEngine.Random.Range(0, 100);
			nextLevelExperience = 100 + (10 * level);
			AddExperience(0);
		}

		public void AddExperience(float amount) {
			currentExperience += amount;
			while (currentExperience >= nextLevelExperience) {
				level += 1;
				currentExperience = (currentExperience - nextLevelExperience);
				nextLevelExperience = 100 + (10 * level);
			}
			GameManager.jobM.UpdateColonistJobCosts(colonist);
			if (GameManager.humanM.selectedHuman == colonist) {
				GameManager.uiM.RemakeSelectedColonistSkills();
			}
		}

		public float CalculateTotalSkillLevel() {
			return level + (currentExperience / nextLevelExperience);
		}
	}

	public List<SkillPrefab> skillPrefabs = new List<SkillPrefab>();

	public void CreateColonistSkills() {
		List<string> stringSkills = Resources.Load<TextAsset>(@"Data/colonistSkills").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('`').ToList();
		foreach (string stringSkill in stringSkills) {
			List<string> stringSkillData = stringSkill.Split('/').ToList();
			skillPrefabs.Add(new SkillPrefab(stringSkillData));
		}
		foreach (SkillPrefab skillPrefab in skillPrefabs) {
			skillPrefab.name = UIManager.SplitByCapitals(skillPrefab.name);
		}
	}

	public enum ProfessionTypeEnum { Nothing, Builder, Miner, Farmer, Forester, Maker };

	public List<Profession> professions = new List<Profession>();

	public class Profession {
		public ProfessionTypeEnum type;
		public string name;

		public string description;

		public SkillPrefab primarySkill;

		public Dictionary<SkillPrefab, int> skillRandomMaxValues = new Dictionary<SkillPrefab, int>();

		public List<Colonist> colonistsInProfessionAtStart = new List<Colonist>();
		public List<Colonist> colonistsInProfession = new List<Colonist>();

		public Profession(List<string> data) {
			type = (ProfessionTypeEnum)Enum.Parse(typeof(ProfessionTypeEnum), data[0]);
			name = type.ToString();

			description = data[1];

			int primarySkillString = 0;
			if (!int.TryParse(data[2], out primarySkillString)) {
				primarySkill = GameManager.colonistM.GetSkillPrefabFromString(data[2]);
			}

			foreach (string skillRandomMaxValueData in data[3].Split(';')) {
				List<string> skillRandomMaxValue = skillRandomMaxValueData.Split(',').ToList();
				skillRandomMaxValues.Add(GameManager.colonistM.GetSkillPrefabFromString(skillRandomMaxValue[0]), int.Parse(skillRandomMaxValue[1]));
			}
		}

		public double CalculateSkillLevelFromPrimarySkill(Colonist colonist, bool round, int decimalPlaces) {
			if (type != ProfessionTypeEnum.Nothing) {
				SkillInstance skillInstance = colonist.skills.Find(skill => skill.prefab == primarySkill);
				double skillLevel = skillInstance.level + (skillInstance.currentExperience / skillInstance.nextLevelExperience);
				if (round) {
					return Math.Round(skillLevel, decimalPlaces);
				}
				return skillLevel;
			}
			return 0;
		}
	}

	public Profession FindProfessionByType(ProfessionTypeEnum type) {
		return professions.Find(profession => profession.type == type);
	}

	public void CreateColonistProfessions() {
		List<string> stringProfessions = Resources.Load<TextAsset>(@"Data/colonistProfessions").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('`').ToList();
		foreach (string stringProfession in stringProfessions) {
			List<string> stringProfessionData = stringProfession.Split('/').ToList();
			professions.Add(new Profession(stringProfessionData));
		}
		foreach (Profession profession in professions) {
			profession.name = UIManager.SplitByCapitals(profession.name);
		}
	}

	public enum TraitsEnum {
		Lazy, Workaholic,
		Alcoholic,
		Antisocial, Socialite,
		Attractive, Unattractive,
		Dieter, Overeater
	};

	public List<TraitPrefab> traitPrefabs = new List<TraitPrefab>();

	public class TraitPrefab {

		public TraitsEnum type;
		public string name;

		public float effectAmount;

		public TraitPrefab(List<string> data) {

		}
	}

	public TraitPrefab GetTraitPrefabFromString(string traitTypeString) {
		return traitPrefabs.Find(traitPrefab => traitPrefab.type == (TraitsEnum)Enum.Parse(typeof(TraitsEnum), traitTypeString));
	}

	public class TraitInstance {

		public Colonist colonist;
		public TraitPrefab prefab;

		public TraitInstance(Colonist colonist, TraitPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}
	}

	public enum NeedsEnum { Rest, Water, Food, Temperature, Shelter, Clothing, Safety, Social, Esteem, Relaxation };

	// TODO Add "relatedNeeds" variable to JobPrefab and then find a way to delete this
	private List<JobManager.JobTypesEnum> needRelatedJobs = new List<JobManager.JobTypesEnum>() {
		JobManager.JobTypesEnum.CollectFood, JobManager.JobTypesEnum.Eat,
		JobManager.JobTypesEnum.CollectWater, JobManager.JobTypesEnum.Drink,
		JobManager.JobTypesEnum.Sleep
	};

	// TODO Add "relatedNeeds" variable to JobPrefab and then find a way to delete this
	private Dictionary<JobManager.JobTypesEnum, NeedsEnum> jobToNeedMap = new Dictionary<JobManager.JobTypesEnum, NeedsEnum>() {
		{ JobManager.JobTypesEnum.Sleep, NeedsEnum.Rest },
		{ JobManager.JobTypesEnum.CollectWater, NeedsEnum.Water },
		{ JobManager.JobTypesEnum.Drink, NeedsEnum.Water },
		{ JobManager.JobTypesEnum.CollectFood, NeedsEnum.Food },
		{ JobManager.JobTypesEnum.Eat, NeedsEnum.Food }
	};

	public Dictionary<NeedsEnum, Func<NeedInstance, float>> needsValueSpecialIncreases = new Dictionary<NeedsEnum, Func<NeedInstance, float>>();

	public void InitializeNeedsValueSpecialIncreases() {
		needsValueSpecialIncreases.Add(NeedsEnum.Rest, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			if (!GameManager.timeM.isDay) {
				totalSpecialIncrease += 0.05f;
			}
			HappinessModifierInstance hmi = need.colonist.happinessModifiers.Find(findHMI => findHMI.prefab.group.type == HappinessModifierGroupsEnum.Rest);
			if (hmi.prefab.type == HappinessModifiersEnum.Rested) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.8f);
			}
			return totalSpecialIncrease;
		});
		needsValueSpecialIncreases.Add(NeedsEnum.Water, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			HappinessModifierInstance hmi = need.colonist.happinessModifiers.Find(findHMI => findHMI.prefab.group.type == HappinessModifierGroupsEnum.Water);
			if (hmi.prefab.type == HappinessModifiersEnum.Quenched) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.5f);
			}
			return totalSpecialIncrease;
		});
		needsValueSpecialIncreases.Add(NeedsEnum.Food, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			HappinessModifierInstance hmi = need.colonist.happinessModifiers.Find(findHMI => findHMI.prefab.group.type == HappinessModifierGroupsEnum.Food);
			if (hmi.prefab.type == HappinessModifiersEnum.Stuffed) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.9f);
			} else if (hmi.prefab.type == HappinessModifiersEnum.Full) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.5f);
			}
			return totalSpecialIncrease;
		});
	}

	public void CalculateNeedValue(NeedInstance need) {
		if (need.colonist.job != null && need.prefab.relatedJobs.Contains(need.colonist.job.prefab.jobType)) {
			return;
		}
		float needIncreaseAmount = need.prefab.baseIncreaseRate;
		foreach (TraitInstance trait in need.colonist.traits) {
			if (need.prefab.traitsAffectingThisNeed.ContainsKey(trait.prefab.type)) {
				needIncreaseAmount *= need.prefab.traitsAffectingThisNeed[trait.prefab.type];
			}
		}
		need.ChangeValue((needIncreaseAmount + (needsValueSpecialIncreases.ContainsKey(need.prefab.type) ? needsValueSpecialIncreases[need.prefab.type](need) : 0)) * GameManager.timeM.deltaTime);
	}

	KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> FindClosestFood(Colonist colonist, float minimumNutritionRequired, bool takeFromOtherColonists, bool eatAnything) {
		List<KeyValuePair<KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>, int>> resourcesPerInventory = new List<KeyValuePair<KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>, int>>();
		int totalNutrition = 0;
		foreach (ResourceManager.Container container in GameManager.resourceM.GetContainersInRegion(colonist.overTile.region).OrderBy(c => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, c.tile.regionBlock, true, true, false))) {
			List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
			foreach (ResourceManager.ResourceAmount ra in container.inventory.resources.Where(ra => ra.resource.classes.Contains(ResourceManager.ResourceClassEnum.Food)).OrderBy(ra => ((ResourceManager.Food)ra.resource).nutrition).ToList()) {
				int numReserved = 0;
				for (int i = 0; i < ra.amount; i++) {
					numReserved += 1;
					totalNutrition += ((ResourceManager.Food)ra.resource).nutrition;
					if (totalNutrition >= minimumNutritionRequired) {
						break;
					}
				}
				resourcesToReserve.Add(new ResourceManager.ResourceAmount(ra.resource, numReserved));
				if (totalNutrition >= minimumNutritionRequired) {
					break;
				}
			}
			if (totalNutrition >= minimumNutritionRequired) {
				resourcesPerInventory.Add(new KeyValuePair<KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>, int>(new KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>(container.inventory, resourcesToReserve), totalNutrition));
				break;
			}
		}
		if (takeFromOtherColonists) {
			//print("Take from other colonists.");
		}
		if (resourcesPerInventory.Count > 0) {
			return resourcesPerInventory[0].Key;
		} else {
			return new KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>(null, null);
		}
	}

	public bool GetFood(NeedInstance need, bool takeFromOtherColonists, bool eatAnything) {
		if (need.colonist.inventory.resources.Find(ra => ra.resource.groupType == ResourceManager.ResourceGroupEnum.Foods) == null) {
			KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> closestFood = FindClosestFood(need.colonist, need.GetValue(), takeFromOtherColonists, eatAnything);

			List<ResourceManager.ResourceAmount> resourcesToReserve = closestFood.Value;
			if (closestFood.Key != null) {
				if (closestFood.Key.container != null) {
					ResourceManager.Container container = closestFood.Key.container;
					container.inventory.ReserveResources(resourcesToReserve, need.colonist);
					JobManager.Job job = new JobManager.Job(container.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectFood), 0);
					need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, job, null, null));
					return true;
				} else if (closestFood.Key.human != null) {
					// TODO
					//Human human = closestFood.Key.human;
					//print("Take from other human.");
				}
			}
		} else {
			need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(need.colonist.overTile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Eat), 0), null, null));
			return true;
		}
		return false;
	}

	public int FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum resourceGroup, Colonist colonist, bool worldTotal, bool includeOtherColonists) {
		if (worldTotal) {
			int total = 0;
			foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources()) {
				if (resource.groupType == resourceGroup) {
					total += resource.GetWorldTotalAmount();
				}
			}
			return total;
		} else {
			int total = 0;

			int amountOnThisColonist = 0;
			foreach (ResourceManager.ResourceAmount resourceAmount in colonist.inventory.resources.Where(ra => ra.resource.groupType == resourceGroup)) {
				amountOnThisColonist += resourceAmount.amount;
			}
			total += amountOnThisColonist;

			int amountUnreservedInContainers = 0;
			foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources().Where(r => r.groupType == resourceGroup)) {
				amountUnreservedInContainers += resource.GetUnreservedContainerTotalAmount();
			}
			total += amountUnreservedInContainers;

			if (includeOtherColonists) {
				int amountOnOtherColonists = 0;
				foreach (Colonist otherColonist in colonists) {
					if (colonist != otherColonist) {
						int amountOnOtherColonist = 0;
						foreach (ResourceManager.ResourceAmount resourceAmount in otherColonist.inventory.resources.Where(ra => ra.resource.groupType == resourceGroup)) {
							amountOnOtherColonist += resourceAmount.amount;
						}
						total += amountOnOtherColonist;
					}
				}
				total += amountOnOtherColonists;
			}

			return total;
		}
	}

	public bool GetSleep(NeedInstance need, bool sleepAnywhere) {
		if (GameManager.resourceM.sleepSpots.Count > 0) {
			List<ResourceManager.SleepSpot> validSleepSpots = GameManager.resourceM.sleepSpots.Where(sleepSpot => sleepSpot.occupyingColonist == null && sleepSpot.tile.region == need.colonist.overTile.region).ToList();
			if (validSleepSpots.Count > 0) {
				ResourceManager.SleepSpot chosenSleepSpot = validSleepSpots.OrderByDescending(sleepSpot => sleepSpot.prefab.restComfortAmount / (PathManager.RegionBlockDistance(need.colonist.overTile.regionBlock, sleepSpot.tile.regionBlock, true, true, false) + 1)).ToList()[0];
				chosenSleepSpot.StartSleeping(need.colonist);
				need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(chosenSleepSpot.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Sleep), 0), null, null));
				return true;
			}
		}
		if (sleepAnywhere) {
			need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(need.colonist.overTile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Sleep), 0), null, null));
			return true;
		}
		return false;
	}

	public static readonly Dictionary<NeedsEnum, Func<NeedInstance, bool>> needsValueFunctions = new Dictionary<NeedsEnum, Func<NeedInstance, bool>>() {
		{ NeedsEnum.Food, delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.CollectFood || need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Eat)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * GameManager.timeM.deltaTime);
					if (GameManager.colonistM.FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) { // true, true
						if (GameManager.timeM.minuteChanged) {
							need.colonist.ReturnJob();
							return GameManager.colonistM.GetFood(need, false, false); // true, true - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (GameManager.colonistM.FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) { // false, true
						if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue))) {
							need.colonist.ReturnJob();
							return GameManager.colonistM.GetFood(need, false, false); // true, false - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (GameManager.colonistM.FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) {
							if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue))) {
								need.colonist.ReturnJob();
								return GameManager.colonistM.GetFood(need, false, false);
							}
						}
					}
					return false;
				}
			}
			return false;
		} },
		{ NeedsEnum.Water, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.CollectWater || need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Drink)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * GameManager.timeM.deltaTime);
					// TODO Find Water Here (Maximum Priority)
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					// TODO Find Water Here (High Priority)
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						// TODO Find Water Here (Low Priority)
					}
					return false;
				}
			}
			return false;
		} },
		{ NeedsEnum.Rest, delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Sleep)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * GameManager.timeM.deltaTime);
					if (GameManager.timeM.minuteChanged) {
						need.colonist.ReturnJob();
						GameManager.colonistM.GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue))) {
						need.colonist.ReturnJob();
						GameManager.colonistM.GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue))) {
							need.colonist.ReturnJob();
							GameManager.colonistM.GetSleep(need, false);
						}

					}
					return false;
				}
			}
			return false;
		} },
		{ NeedsEnum.Clothing, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedsEnum.Shelter, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedsEnum.Temperature, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedsEnum.Safety, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedsEnum.Social, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedsEnum.Esteem, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedsEnum.Relaxation, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} }
	};

	/*

	1440 seconds per in-game day
	@ 0.01 BRI ... 14.4 points per in-game day
	@ 0.05 BRI ... 72 points per in-game day
	@ 0.1 BRI ... 144 points per in-game day

	Convert days to points between 0-100
	(days * 1440) * BRI = pointsAfterDays

	Convert points between 0-100 to days
	(paintsAfterDays / BRI) / 1440 = days

	0	NeedName/
	1	0.01/		BRI			Base rate of increase per second while conditions met
	2	false/		AMinVB		Whether there is any action taken at MinV
	3	0/			MinV		No action
	4	false/		AMaxVB		Whether there is any action taken at MaxV
	5	0/			MaxV		No action
	6	false/		ACVB		Whether there is any action taken at CV
	7	0/			CV			Value until they begin dying from the need not being fulfilled
	8	false/		DB			Whether they can die from this need not being fulfilled
	9	0.0/		DR			Base rate of health loss due to the need not being fulfilled
	10	100/		ClampV		Value above which the food value will be clamped
	11	0/			Priority	The priority of fulfilling the requirements of this need over others
	12	2/			Number of Affected Traits
	13	TraitName,TraitName/	Names of the affected traits
	14	1.1,1.1`	Multiplier of the BRI for each trait

	*/

	public List<NeedPrefab> needPrefabs = new List<NeedPrefab>();

	public class NeedPrefab {

		public NeedsEnum type;
		public string name;

		public float baseIncreaseRate;

		public bool minValueAction;
		public float minValue;

		public bool maxValueAction;
		public float maxValue;

		public bool critValueAction;
		public float critValue;

		public bool canDie;
		public float healthDecreaseRate;

		public int clampValue;

		public int priority;

		public Dictionary<TraitsEnum, float> traitsAffectingThisNeed = new Dictionary<TraitsEnum, float>();

		public List<JobManager.JobTypesEnum> relatedJobs = new List<JobManager.JobTypesEnum>();

		public NeedPrefab(
			NeedsEnum type,
			float baseIncreaseRate,
			bool minValueAction,
			float minValue,
			bool maxValueAction,
			float maxValue,
			bool critValueAction,
			float critValue,
			bool canDie,
			float healthDecreaseRate,
			int clampValue,
			int priority,
			Dictionary<TraitsEnum, float> traitsAffectingThisNeed,
			List<JobManager.JobTypesEnum> relatedJobs
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.baseIncreaseRate = baseIncreaseRate;

			this.minValueAction = minValueAction;
			this.minValue = minValue;

			this.maxValueAction = maxValueAction;
			this.maxValue = maxValue;

			this.critValueAction = critValueAction;
			this.critValue = critValue;

			this.canDie = canDie;
			this.healthDecreaseRate = healthDecreaseRate;

			this.clampValue = clampValue;

			this.priority = priority;

			this.traitsAffectingThisNeed = traitsAffectingThisNeed;

			this.relatedJobs = relatedJobs;
		}
	}

	public NeedPrefab GetNeedPrefabFromEnum(NeedsEnum needsEnumValue) {
		return needPrefabs.Find(needPrefab => needPrefab.type == needsEnumValue);
	}

	public NeedPrefab GetNeedPrefabFromString(string needTypeString) {
		return needPrefabs.Find(needPrefab => needPrefab.type == (NeedsEnum)Enum.Parse(typeof(NeedsEnum), needTypeString));
	}

	public class NeedInstance {

		public Colonist colonist;
		public NeedPrefab prefab;
		private float value = 0;
		private int roundedValue = 0;

		public NeedInstance(Colonist colonist, NeedPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}

		public float GetValue() {
			return value;
		}

		public void SetValue(float newValue) {
			float oldValue = value;
			value = newValue;
			Mathf.Clamp(value, 0, prefab.clampValue);
			roundedValue = Mathf.RoundToInt((value / prefab.clampValue) * 100);
			if (GameManager.humanM.selectedHuman == colonist && Mathf.RoundToInt((oldValue / prefab.clampValue) * 100) != roundedValue) {
				GameManager.uiM.RemakeSelectedColonistNeeds();
			}
		}

		public void ChangeValue(float amount) {
			SetValue(value + amount);
		}

		public int GetRoundedValue() {
			return roundedValue;
		}
	}

	public void CreateColonistNeeds() {
		List<string> needDataStringList = Resources.Load<TextAsset>(@"Data/colonistNeeds").text.Replace("\t", string.Empty).Split(new string[] { "<Need>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string singleNeedDataString in needDataStringList) {

			NeedsEnum type = NeedsEnum.Rest;
			float baseIncreaseRate = 0;
			bool minValueAction = false;
			float minValue = 0;
			bool maxValueAction = false;
			float maxValue = 0;
			bool critValueAction = false;
			float critValue = 0;
			bool canDie = false;
			float healthDecreaseRate = 0;
			int clampValue = 0;
			int priority = 0;
			Dictionary<TraitsEnum, float> traitsAffectingThisNeed = new Dictionary<TraitsEnum, float>();
			List<JobManager.JobTypesEnum> relatedJobs = new List<JobManager.JobTypesEnum>();

			List<string> singleNeedDataLineStringList = singleNeedDataString.Split('\n').ToList();
			foreach (string singleNeedDataLineString in singleNeedDataLineStringList.Skip(1)) {
				if (!string.IsNullOrEmpty(singleNeedDataLineString)) {

					string label = singleNeedDataLineString.Split('>')[0].Replace("<", string.Empty);
					string value = singleNeedDataLineString.Split('>')[1];

					switch (label) {
						case "Type":
							type = (NeedsEnum)Enum.Parse(typeof(NeedsEnum), value);
							break;
						case "BaseIncreaseRate":
							baseIncreaseRate = float.Parse(value);
							break;
						case "MinValueAction":
							minValueAction = bool.Parse(value);
							break;
						case "MinValue":
							minValue = float.Parse(value);
							break;
						case "MaxValueAction":
							maxValueAction = bool.Parse(value);
							break;
						case "MaxValue":
							maxValue = float.Parse(value);
							break;
						case "CritValueAction":
							critValueAction = bool.Parse(value);
							break;
						case "CritValue":
							critValue = float.Parse(value);
							break;
						case "CanDie":
							canDie = bool.Parse(value);
							break;
						case "HealthDecreaseRate":
							healthDecreaseRate = float.Parse(value);
							break;
						case "ClampValue":
							clampValue = int.Parse(value);
							break;
						case "Priority":
							priority = int.Parse(value);
							break;
						case "TraitsAffectingThisNeed":
							if (!string.IsNullOrEmpty(UIManager.RemoveNonAlphanumericChars(value))) {
								foreach (string traitsAffectingThisNeedString in value.Split(',')) {
									TraitsEnum traitEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), traitsAffectingThisNeedString.Split(':')[0]);
									float multiplier = float.Parse(traitsAffectingThisNeedString.Split(':')[1]);
									traitsAffectingThisNeed.Add(traitEnum, multiplier);
								}
							}
							break;
						case "RelatedJobs":
							if (!string.IsNullOrEmpty(UIManager.RemoveNonAlphanumericChars(value))) {
								foreach (string relatedJobString in value.Split(',')) {
									JobManager.JobTypesEnum jobTypeEnum = (JobManager.JobTypesEnum)Enum.Parse(typeof(JobManager.JobTypesEnum), relatedJobString);
									relatedJobs.Add(jobTypeEnum);
								}
							}
							break;
						default:
							MonoBehaviour.print("Unknown need label: \"" + singleNeedDataLineString + "\"");
							break;
					}
				}
			}

			needPrefabs.Add(new NeedPrefab(type, baseIncreaseRate, minValueAction, minValue, maxValueAction, maxValue, critValueAction, critValue, canDie, healthDecreaseRate, clampValue, priority, traitsAffectingThisNeed, relatedJobs));
		}
	}

	public static readonly Dictionary<HappinessModifierGroupsEnum, Action<Colonist>> happinessModifierFunctions = new Dictionary<HappinessModifierGroupsEnum, Action<Colonist>>() {
		{ HappinessModifierGroupsEnum.Death, delegate (Colonist colonist) {
			// TODO Implement colonists viewing deaths and being sad
		} },
		{ HappinessModifierGroupsEnum.Rest, delegate (Colonist colonist) {
			NeedInstance restNeed = colonist.needs.Find(ni => ni.prefab.type == NeedsEnum.Rest);
			if (restNeed.GetValue() >= restNeed.prefab.maxValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Exhausted);
			} else if (restNeed.GetValue() >= restNeed.prefab.minValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Tired);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Exhausted);
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Tired);
			}
		} },
		{ HappinessModifierGroupsEnum.Water, delegate (Colonist colonist) {
			NeedInstance waterNeed = colonist.needs.Find(ni => ni.prefab.type == NeedsEnum.Water);
			if (waterNeed.GetValue() >= waterNeed.prefab.maxValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Dehydrated);
			} else if (waterNeed.GetValue() >= waterNeed.prefab.minValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Thirsty);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Dehydrated);
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Thirsty);
			}
		} },
		{ HappinessModifierGroupsEnum.Food, delegate (Colonist colonist) {
			NeedInstance foodNeed = colonist.needs.Find(ni => ni.prefab.type == NeedsEnum.Food);
			if (foodNeed.GetValue() >= foodNeed.prefab.maxValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Starving);
			} else if (foodNeed.GetValue() >= foodNeed.prefab.minValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Hungry);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Starving);
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Hungry);
			}
		} },
		{ HappinessModifierGroupsEnum.Inventory, delegate (Colonist colonist) {
			if (colonist.inventory.CountResources() > colonist.inventory.maxAmount) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Overencumbered);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Overencumbered);
			}
		} }
	};

	public void CreateHappinessModifiers() {
		List<string> stringHappinessModifierGroups = Resources.Load<TextAsset>(@"Data/happinessModifiers").text.Replace("\t", string.Empty).Split(new string[] { "<HappinessModifierGroup>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string stringHappinessModifierGroup in stringHappinessModifierGroups) {
			HappinessModifierGroup happinessModifierGroup = new HappinessModifierGroup(stringHappinessModifierGroup);
			happinessModifierGroups.Add(happinessModifierGroup);
			happinessModifierPrefabs.AddRange(happinessModifierGroup.prefabs);
		}
	}

	public enum HappinessModifierGroupsEnum {
		Death,
		Food,
		Water,
		Rest,
		Inventory
	};
	public enum HappinessModifiersEnum {
		WitnessDeath,
		Stuffed, Full, Hungry, Starving,
		Dehydrated, Thirsty, Quenched,
		Rested, Tired, Exhausted,
		Overencumbered
	};

	public List<HappinessModifierGroup> happinessModifierGroups = new List<HappinessModifierGroup>();

	public class HappinessModifierGroup {
		public HappinessModifierGroupsEnum type;
		public string name;

		public List<HappinessModifierPrefab> prefabs = new List<HappinessModifierPrefab>();

		public HappinessModifierGroup(string stringHappinessModifierGroup) {
			List<string> stringHappinessModifiers = stringHappinessModifierGroup.Split(new string[] { "<HappinessModifier>" }, StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (HappinessModifierGroupsEnum)Enum.Parse(typeof(HappinessModifierGroupsEnum), stringHappinessModifiers[0]);
			name = UIManager.SplitByCapitals(type.ToString());

			foreach (string stringHappinessModifier in stringHappinessModifiers.Skip(1)) {
				prefabs.Add(new HappinessModifierPrefab(stringHappinessModifier, this));
			}
		}
	}

	public HappinessModifierGroup GetHappinessModifierGroupFromEnum(HappinessModifierGroupsEnum happinessModifierGroupEnum) {
		return happinessModifierGroups.Find(hmiGroup => hmiGroup.type == happinessModifierGroupEnum);
	}

	public List<HappinessModifierPrefab> happinessModifierPrefabs = new List<HappinessModifierPrefab>();

	public class HappinessModifierPrefab {

		public HappinessModifiersEnum type;
		public string name = string.Empty;

		public HappinessModifierGroup group = null;

		public int effectAmount = 0;

		public int effectLengthSeconds = 0;

		public bool infinite = false;

		public HappinessModifierPrefab(string stringHappinessModifier, HappinessModifierGroup group) {
			this.group = group;

			List<string> stringHappinessModifierList = stringHappinessModifier.Split('\n').ToList();
			foreach (string stringHappinessModifierSingle in stringHappinessModifierList.Skip(1)) {

				if (!string.IsNullOrEmpty(stringHappinessModifierSingle)) {

					string label = stringHappinessModifierSingle.Split('>')[0].Replace("<", string.Empty);
					string value = stringHappinessModifierSingle.Split('>')[1];

					switch (label) {
						case "Type":
							type = (HappinessModifiersEnum)Enum.Parse(typeof(HappinessModifiersEnum), value);
							name = UIManager.SplitByCapitals(type.ToString());
							break;
						case "EffectAmount":
							effectAmount = int.Parse(value);
							break;
						case "EffectLengthSeconds":
							infinite = UIManager.RemoveNonAlphanumericChars(value) == "UntilNot";
							if (infinite) {
								effectLengthSeconds = int.MaxValue;
							} else {
								effectLengthSeconds = int.Parse(value);
							}
							break;
						default:
							MonoBehaviour.print("Unknown happiness modifier label: \"" + stringHappinessModifierSingle + "\"");
							break;
					}
				}
			}

			if (string.IsNullOrEmpty(name) || effectAmount == 0 || effectLengthSeconds == 0) {
				MonoBehaviour.print("Potential issue parsing happiness modifier: " + stringHappinessModifier);
			}
		}
	}

	public HappinessModifierPrefab GetHappinessModifierPrefabFromEnum(HappinessModifiersEnum happinessModifierEnum) {
		return happinessModifierPrefabs.Find(hmiPrefab => hmiPrefab.type == happinessModifierEnum);
	}

	public HappinessModifierPrefab GetHappinessModifierPrefabFromString(string happinessModifierTypeString) {
		return happinessModifierPrefabs.Find(happiessModifierPrefab => happiessModifierPrefab.type == (HappinessModifiersEnum)Enum.Parse(typeof(HappinessModifiersEnum), happinessModifierTypeString));
	}

	public class HappinessModifierInstance {
		public Colonist colonist;
		public HappinessModifierPrefab prefab;

		public float timer = 0;

		public HappinessModifierInstance(Colonist colonist, HappinessModifierPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;

			timer = prefab.effectLengthSeconds;
		}

		public void Update() {
			timer -= 1 * GameManager.timeM.deltaTime;
		}
	}

	public List<Colonist> colonists = new List<Colonist>();

	private List<Colonist> deadColonists = new List<Colonist>();

	public class Colonist : HumanManager.Human {

		public bool playerMoved;

		// Job
		public JobManager.Job job;
		public JobManager.Job storedJob;
		public JobManager.Job needJob;

		// Profession
		public Profession profession;
		public Profession oldProfession;

		// Skills
		public List<SkillInstance> skills = new List<SkillInstance>();

		// Traits
		public List<TraitInstance> traits = new List<TraitInstance>();

		// Needs
		public List<NeedInstance> needs = new List<NeedInstance>();

		// Happiness
		public float baseHappiness = 100;
		public float happinessModifiersSum = 100;
		public float effectiveHappiness = 100;
		public List<HappinessModifierInstance> happinessModifiers = new List<HappinessModifierInstance>();

		public Colonist(TileManager.Tile spawnTile, Profession profession, float startingHealth) : base(spawnTile, startingHealth) {
			obj.transform.SetParent(GameObject.Find("ColonistParent").transform, false);

			this.profession = profession;
			profession.colonistsInProfessionAtStart.Add(this);
			profession.colonistsInProfession.Add(this);

			foreach (SkillPrefab skillPrefab in GameManager.colonistM.skillPrefabs) {
				skills.Add(new SkillInstance(this, skillPrefab, true, 0));
			}

			foreach (NeedPrefab needPrefab in GameManager.colonistM.needPrefabs) {
				needs.Add(new NeedInstance(this, needPrefab));
			}
			needs = needs.OrderBy(need => need.prefab.priority).ToList();

			oldProfession = GameManager.colonistM.professions.Find(findProfession => findProfession.type == ProfessionTypeEnum.Nothing);

			GameManager.colonistM.colonists.Add(this);
		}

		public override void Update() {
			moveSpeedMultiplier = (-((inventory.CountResources() - inventory.maxAmount) / (float)inventory.maxAmount)) + 1;
			moveSpeedMultiplier = Mathf.Clamp(moveSpeedMultiplier, 0.1f, 1f);

			if (playerMoved && path.Count <= 0) {
				playerMoved = false;
			}

			base.Update();

			if (dead) {
				return;
			}

			UpdateNeeds();
			UpdateHappinessModifiers();
			UpdateHappiness();

			if (overTileChanged) {
				GameManager.jobM.UpdateColonistJobCosts(this);
			}

			if (job != null) {
				if (!job.started && overTile == job.tile) {
					StartJob();
				}
				if (job.started && overTile == job.tile && !Mathf.Approximately(job.jobProgress, 0)) {
					WorkJob();
				}
			}
			if (job == null) {
				if (path.Count <= 0) {
					List<ResourceManager.Container> validEmptyInventoryContainers = FindValidContainersToEmptyInventory();
					int inventoryCount = inventory.CountResources();
					if (validEmptyInventoryContainers.Count > 0 && ((inventoryCount >= inventory.maxAmount) || (inventoryCount > 0 && GameManager.jobM.GetColonistJobsCountForColonist(this) <= 0))) {
						EmptyInventory(validEmptyInventoryContainers);
					} else {
						Wander(null, 0);
					}
				} else {
					wanderTimer = UnityEngine.Random.Range(10f, 20f);
				}
			}
		}

		public override void Die() {
			base.Die();

			ReturnJob();
			GameManager.colonistM.colonists.Remove(this);
			GameManager.uiM.SetColonistElements();
			GameManager.uiM.SetJobElements();
			foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
				container.inventory.ReleaseReservedResources(this);
			}
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.humanM.SetSelectedHuman(null);
			}
			GameManager.jobM.UpdateAllColonistJobCosts();
		}

		public List<ResourceManager.Container> FindValidContainersToEmptyInventory() {
			return GameManager.resourceM.GetContainersInRegion(overTile.region).Where(container => container.inventory.CountResources() < container.inventory.maxAmount).ToList();
		}

		public void EmptyInventory(List<ResourceManager.Container> validContainers) {
			if (inventory.CountResources() > 0 && validContainers.Count > 0) {
				ReturnJob();
				ResourceManager.Container closestContainer = validContainers.OrderBy(container => PathManager.RegionBlockDistance(container.tile.regionBlock, overTile.regionBlock, true, true, false)).ToList()[0];
				SetJob(new JobManager.ColonistJob(this, new JobManager.Job(closestContainer.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.EmptyInventory), 0), null, null));
			}
		}

		private void UpdateNeeds() {
			foreach (NeedInstance need in needs) {
				GameManager.colonistM.CalculateNeedValue(need);
				bool checkNeed = false;
				if (need.colonist.job == null) {
					checkNeed = true;
				} else {
					if (GameManager.colonistM.needRelatedJobs.Contains(need.colonist.job.prefab.jobType)) {
						if (GameManager.colonistM.jobToNeedMap.ContainsKey(need.colonist.job.prefab.jobType)) {
							if (need.prefab.priority < GameManager.colonistM.GetNeedPrefabFromEnum(GameManager.colonistM.jobToNeedMap[need.colonist.job.prefab.jobType]).priority) {
								checkNeed = true;
							}
						} else {
							checkNeed = true;
						}
					} else {
						checkNeed = true;
					}
				}
				if (checkNeed) {
					if (needsValueFunctions[need.prefab.type](need)) {
						break;
					}
				}
			}
		}

		public void UpdateHappinessModifiers() {
			foreach (HappinessModifierGroup happinessModifierGroup in GameManager.colonistM.happinessModifierGroups) {
				happinessModifierFunctions[happinessModifierGroup.type](this);
			}

			for (int i = 0; i < happinessModifiers.Count; i++) {
				HappinessModifierInstance happinessModifier = happinessModifiers[i];
				happinessModifier.Update();
				if (happinessModifier.timer <= 0) {
					RemoveHappinessModifier(happinessModifier.prefab.type);
					i -= 1;
				}
			}
		}

		public void AddHappinessModifier(HappinessModifiersEnum happinessModifierEnum) {
			HappinessModifierInstance hmi = new HappinessModifierInstance(this, GameManager.colonistM.GetHappinessModifierPrefabFromEnum(happinessModifierEnum));
			HappinessModifierInstance sameGroupHMI = happinessModifiers.Find(findHMI => hmi.prefab.group.type == findHMI.prefab.group.type);
			if (sameGroupHMI != null) {
				RemoveHappinessModifier(sameGroupHMI.prefab.type);
			}
			happinessModifiers.Add(hmi);
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.uiM.RemakeSelectedColonistHappinessModifiers();
			}
		}

		public void RemoveHappinessModifier(HappinessModifiersEnum happinessModifierEnum) {
			happinessModifiers.Remove(happinessModifiers.Find(findHMI => findHMI.prefab.type == happinessModifierEnum));
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.uiM.RemakeSelectedColonistHappinessModifiers();
			}
		}

		public void UpdateHappiness() {
			baseHappiness = Mathf.Clamp(Mathf.RoundToInt(100 - (needs.Sum(need => (need.GetValue() / (need.prefab.priority + 1))))), 0, 100);

			happinessModifiersSum = happinessModifiers.Sum(hM => hM.prefab.effectAmount);

			float targetHappiness = Mathf.Clamp(baseHappiness + happinessModifiersSum, 0, 100);
			float happinessChangeAmount = ((targetHappiness - effectiveHappiness) / (effectiveHappiness <= 0f ? 1f : effectiveHappiness));
			effectiveHappiness += happinessChangeAmount * GameManager.timeM.deltaTime;
			effectiveHappiness = Mathf.Clamp(effectiveHappiness, 0, 100);
		}

		public void SetJob(JobManager.ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			job = colonistJob.job;
			job.colonistResources = colonistJob.colonistResources;
			job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && (job.containerPickups != null && job.containerPickups.Count > 0)) {
				foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
					containerPickup.container.inventory.ReserveResources(containerPickup.resourcesToPickup, this);
				}
			}
			if (job.transferResources != null && job.transferResources.Count > 0) {
				ResourceManager.Container collectContainer = (ResourceManager.Container)job.tile.GetAllObjectInstances().Find(oi => oi is ResourceManager.Container);
				if (collectContainer != null) {
					collectContainer.inventory.ReserveResources(job.transferResources, this);
				}
			}
			job.SetColonist(this);
			MoveToTile(job.tile, !job.tile.walkable);
			GameManager.uiM.SetJobElements();
		}

		public void StartJob() {
			job.started = true;

			job.jobProgress *= (1 + (1 - GetJobSkillMultiplier(job.prefab.jobType)));

			if (job.prefab.jobType == JobManager.JobTypesEnum.Eat) {
				job.jobProgress += needs.Find(need => need.prefab.type == NeedsEnum.Food).GetValue();
			}
			if (job.prefab.jobType == JobManager.JobTypesEnum.Sleep) {
				job.jobProgress += 20f * (needs.Find(need => need.prefab.type == NeedsEnum.Rest).GetValue());
			}

			job.colonistBuildTime = job.jobProgress;

			GameManager.uiM.SetJobElements();
		}

		public void WorkJob() {

			if (job.prefab.jobType == JobManager.JobTypesEnum.HarvestFarm && job.tile.farm == null) {
				job.jobUIElement.Remove();
				job.Remove();
				job = null;
				return;
			} else if (
				job.prefab.jobType == JobManager.JobTypesEnum.EmptyInventory ||
				job.prefab.jobType == JobManager.JobTypesEnum.CollectFood ||
				job.prefab.jobType == JobManager.JobTypesEnum.PickupResources) {

				ResourceManager.Container containerOnTile = GameManager.resourceM.containers.Find(container => container.tile == job.tile);
				if (containerOnTile == null) {
					job.jobUIElement.Remove();
					job.Remove();
					job = null;
					return;
				}
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.Sleep) {
				float currentRestValue = needs.Find(need => need.prefab.type == NeedsEnum.Rest).GetValue();
				float originalRestValue = currentRestValue / (job.jobProgress / job.colonistBuildTime);
				needs.Find(need => need.prefab.type == NeedsEnum.Rest).SetValue(originalRestValue * ((job.jobProgress - 1f * GameManager.timeM.deltaTime) / job.colonistBuildTime));
			}

			if (job.activeTileObject != null) {
				job.activeTileObject.SetActiveSprite(job);
			}

			job.jobProgress -= 1 * GameManager.timeM.deltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress, 0)) {
				job.jobProgress = 0;
				if (job.activeTileObject != null) {
					job.activeTileObject.SetActiveSprite(job);
				}
				FinishJob();
				return;
			}

			job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, ((job.colonistBuildTime - job.jobProgress) / job.colonistBuildTime));
		}

		public void FinishJob() {
			JobManager.Job finishedJob = job;
			job = null;

			MonoBehaviour.Destroy(finishedJob.jobPreview);
			if (finishedJob.prefab.addToTileWhenBuilt) {
				finishedJob.tile.SetTileObject(GameManager.resourceM.CreateTileObjectInstance(finishedJob.prefab, finishedJob.tile, finishedJob.rotationIndex, true));
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).FinishCreation();
				if (finishedJob.prefab.canRotate) {
					finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).obj.GetComponent<SpriteRenderer>().sprite = finishedJob.prefab.bitmaskSprites[finishedJob.rotationIndex];
				}
			}

			SkillInstance skill = GetSkillFromJobType(finishedJob.prefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJob.prefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			JobManager.finishJobFunctions[finishedJob.prefab.jobType](this, finishedJob);

			GameManager.jobM.UpdateSingleColonistJobs(this);
			GameManager.uiM.SetJobElements();
			GameManager.uiM.UpdateSelectedColonistInformation();
			GameManager.uiM.UpdateSelectedContainerInfo();
			GameManager.uiM.UpdateSelectedTradingPostInfo();
			GameManager.uiM.UpdateTileInformation();

			if (storedJob != null) {
				Update();
			}
		}

		public void ReturnJob() {
			foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
				container.inventory.ReleaseReservedResources(this);
			}
			if (storedJob != null) {
				if (GameManager.colonistM.jobToNeedMap.ContainsKey(storedJob.prefab.jobType) || JobManager.nonReturnableJobs.Contains(storedJob.prefab.jobType)) {
					storedJob.Remove();
					if (storedJob.jobUIElement != null) {
						storedJob.jobUIElement.Remove();
					}
					storedJob = null;
					if (job != null) {
						if (GameManager.colonistM.jobToNeedMap.ContainsKey(job.prefab.jobType) || JobManager.nonReturnableJobs.Contains(job.prefab.jobType)) {
							job.Remove();
							if (job.jobUIElement != null) {
								job.jobUIElement.Remove();
							}
						}
						job = null;
					}
					return;
				}
				GameManager.jobM.AddExistingJob(storedJob);
				if (storedJob.jobUIElement != null) {
					storedJob.jobUIElement.Remove();
				}
				if (storedJob.jobPreview != null) {
					storedJob.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				storedJob = null;
				if (job != null) {
					if (job.jobUIElement != null) {
						job.jobUIElement.Remove();
					}
					if (job.jobPreview != null) {
						job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
					}
					job = null;
				}
			} else if (job != null) {
				if (GameManager.colonistM.jobToNeedMap.ContainsKey(job.prefab.jobType) || JobManager.nonReturnableJobs.Contains(job.prefab.jobType)) {
					job.Remove();
					if (job.jobUIElement != null) {
						job.jobUIElement.Remove();
					}
					job = null;
					return;
				}
				GameManager.jobM.AddExistingJob(job);
				if (job.jobUIElement != null) {
					job.jobUIElement.Remove();
				}
				if (job.jobPreview != null) {
					job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				job = null;
			}
		}

		public void MoveToClosestWalkableTile(bool careIfOvertileIsWalkable) {
			if (careIfOvertileIsWalkable ? !overTile.walkable : true) {
				List<TileManager.Tile> walkableSurroundingTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable).ToList();
				if (walkableSurroundingTiles.Count > 0) {
					MoveToTile(walkableSurroundingTiles[UnityEngine.Random.Range(0, walkableSurroundingTiles.Count)], false);
				} else {
					walkableSurroundingTiles.Clear();
					List<TileManager.Tile> potentialWalkableSurroundingTiles = new List<TileManager.Tile>();
					foreach (TileManager.Map.RegionBlock regionBlock in overTile.regionBlock.horizontalSurroundingRegionBlocks) {
						if (regionBlock.tileType.walkable) {
							potentialWalkableSurroundingTiles.AddRange(regionBlock.tiles);
						}
					}
					walkableSurroundingTiles = potentialWalkableSurroundingTiles.Where(tile => tile.surroundingTiles.Find(nTile => !nTile.walkable && nTile.regionBlock == overTile.regionBlock) != null).ToList();
					if (walkableSurroundingTiles.Count > 0) {
						walkableSurroundingTiles = walkableSurroundingTiles.OrderBy(tile => Vector2.Distance(tile.obj.transform.position, overTile.obj.transform.position)).ToList();
						MoveToTile(walkableSurroundingTiles[0], false);
					} else {
						List<TileManager.Tile> validTiles = GameManager.colonyM.colony.map.tiles.Where(tile => tile.walkable).OrderBy(tile => Vector2.Distance(tile.obj.transform.position, overTile.obj.transform.position)).ToList();
						if (validTiles.Count > 0) {
							MoveToTile(validTiles[0], false);
						}
					}
				}
			}
		}

		public void PlayerMoveToTile(TileManager.Tile tile) {
			playerMoved = true;
			ReturnJob();
			if (job != null) {
				MoveToTile(tile, false);
			} else {
				MoveToTile(tile, false);
			}
		}

		public SkillInstance GetSkillFromJobType(JobManager.JobTypesEnum jobType) {
			return skills.Find(findSkill => findSkill.prefab.affectedJobTypes.ContainsKey(jobType));
		}

		public float GetJobSkillMultiplier(JobManager.JobTypesEnum jobType) {
			SkillInstance skill = GetSkillFromJobType(jobType);
			if (skill != null) {
				return 1 * (-(1f / (((skill.prefab.affectedJobTypes[jobType]) * (skill.level)) + 1)) + 1);
			}
			return 1.0f;
		}

		public void ChangeProfession(Profession newProfession) {
			if (profession != newProfession) {
				profession.colonistsInProfession.Remove(this);
				oldProfession = profession;
				profession = newProfession;
				if (newProfession != null) {
					profession.colonistsInProfession.Add(this);
				}
			}
		}

		public override void SetName(string name) {
			base.SetName(name);
			SetNameColour(UIManager.GetColour(UIManager.Colours.LightGreen));
		}
	}

	public void SpawnStartColonists(int amount) {
		SpawnColonists(amount);

		Vector2 averageColonistPosition = new Vector2(0, 0);
		foreach (Colonist colonist in colonists) {
			averageColonistPosition = new Vector2(averageColonistPosition.x + colonist.obj.transform.position.x, averageColonistPosition.y + colonist.obj.transform.position.y);
		}
		averageColonistPosition /= colonists.Count;
		GameManager.cameraM.SetCameraPosition(averageColonistPosition);

		// TEMPORARY COLONIST TESTING STUFF
		colonists[UnityEngine.Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.WheatSeed), UnityEngine.Random.Range(5, 11), false);
		colonists[UnityEngine.Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.Potato), UnityEngine.Random.Range(5, 11), false);
		colonists[UnityEngine.Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.CottonSeed), UnityEngine.Random.Range(5, 11), false);
	}

	public void SpawnColonists(int amount) {
		if (amount > 0) {
			int mapSize = GameManager.colonyM.colony.map.mapData.mapSize;
			for (int i = 0; i < amount; i++) {
				List<TileManager.Tile> walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && o.buildable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
				if (walkableTilesByDistanceToCentre.Count <= 0) {
					foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles.Where(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
						tile.SetTileType(tile.biome.tileTypes[TileManager.TileTypeGroupEnum.Ground], true, true, true);
					}
					walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
				}

				List<TileManager.Tile> validSpawnTiles = new List<TileManager.Tile>();
				TileManager.Tile currentTile = walkableTilesByDistanceToCentre[0];
				float minimumDistance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
				foreach (TileManager.Tile tile in walkableTilesByDistanceToCentre) {
					float distance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
					if (distance < minimumDistance) {
						currentTile = tile;
						minimumDistance = distance;
					}
				}
				List<TileManager.Tile> frontier = new List<TileManager.Tile>() { currentTile };
				List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>();
				while (frontier.Count > 0) {
					currentTile = frontier[0];
					frontier.RemoveAt(0);
					checkedTiles.Add(currentTile);
					validSpawnTiles.Add(currentTile);
					if (validSpawnTiles.Count > 100) {
						break;
					}
					foreach (TileManager.Tile nTile in currentTile.horizontalSurroundingTiles) {
						if (walkableTilesByDistanceToCentre.Contains(nTile) && !checkedTiles.Contains(nTile)) {
							frontier.Add(nTile);
						}
					}
				}
				TileManager.Tile colonistSpawnTile = validSpawnTiles.Count >= amount ? validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)] : walkableTilesByDistanceToCentre[UnityEngine.Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

				new Colonist(colonistSpawnTile, professions[UnityEngine.Random.Range(0, professions.Count)], 1);
			}

			GameManager.uiM.SetColonistElements();
			GameManager.colonyM.colony.map.Bitmasking(GameManager.colonyM.colony.map.tiles, true, true);
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
		}
	}
}