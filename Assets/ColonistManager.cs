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

		foreach (string name in Resources.Load<TextAsset>(@"Data/names").text.Split(' ').ToList()) {
			humanNames.Add(name);
		}

		CreateColonistSkills();
		CreateColonistProfessions();
		CreateColonistNeeds();

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

		public Life(TileManager.Tile spawnTile, float startingHealth) {

			GetScriptReferences();

			overTile = spawnTile;
			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),overTile.obj.transform.position,Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 10; // Life Sprite

			health = startingHealth;
		}

		private Dictionary<int,int> moveSpritesMap = new Dictionary<int,int>() {
			{16,1 },{32,0 },{64,0 },{128,1}
		};
		private float moveTimer;
		public List<TileManager.Tile> path = new List<TileManager.Tile>();


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
					moveTimer += 2 * timeM.deltaTime * overTile.walkSpeed;
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



		public void ChangeHealthValue(float amount) {
			health += amount;
			health = Mathf.Clamp(health, 0f, 1f);
		}
	}

	public class Human : Life {

		public string name;
		public GameObject nameCanvas;

		public int skinIndex;
		public int hairIndex;
		public int shirtIndex;
		public int pantsIndex;

		// Carrying Item

		// Inventory
		public ResourceManager.Inventory inventory;

		public Human(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes, float startingHealth) : base(spawnTile,startingHealth) {
			moveSprites = colonistM.humanMoveSprites[colonistLookIndexes[ColonistLook.Skin]];

			inventory = new ResourceManager.Inventory(this,null,50);

			List<string> validNames = colonistM.humanNames.Where(name => colonistM.colonists.Find(colonistWithName => colonistWithName.name == name) == null).ToList();
			if (validNames.Count > 0) {
				name = validNames[Random.Range(0,validNames.Count)];
			} else {
				name = colonistM.humanNames[Random.Range(0,colonistM.humanNames.Count)];
			}
			string tempName = name.Take(1).ToList()[0].ToString().ToUpper();
			foreach (char character in name.Skip(1)) {
				tempName += character;
			}
			name = tempName;

			obj.name = "Human: " + name;

			nameCanvas = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/Human-Canvas"),obj.transform,false);
			nameCanvas.transform.Find("NameBackground-Image/Name-Text").GetComponent<Text>().text = name;
			Canvas.ForceUpdateCanvases(); // The charInfo.advance is not calculated until the text is rendered
			int textWidthPixels = 0;
			foreach (char character in name) {
				CharacterInfo charInfo;
				Resources.Load<Font>(@"UI/Fonts/Quicksand/Quicksand-Bold").GetCharacterInfo(character,out charInfo,48,FontStyle.Normal);
				textWidthPixels += charInfo.advance;
			}
			nameCanvas.transform.Find("NameBackground-Image").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidthPixels/2.8f,20);
		}

		public new void Update() {
			base.Update();
			nameCanvas.transform.Find("NameBackground-Image").localScale = Vector2.one * Mathf.Clamp(cameraM.cameraComponent.orthographicSize,2,10) * 0.0025f;
			nameCanvas.transform.Find("NameBackground-Image").GetComponent<Image>().color = Color.Lerp(uiM.GetColour(UIManager.Colours.LightRed), uiM.GetColour(UIManager.Colours.LightGreen), health);
		}
	}

	public enum SkillTypeEnum { Building, Mining, Farming, Forestry };

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

		public SkillInstance(Colonist colonist, SkillPrefab prefab, int startingLevel) {
			this.colonist = colonist;
			this.prefab = prefab;

			level = startingLevel;

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

	public enum ProfessionTypeEnum { Nothing, Builder, Miner, Farmer, Forester };

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

	public class TraitInstance {

		public Colonist colonist;
		public TraitPrefab prefab;

		public TraitInstance(Colonist colonist,TraitPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}
	}

	public enum NeedsEnum { Food, Clothing, Shelter, Rest, Temperature, Safety, Social, Esteem, Relaxation };

	public Dictionary<NeedsEnum,System.Action<NeedInstance>> needsValueFunctions = new Dictionary<NeedsEnum,System.Action<NeedInstance>>();

	private Dictionary<NeedsEnum,JobManager.JobTypesEnum> needToJobMap = new Dictionary<NeedsEnum,JobManager.JobTypesEnum>() {
		{NeedsEnum.Food,JobManager.JobTypesEnum.Eat },
		{ NeedsEnum.Rest,JobManager.JobTypesEnum.Sleep}
	};

	public Dictionary<NeedsEnum,System.Func<NeedInstance,float>> needsValueSpecialIncreases = new Dictionary<NeedsEnum,System.Func<NeedInstance,float>>();

	public void InitializeNeedsValueSpecialIncreases() {
		needsValueSpecialIncreases.Add(NeedsEnum.Rest,delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			if (!timeM.isDay) {
				totalSpecialIncrease += 0.05f;
			}
			return totalSpecialIncrease;
		});
	}

	public void CalculateNeedValue(NeedInstance need) {
		if (needToJobMap.ContainsKey(need.prefab.type) && need.colonist.job != null && need.colonist.job.prefab.jobType == needToJobMap[need.prefab.type]) {
			return;
		}
		float needIncreaseAmount = need.prefab.baseIncreaseRate;
		foreach (TraitInstance trait in need.colonist.traits) {
			if (need.prefab.traitsAffectingThisNeed.ContainsKey(trait.prefab.type)) {
				needIncreaseAmount *= need.prefab.traitsAffectingThisNeed[trait.prefab.type];
			}
		}
		need.value += (needIncreaseAmount + (needsValueSpecialIncreases.ContainsKey(need.prefab.type) ? needsValueSpecialIncreases[need.prefab.type](need) : 0)) * timeM.deltaTime * (debugM.debugMode ? 25f : 1f);
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
			print("Take from other colonists.");
		}
		if (resourcesPerInventory.Count > 0) {
			return resourcesPerInventory[0].Key;
		} else {
			return new KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>>(null,null);
		}
	}

	void GetFood(NeedInstance need, bool takeFromOtherColonists,bool eatAnything) {
		if (need.colonist.inventory.resources.Find(ra => ra.resource.resourceGroup.type == ResourceManager.ResourceGroupsEnum.Foods) == null) {
			KeyValuePair<ResourceManager.Inventory,List<ResourceManager.ResourceAmount>> closestFood = FindClosestFood(need.colonist,need.value,takeFromOtherColonists,eatAnything);
			
			List<ResourceManager.ResourceAmount> resourcesToReserve = closestFood.Value;
			if (closestFood.Key != null) {
				if (closestFood.Key.container != null) {
					ResourceManager.Container container = closestFood.Key.container;
					container.inventory.ReserveResources(resourcesToReserve, need.colonist);
					JobManager.Job job = new JobManager.Job(container.parentObject.tile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.CollectFood), 0);
					need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, job, null, null, jobM, pathM));
					return;
				} else if (closestFood.Key.human != null) {
					Human human = closestFood.Key.human;
					print("Take food from other human.");
				}
			}
		} else {
			need.colonist.SetJob(new JobManager.ColonistJob(need.colonist,new JobManager.Job(need.colonist.overTile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Eat),0),null,null,jobM,pathM));
			return;
		}
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

	public void InitializeNeedsValueFunctions() {
		needsValueFunctions.Add(NeedsEnum.Food,delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.CollectFood || need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Eat)) {
				if (need.prefab.criticalValueAction && need.value >= need.prefab.criticalValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * Time.deltaTime);
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,true,true) > 0) {
						if (need.colonist.job != null) {
							need.colonist.ReturnJob();
						}
						GetFood(need,true,true);
					}
				}
				if (need.prefab.maximumValueAction && need.value >= need.prefab.maximumValue) {
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,false,true) > 0) {
						if (need.colonist.job != null) {
							need.colonist.ReturnJob();
						}
						GetFood(need,true,false);
					}
				}
				if (need.prefab.minimumValueAction && need.value >= need.prefab.minimumValue) {
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,false,false) > 0) {
						if (timeM.minuteChanged && Random.Range(0f,1f) < ((need.value - need.prefab.minimumValue) / (need.prefab.maximumValue - need.prefab.minimumValue))) {
							GetFood(need,false,false);
						}
					}
				}
			}
		});
		needsValueFunctions.Add(NeedsEnum.Rest,delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Sleep)) {
				if (need.prefab.criticalValueAction && need.value >= need.prefab.criticalValue) {

				}
				if (need.prefab.maximumValueAction && need.value >= need.prefab.maximumValue) {

				}
				if (need.prefab.minimumValueAction && need.value >= need.prefab.minimumValue) {

				}
			}
		});
		needsValueFunctions.Add(NeedsEnum.Clothing,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
		});
		needsValueFunctions.Add(NeedsEnum.Shelter,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
		});
		needsValueFunctions.Add(NeedsEnum.Temperature,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
		});
		needsValueFunctions.Add(NeedsEnum.Safety,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
		});
		needsValueFunctions.Add(NeedsEnum.Social,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
		});
		needsValueFunctions.Add(NeedsEnum.Esteem,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
		});
		needsValueFunctions.Add(NeedsEnum.Relaxation,delegate (NeedInstance need) {
			need.value = 0; // Value set to 0 while need not being used
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
	7	0/			CV			Percentage over MaxT until they begin dying from the need not being fulfilled
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

		public NeedPrefab(List<string> data) {

			type = (NeedsEnum)System.Enum.Parse(typeof(NeedsEnum),data[0]);
			name = type.ToString();

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
			needPrefabs.Add(new NeedPrefab(stringNeedData));
		}
		foreach (NeedPrefab needPrefab in needPrefabs) {
			needPrefab.name = uiM.SplitByCapitals(needPrefab.name);
		}
	}

	public enum HappinessModifiersEnum { WitnessDeath, };

	public List<HappinessModifierPrefab> happinessModifierPrefabs = new List<HappinessModifierPrefab>();

	public class HappinessModifierPrefab {

		public HappinessModifiersEnum type;
		public string name;

		public NeedPrefab needPrefab;

		public int effectAmount;

		public int effectLengthSeconds;

		public HappinessModifierPrefab() {

		}
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
		public int baseHappiness;
		public int happinessModifiersSum;
		public int effectiveHappiness;
		public List<HappinessModifierInstance> happinessModifiers = new List<HappinessModifierInstance>();

		public Colonist(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes, Profession profession, float startingHealth) : base(spawnTile,colonistLookIndexes,startingHealth) {
			obj.transform.SetParent(GameObject.Find("ColonistParent").transform,false);

			this.profession = profession;
			profession.colonistsInProfessionAtStart.Add(this);
			profession.colonistsInProfession.Add(this);

			foreach (SkillPrefab skillPrefab in colonistM.skillPrefabs) {
				skills.Add(new SkillInstance(this,skillPrefab,Random.Range((profession.primarySkill != null && profession.primarySkill.type == skillPrefab.type ? Mathf.RoundToInt(profession.skillRandomMaxValues[skillPrefab]/2f) : 0),profession.skillRandomMaxValues[skillPrefab])));
			}

			foreach (NeedPrefab needPrefab in colonistM.needPrefabs) {
				needs.Add(new NeedInstance(this,needPrefab));
			}
			needs = needs.OrderBy(need => need.prefab.priority).ToList();

			oldProfession = colonistM.professions.Find(findProfession => findProfession.type == ProfessionTypeEnum.Nothing);
		}

		public new void Update() {
			base.Update();
			UpdateNeeds();
			if (overTileChanged) {
				jobM.UpdateColonistJobCosts(this);
			}
			if (job == null) {
				if (path.Count <= 0) {
					List<ResourceManager.Container> validContainers = resourceM.containers.Where(container => container.parentObject.tile.region == overTile.region && container.inventory.CountResources() < container.inventory.maxAmount).ToList();
					if (inventory.CountResources() > 0 && resourceM.containers.Count > 0 && validContainers.Count > 0) {
						List<ResourceManager.Container> closestContainers = validContainers.OrderBy(container => pathM.RegionBlockDistance(container.parentObject.tile.regionBlock,overTile.regionBlock,true,true,false)).ToList();
						if (closestContainers.Count > 0) {
							ResourceManager.Container closestContainer = closestContainers[0];
							SetJob(new JobManager.ColonistJob(this,new JobManager.Job(closestContainer.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.EmptyInventory),0),null,null,jobM,pathM));
						}
					} else {
						Wander();
					}
				} else {
					wanderTimer = Random.Range(10f,20f);
				}
			} else {
				if (!job.started && overTile == job.tile) {
					StartJob();
				}
				if (job.started && overTile == job.tile && !Mathf.Approximately(job.jobProgress,0)) {
					WorkJob();
				}
			}
		}

		private void UpdateNeeds() {
			foreach (NeedInstance need in needs) {
				colonistM.CalculateNeedValue(need);
				colonistM.needsValueFunctions[need.prefab.type](need);
			}
			for (int i = 0; i < happinessModifiers.Count; i++) {
				HappinessModifierInstance happinessModifier = happinessModifiers[i];
				happinessModifier.Update(timeM);
				if (happinessModifier.timer > 0) {
					happinessModifiers.Remove(happinessModifier);
					i -= 1;
				}
			}
			baseHappiness = Mathf.Clamp(Mathf.RoundToInt(needs.Sum(need => need.value * need.prefab.priority)),0,100);
			happinessModifiersSum = happinessModifiers.Sum(hM => hM.prefab.effectAmount);
			effectiveHappiness = baseHappiness + happinessModifiersSum;
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

		public void SetJob(JobManager.ColonistJob colonistJob) {
			job = colonistJob.job;
			job.colonistResources = colonistJob.colonistResources;
			if (colonistJob.containerPickups != null) {
				print("CONTAINER PICKUPS: " + colonistJob.containerPickups.Count);
			}
			job.containerPickups = colonistJob.containerPickups;
			if (job.containerPickups != null && job.containerPickups.Count > 0) {
				foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
					containerPickup.container.inventory.ReserveResources(containerPickup.resourcesToPickup,this);
				}
			}
			job.SetColonist(this,resourceM,colonistM,jobM,pathM);
			MoveToTile(job.tile,!job.tile.walkable);
		}

		public void StartJob() {
			job.started = true;

			job.jobProgress *= (1 + (1 - GetJobSkillMultiplier(job.prefab.jobType)));

			if (job.prefab.jobType == JobManager.JobTypesEnum.Eat) {
				job.jobProgress += 10 * (needs.Find(need => need.prefab.type == NeedsEnum.Food).value / 100f);
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
			}

			job.jobProgress -= 1 * timeM.deltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress,0)) {
				job.jobProgress = 0;
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

			if (finishedJob.prefab.tileObjectPrefabSubGroup.tileObjectPrefabGroup.type != ResourceManager.TileObjectPrefabGroupsEnum.None) {
				GetSkillFromJobType(finishedJob.prefab.jobType).AddExperience(finishedJob.prefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			jobM.finishJobFunctions[finishedJob.prefab.jobType](this,finishedJob);

			jobM.UpdateSingleColonistJobs(this);
			uiM.SetJobElements();
			uiM.UpdateSelectedColonistInformation();
			uiM.UpdateSelectedContainerInfo();
		}

		public void ReturnJob() {
			jobM.AddExistingJob(job);
			job.jobUIElement.Remove(uiM);
			job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,0.25f);
			job = null;
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
			if (job != null) {
				jobM.AddExistingJob(job);
				job = null;
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
				profession = newProfession;
				if (newProfession != null) {
					profession.colonistsInProfession.Add(this);
				}
			}
		}
	}

	public List<Trader> traders = new List<Trader>();

	public class Trader : Human {
		public Trader(TileManager.Tile spawnTile, Dictionary<ColonistLook, int> colonistLookIndexes, float startingHealth) : base(spawnTile, colonistLookIndexes, startingHealth) {

		}
	}

	public List<List<Sprite>> humanMoveSprites = new List<List<Sprite>>();
	public enum ColonistLook { Skin, Hair, Shirt, Pants };

	public List<string> humanNames = new List<string>();

	public void SpawnColonists(int amount) {
		for (int i = 0; i < 3; i++) {
			List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
			humanMoveSprites.Add(innerHumanMoveSprites);
		}

		int mapSize = tileM.map.mapData.mapSize;
		for (int i = 0;i < amount;i++) {

			Dictionary<ColonistLook,int> colonistLookIndexes = new Dictionary<ColonistLook,int>() {
				{ColonistLook.Skin, Random.Range(0,3) },{ColonistLook.Hair, Random.Range(0,0) },
				{ColonistLook.Shirt, Random.Range(0,0) },{ColonistLook.Pants, Random.Range(0,0) }
			};

			List<TileManager.Tile> walkableTilesByDistanceToCentre = tileM.map.tiles.Where(o => o.walkable && o.tileType.buildable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
			if (walkableTilesByDistanceToCentre.Count <= 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles.Where(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f)) <= 4f)) {
					tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Grass),true,true,true,true);
				}
				tileM.map.Bitmasking(tileM.map.tiles);
				walkableTilesByDistanceToCentre = tileM.map.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
			}
			TileManager.Tile colonistSpawnTile = walkableTilesByDistanceToCentre[Random.Range(0,(walkableTilesByDistanceToCentre.Count > 30 ? 30 : walkableTilesByDistanceToCentre.Count))];

			Colonist colonist = new Colonist(colonistSpawnTile,colonistLookIndexes,professions[Random.Range(0,professions.Count)],1);
			colonists.Add(colonist);
		}

		colonists[Random.Range(0,colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.WheatSeeds),Random.Range(5,11));
		colonists[Random.Range(0,colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.PotatoSeeds),Random.Range(5,11));

		uiM.SetColonistElements();
	}

	public Colonist selectedColonist;
	private GameObject selectedColonistIndicator;

	void Update() {
		SetSelectedColonistFromInput();
		foreach (Colonist colonist in colonists) {
			colonist.Update();
		}
		if (timeM.timeModifier > 0) {
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
		if (Input.GetMouseButtonDown(1)) {
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
		selectedColonistIndicator.name = "Selected Colonist Indicator";
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
}