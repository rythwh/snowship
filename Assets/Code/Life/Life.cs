using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;
using Snowship.NMap;
using Snowship.NPath;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NLife
{
	public abstract class Life
	{
		public int Id { get; }

		// Properties
		public string Name { get; protected set; }
		public Vector2 Position { get; protected set; }
		internal Vector2 PreviousPosition { get; private set; }
		public Tile Tile { get; protected set; }
		public Tile PreviousTile { get; protected set; }
		public bool Visible;
		public float Health { get; protected set; }
		public Gender Gender { get; protected set; }
		public bool IsDead { get; protected set; } = false;
		private LifeData data;

		// Pathing
		private float moveTimer;
		private List<Tile> path = new();
		protected float MoveSpeedMultiplier = 1f;
		private float moveSpeedRampingMultiplier;
		public bool IsMoving => path.Count > 0;
		public Tile NextTile => IsMoving ? path[0] : null;

		// Events
		public event Action<string> NameChanged;
		public event Action<Vector2> PositionChanged;
		public event Action<Tile> TileChanged;
		public event Action<bool> VisibilityChanged;
		public event Action<float> HealthChanged;
		public event Action Died;

		// Managers
		private IMapQuery MapQuery => GameManager.Get<IMapQuery>();
		private TimeManager TimeM => GameManager.Get<TimeManager>();

		protected Life(int id, Tile spawnTile, LifeData data) {
			Id = id;
			SetTile(spawnTile);
			Name = data.Name;
			Position = spawnTile.PositionWorld;
			Health = 1;
			Gender = GetRandomGender();

			PreviousPosition = Position;
		}

		protected void SetName(string name) {
			Name = name;
			NameChanged?.Invoke(Name);
		}

		private void SetTile(Tile tile) {
			if (Tile == tile) {
				return;
			}
			PreviousTile = Tile;
			Tile = tile;
			TileChanged?.Invoke(Tile);
			SetVisible(Tile.visible);
		}

		private static Gender GetRandomGender() {
			return (Gender)Random.Range(0, Enum.GetNames(typeof(Gender)).Length);
		}

		private void SetVisible(bool visible) {
			Visible = visible;
			VisibilityChanged?.Invoke(Visible);
		}

		public virtual void Update() {
			MoveToTile(null, false);
		}

		public bool MoveToTile(Tile endTile, bool allowEndTileNonWalkable) {
			if (Tile == endTile) {
				return true;
			}
			if (endTile != null) {
				path = Path.FindPathToTile(Tile, endTile, allowEndTileNonWalkable);
				if (path.Count == 0) {
					return false;
				}
				moveTimer = 0;
				PreviousPosition = Position;
			}
			if (path.Count > 0) {

				Position = Vector2.Lerp(PreviousPosition, path[0].obj.transform.position, moveTimer);
				SetTile(MapQuery.Map.GetTileFromPosition(Position));

				if (moveTimer >= 1f) {
					PreviousPosition = Position;
					Position = Tile.obj.transform.position;
					moveTimer = 0;
					path.RemoveAt(0);
				} else {
					moveSpeedRampingMultiplier = Mathf.Clamp01(moveSpeedRampingMultiplier + TimeM.Time.DeltaTime);
					moveTimer += 2 * TimeM.Time.DeltaTime * Tile.walkSpeed * MoveSpeedMultiplier * moveSpeedRampingMultiplier;
				}
			} else {
				path.Clear();
				if (!Mathf.Approximately(Vector2.Distance(Position, Tile.obj.transform.position), 0f)) {
					path.Add(Tile);
					moveTimer = 0;
				}
				moveSpeedRampingMultiplier = 0;
				PositionChanged?.Invoke(Position);
			}
			if (Position != PreviousPosition) {
				PositionChanged?.Invoke(Position);
			}
			return true;
		}

		public void ChangeHealthValue(float amount) {
			IsDead = false;
			Health += amount;
			switch (Health) {
				case > 1:
					Health = 1f;
					break;
				case <= 0:
					Health = 0f;
					IsDead = true;
					break;
			}
			HealthChanged?.Invoke(Health);
		}

		public virtual void Die() {
			// TODO Implement death (instance still exists but will be in a dead-state)
			Died?.Invoke();
		}

		public virtual void Remove() {
			GameManager.Get<LifeManager>().Life.Remove(this);
		}
	}
}
