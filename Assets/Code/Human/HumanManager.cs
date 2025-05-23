﻿using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUI;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

public class HumanManager : IManager {

	private readonly List<string> maleNames = new List<string>();
	private readonly List<string> femaleNames = new List<string>();

	public List<List<Sprite>> humanMoveSprites = new List<List<Sprite>>();

	public event Action<Human> OnHumanSelected;

	public void CreateNames() {
		foreach (string name in Resources.Load<TextAsset>(@"Data/names-male").text.Split(' ').ToList()) {
			maleNames.Add(name.ToLower());
		}
		foreach (string name in Resources.Load<TextAsset>(@"Data/names-female").text.Split(' ').ToList()) {
			femaleNames.Add(name.ToLower());
		}
	}

	public void CreateHumanSprites() {
		for (int i = 0; i < 3; i++) {
			List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
			humanMoveSprites.Add(innerHumanMoveSprites);
		}
	}

	public Human selectedHuman;

	public void OnUpdate() {
		if (GameManager.Get<TileManager>().mapState == TileManager.MapState.Generated) {
			SetSelectedHumanFromClick();
		}
		if (Input.GetKey(KeyCode.F) && selectedHuman != null) {
			GameManager.Get<CameraManager>().SetCameraPosition(selectedHuman.obj.transform.position);
		}
	}

	public string GetName(LifeManager.Life.Gender gender) {
		string chosenName = "NAME";
		List<string> names = (gender == LifeManager.Life.Gender.Male ? maleNames : femaleNames);
		List<string> validNames = names.Where(name => humans.Find(human => human.name.ToLower() == name.ToLower()) == null).ToList();
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

	public List<Human> humans = new List<Human>();

	public class Human : LifeManager.Life, IInventory
	{

		public string name;
		public GameObject nameCanvas;
		public GameObject humanObj;

		public Dictionary<Appearance, int> bodyIndices;
		public Dictionary<Appearance, Clothing> clothes = new Dictionary<Appearance, Clothing>() {
			{ Appearance.Hat, null },
			{ Appearance.Top, null },
			{ Appearance.Bottoms, null },
			{ Appearance.Scarf, null },
			{ Appearance.Backpack, null }
		};

		// Carrying Item

		// Inventory
		private readonly Inventory inventory;

		public enum Appearance { Skin, Hair, Hat, Top, Bottoms, Scarf, Backpack };

		protected float wanderTimer = UnityEngine.Random.Range(10f, 20f);

		public event Action<Appearance, Clothing> OnClothingChanged;

		public Human(TileManager.Tile spawnTile, float startingHealth) : base(spawnTile, startingHealth) {
			bodyIndices = GetBodyIndices(gender);

			moveSprites = GameManager.Get<HumanManager>().humanMoveSprites[bodyIndices[Appearance.Skin]];

			inventory = new Inventory(this, 50000, 50000);

			SetName(GameManager.Get<HumanManager>().GetName(gender));

			humanObj = MonoBehaviour.Instantiate(GameManager.Get<ResourceManager>().humanPrefab, obj.transform, false);
			int appearanceIndex = 1;
			foreach (Appearance appearance in clothes.Keys) {
				humanObj.transform.Find(appearance.ToString()).GetComponent<SpriteRenderer>().sortingOrder = obj.GetComponent<SpriteRenderer>().sortingOrder + appearanceIndex;
				appearanceIndex += 1;
			}

			GameManager.Get<HumanManager>().humans.Add(this);
		}

		protected Human() {

		}

		public static Dictionary<Appearance, int> GetBodyIndices(Gender gender) {
			Dictionary<Appearance, int> bodyIndices = new Dictionary<Appearance, int>() {
				{ Appearance.Skin, UnityEngine.Random.Range(0,3) },
				{ Appearance.Hair, UnityEngine.Random.Range(0,0) }
			};
			return bodyIndices;
		}

		public virtual void SetName(string name) {
			this.name = name;

			obj.name = "Human: " + name;

			if (nameCanvas != null) {
				MonoBehaviour.Destroy(nameCanvas);
			}
			nameCanvas = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/Human-Canvas"), obj.transform, false);
			SetNameCanvas(name);
		}

		public void SetNameCanvas(string name) {
			nameCanvas.transform.Find("NameBackground-Image/Name-Text").GetComponent<Text>().text = name;
		}

		public void SetNameColour(Color nameColour) {
			nameCanvas.transform.Find("NameBackground-Image/NameColour-Image").GetComponent<Image>().color = nameColour;
		}

		public virtual string GetCurrentActionString() {
			return "Doing human things.";
		}

		public virtual string GetStoredActionString() {
			return string.Empty;
		}

		public override void Update() {
			base.Update();

			nameCanvas.transform.Find("NameBackground-Image").localScale = Vector2.one * Mathf.Clamp(GameManager.Get<CameraManager>().camera.orthographicSize, 2, 10) * 0.001f;
			nameCanvas.transform.Find("NameBackground-Image/HealthIndicator-Image").GetComponent<Image>().color = Color.Lerp(
				ColourUtilities.GetColour(ColourUtilities.EColour.LightRed100),
				ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen100),
				Health
			);

			SetMoveSprite();
		}

		public virtual void ChangeClothing(Appearance appearance, Clothing clothing) {
			if (clothes[appearance] != clothing) {

				if (clothing != null) {
					humanObj.transform.Find(appearance.ToString()).GetComponent<SpriteRenderer>().sprite = clothing.image;
					inventory.ChangeResourceAmount(clothing, -1, false);
				}

				if (clothes[appearance] != null) {
					humanObj.transform.Find(appearance.ToString()).GetComponent<SpriteRenderer>().sprite = GameManager.Get<ResourceManager>().clearSquareSprite;
					inventory.ChangeResourceAmount(clothes[appearance], 1, false);
				}

				clothes[appearance] = clothing;

				SetColour(overTile.sr.color);

				OnClothingChanged?.Invoke(appearance, clothing);
			}
		}

		public override void SetColour(Color newColour) {
			base.SetColour(newColour);

			foreach (Appearance appearance in clothes.Keys) {
				humanObj.transform.Find(appearance.ToString()).GetComponent<SpriteRenderer>().color = new Color(newColour.r, newColour.g, newColour.b, 1f);
			}
		}

		private void SetMoveSprite() {
			int moveSpriteIndex = CalculateMoveSpriteIndex();
			foreach (KeyValuePair<Appearance, Clothing> appearanceToClothingKVP in clothes) {
				if (appearanceToClothingKVP.Value != null) {
					humanObj.transform.Find(appearanceToClothingKVP.Key.ToString()).GetComponent<SpriteRenderer>().sprite = appearanceToClothingKVP.Value.moveSprites[moveSpriteIndex];
				}
			}
		}

		protected void Wander(TileManager.Tile stayNearTile, int stayNearTileDistance) {
			if (wanderTimer <= 0) {
				List<TileManager.Tile> validWanderTiles = overTile.surroundingTiles
					.Where(tile => tile != null && tile.walkable && tile.buildable && GameManager.Get<HumanManager>().humans.Find(human => human.overTile == tile) == null)
					.Where(tile => tile.GetObjectInstanceAtLayer(2) == null)
					.ToList();
				if (stayNearTile != null) {
					validWanderTiles = validWanderTiles.Where(t => Vector2.Distance(t.obj.transform.position, stayNearTile.obj.transform.position) <= stayNearTileDistance).ToList();
					if (Vector2.Distance(obj.transform.position, stayNearTile.obj.transform.position) > stayNearTileDistance) {
						MoveToTile(stayNearTile, false);
						return;
					}
				}
				if (validWanderTiles.Count > 0) {
					MoveToTile(validWanderTiles[UnityEngine.Random.Range(0, validWanderTiles.Count)], false);
				}
				wanderTimer = UnityEngine.Random.Range(10f, 20f);
			} else {
				wanderTimer -= 1 * GameManager.Get<TimeManager>().Time.DeltaTime;
			}
		}

		public override void Remove() {
			base.Remove();
			GameManager.Get<HumanManager>().humans.Remove(this);
		}

		public Inventory GetInventory() {
			return inventory;
		}
	}

