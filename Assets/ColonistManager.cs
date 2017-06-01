using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColonistManager : MonoBehaviour {

	private TileManager tileM;
	private CameraManager cameraM;

	void Awake() {
		tileM = GetComponent<TileManager>();
		cameraM = GetComponent<CameraManager>();
	}

	public List<Life> animals = new List<Life>();

	public class Life {

		public TileManager tileM;
		public PathManager pathM;
		public TimeManager timeM;
		public ColonistManager colonistM;
		public JobManager jobM;
		public ResourceManager resourceM;

		void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			tileM = GM.GetComponent<TileManager>();
			pathM = GM.GetComponent<PathManager>();
			timeM = GM.GetComponent<TimeManager>();
			colonistM = GM.GetComponent<ColonistManager>();
			jobM = GM.GetComponent<JobManager>();
			resourceM = GM.GetComponent<ResourceManager>();
		}

		public int health;

		public GameObject obj;

		public TileManager.Tile overTile;
		public List<Sprite> moveSprites = new List<Sprite>();

		public Life(TileManager.Tile spawnTile) {

			GetScriptReferences();

			overTile = spawnTile;
			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),overTile.obj.transform.position,Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 3;
		}

		private Dictionary<int,int> moveSpritesMap = new Dictionary<int,int>() {
			{16,1 },{32,0 },{64,0 },{128,1}
		};
		private float moveTimer;
		public List<TileManager.Tile> path = new List<TileManager.Tile>();

		public void Update() {
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
				overTile = tileM.GetTileFromPosition(obj.transform.position);

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

		// Skills/Abilities

		public Human(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes) : base(spawnTile) {
			moveSprites = colonistM.humanMoveSprites[colonistLookIndexes[ColonistLook.Skin]];
		}
	}

	public List<Colonist> colonists = new List<Colonist>();

	public class Colonist : Human {

		public JobManager.Job job;

		public bool playerMoved;

		public Colonist(TileManager.Tile spawnTile,Dictionary<ColonistLook,int> colonistLookIndexes) : base(spawnTile,colonistLookIndexes) {
			obj.transform.SetParent(GameObject.Find("ColonistParent").transform,false);
		}

		public new void Update() {
			base.Update();
			if (job != null && !job.started && overTile == job.tile) {
				StartJob();
			}
			if (job != null && job.started && overTile == job.tile && !Mathf.Approximately(job.jobProgress,0)) {
				WorkJob();
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
		}

		public void WorkJob() {
			job.jobProgress -= 1 * timeM.deltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress,0)) {
				job.jobProgress = 0;
				FinishJob();
				return;
			}

			job.tile.objectInstances[job.prefab.layer].obj.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,((job.prefab.timeToBuild - job.jobProgress) / job.prefab.timeToBuild));
		}

		public void FinishJob() {
			job.tile.objectInstances[job.prefab.layer].obj.GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);

			job.tile.objectInstances[job.prefab.layer].FinishCreation();

			if (job.prefab.jobType == JobManager.JobTypesEnum.Build) {

			} else if (job.prefab.jobType == JobManager.JobTypesEnum.Remove) {

			}

			job = null;
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

			List<TileManager.Tile> walkableTilesByDistanceToCentre = tileM.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f))).ToList();
			if (walkableTilesByDistanceToCentre.Count <= 0) {
				foreach (TileManager.Tile tile in tileM.tiles.Where(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f)) <= 4f)) {
					tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Grass),true);
				}
				walkableTilesByDistanceToCentre = tileM.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position,new Vector2(mapSize / 2f,mapSize / 2f))).ToList();
			}
			TileManager.Tile colonistSpawnTile = walkableTilesByDistanceToCentre[Random.Range(0,(walkableTilesByDistanceToCentre.Count > 30 ? 30 : walkableTilesByDistanceToCentre.Count))];

			Colonist colonist = new Colonist(colonistSpawnTile,colonistLookIndexes);
			colonists.Add(colonist);
		}
	}

	public Colonist selectedColonist;
	private GameObject selectedColonistIndicator;

	void Update() {
		SetSelectedColonist();
		foreach (Colonist colonist in colonists) {
			colonist.Update();
		}
	}

	void SetSelectedColonist() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(0)) {
			bool foundColonist = false;
			Colonist newSelectedColonist = colonists.Find(colonist => Vector2.Distance(colonist.obj.transform.position,mousePosition) < 0.5f);
			if (newSelectedColonist != null) {
				DeselectSelectedColonist();
				selectedColonist = newSelectedColonist;
				foundColonist = true;
			}

			if (foundColonist) {
				selectedColonistIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),selectedColonist.obj.transform,false);
				SpriteRenderer sCISR = selectedColonistIndicator.GetComponent<SpriteRenderer>();
				sCISR.sprite = Resources.Load<Sprite>(@"UI/selectionCorners");
				sCISR.sortingOrder = 4;
				sCISR.color = new Color(1f,1f,1f,0.75f);
				selectedColonistIndicator.transform.localScale = new Vector2(1f,1f) * 1.2f;
			}

			if (!foundColonist && selectedColonist != null) {
				selectedColonist.PlayerMoveToTile(tileM.GetTileFromPosition(mousePosition));
			}
		}
		if (Input.GetMouseButtonDown(1)) {
			DeselectSelectedColonist();
		}
	}

	void DeselectSelectedColonist() {
		selectedColonist = null;
		Destroy(selectedColonistIndicator);
	}
}
