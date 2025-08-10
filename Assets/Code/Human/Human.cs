using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.Tile;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NProfession;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Snowship.NHuman
{
	public class Human : Life, IInventory
	{
		// General
		public string Name;
		private GameObject nameCanvas;
		private GameObject humanObj;

		public virtual string Title { get; } = "Traveler";
		public virtual ILocation OriginLocation { get; protected set; }

		// Inventory
		public Inventory Inventory { get; }

		// Components
		public JobComponent Jobs { get; }
		public ProfessionsComponent Professions { get; }
		public NeedsComponent Needs { get; }
		public MoodsComponent Moods { get; }
		public SkillsComponent Skills { get; }
		public TraitsComponent Traits { get; }

		// Wandering
		protected const int WanderTimerMin = 10;
		protected const int WanderTimerMax = 20;
		protected float WanderTimer = UnityEngine.Random.Range(WanderTimerMin, WanderTimerMax);

		// Body
		public Dictionary<BodySection, int> bodyIndices;
		public Dictionary<BodySection, Clothing> clothes = new() {
			{ BodySection.Hat, null },
			{ BodySection.Top, null },
			{ BodySection.Bottoms, null },
			{ BodySection.Scarf, null },
			{ BodySection.Backpack, null }
		};

		// TODO Carrying Item

		// Events
		public event Action<BodySection, Clothing> OnClothingChanged;

		public Human(Tile spawnTile, float startingHealth) : base(spawnTile, startingHealth) {

			SetName(GameManager.Get<HumanManager>().GetName(gender));

			Inventory = new Inventory(this, 50000, 50000);

			Jobs = new JobComponent(this);
			Professions = new ProfessionsComponent();
			Needs = new NeedsComponent(this);
			Moods = new MoodsComponent(this);
			Skills = new SkillsComponent(this);
			Traits = new TraitsComponent();

			bodyIndices = GetBodyIndices(gender);
			moveSprites = GameManager.Get<HumanManager>().humanMoveSprites[bodyIndices[BodySection.Skin]];

			humanObj = Object.Instantiate(GameManager.Get<ResourceManager>().humanPrefab, obj.transform, false);
			int appearanceIndex = 1;
			foreach (BodySection appearance in clothes.Keys) {
				humanObj.transform.Find(appearance.ToString()).GetComponent<SpriteRenderer>().sortingOrder = obj.GetComponent<SpriteRenderer>().sortingOrder + appearanceIndex;
				appearanceIndex += 1;
			}

			GameManager.Get<HumanManager>().humans.Add(this);
		}

		protected Human() {
		}

		public virtual void SetName(string name) {
			Name = name;

			obj.name = "Human: " + name;

			if (nameCanvas) {
				nameCanvas.transform.Find("NameBackground-Image/Name-Text").GetComponent<Text>().text = name;
				return;
			}
			nameCanvas = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/Human-Canvas"), obj.transform, false);
			nameCanvas.transform.Find("NameBackground-Image/Name-Text").GetComponent<Text>().text = name;
			nameCanvas.GetComponent<Canvas>().sortingOrder = (int)SortingOrder.UI;
		}

		public static Dictionary<BodySection, int> GetBodyIndices(Gender gender) {
			Dictionary<BodySection, int> bodyIndices = new() {
				{ BodySection.Skin, UnityEngine.Random.Range(0, 3) },
				{ BodySection.Hair, UnityEngine.Random.Range(0, 0) }
			};
			return bodyIndices;
		}

		protected void SetNameColour(Color nameColour) {
			nameCanvas.transform.Find("NameBackground-Image/NameColour-Image").GetComponent<Image>().color = nameColour;
		}

		public override void Update() {
			base.Update();

			nameCanvas.transform.Find("NameBackground-Image").localScale = Vector2.one * (Mathf.Clamp(GameManager.Get<CameraManager>().camera.orthographicSize, 2, 10) * 0.001f);
			nameCanvas.transform.Find("NameBackground-Image/HealthIndicator-Image").GetComponent<Image>().color = Color.Lerp(
				ColourUtilities.GetColour(ColourUtilities.EColour.LightRed100),
				ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen100),
				Health
			);

			MoveSpeedMultiplier = -((Inventory.UsedWeight() - Inventory.maxWeight) / (float)Inventory.maxWeight) + 1;
			MoveSpeedMultiplier = Mathf.Clamp(MoveSpeedMultiplier, 0.1f, 1f);

			SetMoveSprite();
		}

		public virtual void ChangeClothing(BodySection bodySection, Clothing clothing) {
			if (clothes[bodySection] != clothing) {

				if (clothing != null) {
					humanObj.transform.Find(bodySection.ToString()).GetComponent<SpriteRenderer>().sprite = clothing.image;
					Inventory.ChangeResourceAmount(clothing, -1, false);
				}

				if (clothes[bodySection] != null) {
					humanObj.transform.Find(bodySection.ToString()).GetComponent<SpriteRenderer>().sprite = GameManager.Get<ResourceManager>().clearSquareSprite;
					Inventory.ChangeResourceAmount(clothes[bodySection], 1, false);
				}

				clothes[bodySection] = clothing;

				SetColour(Tile.sr.color);

				OnClothingChanged?.Invoke(bodySection, clothing);
			}
		}

		public override void SetColour(Color newColour) {
			base.SetColour(newColour);

			foreach (BodySection appearance in clothes.Keys) {
				humanObj.transform.Find(appearance.ToString()).GetComponent<SpriteRenderer>().color = new Color(newColour.r, newColour.g, newColour.b, 1f);
			}
		}

		private void SetMoveSprite() {
			int moveSpriteIndex = CalculateMoveSpriteIndex();
			foreach (KeyValuePair<BodySection, Clothing> appearanceToClothingKVP in clothes) {
				if (appearanceToClothingKVP.Value != null) {
					humanObj.transform.Find(appearanceToClothingKVP.Key.ToString()).GetComponent<SpriteRenderer>().sprite = appearanceToClothingKVP.Value.moveSprites[moveSpriteIndex];
				}
			}
		}

		protected void Wander(Tile stayNearTile, int stayNearTileDistance) {
			if (WanderTimer <= 0) {
				List<Tile> validWanderTiles = Tile.surroundingTiles
					.Where(tile => tile != null && tile.walkable && tile.buildable && GameManager.Get<HumanManager>().humans.Find(human => human.Tile == tile) == null)
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
				WanderTimer = UnityEngine.Random.Range(10f, 20f);
			} else {
				WanderTimer -= 1 * GameManager.Get<TimeManager>().Time.DeltaTime;
			}
		}

		public void MoveToClosestWalkableTile(bool careIfOvertileIsWalkable) {
			if (!careIfOvertileIsWalkable || !Tile.walkable) {
				List<Tile> walkableSurroundingTiles = Tile.surroundingTiles.Where(tile => tile != null && tile.walkable).ToList();
				if (walkableSurroundingTiles.Count > 0) {
					MoveToTile(walkableSurroundingTiles[UnityEngine.Random.Range(0, walkableSurroundingTiles.Count)], false);
				} else {
					walkableSurroundingTiles.Clear();
					List<Tile> potentialWalkableSurroundingTiles = new();
					foreach (RegionBlock regionBlock in Tile.regionBlock.horizontalSurroundingRegionBlocks) {
						if (regionBlock.tileType.walkable) {
							potentialWalkableSurroundingTiles.AddRange(regionBlock.tiles);
						}
					}
					walkableSurroundingTiles = potentialWalkableSurroundingTiles.Where(tile => tile.surroundingTiles.Find(nTile => !nTile.walkable && nTile.regionBlock == Tile.regionBlock) != null).ToList();
					if (walkableSurroundingTiles.Count > 0) {
						walkableSurroundingTiles = walkableSurroundingTiles.OrderBy(tile => Vector2.Distance(tile.obj.transform.position, Tile.obj.transform.position)).ToList();
						MoveToTile(walkableSurroundingTiles[0], false);
					} else {
						List<Tile> validTiles = GameManager.Get<MapManager>().Map.tiles.Where(tile => tile.walkable).OrderBy(tile => Vector2.Distance(tile.obj.transform.position, Tile.obj.transform.position)).ToList();
						if (validTiles.Count > 0) {
							MoveToTile(validTiles[0], false);
						}
					}
				}
			}
		}

		public override void Die() {
			base.Die();

			foreach (Container container in Container.containers) {
				container.Inventory.ReleaseReservedResources(this);
			}
			if (GameManager.Get<HumanManager>().selectedHuman == this) {
				GameManager.Get<HumanManager>().SetSelectedHuman(null);
			}
		}

		public override void Remove() {
			base.Remove();
			GameManager.Get<HumanManager>().humans.Remove(this);
		}
	}
}
