using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColonistManager : MonoBehaviour {

	private TileManager tileM;
	private CameraManager cameraM;
	private UIManager uiM;
	private PathManager pathM;

	void Awake() {
		tileM = GetComponent<TileManager>();
		cameraM = GetComponent<CameraManager>();
		uiM = GetComponent<UIManager>();
		pathM = GetComponent<PathManager>();

		CreateColonistSkills();
		CreateColonistProfessions();
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

		void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			pathM = GM.GetComponent<PathManager>();
			timeM = GM.GetComponent<TimeManager>();
			colonistM = GM.GetComponent<ColonistManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public int health;

		public GameObject obj;

		public TileManager.Tile overTile;
		public List<Sprite> moveSprites = new List<Sprite>();

		public Life(TileManager.Tile spawnTile) {

			GetScriptReferences();

			overTile = spawnTile;
			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),overTile.obj.transform.position,Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 10; // Life Sprite
		}

		private Dictionary<int,int> moveSpritesMap = new Dictionary<int,int>() {
			{16,1 },{32,0 },{64,0 },{128,1}
		};
		private float moveTimer;
		public List<TileManager.Tile> path = new List<TileManager.Tile>();

		public void Update() {
			overTile = tileM.GetTileFromPosition(obj.transform.position);
			MoveToTile(null);
		}

		private Vector2 oldPosition;
		public bool MoveToTile(TileManager.Tile tile) {
			if (tile != null) {
				path = pathM.FindPathToTile(overTile,tile);
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
	}

	public class Human : Life {

		public string name;

		public int skinIndex;
		public int hairIndex;
		public int shirtIndex;
		public int pantsIndex;

		// Carrying Item

		// Inventory
		public ResourceManager.Inventory inventory;

		public Human(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes) : base(spawnTile) {
			moveSprites = colonistM.humanMoveSprites[colonistLookIndexes[ColonistLook.Skin]];

			inventory = new ResourceManager.Inventory(this,null,50);
		}
	}

	public enum SkillTypeEnum { Building, Mining, Farming };

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

	public enum ProfessionTypeEnum { Nothing, Builder, Miner, Farmer };

	public List<Profession> professions = new List<Profession>();

	public class Profession {
		public ProfessionTypeEnum type;
		public string name;

		public string description;

		public Dictionary<SkillPrefab,int> skillRandomMaxValues = new Dictionary<SkillPrefab,int>();

		public Profession(List<string> data, ColonistManager colonistM) {
			type = (ProfessionTypeEnum)System.Enum.Parse(typeof(ProfessionTypeEnum),data[0]);
			name = type.ToString();

			description = data[1];

			foreach (string skillRandomMaxValueData in data[2].Split(';')) {
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

	public List<Colonist> colonists = new List<Colonist>();

	public class Colonist : Human {

		public JobManager.Job job;

		public bool playerMoved;

		public List<SkillInstance> skills = new List<SkillInstance>();

		public Profession profession;

		public Colonist(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes, Profession profession) : base(spawnTile,colonistLookIndexes) {
			obj.transform.SetParent(GameObject.Find("ColonistParent").transform,false);

			this.profession = profession;

			foreach (SkillPrefab skillPrefab in colonistM.skillPrefabs) {
				skills.Add(new SkillInstance(this,skillPrefab,Random.Range(0,profession.skillRandomMaxValues[skillPrefab])));
			}
		}

		public new void Update() {
			base.Update();
			if (job == null && path.Count <= 0) {
				Wander();
			} else {
				wanderTimer = Random.Range(10f,20f);
			}
			if (job != null && !job.started && overTile == job.tile) {
				StartJob();
			}
			if (job != null && job.started && overTile == job.tile && !Mathf.Approximately(job.jobProgress,0)) {
				WorkJob();
			}
		}

		private float wanderTimer = Random.Range(10f,20f);
		private void Wander() {
			if (wanderTimer <= 0) {
				List<TileManager.Tile> validWanderTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable && tile.tileType.buildable && colonistM.colonists.Find(colonist => colonist.overTile == tile) == null).ToList();
				if (validWanderTiles.Count > 0) {
					MoveToTile(validWanderTiles[Random.Range(0,validWanderTiles.Count)]);
				}
				wanderTimer = Random.Range(10f,20f);
			} else {
				wanderTimer -= 1 * timeM.deltaTime;
			}
		}

		public void SetJob(JobManager.Job job) {
			this.job = job;
			MoveToTile(job.tile);
		}

		public void StartJob() {
			job.started = true;
			Destroy(job.jobPreview);
			job.tile.SetTileObject(job.prefab);

			job.jobProgress *= (1 + (1 - GetJobSkillMultiplier(job.prefab.jobType)));
			job.colonistBuildTime = job.jobProgress;

			uiM.SetJobList();
		}

		public void WorkJob() {
			job.jobProgress -= 1 * timeM.deltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress,0)) {
				job.jobProgress = 0;
				FinishJob();
				return;
			}

			job.tile.objectInstances[job.prefab.layer].obj.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,((job.colonistBuildTime - job.jobProgress) / job.colonistBuildTime));
		}

		public void FinishJob() {
			job.tile.objectInstances[job.prefab.layer].obj.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);

			job.tile.objectInstances[job.prefab.layer].FinishCreation();

			GetSkillFromJobType(job.prefab.jobType).AddExperience(job.prefab.timeToBuild);

			if (!overTile.walkable) {
				List<TileManager.Tile> walkableSurroundingTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable).ToList();
				if (walkableSurroundingTiles.Count > 0) {
					MoveToTile(walkableSurroundingTiles[Random.Range(0,walkableSurroundingTiles.Count)]);
				}
			}

			if (job.prefab.jobType == JobManager.JobTypesEnum.Build) {

			} else if (job.prefab.jobType == JobManager.JobTypesEnum.Remove) {

			}

			job = null;

			uiM.SetJobList();
		}

		public void PlayerMoveToTile(TileManager.Tile tile) {
			if (job != null) {
				jobM.AddExistingJob(job);
				job = null;
				MoveToTile(tile);
			} else {
				MoveToTile(tile);
			}
		}

		public SkillInstance GetSkillFromJobType(JobManager.JobTypesEnum jobType) {
			return skills.Find(findSkill => findSkill.prefab.affectedJobTypes.ContainsKey(jobType));
		}

		public float GetJobSkillMultiplier(JobManager.JobTypesEnum jobType) {
			//return (1 + skills.Where(skill => skill.prefab.affectedJobTypes.ContainsKey(jobType)).Sum(skill => skill.currentAffectedJobTypes[jobType] - 1));
			SkillInstance skill = GetSkillFromJobType(jobType);
			if (skill != null) {
				return 1 * (-(1f / (((skill.prefab.affectedJobTypes[jobType]) * (skill.level)) + 1)) + 1);
			}
			return 1.0f;
		}
	}

	public List<Trader> traders = new List<Trader>();

	public class Trader : Human {
		public Trader(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes) : base(spawnTile,colonistLookIndexes) {
			
		}
	}

	public List<List<Sprite>> humanMoveSprites = new List<List<Sprite>>();
	public enum ColonistLook { Skin, Hair, Shirt, Pants };

	public void SpawnColonists(int amount) {
		for (int i = 0; i < 3; i++) {
			List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
			humanMoveSprites.Add(innerHumanMoveSprites);
		}

		int mapSize = tileM.mapSize;
		for (int i = 0;i < amount;i++) {

			Dictionary<ColonistLook,int> colonistLookIndexes = new Dictionary<ColonistLook,int>() {
				{ColonistLook.Skin, Random.Range(0,3) },{ColonistLook.Hair, Random.Range(0,0) },
				{ColonistLook.Shirt, Random.Range(0,0) },{ColonistLook.Pants, Random.Range(0,0) }
			};

			List<TileManager.Tile> walkableTilesByDistanceToCentre = tileM.tiles.Where(o => o.walkable && o.tileType.buildable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
			if (walkableTilesByDistanceToCentre.Count <= 0) {
				foreach (TileManager.Tile tile in tileM.tiles.Where(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f)) <= 4f)) {
					tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Grass),true,true,true,true);
				}
				tileM.Bitmasking(tileM.tiles);
				walkableTilesByDistanceToCentre = tileM.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
			}
			TileManager.Tile colonistSpawnTile = walkableTilesByDistanceToCentre[Random.Range(0,(walkableTilesByDistanceToCentre.Count > 30 ? 30 : walkableTilesByDistanceToCentre.Count))];

			Colonist colonist = new Colonist(colonistSpawnTile,colonistLookIndexes,professions[Random.Range(0,professions.Count)]);
			colonists.Add(colonist);
		}

		uiM.SetColonistList();
	}

	public Colonist selectedColonist;
	private GameObject selectedColonistIndicator;

	void Update() {
		SetSelectedColonistFromInput();
		foreach (Colonist colonist in colonists) {
			colonist.Update();
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
				selectedColonist.PlayerMoveToTile(tileM.GetTileFromPosition(mousePosition));
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
