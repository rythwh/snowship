using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ColonistManager : MonoBehaviour {

	private TileManager tileM;
	private CameraManager cameraM;
	private UIManager uiM;
	private ResourceManager resourceM;
	private PathManager pathM;
	private JobManager jobM;
	private TimeManager timeM;
	private DebugManager debugM;

	private void GetScriptReferences() {
		tileM = GetComponent<TileManager>();
		cameraM = GetComponent<CameraManager>();
		uiM = GetComponent<UIManager>();
		resourceM = GetComponent<ResourceManager>();
		pathM = GetComponent<PathManager>();
		jobM = GetComponent<JobManager>();
		timeM = GetComponent<TimeManager>();
		debugM = GetComponent<DebugManager>();
	}

	void Awake() {
		GetScriptReferences();

		foreach (string name in Resources.Load<TextAsset>(@"Data/names-male").text.Split(' ').ToList()) {
			maleNames.Add(name.ToLower());
		}
		foreach (string name in Resources.Load<TextAsset>(@"Data/names-female").text.Split(' ').ToList()) {
			femaleNames.Add(name.ToLower());
		}

		for (int i = 0; i < 3; i++) {
			List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
			humanMoveSprites.Add(innerHumanMoveSprites);
		}

		CreateColonistSkills();
		CreateColonistProfessions();
		CreateColonistNeeds();
		CreateHappinessModifiers();

		InitializeNeedsValueFunctions();
		InitializeHappinessModifierFunctions();
	}

	public List<Life> animals = new List<Life>();

	public class Life {

		public TileManager tileM;
		public PathManager pathM;
		public TimeManager timeM;
		public ColonistManager colonistM;
		public JobManager jobM;
		public ResourceManager resourceM;
		public UIManager uiM;
		public CameraManager cameraM;

		void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			pathM = GM.GetComponent<PathManager>();
			timeM = GM.GetComponent<TimeManager>();
			colonistM = GM.GetComponent<ColonistManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
			uiM = GM.GetComponent<UIManager>();
			cameraM = GM.GetComponent<CameraManager>();
		}

		public float health;

		public GameObject obj;

		public bool overTileChanged = false;
		public TileManager.Tile overTile;
		public List<Sprite> moveSprites = new List<Sprite>();

		public Gender gender;

		public Life(TileManager.Tile spawnTile, float startingHealth, Gender gender) {

			GetScriptReferences();

			overTile = spawnTile;
			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),overTile.obj.transform.position,Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 10; // Life Sprite

			health = startingHealth;

			this.gender = gender;
		}

		private Dictionary<int,int> moveSpritesMap = new Dictionary<int,int>() {
			{16,1 },{32,0 },{64,0 },{128,1}
		};
		private float moveTimer;
		public List<TileManager.Tile> path = new List<TileManager.Tile>();
		public float moveSpeedMultiplier = 1f;

		public void Update() {
			overTileChanged = false;
			TileManager.Tile newOverTile = tileM.map.GetTileFromPosition(obj.transform.position);
			if (overTile != newOverTile) {
				overTileChanged = true;
				overTile = newOverTile;
				SetColour(overTile.sr.color);
			}
			MoveToTile(null,false);
		}

		private Vector2 oldPosition;
		public bool MoveToTile(TileManager.Tile tile, bool allowEndTileNonWalkable) {
			if (tile != null) {
				path = pathM.FindPathToTile(overTile,tile,allowEndTileNonWalkable);
				if (path.Count > 0) {
					SetMoveSprite();
				}
				moveTimer = 0;
				oldPosition = obj.transform.position;
			}
			if (path.Count > 0) {
				
				obj.transform.position = Vector2.Lerp(oldPosition,path[0].obj.transform.position,moveTimer);

				if (moveTimer >= 1f) {
					oldPosition = obj.transform.position;
					obj.transform.position = path[0].obj.transform.position;
					moveTimer = 0;
					path.RemoveAt(0);
					if (path.Count > 0) {
						SetMoveSprite();
					}
				} else {
					moveTimer += 2 * timeM.deltaTime * overTile.walkSpeed * moveSpeedMultiplier;
				}
			} else {
				obj.GetComponent<SpriteRenderer>().sprite = moveSprites[0];
				path.Clear();
				return true;
			}
			return false;
		}

		public void SetMoveSprite() {
			int bitsum = 0;
			for (int i = 0; i < overTile.surroundingTiles.Count; i++) {
				if (overTile.surroundingTiles[i] == path[0]) {
					bitsum = Mathf.RoundToInt(Mathf.Pow(2,i));
				}
			}
			if (bitsum >= 16) {
				bitsum = moveSpritesMap[bitsum];
			}
			obj.GetComponent<SpriteRenderer>().sprite = moveSprites[bitsum];
		}

		public void SetColour(Color newColour) {
			obj.GetComponent<SpriteRenderer>().color = new Color(newColour.r,newColour.g,newColour.b,1f);
		}

		public bool dead = false;

		public void ChangeHealthValue(float amount) {
			dead = false;
			health += amount;
			if (health > 1f) {
				health = 1f;
			} else if (health <= 0f) {
				Die();
				health = 0f;
				dead = true;
			}
		}

		public void Die() {
			Destroy(obj);
		}
	}

	public string GetName(Gender gender) {
		string chosenName = "NAME";
		List<string> names = (gender == Gender.Male ? maleNames : femaleNames);
		List<string> validNames = names.Where(name => colonists.Find(colonistWithName => colonistWithName.name.ToLower() == name.ToLower()) == null).ToList();
		if (validNames.Count > 0) {
			chosenName = validNames[Random.Range(0, validNames.Count)];
		} else {
			chosenName = names[Random.Range(0, names.Count)];
		}
		string tempName = chosenName.Take(1).ToList()[0].ToString().ToUpper();
		foreach (char character in chosenName.Skip(1)) {
			tempName += character;
		}
		chosenName = tempName;
		return chosenName;
	}

	public class Human : Life {

		public string name;
		public GameObject nameCanvas;

		public Dictionary<HumanLook, int> humanLookIndices;

		// Carrying Item

		// Inventory
		public ResourceManager.Inventory inventory;

		public Human(TileManager.Tile spawnTile,Dictionary<HumanLook,int> humanLookIndices, float startingHealth, Gender gender) : base(spawnTile,startingHealth, gender) {
			moveSprites = colonistM.humanMoveSprites[humanLookIndices[HumanLook.Skin]];
			this.humanLookIndices = humanLookIndices;

			inventory = new ResourceManager.Inventory(this,null,50);

			name = colonistM.GetName(gender);

			obj.name = "Human: " + name;

			nameCanvas = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/Human-Canvas"),obj.transform,false);
			SetNameCanvas(name);
		}

		public void SetNameCanvas(string name) {
			nameCanvas.transform.Find("NameBackground-Image/Name-Text").GetComponent<Text>().text = name;
			Canvas.ForceUpdateCanvases(); // The charInfo.advance is not calculated until the text is rendered
			int textWidthPixels = 0;
			foreach (char character in name) {
				CharacterInfo charInfo;
				Resources.Load<Font>(@"UI/Fonts/Quicksand/Quicksand-Bold").GetCharacterInfo(character, out charInfo, 48, FontStyle.Normal);
				textWidthPixels += charInfo.advance;
			}
			nameCanvas.transform.Find("NameBackground-Image").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidthPixels / 2.8f, 20);
		}

		public new void Update() {
			base.Update();
			nameCanvas.transform.Find("NameBackground-Image").localScale = Vector2.one * Mathf.Clamp(cameraM.cameraComponent.orthographicSize,2,10) * 0.0025f;
			nameCanvas.transform.Find("NameBackground-Image").GetComponent<Image>().color = Color.Lerp(uiM.GetColour(UIManager.Colours.LightRed), uiM.GetColour(UIManager.Colours.LightGreen), health);
		}
	}

	public enum SkillTypeEnum { Building, Mining, Digging, Farming, Forestry, Crafting };

	public class SkillPrefab {

		public SkillTypeEnum type;
		public string name;

		public Dictionary<JobManager.JobTypesEnum,float> affectedJobTypes = new Dictionary<JobManager.JobTypesEnum,float>();

		public SkillPrefab(List<string> data) {
			type = (SkillTypeEnum)System.Enum.Parse(typeof(SkillTypeEnum),data[0]);
			name = type.ToString();

			foreach (string affectedJobTypeString in data[1].Split(';')) {
				List<string> affectedJobTypeData = affectedJobTypeString.Split(',').ToList();
				affectedJobTypes.Add((JobManager.JobTypesEnum)System.Enum.Parse(typeof(JobManager.JobTypesEnum),affectedJobTypeData[0]),float.Parse(affectedJobTypeData[1]));
			}
		}
	}

	public SkillPrefab GetSkillPrefabFromString(string skillTypeString) {
		return skillPrefabs.Find(skillPrefab => skillPrefab.type == (SkillTypeEnum)System.Enum.Parse(typeof(SkillTypeEnum),skillTypeString));
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
				level = Random.Range((colonist.profession.primarySkill != null && colonist.profession.primarySkill.type == prefab.type ? Mathf.RoundToInt(colonist.profession.skillRandomMaxValues[prefab] / 2f) : 0), colonist.profession.skillRandomMaxValues[prefab]);
			} else {
				level = startingLevel;
			}

			currentExperience = Random.Range(0,100);
			nextLevelExperience = 100 + (10 * level);
			AddExperience(0);
		}

		public void AddExperience(float amount) {
			currentExperience += amount;
			while (currentExperience >= nextLevelExperience) {
				level += 1;
				currentExperience = (currentExperience - nextLevelExperience);
				nextLevelExperience = 100 + (10 * level);
				colonist.jobM.UpdateColonistJobCosts(colonist);
			}
		}
	}

	public List<SkillPrefab> skillPrefabs = new List<SkillPrefab>();

	void CreateColonistSkills() {
		List<string> stringSkills = Resources.Load<TextAsset>(@"Data/colonistskills").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringSkill in stringSkills) {
			List<string> stringSkillData = stringSkill.Split('/').ToList();
			skillPrefabs.Add(new SkillPrefab(stringSkillData));
		}
		foreach (SkillPrefab skillPrefab in skillPrefabs) {
			skillPrefab.name = uiM.SplitByCapitals(skillPrefab.name);
		}
	}

	public enum ProfessionTypeEnum { Nothing, Builder, Miner, Farmer, Forester, Maker };

	public List<Profession> professions = new List<Profession>();

	public class Profession {
		public ProfessionTypeEnum type;
		public string name;

		public string description;

		public SkillPrefab primarySkill;

		public Dictionary<SkillPrefab,int> skillRandomMaxValues = new Dictionary<SkillPrefab,int>();

		public List<Colonist> colonistsInProfessionAtStart = new List<Colonist>();
		public List<Colonist> colonistsInProfession = new List<Colonist>();

		public Profession(List<string> data, ColonistManager colonistM) {
			type = (ProfessionTypeEnum)System.Enum.Parse(typeof(ProfessionTypeEnum),data[0]);
			name = type.ToString();

			description = data[1];

			int primarySkillString = 0;
			if (!int.TryParse(data[2],out primarySkillString)) {
				primarySkill = colonistM.GetSkillPrefabFromString(data[2]);
			}

			foreach (string skillRandomMaxValueData in data[3].Split(';')) {
				List<string> skillRandomMaxValue = skillRandomMaxValueData.Split(',').ToList();
				skillRandomMaxValues.Add(colonistM.GetSkillPrefabFromString(skillRandomMaxValue[0]),int.Parse(skillRandomMaxValue[1]));
			}
		}
	}

	void CreateColonistProfessions() {
		List<string> stringProfessions = Resources.Load<TextAsset>(@"Data/colonistprofessions").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringProfession in stringProfessions) {
			List<string> stringProfessionData = stringProfession.Split('/').ToList();
			professions.Add(new Profession(stringProfessionData,this));
		}
		foreach (Profession profession in professions) {
			profession.name = uiM.SplitByCapitals(profession.name);
		}
	}

	public enum TraitsEnum { Lazy, Workaholic, Alcoholic, Antisocial, Socialite, Attractive, Unattractive, Dieter, Overeater };

	public List<TraitPrefab> traitPrefabs = new List<TraitPrefab>();

	public class TraitPrefab {

		public TraitsEnum type;
		public string name;

		public float effectAmount;

		public TraitPrefab(List<string> data) {

		}
	}

	public TraitPrefab GetTraitPrefabFromString(string traitTypeString) {
		return traitPrefabs.Find(traitPrefab => traitPrefab.type == (TraitsEnum)System.Enum.Parse(typeof(TraitsEnum), traitTypeString));
	}

	public class TraitInstance {

		public Colonist colonist;
		public TraitPrefab prefab;

		public TraitInstance(Colonist colonist,TraitPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}
	}

	public enum NeedsEnum { Food, Clothing, Shelter, Rest, Temperature, Safety, Social, Esteem, Relaxation };

	public Dictionary<NeedsEnum, System.Func<NeedInstance, bool>> needsValueFunctions = new Dictionary<NeedsEnum, System.Func<NeedInstance, bool>>();

	private List<JobManager.JobTypesEnum> needRelatedJobs = new List<JobManager.JobTypesEnum>() {
		JobManager.JobTypesEnum.CollectFood, JobManager.JobTypesEnum.Eat, JobManager.JobTypesEnum.Sleep
	};

	private Dictionary<JobManager.JobTypesEnum, NeedsEnum> jobToNeedMap = new Dictionary<JobManager.JobTypesEnum, NeedsEnum>() {
		{ JobManager.JobTypesEnum.CollectFood, NeedsEnum.Food },
		{ JobManager.JobTypesEnum.Eat, NeedsEnum.Food },
		{ JobManager.JobTypesEnum.Sleep, NeedsEnum.Rest }
	};

	private Dictionary<NeedsEnum, List<JobManager.JobTypesEnum>> needToJobsMap = new Dictionary<NeedsEnum, List<JobManager.JobTypesEnum>>() {
		{ NeedsEnum.Food, new List<JobManager.JobTypesEnum>() { JobManager.JobTypesEnum.CollectFood, JobManager.JobTypesEnum.Eat } },
		{ NeedsEnum.Rest, new List<JobManager.JobTypesEnum>() { JobManager.JobTypesEnum.Sleep } }
	};
	public Dictionary<NeedsEnum, List<JobManager.JobTypesEnum>> GetNeedToJobsMap() {
		return needToJobsMap;
	}

	public Dictionary<NeedsEnum,System.Func<NeedInstance,float>> needsValueSpecialIncreases = new Dictionary<NeedsEnum,System.Func<NeedInstance,float>>();

	public void InitializeNeedsValueSpecialIncreases() {
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
		needsValueSpecialIncreases.Add(NeedsEnum.Rest,delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			if (!timeM.isDay) {
				totalSpecialIncrease += 0.05f;
			}
			HappinessModifierInstance hmi = need.colonist.happinessModifiers.Find(findHMI => findHMI.prefab.group.type == HappinessModifierGroupsEnum.Rest);
			if (hmi.prefab.type == HappinessModifiersEnum.Rested) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.8f);
			}
			return totalSpecialIncrease;
		});
	}

	public void CalculateNeedValue(NeedInstance need) {
		if (needToJobsMap.ContainsKey(need.prefab.type) && need.colonist.job != null && needToJobsMap[need.prefab.type].Contains(need.colonist.job.prefab.jobType)) {
			return;
		}
		float needIncreaseAmount = need.prefab.baseIncreaseRate;
		foreach (TraitInstance trait in need.colonist.traits) {
			if (need.prefab.traitsAffectingThisNeed.ContainsKey(trait.prefab.type)) {
				needIncreaseAmount *= need.prefab.traitsAffectingThisNeed[trait.prefab.type];
			}
		}
		need.value += (needIncreaseAmount + (needsValueSpecialIncreases.ContainsKey(need.prefab.type) ? needsValueSpecialIncreases[need.prefab.type](need) : 0)) * timeM.deltaTime;
		need.value = Mathf.Clamp(need.value,0,need.prefab.clampValue);
	}

	KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>> FindClosestFood(Colonist colonist, float minimumNutritionRequired,bool takeFromOtherColonists,bool eatAnything) {
		List<KeyValuePair<KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>>,int>> resourcesPerInventory = new List<KeyValuePair<KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>>,int>>();
		foreach (ResourceManager.Container container in resourceM.containers.Where(c => c.parentObject.tile.region == colonist.overTile.region).OrderBy(c => pathM.RegionBlockDistance(colonist.overTile.regionBlock,c.parentObject.tile.regionBlock,true,true,false))) {
			if (container.inventory.resources.Find(ra => ra.resource.resourceGroup.type == ResourceManager.ResourceGroupsEnum.Foods) != null) {
				List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
				int totalNutrition = 0;
				foreach (ResourceManager.ResourceAmount ra in container.inventory.resources.Where(ra => ra.resource.resourceGroup.type == ResourceManager.ResourceGroupsEnum.Foods).OrderBy(ra => ra.resource.nutrition).ToList()) {
					int numReserved = 0;
					for (int i = 0; i < ra.amount; i++) {
						numReserved += 1;
						totalNutrition += ra.resource.nutrition;
						if (totalNutrition >= minimumNutritionRequired) {
							break;
						}
					}
					resourcesToReserve.Add(new ResourceManager.ResourceAmount(ra.resource,numReserved));
					if (totalNutrition >= minimumNutritionRequired) {
						break;
					}
				}
				if (totalNutrition >= minimumNutritionRequired) {
					resourcesPerInventory.Add(new KeyValuePair<KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>>,int>(new KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>>(container.inventory,resourcesToReserve),totalNutrition));
					break;
				}
			}
		}
		if (takeFromOtherColonists) {
			//print("Take from other colonists.");
		}
		if (resourcesPerInventory.Count > 0) {
			return resourcesPerInventory[0].Key;
		} else {
			return new KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>>(null,null);
		}
	}

	public bool GetFood(NeedInstance need, bool takeFromOtherColonists, bool eatAnything) {
		if (need.colonist.inventory.resources.Find(ra => ra.resource.resourceGroup.type == ResourceManager.ResourceGroupsEnum.Foods) == null) {
			KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> closestFood = FindClosestFood(need.colonist, need.value, takeFromOtherColonists, eatAnything);

			List<ResourceManager.ResourceAmount> resourcesToReserve = closestFood.Value;
			if (closestFood.Key != null) {
				if (closestFood.Key.container != null) {
					ResourceManager.Container container = closestFood.Key.container;
					container.inventory.ReserveResources(resourcesToReserve, need.colonist);
					JobManager.Job job = new JobManager.Job(container.parentObject.tile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.CollectFood), 0);
					need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, job, null, null, jobM, pathM));
					return true;
				} else if (closestFood.Key.human != null) {
					//Human human = closestFood.Key.human;
					//print("Take from other human.");
				}
			}
		} else {
			need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(need.colonist.overTile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Eat), 0), null, null, jobM, pathM));
			return true;
		}
		return false;
	}

	public int FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum resourceGroup, Colonist colonist, bool worldTotal, bool includeOtherColonists) {
		if (worldTotal) {
			int total = 0;
			foreach (ResourceManager.Resource resource in resourceM.resources) {
				if (resource.resourceGroup.type == resourceGroup) {
					total += resource.worldTotalAmount;
				}
			}
			return total;
		} else {
			int total = 0;

			int amountOnThisColonist = 0;
			foreach (ResourceManager.ResourceAmount resourceAmount in colonist.inventory.resources) {
				if (resourceAmount.resource.resourceGroup.type == resourceGroup) {
					amountOnThisColonist += resourceAmount.amount;
				}
			}
			total += amountOnThisColonist;

			int amountUnreservedInContainers = 0;
			foreach (ResourceManager.Resource resource in resourceM.resources) {
				if (resource.resourceGroup.type == resourceGroup) {
					amountUnreservedInContainers += resource.unreservedContainerTotalAmount;
				}
			}
			total += amountUnreservedInContainers;

			if (includeOtherColonists) {
				int amountOnOtherColonists = 0;
				foreach (Colonist otherColonist in colonists) {
					if (colonist != otherColonist) {

					}
				}
				total += amountOnOtherColonists;
			}

			return total;
		}
	}

	public bool GetSleep(NeedInstance need, bool sleepAnywhere) {
		if (resourceM.sleepSpots.Count > 0) {
			List<ResourceManager.SleepSpot> validSleepSpots = resourceM.sleepSpots.Where(sleepSpot => sleepSpot.occupyingColonist == null && sleepSpot.parentObject.tile.region == need.colonist.overTile.region).ToList();
			if (validSleepSpots.Count > 0) {
				ResourceManager.SleepSpot chosenSleepSpot = validSleepSpots.OrderByDescending(sleepSpot => sleepSpot.parentObject.prefab.restComfortAmount / (pathM.RegionBlockDistance(need.colonist.overTile.regionBlock, sleepSpot.parentObject.tile.regionBlock, true, true, false) + 1)).ToList()[0];
				chosenSleepSpot.StartSleeping(need.colonist);
				need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(chosenSleepSpot.parentObject.tile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Sleep), 0), null, null, jobM, pathM));
				return true;
			}
		}
		if (sleepAnywhere) {
			need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(need.colonist.overTile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Sleep), 0), null, null, jobM, pathM));
			return true;
		}
		return false;
	}

	public void InitializeNeedsValueFunctions() {
		needsValueFunctions.Add(NeedsEnum.Food,delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.CollectFood || need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Eat)) {
				if (need.prefab.criticalValueAction && need.value >= need.prefab.criticalValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * timeM.deltaTime);
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,false,false) > 0) { // true, true
						if (timeM.minuteChanged) {
							need.colonist.ReturnJob();
							return GetFood(need, false, false); // true, true
						}
					}
					return false;
				}
				if (need.prefab.maximumValueAction && need.value >= need.prefab.maximumValue) {
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,false,false) > 0) { // false, true
						if (timeM.minuteChanged && Random.Range(0f, 1f) < ((need.value - need.prefab.maximumValue) / (need.prefab.criticalValue - need.prefab.maximumValue))) {
							need.colonist.ReturnJob();
							return GetFood(need, false, false); // true, false
						}
					}
					return false;
				}
				if (need.prefab.minimumValueAction && need.value >= need.prefab.minimumValue) {
					if (need.colonist.job == null) {
						if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods, need.colonist, false, false) > 0) {
							if (timeM.minuteChanged && Random.Range(0f, 1f) < ((need.value - need.prefab.minimumValue) / (need.prefab.maximumValue - need.prefab.minimumValue))) {
								need.colonist.ReturnJob();
								return GetFood(need, false, false);
							}
						}
					}
					return false;
				}
			}
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Rest,delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Sleep)) {
				if (need.prefab.criticalValueAction && need.value >= need.prefab.criticalValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * timeM.deltaTime);
					if (timeM.minuteChanged) {
						need.colonist.ReturnJob();
						GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.maximumValueAction && need.value >= need.prefab.maximumValue) {
					if (timeM.minuteChanged && Random.Range(0f, 1f) < ((need.value - need.prefab.maximumValue) / (need.prefab.criticalValue - need.prefab.maximumValue))) {
						need.colonist.ReturnJob();
						GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.minimumValueAction && need.value >= need.prefab.minimumValue) {
					if (need.colonist.job == null) {
						if (timeM.minuteChanged && Random.Range(0f, 1f) < ((need.value - need.prefab.minimumValue) / (need.prefab.maximumValue - need.prefab.minimumValue))) {
							need.colonist.ReturnJob();
							GetSleep(need, false);
						}
						
					}
					return false;
				}
			}
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Clothing,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Shelter,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Temperature,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Safety,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Social,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Esteem,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Relaxation,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
			return false;
		});
	}

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

		public bool minimumValueAction;
		public float minimumValue;

		public bool maximumValueAction;
		public float maximumValue;

		public bool criticalValueAction;
		public float criticalValue;

		public bool canDie;
		public float healthDecreaseRate;

		public int clampValue;

		public int priority;

		public Dictionary<TraitsEnum,float> traitsAffectingThisNeed = new Dictionary<TraitsEnum,float>();

		public NeedPrefab(List<string> data, UIManager uiM) {

			type = (NeedsEnum)System.Enum.Parse(typeof(NeedsEnum),data[0]);
			name = uiM.SplitByCapitals(type.ToString());

			baseIncreaseRate = float.Parse(data[1]);

			minimumValueAction = bool.Parse(data[2]);
			minimumValue = float.Parse(data[3]);

			maximumValueAction = bool.Parse(data[4]);
			maximumValue = float.Parse(data[5]);

			criticalValueAction = bool.Parse(data[6]);
			criticalValue = float.Parse(data[7]);

			canDie = bool.Parse(data[8]);
			healthDecreaseRate = float.Parse(data[9]);

			clampValue = int.Parse(data[10]);

			priority = int.Parse(data[11]);

			if (int.Parse(data[12]) != 0) {
				List<string> traitNameStrings = data[13].Split(',').ToList();
				List<string> traitEffectAmountStrings = data[14].Split(',').ToList();

				for (int i = 0; i < traitNameStrings.Count; i++) {
					traitsAffectingThisNeed.Add((TraitsEnum)System.Enum.Parse(typeof(TraitsEnum),traitNameStrings[i]),float.Parse(traitEffectAmountStrings[i]));
				}
			}
		}
	}

	public NeedPrefab GetNeedPrefabFromEnum(NeedsEnum needsEnumValue) {
		return needPrefabs.Find(needPrefab => needPrefab.type == needsEnumValue);
	}

	public NeedPrefab GetNeedPrefabFromString(string needTypeString) {
		return needPrefabs.Find(needPrefab => needPrefab.type == (NeedsEnum)System.Enum.Parse(typeof(NeedsEnum), needTypeString));
	}

	public class NeedInstance {

		public Colonist colonist;
		public NeedPrefab prefab;
		public float value = 0;

		public NeedInstance(Colonist colonist,NeedPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}
	}

	void CreateColonistNeeds() {
		List<string> stringNeeds = Resources.Load<TextAsset>(@"Data/colonistneeds").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringNeed in stringNeeds) {
			List<string> stringNeedData = stringNeed.Split('/').ToList();
			needPrefabs.Add(new NeedPrefab(stringNeedData, uiM));
		}
	}

	void CreateHappinessModifiers() {
		List<string> stringHappinessModifierGroups = Resources.Load<TextAsset>(@"Data/happinessModifiers").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('~')[0].Split(',').ToList();
		List<string> stringHappinessModifiers = Resources.Load<TextAsset>(@"Data/happinessModifiers").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('~')[1].Split('`').ToList();
		foreach (string stringHappinessModifierGroup in stringHappinessModifierGroups) {
			happinessModifierGroups.Add(new HappinessModifierGroup(stringHappinessModifierGroup, uiM));
		}
		foreach (string stringHappinessModifier in stringHappinessModifiers) {
			List<string> stringHappinessModifierData = stringHappinessModifier.Split('/').ToList();
			happinessModifierPrefabs.Add(new HappinessModifierPrefab(stringHappinessModifierData, this, uiM));
		}
	}

	public enum HappinessModifierGroupsEnum { Death, Food, Rest, Inventory };
	public enum HappinessModifiersEnum { WitnessDeath, Stuffed, Full, Hungry, Starving, Rested, Tired, Exhausted, Overencumbered };

	public List<HappinessModifierGroup> happinessModifierGroups = new List<HappinessModifierGroup>();

	public class HappinessModifierGroup {
		public HappinessModifierGroupsEnum type;
		public string name;

		public List<HappinessModifierPrefab> prefabs = new List<HappinessModifierPrefab>();

		public HappinessModifierGroup(string typeString, UIManager uiM) {
			type = (HappinessModifierGroupsEnum)System.Enum.Parse(typeof(HappinessModifierGroupsEnum), typeString);
			name = uiM.SplitByCapitals(type.ToString());
		}
	}

	public HappinessModifierGroup GetHappinessModifierGroupFromEnum(HappinessModifierGroupsEnum happinessModifierGroupEnum) {
		return happinessModifierGroups.Find(hmiGroup => hmiGroup.type == happinessModifierGroupEnum);
	}

	public List<HappinessModifierPrefab> happinessModifierPrefabs = new List<HappinessModifierPrefab>();

	public class HappinessModifierPrefab {

		public HappinessModifiersEnum type;
		public string name;

		public HappinessModifierGroup group;

		public int effectAmount;

		public int effectLengthSeconds;

		public bool infinite;

		public HappinessModifierPrefab(List<string> dataSplit, ColonistManager colonistM, UIManager uiM) {
			type = (HappinessModifiersEnum)System.Enum.Parse(typeof(HappinessModifiersEnum), dataSplit[0]);
			name = uiM.SplitByCapitals(type.ToString());

			group = colonistM.GetHappinessModifierGroupFromEnum((HappinessModifierGroupsEnum)System.Enum.Parse(typeof(HappinessModifierGroupsEnum), dataSplit[1]));
			group.prefabs.Add(this);

			effectAmount = int.Parse(dataSplit[2]);

			effectLengthSeconds = int.Parse(dataSplit[3]);

			if (effectLengthSeconds < 0) {
				effectLengthSeconds = int.MaxValue;
				infinite = true;
			} else {
				infinite = false;
			}
		}
	}

	public HappinessModifierPrefab GetHappinessModifierPrefabFromEnum(HappinessModifiersEnum happinessModifierEnum) {
		return happinessModifierPrefabs.Find(hmiPrefab => hmiPrefab.type == happinessModifierEnum);
	}

	public HappinessModifierPrefab GetHappinessModifierPrefabFromString(string happinessModifierTypeString) {
		return happinessModifierPrefabs.Find(happiessModifierPrefab => happiessModifierPrefab.type == (HappinessModifiersEnum)System.Enum.Parse(typeof(HappinessModifiersEnum), happinessModifierTypeString));
	}

	public class HappinessModifierInstance {
		public Colonist colonist;
		public HappinessModifierPrefab prefab;

		public float timer = 0;

		public HappinessModifierInstance(Colonist colonist,HappinessModifierPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;

			timer = prefab.effectLengthSeconds;
		}

		public void Update(TimeManager timeM) {
			timer -= 1 * timeM.deltaTime;
		}
	}

	public Dictionary<HappinessModifiersEnum,System.Action<HappinessModifierInstance>> happinessModifierFunctions = new Dictionary<HappinessModifiersEnum,System.Action<HappinessModifierInstance>>();

	public void InitializeHappinessModifierFunctions() {
		happinessModifierFunctions.Add(HappinessModifiersEnum.WitnessDeath,delegate (HappinessModifierInstance happinessModifierInstance) {

		});
	}

	public List<Colonist> colonists = new List<Colonist>();

	public class Colonist : Human {

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

		public Colonist(TileManager.Tile spawnTile,Dictionary<HumanLook,int> humanLookIndices, Profession profession, float startingHealth, Gender gender) : base(spawnTile,humanLookIndices,startingHealth,gender) {
			obj.transform.SetParent(GameObject.Find("ColonistParent").transform,false);

			this.profession = profession;
			profession.colonistsInProfessionAtStart.Add(this);
			profession.colonistsInProfession.Add(this);

			foreach (SkillPrefab skillPrefab in colonistM.skillPrefabs) {
				skills.Add(new SkillInstance(this,skillPrefab,true,0));
			}

			foreach (NeedPrefab needPrefab in colonistM.needPrefabs) {
				needs.Add(new NeedInstance(this,needPrefab));
			}
			needs = needs.OrderBy(need => need.prefab.priority).ToList();

			oldProfession = colonistM.professions.Find(findProfession => findProfession.type == ProfessionTypeEnum.Nothing);
		}

		public new void Update() {
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

			if (overTileChanged) {
				jobM.UpdateColonistJobCosts(this);
			}

			if (job != null) {
				if (!job.started && overTile == job.tile) {
					StartJob();
				}
				if (job.started && overTile == job.tile && !Mathf.Approximately(job.jobProgress,0)) {
					WorkJob();
				}
			}
			if (job == null) {
				if (path.Count <= 0) {
					List<ResourceManager.Container> validEmptyInventoryContainers = FindValidContainersToEmptyInventory();
					if (inventory.CountResources() > 0 && validEmptyInventoryContainers.Count > 0) {
						EmptyInventory(validEmptyInventoryContainers);
					} else {
						Wander();
					}
				} else {
					wanderTimer = Random.Range(10f, 20f);
				}
			}
		}

		public new void Die() {
			base.Die();
			ReturnJob();
			colonistM.colonists.Remove(this);
			uiM.SetColonistElements();
			uiM.SetJobElements();
			foreach (ResourceManager.Container container in resourceM.containers) {
				container.inventory.ReleaseReservedResources(this);
			}
			if (colonistM.selectedColonist == this) {
				colonistM.SetSelectedColonist(null);
			}
			jobM.UpdateAllColonistJobCosts();
		}

		public List<ResourceManager.Container> FindValidContainersToEmptyInventory() {
			return resourceM.containers.Where(container => container.parentObject.tile.region == overTile.region && container.inventory.CountResources() < container.inventory.maxAmount).ToList();
		}

		public void EmptyInventory(List<ResourceManager.Container> validContainers) {
			if (inventory.CountResources() > 0 && validContainers.Count > 0) {
				ResourceManager.Container closestContainer = validContainers.OrderBy(container => pathM.RegionBlockDistance(container.parentObject.tile.regionBlock, overTile.regionBlock, true, true, false)).ToList()[0];
				SetJob(new JobManager.ColonistJob(this, new JobManager.Job(closestContainer.parentObject.tile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.EmptyInventory), 0), null, null, jobM, pathM));
			}
		}

		private void UpdateNeeds() {
			foreach (NeedInstance need in needs) {
				colonistM.CalculateNeedValue(need);
				bool checkNeed = false;
				if (need.colonist.job == null) {
					checkNeed = true;
				} else {
					if (colonistM.needRelatedJobs.Contains(need.colonist.job.prefab.jobType)) {
						if (colonistM.jobToNeedMap.ContainsKey(need.colonist.job.prefab.jobType)) {
							if (need.prefab.priority < colonistM.GetNeedPrefabFromEnum(colonistM.jobToNeedMap[need.colonist.job.prefab.jobType]).priority) {
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
					if (colonistM.needsValueFunctions[need.prefab.type](need)) {
						break;
					}
				}
			}

			if (needs.Find(need => need.prefab.type == NeedsEnum.Food).value >= colonistM.GetNeedPrefabFromEnum(NeedsEnum.Food).maximumValue) {
				if (happinessModifiers.Find(hm => hm.prefab.type == HappinessModifiersEnum.Starving) == null) {
					AddHappinessModifier(HappinessModifiersEnum.Starving);
				}
			} else if (needs.Find(need => need.prefab.type == NeedsEnum.Food).value >= (1 * 1440 * colonistM.GetNeedPrefabFromEnum(NeedsEnum.Food).baseIncreaseRate)) {
				if (happinessModifiers.Find(hm => hm.prefab.type == HappinessModifiersEnum.Hungry) == null) {
					AddHappinessModifier(HappinessModifiersEnum.Hungry);
				}
			} else {
				RemoveHappinessModifier(HappinessModifiersEnum.Starving);
				RemoveHappinessModifier(HappinessModifiersEnum.Hungry);
			}
			if (needs.Find(need => need.prefab.type == NeedsEnum.Rest).value >= colonistM.GetNeedPrefabFromEnum(NeedsEnum.Rest).maximumValue) {
				if (happinessModifiers.Find(hm => hm.prefab.type == HappinessModifiersEnum.Exhausted) == null) {
					AddHappinessModifier(HappinessModifiersEnum.Exhausted);
				}
			} else if (needs.Find(need => need.prefab.type == NeedsEnum.Rest).value >= (1 * 1440 * colonistM.GetNeedPrefabFromEnum(NeedsEnum.Rest).baseIncreaseRate)) {
				if (happinessModifiers.Find(hm => hm.prefab.type == HappinessModifiersEnum.Tired) == null) {
					AddHappinessModifier(HappinessModifiersEnum.Tired);
				}
			} else {
				RemoveHappinessModifier(HappinessModifiersEnum.Exhausted);
				RemoveHappinessModifier(HappinessModifiersEnum.Tired);
			}
			if (inventory.CountResources() > inventory.maxAmount) {
				if (happinessModifiers.Find(hm => hm.prefab.type == HappinessModifiersEnum.Overencumbered) == null) {
					AddHappinessModifier(HappinessModifiersEnum.Overencumbered);
				}
			} else {
				RemoveHappinessModifier(HappinessModifiersEnum.Overencumbered);
			}

			for (int i = 0; i < happinessModifiers.Count; i++) {
				HappinessModifierInstance happinessModifier = happinessModifiers[i];
				happinessModifier.Update(timeM);
				if (happinessModifier.timer <= 0) {
					RemoveHappinessModifier(happinessModifier.prefab.type);
					i -= 1;
				}
			}

			baseHappiness = Mathf.Clamp(Mathf.RoundToInt(100 - (needs.Sum(need => (need.value / (need.prefab.priority + 1))))), 0, 100);

			happinessModifiersSum = happinessModifiers.Sum(hM => hM.prefab.effectAmount);

			float targetHappiness = Mathf.Clamp(baseHappiness + happinessModifiersSum, 0, 100);
			float happinessChangeAmount = ((targetHappiness - effectiveHappiness) / (effectiveHappiness <= 0f ? 1f : effectiveHappiness));
			effectiveHappiness += happinessChangeAmount * timeM.deltaTime;
			effectiveHappiness = Mathf.Clamp(effectiveHappiness, 0, 100);
		}

		public void AddHappinessModifier(HappinessModifiersEnum happinessModifierEnum) {
			HappinessModifierInstance hmi = new HappinessModifierInstance(this, colonistM.GetHappinessModifierPrefabFromEnum(happinessModifierEnum));
			HappinessModifierInstance sameGroupHMI = happinessModifiers.Find(findHMI => hmi.prefab.group.type == findHMI.prefab.group.type);
			if (sameGroupHMI != null) {
				RemoveHappinessModifier(sameGroupHMI.prefab.type);
			}
			happinessModifiers.Add(hmi);
		}

		public void RemoveHappinessModifier(HappinessModifiersEnum happinessModifierEnum) {
			happinessModifiers.Remove(happinessModifiers.Find(findHMI => findHMI.prefab.type == happinessModifierEnum));
		}

		private float wanderTimer = Random.Range(10f,20f);
		private void Wander() {
			if (wanderTimer <= 0) {
				List<TileManager.Tile> validWanderTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable && tile.tileType.buildable && colonistM.colonists.Find(colonist => colonist.overTile == tile) == null).ToList();
				if (validWanderTiles.Count > 0) {
					MoveToTile(validWanderTiles[Random.Range(0,validWanderTiles.Count)],false);
				}
				wanderTimer = Random.Range(10f,20f);
			} else {
				wanderTimer -= 1 * timeM.deltaTime;
			}
		}

		public void SetJob(JobManager.ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			job = colonistJob.job;
			job.colonistResources = colonistJob.colonistResources;
			job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && (job.containerPickups != null && job.containerPickups.Count > 0)) {
				foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
					containerPickup.container.inventory.ReserveResources(containerPickup.resourcesToPickup,this);
				}
			}
			job.SetColonist(this,resourceM,colonistM,jobM,pathM);
			MoveToTile(job.tile,!job.tile.walkable);
			uiM.SetJobElements();
		}

		public void StartJob() {
			job.started = true;

			job.jobProgress *= (1 + (1 - GetJobSkillMultiplier(job.prefab.jobType)));

			if (job.prefab.jobType == JobManager.JobTypesEnum.Eat) {
				job.jobProgress += needs.Find(need => need.prefab.type == NeedsEnum.Food).value;
			}
			if (job.prefab.jobType == JobManager.JobTypesEnum.Sleep) {
				job.jobProgress += 20f * (needs.Find(need => need.prefab.type == NeedsEnum.Rest).value);
			}

			job.colonistBuildTime = job.jobProgress;

			uiM.SetJobElements();
		}

		public void WorkJob() {

			if (job.prefab.jobType == JobManager.JobTypesEnum.HarvestFarm && job.tile.farm == null) {
				job.jobUIElement.Remove(uiM);
				job.Remove();
				job = null;
				return;
			} else if (
				job.prefab.jobType == JobManager.JobTypesEnum.EmptyInventory ||
				job.prefab.jobType == JobManager.JobTypesEnum.CollectFood ||
				job.prefab.jobType == JobManager.JobTypesEnum.PickupResources) {

				ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == job.tile);
				if (containerOnTile == null) {
					job.jobUIElement.Remove(uiM);
					job.Remove();
					job = null;
					return;
				}
			} else if (job.prefab.jobType == JobManager.JobTypesEnum.Sleep) {
				float currentRestValue = needs.Find(need => need.prefab.type == NeedsEnum.Rest).value;
				float originalRestValue = currentRestValue / (job.jobProgress / job.colonistBuildTime);
				needs.Find(need => need.prefab.type == NeedsEnum.Rest).value = originalRestValue * ((job.jobProgress - 1f * timeM.deltaTime) / job.colonistBuildTime);
			}

			if (job.activeTileObject != null) {
				job.activeTileObject.SetActiveSprite(true,job);
			}

			job.jobProgress -= 1 * timeM.deltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress,0)) {
				job.jobProgress = 0;
				if (job.activeTileObject != null) {
					job.activeTileObject.SetActiveSprite(false,job);
				}
				FinishJob();
				return;
			}

			job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,((job.colonistBuildTime - job.jobProgress) / job.colonistBuildTime));
		}

		public void FinishJob() {
			JobManager.Job finishedJob = job;
			job = null;

			Destroy(finishedJob.jobPreview);
			if (finishedJob.prefab.addToTileWhenBuilt) {
				finishedJob.tile.SetTileObject(finishedJob.prefab,finishedJob.rotationIndex);
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).obj.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).FinishCreation();
				if (!resourceM.GetBitmaskingTileObjects().Contains(finishedJob.prefab.type) && finishedJob.prefab.bitmaskSprites.Count > 0) {
					finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).obj.GetComponent<SpriteRenderer>().sprite = finishedJob.prefab.bitmaskSprites[finishedJob.rotationIndex];
				}
			}

			SkillInstance skill = GetSkillFromJobType(finishedJob.prefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJob.prefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			jobM.finishJobFunctions[finishedJob.prefab.jobType](this,finishedJob);

			jobM.UpdateSingleColonistJobs(this);
			uiM.SetJobElements();
			uiM.UpdateSelectedColonistInformation();
			uiM.UpdateSelectedContainerInfo();

			if (storedJob != null) {
				Update();
			}
		}

		public void ReturnJob() {
			foreach (ResourceManager.Container container in resourceM.containers) {
				container.inventory.ReleaseReservedResources(this);
			}
			if (storedJob != null) {
				if (colonistM.jobToNeedMap.ContainsKey(storedJob.prefab.jobType)) {
					storedJob.Remove();
					if (storedJob.jobUIElement != null) {
						storedJob.jobUIElement.Remove(uiM);
					}
					storedJob = null;
					if (job != null) {
						if (colonistM.jobToNeedMap.ContainsKey(job.prefab.jobType)) {
							job.Remove();
							if (job.jobUIElement != null) {
								job.jobUIElement.Remove(uiM);
							}
						}
						job = null;
					}
					return;
				}
				jobM.AddExistingJob(storedJob);
				if (storedJob.jobUIElement != null) {
					storedJob.jobUIElement.Remove(uiM);
				}
				if (storedJob.jobPreview != null) {
					storedJob.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				storedJob = null;
				if (job != null) {
					if (job.jobUIElement != null) {
						job.jobUIElement.Remove(uiM);
					}
					if (job.jobPreview != null) {
						job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
					}
					job = null;
				}
			} else if (job != null) {
				if (colonistM.jobToNeedMap.ContainsKey(job.prefab.jobType)) {
					job.Remove();
					if (job.jobUIElement != null) {
						job.jobUIElement.Remove(uiM);
					}
					job = null;
					return;
				}
				jobM.AddExistingJob(job);
				if (job.jobUIElement != null) {
					job.jobUIElement.Remove(uiM);
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
					MoveToTile(walkableSurroundingTiles[Random.Range(0,walkableSurroundingTiles.Count)],false);
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
						walkableSurroundingTiles = walkableSurroundingTiles.OrderBy(tile => Vector2.Distance(tile.obj.transform.position,overTile.obj.transform.position)).ToList();
						MoveToTile(walkableSurroundingTiles[0],false);
					} else {
						List<TileManager.Tile> validTiles = tileM.map.tiles.Where(tile => tile.walkable).OrderBy(tile => Vector2.Distance(tile.obj.transform.position,overTile.obj.transform.position)).ToList();
						if (validTiles.Count > 0) {
							MoveToTile(validTiles[0],false);
						}
					}
				}
			}
		}

		public void PlayerMoveToTile(TileManager.Tile tile) {
			playerMoved = true;
			ReturnJob();
			if (job != null) {
				MoveToTile(tile,false);
			} else {
				MoveToTile(tile,false);
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

		public void LoadColonistData(
			Vector2 position,
			string name,
			Dictionary<HumanLook, int> humanLookIndices,
			float health,
			Profession profession,
			Profession oldProfession,
			ResourceManager.Inventory inventory,
			JobManager.Job job,
			JobManager.Job storedJob,
			List<SkillInstance> skills,
			List<TraitInstance> traits,
			List<NeedInstance> needs,
			float baseHappiness,
			float effectiveHappiness,
			List<HappinessModifierInstance> happinessModifiers,
			bool playerMoved,
			TileManager.Tile pathEnd) {

				obj.transform.position = position;

				this.name = name;
				SetNameCanvas(name);

				moveSprites = colonistM.humanMoveSprites[humanLookIndices[HumanLook.Skin]];
				this.humanLookIndices = humanLookIndices;

				ChangeHealthValue(-(this.health - health));

				ChangeProfession(oldProfession);
				ChangeProfession(profession);

				this.inventory = inventory;

				if (storedJob != null) {
					SetJob(new JobManager.ColonistJob(this, storedJob, storedJob.colonistResources, storedJob.containerPickups, jobM, pathM));
				} else if (job != null) {
					SetJob(new JobManager.ColonistJob(this, job, job.colonistResources, job.containerPickups, jobM, pathM));
				}

				this.skills = skills;
				this.traits = traits;
				this.needs = needs;

				this.baseHappiness = baseHappiness;
				this.effectiveHappiness = effectiveHappiness;
				this.happinessModifiers = happinessModifiers;

				if (playerMoved) {
					PlayerMoveToTile(pathEnd);
				}
		}
	}

	public List<Trader> traders = new List<Trader>();

	public class Trader : Human {
		public Trader(TileManager.Tile spawnTile, Dictionary<HumanLook, int> humanLookIndices, float startingHealth, Gender gender) : base(spawnTile, humanLookIndices, startingHealth, gender) {

		}
	}

	public List<List<Sprite>> humanMoveSprites = new List<List<Sprite>>();
	public enum HumanLook { Skin, Hair, Shirt, Pants };
	public enum Gender { Male, Female };

	public List<string> maleNames = new List<string>();
	public List<string> femaleNames = new List<string>();

	public Dictionary<HumanLook, int> GetHumanLookIndices(Gender gender) {
		Dictionary<HumanLook, int> humanLookIndices = new Dictionary<HumanLook, int>() {
			{HumanLook.Skin, Random.Range(0,3) },
			{HumanLook.Hair, Random.Range(0,0) },
			{HumanLook.Shirt, Random.Range(0,0) },
			{HumanLook.Pants, Random.Range(0,0) }
		};
		return humanLookIndices;
	}

	public Gender GetRandomGender() {
		return (Gender)Random.Range(0, System.Enum.GetNames(typeof(Gender)).Length);
	}

	public void SpawnStartColonists(int amount) {
		SpawnColonists(amount);

		Vector2 averageColonistPosition = new Vector2(0, 0);
		foreach (Colonist colonist in colonists) {
			averageColonistPosition = new Vector2(averageColonistPosition.x + colonist.obj.transform.position.x, averageColonistPosition.y + colonist.obj.transform.position.y);
		}
		averageColonistPosition /= colonists.Count;
		cameraM.SetCameraPosition(averageColonistPosition);

		// TEMPORARY COLONIST TESTING STUFF
		colonists[Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.WheatSeed), Random.Range(5, 11));
		colonists[Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Potato), Random.Range(5, 11));
		colonists[Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.CottonSeed), Random.Range(5, 11));
	}

	public void SpawnColonists(int amount) {
		if (amount > 0) {
			int mapSize = tileM.map.mapData.mapSize;
			for (int i = 0; i < amount; i++) {
				Gender gender = GetRandomGender();

				Dictionary<HumanLook, int> humanLookIndices = GetHumanLookIndices(gender);

				List<TileManager.Tile> walkableTilesByDistanceToCentre = tileM.map.tiles.Where(o => o.walkable && o.tileType.buildable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
				if (walkableTilesByDistanceToCentre.Count <= 0) {
					foreach (TileManager.Tile tile in tileM.map.tiles.Where(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
						tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Grass), true, true, true, true);
					}
					tileM.map.Bitmasking(tileM.map.tiles);
					walkableTilesByDistanceToCentre = tileM.map.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
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
				TileManager.Tile colonistSpawnTile = validSpawnTiles.Count >= amount ? validSpawnTiles[Random.Range(0, validSpawnTiles.Count)] : walkableTilesByDistanceToCentre[Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

				Colonist colonist = new Colonist(colonistSpawnTile, humanLookIndices, professions[Random.Range(0, professions.Count)], 1, gender);
				AddColonist(colonist);
			}
		}
	}

	public void AddColonist(Colonist colonist) {
		colonists.Add(colonist);
		uiM.SetColonistElements();
		tileM.map.SetTileBrightness(timeM.GetTileBrightnessTime());
	}

	public Colonist selectedColonist;
	private GameObject selectedColonistIndicator;

	private List<Colonist> deadColonists = new List<Colonist>();

	void Update() {
		SetSelectedColonistFromInput();
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

		if (!timeM.GetPaused()) {
			jobM.GiveJobsToColonists();
		}
		if (Input.GetKey(KeyCode.F) && selectedColonist != null) {
			cameraM.SetCameraPosition(selectedColonist.obj.transform.position);
			cameraM.SetCameraZoom(5);
		}
	}

	void SetSelectedColonistFromInput() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
			bool foundColonist = false;
			Colonist newSelectedColonist = colonists.Find(colonist => Vector2.Distance(colonist.obj.transform.position,mousePosition) < 0.5f);
			if (newSelectedColonist != null) {
				DeselectSelectedColonist();
				selectedColonist = newSelectedColonist;
				foundColonist = true;
				uiM.SetSelectedColonistInformation();
			}

			if (foundColonist) {
				CreateColonistIndicator();
			}

			if (!foundColonist && selectedColonist != null) {
				selectedColonist.PlayerMoveToTile(tileM.map.GetTileFromPosition(mousePosition));
			}
		}
		if (Input.GetMouseButtonDown(1) && tileM.generated) {
			DeselectSelectedColonist();
		}
	}

	public void SetSelectedColonist(Colonist colonist) {
		DeselectSelectedColonist();
		if (colonist != null) {
			selectedColonist = colonist;
			CreateColonistIndicator();
		}
		uiM.SetSelectedColonistInformation();
	}

	void CreateColonistIndicator() {
		selectedColonistIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),selectedColonist.obj.transform,false);
		selectedColonistIndicator.name = "SelectedColonistIndicator";
		SpriteRenderer sCISR = selectedColonistIndicator.GetComponent<SpriteRenderer>();
		sCISR.sprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		sCISR.sortingOrder = 20; // Selected Colonist Indicator Sprite
		sCISR.color = new Color(1f,1f,1f,0.75f);
		selectedColonistIndicator.transform.localScale = new Vector2(1f,1f) * 1.2f;
	}

	void DeselectSelectedColonist() {
		selectedColonist = null;
		Destroy(selectedColonistIndicator);
		uiM.SetSelectedColonistInformation();
	}

	public void GenerateName() {

	}

	private float traderTimer = 0; // The time since the last trader visited
	private int traderTimeMin = 1440; // Traders will only come once every traderTimeMin in-game minutes
	private int traderTimeMax = 10080; // Traders will definitely come if the traderTimer is greater than traderTimeMax
	public void UpdateTraders() {
		traderTimer += timeM.deltaTime;
		if (traderTimer > traderTimeMin && timeM.minuteChanged) {
			if (Random.Range(0f, 1f) < ((traderTimer - traderTimeMin) / (traderTimeMax - traderTimeMin))) {
				SpawnTrader();
			}
		}
	}

	public void SpawnTrader() {
		traderTimer = 0;
		Gender gender = GetRandomGender();
		Dictionary<HumanLook, int> humanLookIndices = GetHumanLookIndices(gender);
		List<TileManager.Tile> validSpawnTiles = tileM.map.edgeTiles.Where(tile => tile.walkable && !tileM.GetLiquidWaterEquivalentTileTypes().Contains(tile.tileType.type)).ToList();
		if (validSpawnTiles.Count > 0) {
			traders.Add(new Trader(validSpawnTiles[Random.Range(0, validSpawnTiles.Count)], humanLookIndices, 1f, gender));
		}
	}
}