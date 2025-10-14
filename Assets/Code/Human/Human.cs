using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;
using Snowship.NColonist;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NProfession;
using Snowship.NResource;
using Snowship.NResource.NInventory;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NHuman
{
	public class Human : Life, IInventoriable
	{
		public virtual string Title { get; } = "Traveler";
		public virtual ILocation OriginLocation { get; protected set; }

		// Inventory
		public Inventory Inventory { get; protected set; }

		// Components
		public JobComponent Jobs { get; protected set; }
		public ProfessionsComponent Professions { get; protected set; }
		public NeedsComponent Needs { get; protected set; }
		public MoodsComponent Moods { get; protected set; }
		public SkillsComponent Skills { get; protected set; }
		public TraitsComponent Traits { get; protected set; }

		// Wandering
		protected const int WanderTimerMin = 10;
		protected const int WanderTimerMax = 20;
		protected float WanderTimer = Random.Range(WanderTimerMin, WanderTimerMax);

		// Body
		internal Dictionary<EBodySection, int> BodyTypeProperties;
		public readonly Dictionary<EBodySection, Clothing> Clothes = new() {
			{ EBodySection.Hat, null },
			{ EBodySection.Top, null },
			{ EBodySection.Bottoms, null },
			{ EBodySection.Scarf, null },
			{ EBodySection.Backpack, null }
		};

		// TODO Carrying Item

		// Events
		public event Action<EBodySection, Clothing> OnClothingChanged;

		// Managers
		private IHumanQuery HumanQuery => GameManager.Get<IHumanQuery>();

		protected Human(int id, Tile spawnTile, HumanData data) : base(id, spawnTile, data) {

			SetName(data.Name);

			Inventory = new Inventory(this, 50000, 50000);

			Jobs = new JobComponent(this);
			Professions = new ProfessionsComponent();
			Needs = new NeedsComponent(this);
			Moods = new MoodsComponent(this);
			Skills = new SkillsComponent(this);
			Traits = new TraitsComponent();

			BodyTypeProperties = SetBodyType(Gender);
		}

		private static Dictionary<EBodySection, int> SetBodyType(Gender gender) {
			Dictionary<EBodySection, int> bodyIndices = new() {
				{ EBodySection.Skin, Random.Range(0, 3) },
				{ EBodySection.Hair, Random.Range(0, 0) }
			};
			return bodyIndices;
		}

		public override void Update() {
			base.Update();

			MoveSpeedMultiplier = -((Inventory.UsedWeight() - Inventory.maxWeight) / (float)Inventory.maxWeight) + 1;
			MoveSpeedMultiplier = Mathf.Clamp(MoveSpeedMultiplier, 0.1f, 1f);
		}

		public virtual void ChangeClothing(EBodySection bodySection, Clothing clothing) {
			if (Clothes[bodySection] == clothing) {
				return;
			}

			if (clothing != null) {
				Inventory.ChangeResourceAmount(clothing, -1, false);
			}

			if (Clothes[bodySection] != null) {
				Inventory.ChangeResourceAmount(Clothes[bodySection], 1, false);
			}

			Clothes[bodySection] = clothing;

			OnClothingChanged?.Invoke(bodySection, clothing);
		}

		protected void Wander((Vector2 position, int maxDistance)? stayNearOptional = null) {
			if (WanderTimer <= 0) {
				List<Tile> validWanderTiles = Tile.SurroundingTiles[EGridConnectivity.EightWay]
					.Where(tile => tile is { walkable: true, buildable: true })
					.Where(tile => tile.GetObjectInstanceAtLayer(2) == null)
					.Where(tile => HumanQuery.Humans.FirstOrDefault(human => human.Tile == tile) == null)
					.ToList();
				if (stayNearOptional is { } stayNear) {
					validWanderTiles = validWanderTiles.Where(t => Vector2.Distance(t.PositionGrid, stayNear.position) <= stayNear.maxDistance).ToList();
				}
				if (validWanderTiles.Count > 0) {
					MoveToTile(validWanderTiles[Random.Range(0, validWanderTiles.Count)], false);
				}
				WanderTimer = Random.Range(5, 10);
			} else {
				WanderTimer -= 1 * GameManager.Get<TimeManager>().Time.DeltaTime;
			}
		}

		public void MoveToClosestWalkableTile(bool careIfOvertileIsWalkable) {
			if (!careIfOvertileIsWalkable || !Tile.walkable) {
				List<Tile> walkableSurroundingTiles = Tile.SurroundingTiles[EGridConnectivity.EightWay].Where(tile => tile != null && tile.walkable).ToList();
				if (walkableSurroundingTiles.Count > 0) {
					MoveToTile(walkableSurroundingTiles[Random.Range(0, walkableSurroundingTiles.Count)], false);
				} else {
					walkableSurroundingTiles.Clear();
					List<Tile> potentialWalkableSurroundingTiles = new();
					foreach (RegionBlock regionBlock in Tile.regionBlock.horizontalSurroundingRegionBlocks) {
						if (regionBlock.tileType.walkable) {
							potentialWalkableSurroundingTiles.AddRange(regionBlock.tiles);
						}
					}
					walkableSurroundingTiles = potentialWalkableSurroundingTiles.Where(tile => tile.SurroundingTiles[EGridConnectivity.EightWay].Find(nTile => !nTile.walkable && nTile.regionBlock == Tile.regionBlock) != null).ToList();
					if (walkableSurroundingTiles.Count > 0) {
						walkableSurroundingTiles = walkableSurroundingTiles.OrderBy(tile => Vector2.Distance(tile.PositionGrid, Tile.PositionGrid)).ToList();
						MoveToTile(walkableSurroundingTiles[0], false);
					} else {
						List<Tile> validTiles = GameManager.Get<IMapQuery>().Map.tiles.Where(tile => tile.walkable).OrderBy(tile => Vector2.Distance(tile.PositionGrid, Tile.PositionGrid)).ToList();
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
	}
}