	private void SetSelectedHumanFromClick() {
		if (Input.GetMouseButtonDown(0) && !GameManager.Get<UIManagerOld>().IsPointerOverUI()) {
			Vector2 mousePosition = GameManager.Get<CameraManager>().camera.ScreenToWorldPoint(Input.mousePosition);
			Human newSelectedHuman = humans.Find(human => Vector2.Distance(human.obj.transform.position, mousePosition) < 0.5f);
			if (newSelectedHuman != null) {
				SetSelectedHuman(newSelectedHuman);
			} else if (selectedHuman != null && selectedHuman is Colonist colonist) {
				colonist.PlayerMoveToTile(GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(mousePosition));
			}
		}
		if (Input.GetMouseButtonDown(1)) {
			SetSelectedHuman(null);
		}
	}

	public void SetSelectedHuman(Human human) {
		selectedHuman = human;

		SetSelectedHumanIndicator();

		//GameManager.Get<UIManagerOld>().SetSelectedColonistInformation(false); // TODO Fire event for human being selected
		//GameManager.Get<UIManagerOld>().SetSelectedTraderMenu();
		//GameManager.Get<UIManagerOld>().SetRightListPanelSize();

		OnHumanSelected?.Invoke(selectedHuman);
	}

	private GameObject selectedHumanIndicator;

	void SetSelectedHumanIndicator() {
		if (selectedHumanIndicator != null) {
			MonoBehaviour.Destroy(selectedHumanIndicator);
		}
		if (selectedHuman != null) {
			selectedHumanIndicator = MonoBehaviour.Instantiate(GameManager.Get<ResourceManager>().tilePrefab, selectedHuman.obj.transform, false);
			selectedHumanIndicator.name = "SelectedHumanIndicator";
			selectedHumanIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;

			SpriteRenderer sCISR = selectedHumanIndicator.GetComponent<SpriteRenderer>();
			sCISR.sprite = GameManager.Get<ResourceManager>().selectionCornersSprite;
			sCISR.sortingOrder = 20; // Selected Colonist Indicator Sprite
			sCISR.color = new Color(1f, 1f, 1f, 0.75f);
		}
	}

	public Human FindHumanByName(string name) {
		return humans.Find(human => human.name == name);
	}
}