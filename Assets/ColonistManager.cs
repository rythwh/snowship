using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class ColonistManager : MonoBehaviour {

	private TileManager tileM;
	private CameraManager cameraM;
	private UIManager uiM;
	private ResourceManager resourceM;
	private PathManager pathM;
	private JobManager jobM;
	private TimeManager timeM;

	private void GetScriptReferences() {
		tileM = GetComponent<TileManager>();
		cameraM = GetComponent<CameraManager>();
		uiM = GetComponent<UIManager>();
		resourceM = GetComponent<ResourceManager>();
		pathM = GetComponent<PathManager>();
		jobM = GetComponent<JobManager>();
		timeM = GetComponent<TimeManager>();
	}

	public List<string> maleNames = new List<string>();
	public List<string> femaleNames = new List<string>();

	void Awake() {
		GetScriptReferences();

		List<string> maleNamesRaw = Resources.Load<TextAsset>(@"Data/namesMale").text.Split(' ').ToList();
		foreach (string name in maleNamesRaw) {
			maleNames.Add(name.ToLower());
		}
		List<string> femaleNamesRaw = Resources.Load<TextAsset>(@"Data/namesFemale").text.Split(' ').ToList();
		foreach (string name in femaleNamesRaw) {
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

		public enum Gender { Male, Female };

		public bool dead = false;

		public Life(TileManager.Tile spawnTile, float startingHealth) {
			GetScriptReferences();

			gender = GetRandomGender();

			overTile = spawnTile;
			obj = Instantiate(spawnTile.map.resourceM.tilePrefab, overTile.obj.transform.position, Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 10; // Life Sprite

			health = startingHealth;
		}

		public static Gender GetRandomGender() {
			return (Gender)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Gender)).Length);
		}

		private static readonly Dictionary<int,int> moveSpritesMap = new Dictionary<int,int>() {
			{16,1 },{32,0 },{64,0 },{128,1}
		};
		private float moveTimer;
		public int startPathLength = 0;
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
				startPathLength = 0;
				path = pathM.FindPathToTile(overTile,tile,allowEndTileNonWalkable);
				startPathLength = path.Count;
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

	public string GetName(Life.Gender gender) {
		string chosenName = "NAME";
		List<string> names = (gender == Life.Gender.Male ? maleNames : femaleNames);
		List<string> validNames = names.Where(name => colonists.Find(colonistWithName => colonistWithName.name.ToLower() == name.ToLower()) == null).ToList();
		if (validNames.Count > 0) {
			chosenName = validNames[UnityEngine.Random.Range(0, validNames.Count)];
		} else {
			chosenName = names[UnityEngine.Random.Range(0, names.Count)];
		}
		string tempName = chosenName.Take(1).ToList()[0].ToString().ToUpper();
		foreach (char character in chosenName.Skip(1)) {
			tempName += character;
		}
		chosenName = tempName;
		return chosenName;
	}

	public List<List<Sprite>> humanMoveSprites = new List<List<Sprite>>();

	public class Human : Life {

		public string name;
		public GameObject nameCanvas;

		public Dictionary<Appearance, int> appearanceIndices;

		// Carrying Item

		// Inventory
		public ResourceManager.Inventory inventory;

		public enum Appearance { Skin, Hair, Hat, Top, Bottoms, Scarf, Gloves, Shoes };

		public Human(TileManager.Tile spawnTile, float startingHealth) : base(spawnTile, startingHealth) {
			appearanceIndices = GetHumanLookIndices(gender);

			moveSprites = colonistM.humanMoveSprites[appearanceIndices[Appearance.Skin]];

			inventory = new ResourceManager.Inventory(this,null,50);

			name = colonistM.GetName(gender);

			obj.name = "Human: " + name;

			nameCanvas = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/Human-Canvas"),obj.transform,false);
			SetNameCanvas(name);
		}

		public static Dictionary<Appearance, int> GetHumanLookIndices(Gender gender) {
			Dictionary<Appearance, int> appearanceIndices = new Dictionary<Appearance, int>() {
			{ Appearance.Skin, UnityEngine.Random.Range(0,3) },
			{ Appearance.Hair, UnityEngine.Random.Range(0,0) },
			{ Appearance.Hat, UnityEngine.Random.Range(0,0) },
			{ Appearance.Top, UnityEngine.Random.Range(0,0) },
			{ Appearance.Bottoms, UnityEngine.Random.Range(0,0) },
			{ Appearance.Scarf, UnityEngine.Random.Range(0,0) },
			{ Appearance.Gloves, UnityEngine.Random.Range(0,0) },
			{ Appearance.Shoes, UnityEngine.Random.Range(0,0) }
		};
			return appearanceIndices;
		}

		public void SetNameCanvas(string name) {
			Font nameCanvasFont = Resources.Load<Font>(@"UI/Fonts/Quicksand/Quicksand-Bold");
			nameCanvas.transform.Find("NameBackground-Image/Name-Text").GetComponent<Text>().text = name;
			Canvas.ForceUpdateCanvases(); // The charInfo.advance is not calculated until the text is rendered
			int textWidthPixels = 0;
			foreach (char character in name) {
				CharacterInfo charInfo;
				nameCanvasFont.GetCharacterInfo(character, out charInfo, 48, FontStyle.Normal);
				textWidthPixels += charInfo.advance;
			}
			nameCanvas.transform.Find("NameBackground-Image").GetComponent<RectTransform>().sizeDelta = new Vector2(textWidthPixels / 2.8f, 20);
		}

		public new void Update() {
			base.Update();
			nameCanvas.transform.Find("NameBackground-Image").localScale = Vector2.one * Mathf.Clamp(cameraM.cameraComponent.orthographicSize,2,10) * 0.0025f;
			nameCanvas.transform.Find("NameBackground-Image/HealthIndicator-Image").GetComponent<Image>().color = Color.Lerp(UIManager.GetColour(UIManager.Colours.LightRed), UIManager.GetColour(UIManager.Colours.LightGreen), health);
		}

		public void SetNameColour(Color nameColour) {
			nameCanvas.transform.Find("NameBackground-Image/NameColour-Image").GetComponent<Image>().color = nameColour;
		}
	}

	private void SetSelectedHumanFromClick() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(0) && !uiM.IsPointerOverUI()) {
			List<Human> humans = new List<Human>();
			humans.AddRange(colonists.ToArray());
			humans.AddRange(traders.ToArray());
			Human newSelectedHuman = humans.Find(human => Vector2.Distance(human.obj.transform.position, mousePosition) < 0.5f);
			if (newSelectedHuman != null) {
				SetSelectedHuman(newSelectedHuman);
			} else if (selectedHuman != null && selectedHuman is Colonist) {
				((Colonist)selectedHuman).PlayerMoveToTile(tileM.map.GetTileFromPosition(mousePosition));
			}
		}
		if (Input.GetMouseButtonDown(1)) {
			SetSelectedHuman(null);
		}
	}

	public void SetSelectedHuman(Human human) {
		selectedHuman = human;

		SetSelectedHumanIndicator();

		uiM.SetSelectedColonistInformation();
		uiM.SetSelectedTraderMenu();
		uiM.SetRightListPanelSize();
	}

	void SetSelectedHumanIndicator() {
		if (selectedHumanIndicator != null) {
			Destroy(selectedHumanIndicator);
		}
		if (selectedHuman != null) {
			selectedHumanIndicator = Instantiate(resourceM.tilePrefab, selectedHuman.obj.transform, false);
			selectedHumanIndicator.name = "SelectedHumanIndicator";
			selectedHumanIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;

			SpriteRenderer sCISR = selectedHumanIndicator.GetComponent<SpriteRenderer>();
			sCISR.sprite = resourceM.selectionCornersSprite;
			sCISR.sortingOrder = 20; // Selected Colonist Indicator Sprite
			sCISR.color = new Color(1f, 1f, 1f, 0.75f);
		}
	}

	public enum SkillTypeEnum { Building, Mining, Digging, Farming, Forestry, Crafting };

	public class SkillPrefab {

		public SkillTypeEnum type;
		public string name;

		public Dictionary<JobManager.JobTypesEnum,float> affectedJobTypes = new Dictionary<JobManager.JobTypesEnum,float>();

		public SkillPrefab(List<string> data) {
			type = (SkillTypeEnum)Enum.Parse(typeof(SkillTypeEnum),data[0]);
			name = type.ToString();

			foreach (string affectedJobTypeString in data[1].Split(';')) {
				List<string> affectedJobTypeData = affectedJobTypeString.Split(',').ToList();
				affectedJobTypes.Add((JobManager.JobTypesEnum)Enum.Parse(typeof(JobManager.JobTypesEnum),affectedJobTypeData[0]),float.Parse(affectedJobTypeData[1]));
			}
		}
	}

	public SkillPrefab GetSkillPrefabFromString(string skillTypeString) {
		return skillPrefabs.Find(skillPrefab => skillPrefab.type == (SkillTypeEnum)Enum.Parse(typeof(SkillTypeEnum),skillTypeString));
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

			currentExperience = UnityEngine.Random.Range(0,100);
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
			colonist.jobM.UpdateColonistJobCosts(colonist);
			if (colonist.colonistM.selectedHuman == colonist) {
				colonist.uiM.RemakeSelectedColonistSkills();
			}
		}

		public float CalculateTotalSkillLevel() {
			return level + (currentExperience / nextLevelExperience);
		}
	}

	public List<SkillPrefab> skillPrefabs = new List<SkillPrefab>();

	void CreateColonistSkills() {
		List<string> stringSkills = Resources.Load<TextAsset>(@"Data/colonistSkills").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
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

		public Dictionary<SkillPrefab,int> skillRandomMaxValues = new Dictionary<SkillPrefab,int>();

		public List<Colonist> colonistsInProfessionAtStart = new List<Colonist>();
		public List<Colonist> colonistsInProfession = new List<Colonist>();

		public Profession(List<string> data, ColonistManager colonistM) {
			type = (ProfessionTypeEnum)Enum.Parse(typeof(ProfessionTypeEnum),data[0]);
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

	void CreateColonistProfessions() {
		List<string> stringProfessions = Resources.Load<TextAsset>(@"Data/colonistProfessions").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringProfession in stringProfessions) {
			List<string> stringProfessionData = stringProfession.Split('/').ToList();
			professions.Add(new Profession(stringProfessionData,this));
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

		public TraitInstance(Colonist colonist,TraitPrefab prefab) {
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

	public Dictionary<NeedsEnum,Func<NeedInstance,float>> needsValueSpecialIncreases = new Dictionary<NeedsEnum,Func<NeedInstance,float>>();

	public void InitializeNeedsValueSpecialIncreases() {
		needsValueSpecialIncreases.Add(NeedsEnum.Rest, delegate (NeedInstance need) {
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
		need.ChangeValue((needIncreaseAmount + (needsValueSpecialIncreases.ContainsKey(need.prefab.type) ? needsValueSpecialIncreases[need.prefab.type](need) : 0)) * timeM.deltaTime);
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
			KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> closestFood = FindClosestFood(need.colonist, need.GetValue(), takeFromOtherColonists, eatAnything);

			List<ResourceManager.ResourceAmount> resourcesToReserve = closestFood.Value;
			if (closestFood.Key != null) {
				if (closestFood.Key.container != null) {
					ResourceManager.Container container = closestFood.Key.container;
					container.inventory.ReserveResources(resourcesToReserve, need.colonist);
					JobManager.Job job = new JobManager.Job(container.parentObject.tile, resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.CollectFood), 0);
					need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, job, null, null, jobM, pathM));
					return true;
				} else if (closestFood.Key.human != null) {
					// TODO
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
					total += resource.GetWorldTotalAmount();
				}
			}
			return total;
		} else {
			int total = 0;

			int amountOnThisColonist = 0;
			foreach (ResourceManager.ResourceAmount resourceAmount in colonist.inventory.resources.Where(ra => ra.resource.resourceGroup.type == resourceGroup)) {
				amountOnThisColonist += resourceAmount.amount;
			}
			total += amountOnThisColonist;

			int amountUnreservedInContainers = 0;
			foreach (ResourceManager.Resource resource in resourceM.resources.Where(r => r.resourceGroup.type == resourceGroup)) {
				amountUnreservedInContainers += resource.GetUnreservedContainerTotalAmount();
			}
			total += amountUnreservedInContainers;

			if (includeOtherColonists) {
				int amountOnOtherColonists = 0;
				foreach (Colonist otherColonist in colonists) {
					if (colonist != otherColonist) {
						int amountOnOtherColonist = 0;
						foreach (ResourceManager.ResourceAmount resourceAmount in otherColonist.inventory.resources.Where(ra => ra.resource.resourceGroup.type == resourceGroup)) {
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

	public Dictionary<NeedsEnum, Func<NeedInstance, bool>> needsValueFunctions = new Dictionary<NeedsEnum, Func<NeedInstance, bool>>();

	public void InitializeNeedsValueFunctions() {
		needsValueFunctions.Add(NeedsEnum.Food,delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.CollectFood || need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Eat)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * timeM.deltaTime);
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,false,false) > 0) { // true, true
						if (timeM.minuteChanged) {
							need.colonist.ReturnJob();
							return GetFood(need, false, false); // true, true - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods,need.colonist,false,false) > 0) { // false, true
						if (timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue))) {
							need.colonist.ReturnJob();
							return GetFood(need, false, false); // true, false - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (FindAvailableResourceAmount(ResourceManager.ResourceGroupsEnum.Foods, need.colonist, false, false) > 0) {
							if (timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue))) {
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
		needsValueFunctions.Add(NeedsEnum.Water, delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.CollectWater || need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Drink)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * timeM.deltaTime);
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
		});
		needsValueFunctions.Add(NeedsEnum.Rest,delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobTypesEnum.Sleep)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * timeM.deltaTime);
					if (timeM.minuteChanged) {
						need.colonist.ReturnJob();
						GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue))) {
						need.colonist.ReturnJob();
						GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue))) {
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
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Shelter,delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Temperature,delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Safety,delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Social,delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Esteem,delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		});
		needsValueFunctions.Add(NeedsEnum.Relaxation,delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
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

		public Dictionary<TraitsEnum,float> traitsAffectingThisNeed = new Dictionary<TraitsEnum,float>();

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

		public NeedInstance(Colonist colonist,NeedPrefab prefab) {
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
			if (colonist.colonistM.selectedHuman == colonist && Mathf.RoundToInt((oldValue / prefab.clampValue) * 100) != roundedValue) {
				colonist.uiM.RemakeSelectedColonistNeeds();
			}
		}

		public void ChangeValue(float amount) {
			SetValue(value + amount);
		}

		public int GetRoundedValue() {
			return roundedValue;
		}
	}

	void CreateColonistNeeds() {
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
							print("Unknown need label: \"" + singleNeedDataLineString + "\"");
							break;
					}
				}
			}

			needPrefabs.Add(new NeedPrefab(type, baseIncreaseRate, minValueAction, minValue, maxValueAction, maxValue, critValueAction, critValue, canDie, healthDecreaseRate, clampValue, priority, traitsAffectingThisNeed, relatedJobs));
		}
	}

	public Dictionary<HappinessModifierGroupsEnum, Action<Colonist>> happinessModifierFunctions = new Dictionary<HappinessModifierGroupsEnum, Action<Colonist>>();

	public void InitializeHappinessModifierFunctions() {
		happinessModifierFunctions.Add(HappinessModifierGroupsEnum.Death, delegate (Colonist colonist) {
			// TODO Implement colonists viewing deaths and being sad
		});
		happinessModifierFunctions.Add(HappinessModifierGroupsEnum.Rest, delegate (Colonist colonist) {
			NeedInstance restNeed = colonist.needs.Find(ni => ni.prefab.type == NeedsEnum.Rest);
			if (restNeed.GetValue() >= restNeed.prefab.maxValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Exhausted);
			} else if (restNeed.GetValue() >= restNeed.prefab.minValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Tired);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Exhausted);
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Tired);
			}
		});
		happinessModifierFunctions.Add(HappinessModifierGroupsEnum.Water, delegate (Colonist colonist) {
			NeedInstance waterNeed = colonist.needs.Find(ni => ni.prefab.type == NeedsEnum.Water);
			if (waterNeed.GetValue() >= waterNeed.prefab.maxValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Dehydrated);
			} else if (waterNeed.GetValue() >= waterNeed.prefab.minValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Thirsty);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Dehydrated);
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Thirsty);
			}
		});
		happinessModifierFunctions.Add(HappinessModifierGroupsEnum.Food, delegate (Colonist colonist) {
			NeedInstance foodNeed = colonist.needs.Find(ni => ni.prefab.type == NeedsEnum.Food);
			if (foodNeed.GetValue() >= foodNeed.prefab.maxValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Starving);
			} else if (foodNeed.GetValue() >= foodNeed.prefab.minValue) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Hungry);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Starving);
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Hungry);
			}
		});
		happinessModifierFunctions.Add(HappinessModifierGroupsEnum.Inventory, delegate (Colonist colonist) {
			if (colonist.inventory.CountResources() > colonist.inventory.maxAmount) {
				colonist.AddHappinessModifier(HappinessModifiersEnum.Overencumbered);
			} else {
				colonist.RemoveHappinessModifier(HappinessModifiersEnum.Overencumbered);
			}
		});
	}

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
							print("Unknown happiness modifier label: \"" + stringHappinessModifierSingle + "\"");
							break;
					}
				}
			}

			if (string.IsNullOrEmpty(name) || effectAmount == 0 || effectLengthSeconds == 0) {
				print("Potential issue parsing happiness modifier: " + stringHappinessModifier);
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

		public void Update(TimeManager timeM) {
			timer -= 1 * timeM.deltaTime;
		}
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

		public Colonist(TileManager.Tile spawnTile, Profession profession, float startingHealth) : base(spawnTile, startingHealth) {
			SetNameColour(UIManager.GetColour(UIManager.Colours.LightGreen));

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
			UpdateHappinessModifiers();
			UpdateHappiness();

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
					if ((inventory.CountResources() >= inventory.maxAmount && jobM.GetColonistJobsCountForColonist(this) == 0) && validEmptyInventoryContainers.Count > 0) {
						EmptyInventory(validEmptyInventoryContainers);
					} else {
						Wander();
					}
				} else {
					wanderTimer = UnityEngine.Random.Range(10f, 20f);
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
			if (colonistM.selectedHuman == this) {
				colonistM.SetSelectedHuman(null);
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
		}

		public void UpdateHappinessModifiers() {
			foreach (HappinessModifierGroup happinessModifierGroup in colonistM.happinessModifierGroups) {
				colonistM.happinessModifierFunctions[happinessModifierGroup.type](this);
			}

			for (int i = 0; i < happinessModifiers.Count; i++) {
				HappinessModifierInstance happinessModifier = happinessModifiers[i];
				happinessModifier.Update(timeM);
				if (happinessModifier.timer <= 0) {
					RemoveHappinessModifier(happinessModifier.prefab.type);
					i -= 1;
				}
			}
		}

		public void AddHappinessModifier(HappinessModifiersEnum happinessModifierEnum) {
			HappinessModifierInstance hmi = new HappinessModifierInstance(this, colonistM.GetHappinessModifierPrefabFromEnum(happinessModifierEnum));
			HappinessModifierInstance sameGroupHMI = happinessModifiers.Find(findHMI => hmi.prefab.group.type == findHMI.prefab.group.type);
			if (sameGroupHMI != null) {
				RemoveHappinessModifier(sameGroupHMI.prefab.type);
			}
			happinessModifiers.Add(hmi);
			if (colonistM.selectedHuman == this) {
				uiM.RemakeSelectedColonistHappinessModifiers();
			}
		}

		public void RemoveHappinessModifier(HappinessModifiersEnum happinessModifierEnum) {
			happinessModifiers.Remove(happinessModifiers.Find(findHMI => findHMI.prefab.type == happinessModifierEnum));
			if (colonistM.selectedHuman == this) {
				uiM.RemakeSelectedColonistHappinessModifiers();
			}
		}

		public void UpdateHappiness() {
			baseHappiness = Mathf.Clamp(Mathf.RoundToInt(100 - (needs.Sum(need => (need.GetValue() / (need.prefab.priority + 1))))), 0, 100);

			happinessModifiersSum = happinessModifiers.Sum(hM => hM.prefab.effectAmount);

			float targetHappiness = Mathf.Clamp(baseHappiness + happinessModifiersSum, 0, 100);
			float happinessChangeAmount = ((targetHappiness - effectiveHappiness) / (effectiveHappiness <= 0f ? 1f : effectiveHappiness));
			effectiveHappiness += happinessChangeAmount * timeM.deltaTime;
			effectiveHappiness = Mathf.Clamp(effectiveHappiness, 0, 100);
		}

		private float wanderTimer = UnityEngine.Random.Range(10f,20f);
		private void Wander() {
			if (wanderTimer <= 0) {
				List<TileManager.Tile> validWanderTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable && tile.tileType.buildable && colonistM.colonists.Find(colonist => colonist.overTile == tile) == null).ToList();
				if (validWanderTiles.Count > 0) {
					MoveToTile(validWanderTiles[UnityEngine.Random.Range(0,validWanderTiles.Count)],false);
				}
				wanderTimer = UnityEngine.Random.Range(10f,20f);
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
				job.jobProgress += needs.Find(need => need.prefab.type == NeedsEnum.Food).GetValue();
			}
			if (job.prefab.jobType == JobManager.JobTypesEnum.Sleep) {
				job.jobProgress += 20f * (needs.Find(need => need.prefab.type == NeedsEnum.Rest).GetValue());
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
				float currentRestValue = needs.Find(need => need.prefab.type == NeedsEnum.Rest).GetValue();
				float originalRestValue = currentRestValue / (job.jobProgress / job.colonistBuildTime);
				needs.Find(need => need.prefab.type == NeedsEnum.Rest).SetValue(originalRestValue * ((job.jobProgress - 1f * timeM.deltaTime) / job.colonistBuildTime));
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
				if (finishedJob.prefab.canRotate) {
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
					MoveToTile(walkableSurroundingTiles[UnityEngine.Random.Range(0,walkableSurroundingTiles.Count)],false);
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
			Dictionary<Appearance, int> appearanceIndices,
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

				moveSprites = colonistM.humanMoveSprites[appearanceIndices[Appearance.Skin]];
				this.appearanceIndices = appearanceIndices;

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

	public void SpawnStartColonists(int amount) {
		SpawnColonists(amount);

		Vector2 averageColonistPosition = new Vector2(0, 0);
		foreach (Colonist colonist in colonists) {
			averageColonistPosition = new Vector2(averageColonistPosition.x + colonist.obj.transform.position.x, averageColonistPosition.y + colonist.obj.transform.position.y);
		}
		averageColonistPosition /= colonists.Count;
		cameraM.SetCameraPosition(averageColonistPosition);

		// TEMPORARY COLONIST TESTING STUFF
		colonists[UnityEngine.Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.WheatSeed), UnityEngine.Random.Range(5, 11));
		colonists[UnityEngine.Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.Potato), UnityEngine.Random.Range(5, 11));
		colonists[UnityEngine.Random.Range(0, colonists.Count)].inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.CottonSeed), UnityEngine.Random.Range(5, 11));
	}

	public void SpawnColonists(int amount) {
		if (amount > 0) {
			int mapSize = tileM.map.mapData.mapSize;
			for (int i = 0; i < amount; i++) {
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
				TileManager.Tile colonistSpawnTile = validSpawnTiles.Count >= amount ? validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)] : walkableTilesByDistanceToCentre[UnityEngine.Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

				Colonist colonist = new Colonist(colonistSpawnTile, professions[UnityEngine.Random.Range(0, professions.Count)], 1);
				AddColonist(colonist);
			}
		}
	}

	public void AddColonist(Colonist colonist) {
		colonists.Add(colonist);
		uiM.SetColonistElements();
		tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
	}

	public Human selectedHuman;
	private GameObject selectedHumanIndicator;

	private List<Colonist> deadColonists = new List<Colonist>();

	void Update() {
		if (tileM.generated) {
			SetSelectedHumanFromClick();
		}
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
		if (Input.GetKeyDown(KeyCode.F) && selectedHuman != null) {
			cameraM.SetCameraZoom(5);
		}
		if (Input.GetKey(KeyCode.F) && selectedHuman != null) {
			cameraM.SetCameraPosition(selectedHuman.obj.transform.position);
		}

		foreach (Trader trader in traders) {
			trader.Update();
		}
	}

	public List<Trader> traders = new List<Trader>(); // TODO Remove Caravan from map and delete its traders from this list

	public class Trader : Human {

		public string originLocation; // This should be changed to a "Location" object (to be created in TileManager?) that stores info

		public Caravan caravan;

		public Trader(TileManager.Tile spawnTile, float startingHealth, Caravan caravan) : base(spawnTile, startingHealth) {
			this.caravan = caravan;

			SetNameColour(UIManager.GetColour(UIManager.Colours.LightPurple));
			obj.transform.SetParent(GameObject.Find("TraderParent").transform, false);
			MoveToTile(colonistM.colonists[0].overTile, false);
		}
	}

	private float traderTimer = 0; // The time since the last trader visited
	private static readonly int traderTimeMin = 1440; // Traders will only come once every traderTimeMin in-game minutes
	private static readonly int traderTimeMax = 10080; // Traders will definitely come if the traderTimer is greater than traderTimeMax
	public void UpdateTraders() {
		traderTimer += timeM.deltaTime;
		if (traderTimer > traderTimeMin && timeM.minuteChanged) {
			if (UnityEngine.Random.Range(0f, 1f) < ((traderTimer - traderTimeMin) / (traderTimeMax - traderTimeMin))) {
				traderTimer = 0;
				SpawnCaravan(CaravanTypeEnum.Foot, 4);
			}
		}
	}

	public List<Caravan> caravans = new List<Caravan>();

	public void SpawnCaravan(CaravanTypeEnum caravanType, int maxNumTraders) {
		List<TileManager.Tile> validSpawnTiles = null;
		if (caravanType == CaravanTypeEnum.Foot || caravanType == CaravanTypeEnum.Wagon) {
			validSpawnTiles = tileM.map.edgeTiles.Where(tile => tile.walkable && !TileManager.liquidWaterEquivalentTileTypes.Contains(tile.tileType.type)).ToList();
		} else if (caravanType == CaravanTypeEnum.Boat) {
			validSpawnTiles = new List<TileManager.Tile>(); // TODO Implement boat caravans
		}
		if (validSpawnTiles.Count > 0) {
			TileManager.Tile targetSpawnTile = validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)];

			List<TileManager.Tile> edgeTilesConnectedToTargetSpawnTile = new List<TileManager.Tile>();

			List<TileManager.Tile> spawnTilesFrontier = new List<TileManager.Tile>() { targetSpawnTile };
			List<TileManager.Tile> spawnTilesChecked = new List<TileManager.Tile>();
			while (spawnTilesFrontier.Count > 0) {
				TileManager.Tile currentTile = spawnTilesFrontier[0];
				spawnTilesFrontier.RemoveAt(0);
				spawnTilesChecked.Add(currentTile);
				edgeTilesConnectedToTargetSpawnTile.Add(currentTile);
				foreach (TileManager.Tile nTile in currentTile.horizontalSurroundingTiles) {
					if (!spawnTilesChecked.Contains(nTile) && validSpawnTiles.Contains(nTile)) {
						spawnTilesFrontier.Add(nTile);
					}
				}
			}

			if (edgeTilesConnectedToTargetSpawnTile.Count > 0) {
				int numTraders = UnityEngine.Random.Range(1, maxNumTraders + 1);
				List<TileManager.Tile> edgeTilesCloseToTargetSpawnTile = edgeTilesConnectedToTargetSpawnTile.Where(tile => Vector2.Distance(tile.obj.transform.position, targetSpawnTile.obj.transform.position) <= numTraders * 2).ToList();
				if (edgeTilesCloseToTargetSpawnTile.Count > 0) {
					if (edgeTilesCloseToTargetSpawnTile.Count < numTraders) {
						numTraders = edgeTilesCloseToTargetSpawnTile.Count;
					}
					Caravan newCaravan = new Caravan(numTraders, CaravanTypeEnum.Foot, edgeTilesCloseToTargetSpawnTile, resourceM);
					caravans.Add(newCaravan);
					traders.AddRange(newCaravan.traders);

					uiM.SetCaravanElements();
				}
			}
		}
	}

	public enum CaravanTypeEnum { Foot, Wagon, Boat };

	public class Caravan {
		private ResourceManager resourceM;

		public List<Trader> traders = new List<Trader>();
		public int amount;
		public CaravanTypeEnum caravanType;

		public ResourceManager.Inventory inventory;

		public List<ResourceManager.TradeResourceAmount> resourcesToTrade = new List<ResourceManager.TradeResourceAmount>();

		public List<List<ResourceManager.TradeResourceAmount>> confirmedResourcesToTrade = new List<List<ResourceManager.TradeResourceAmount>>();

		public Caravan(int amount, CaravanTypeEnum caravanType, List<TileManager.Tile> spawnTiles, ResourceManager resourceM) {
			this.resourceM = resourceM;

			this.amount = amount;
			this.caravanType = caravanType;
			for (int i = 0; i < amount && spawnTiles.Count > 0; i++) {
				TileManager.Tile spawnTile = spawnTiles[UnityEngine.Random.Range(0, spawnTiles.Count)];
				SpawnTrader(spawnTile);
				spawnTiles.Remove(spawnTile);
			}

			inventory = new ResourceManager.Inventory(null, null, int.MaxValue);

			foreach (ResourceManager.Resource resource in resourceM.resources) {
				if (UnityEngine.Random.Range(0, 100) < 50) {
					inventory.ChangeResourceAmount(resource, UnityEngine.Random.Range(0, 50));
				}
			}
		}

		public void SpawnTrader(TileManager.Tile spawnTile) {
			traders.Add(new Trader(spawnTile, 1f, this));
		}

		public List<ResourceManager.TradeResourceAmount> GetTradeResourceAmounts() {
			List<ResourceManager.TradeResourceAmount> tradeResourceAmounts = new List<ResourceManager.TradeResourceAmount>();
			tradeResourceAmounts.AddRange(resourcesToTrade);

			List<ResourceManager.ResourceAmount> caravanResourceAmounts = inventory.resources;
			List<ResourceManager.ResourceAmount> colonyResourceAmounts = resourceM.GetFilteredResources(true, false, true, false);

			foreach (ResourceManager.ResourceAmount resourceAmount in caravanResourceAmounts) {
				ResourceManager.TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new ResourceManager.TradeResourceAmount(resourceAmount.resource, resourceAmount.amount, DeterminePriceForResource(resourceAmount.resource), this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			foreach (ResourceManager.ResourceAmount resourceAmount in colonyResourceAmounts) {
				ResourceManager.TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new ResourceManager.TradeResourceAmount(resourceAmount.resource, 0, DeterminePriceForResource(resourceAmount.resource), this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			tradeResourceAmounts = tradeResourceAmounts.OrderByDescending(tra => tra.caravanAmount).ThenByDescending(tra => tra.resource.GetAvailableAmount()).ThenBy(tra => tra.resource.name).ToList();

			return tradeResourceAmounts;
		}

		public bool DetermineImportanceForResource(ResourceManager.Resource resource) {
			return false; // TODO This needs to be implemented once the originLocation is properly implemented (see above)
		}

		public ResourceManager.Resource.Price DeterminePriceForResource(ResourceManager.Resource resource) {
			// TODO This needs to be implemented once the originLocation is properly implemented and use DetermineImportanceForResource(...)
			//return new ResourceManager.Resource.Price(UnityEngine.Random.Range(0, 100), UnityEngine.Random.Range(0, 100), UnityEngine.Random.Range(0, 100));
			return resource.price;
		}

		public void SetSelectedResource(ResourceManager.TradeResourceAmount tradeResourceAmount) {
			ResourceManager.TradeResourceAmount existingTradeResourceAmount = resourcesToTrade.Find(tra => tra.resource == tradeResourceAmount.resource);
			if (existingTradeResourceAmount == null) {
				resourcesToTrade.Add(tradeResourceAmount);
			} else {
				if (existingTradeResourceAmount.GetTradeAmount() == 0) {
					resourcesToTrade.Remove(tradeResourceAmount);
				} else {
					existingTradeResourceAmount.SetTradeAmount(tradeResourceAmount.GetTradeAmount());
				}
			}
		}

		public void ConfirmTrade() {
			confirmedResourcesToTrade.Add(new List<ResourceManager.TradeResourceAmount>(resourcesToTrade));
			resourcesToTrade.Clear();
		}
	}
}