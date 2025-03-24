using System;
using System.Collections.Generic;
using Snowship.NColony;
using Snowship.NTime;
using Snowship.NUtilities;
using Snowship.Selectable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Snowship.NLife
{
	public class Life : ISelectable
	{
		public float Health { get; protected set; }

		public GameObject obj;

		// ReSharper disable once InconsistentNaming
		private TileManager.Tile _tile;
		public TileManager.Tile Tile {
			get => _tile;
			private set {
				if (_tile == value) {
					return;
				}
				_tile = value;
				OnTileChanged?.Invoke(this, _tile);
			}
		}

		public List<Sprite> moveSprites = new();
		private static readonly int[] moveSpriteIndices = { 1, 2, 0, 3, 1, 0, 0, 1 };
		private float moveTimer;
		public List<TileManager.Tile> path = new();
		public float moveSpeedMultiplier = 1f;
		public Vector2 previousPosition;
		private float moveSpeedRampingMultiplier;

		public Gender gender;

		public bool dead = false;

		public bool visible;

		public enum Gender { Male, Female };

		public event Action<Life, TileManager.Tile> OnTileChanged;
		public event Action<float> OnHealthChanged;
		public event Action<Life> OnDied;

		protected Life(TileManager.Tile spawnTile, float startingHealth) {
			_tile = spawnTile;
			Health = startingHealth;

			OnTileChanged += OnTileChangedInvoked;

			gender = GetRandomGender();

			obj = Object.Instantiate(GameManager.Get<ResourceManager>().tilePrefab, Tile.obj.transform.position, Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.Life; // Life Sprite

			previousPosition = obj.transform.position;

			GameManager.Get<LifeManager>().life.Add(this);
		}

		protected Life() {
			OnTileChanged += OnTileChangedInvoked;
		}

		private void OnTileChangedInvoked(Life life, TileManager.Tile tile) {
			SetColour(Tile.sr.color);
			SetVisible(Tile.visible);
		}

		private static Gender GetRandomGender() {
			return (Gender)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Gender)).Length);
		}

		public virtual void Update() {
			MoveToTile(null, false);
			SetMoveSprite();
		}

		public bool MoveToTile(TileManager.Tile tile, bool allowEndTileNonWalkable) {
			if (Tile == tile) {
				return true;
			}
			if (tile != null) {
				path = PathManager.FindPathToTile(Tile, tile, allowEndTileNonWalkable);
				if (path.Count == 0) {
					return false;
				}
				moveTimer = 0;
				previousPosition = obj.transform.position;
			}
			if (path.Count > 0) {

				obj.transform.position = Vector2.Lerp(previousPosition, path[0].obj.transform.position, moveTimer);
				Tile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(obj.transform.position);

				if (moveTimer >= 1f) {
					previousPosition = obj.transform.position;
					obj.transform.position = Tile.obj.transform.position;
					moveTimer = 0;
					path.RemoveAt(0);
				} else {
					moveSpeedRampingMultiplier = Mathf.Clamp01(moveSpeedRampingMultiplier + GameManager.Get<TimeManager>().Time.DeltaTime);
					moveTimer += 2 * GameManager.Get<TimeManager>().Time.DeltaTime * Tile.walkSpeed * moveSpeedMultiplier * moveSpeedRampingMultiplier;
				}
			} else {
				path.Clear();
				if (!Mathf.Approximately(Vector2.Distance(obj.transform.position, Tile.obj.transform.position), 0f)) {
					path.Add(Tile);
					moveTimer = 0;
				}
				moveSpeedRampingMultiplier = 0;
			}
			return true;
		}

		public int CalculateMoveSpriteIndex() {
			int moveSpriteIndex = 0;
			if (path.Count > 0) {
				TileManager.Tile previousTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(previousPosition);
				if (previousTile != path[0]) {
					moveSpriteIndex = previousTile.surroundingTiles.IndexOf(path[0]);
					if (moveSpriteIndex == -1) {
						moveSpriteIndex = 0;
					}
					moveSpriteIndex = moveSpriteIndices[moveSpriteIndex];
				}
			}
			return moveSpriteIndex;
		}

		private void SetMoveSprite() {
			obj.GetComponent<SpriteRenderer>().sprite = moveSprites[CalculateMoveSpriteIndex()];
		}

		public virtual void SetColour(Color newColour) {
			obj.GetComponent<SpriteRenderer>().color = new Color(newColour.r, newColour.g, newColour.b, 1f);
		}

		public void ChangeHealthValue(float amount) {
			dead = false;
			Health += amount;
			if (Health > 1f) {
				Health = 1f;
			} else if (Health <= 0f) {
				Health = 0f;
				dead = true;
			}
			OnHealthChanged?.Invoke(Health);
		}

		public virtual void Die() {
			// TODO Implement death (instance still exists but will be in a death-state)
			OnDied?.Invoke(this);
		}

		public virtual void Remove() {
			Object.Destroy(obj);
			GameManager.Get<LifeManager>().life.Remove(this);
		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);
		}

		void ISelectable.Select() {

		}

		void ISelectable.Deselect() {

		}
	}
}