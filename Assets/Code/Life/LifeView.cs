using JetBrains.Annotations;
using Snowship.NCamera;
using Snowship.NMap;
using Snowship.NMap.NTile;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NLife
{
	public abstract class LifeView<TLife> : MonoBehaviour where TLife : Life
	{
		protected TLife Model { get; private set; }

		private LifeViewModule[] modules;

		protected Sprite[] MoveSprites;
		private readonly int[] moveSpriteIndices = { 1, 2, 0, 3, 1, 0, 0, 1 };

		private ICameraEvents CameraEvents => GameManager.Get<ICameraEvents>();
		private ICameraQuery CameraQuery => GameManager.Get<ICameraQuery>();
		private IMapQuery MapQuery => GameManager.Get<IMapQuery>();

		[SerializeField] protected SpriteRenderer uBodySpriteRenderer;
		[SerializeField] private WorldSpaceNameBox uNameBox;
		[SerializeField] protected BodySectionView[] uBodySections;

		public virtual void Bind([NotNull] TLife model, Sprite[] moveSprites) {

			Unbind();

			Model = model;
			MoveSprites = moveSprites;

			Model.NameChanged += OnModelNameChanged;
			Model.PositionChanged += OnModelPositionChanged;
			Model.TileChanged += OnModelTileChanged;
			Model.VisibilityChanged += OnModelVisibilityChanged;
			Model.HealthChanged += OnModelHealthChanged;
			Model.Died += OnModelDied;

			CameraEvents.OnCameraZoomChanged += OnCameraZoomChanged;

			transform.SetParent(GameManager.Get<SharedReferences>().LifeParent, false);
			transform.position = model.Position;

			modules = GetComponents<LifeViewModule>();
			foreach (LifeViewModule module in modules) {
				module.OnBind(Model, this);
			}

			foreach (BodySectionView bodySectionView in uBodySections) {
				bodySectionView.Initialize();
			}

			OnAfterBind();
		}

		public virtual void Unbind() {
			if (Model == null) {
				return;
			}

			Model.NameChanged -= OnModelNameChanged;
			Model.PositionChanged -= OnModelPositionChanged;
			Model.TileChanged -= OnModelTileChanged;
			Model.VisibilityChanged -= OnModelVisibilityChanged;
			Model.HealthChanged -= OnModelHealthChanged;
			Model.Died -= OnModelDied;

			CameraEvents.OnCameraZoomChanged -= OnCameraZoomChanged;

			if (modules != null) {
				foreach (LifeViewModule module in modules) {
					module.OnUnbind();
				}
				modules = null;
			}

			OnBeforeUnbind();
			Model = null;
		}

		protected virtual void OnAfterBind() {
			OnModelNameChanged(Model.Name);
			OnModelPositionChanged(Model.Position);
			OnModelTileChanged(Model.Tile);
			OnModelVisibilityChanged(Model.Visible);
			OnModelHealthChanged(Model.Health);

			OnCameraZoomChanged(CameraQuery.CurrentZoom, CameraQuery.CurrentPosition);
		}

		protected virtual void OnBeforeUnbind() { }

		public void ApplyTileColour() {
			SetColour(Model.Tile.sr.color);
		}

		protected virtual void OnModelNameChanged(string name) {
			gameObject.name = $"{Model.GetType().Name}-{name}";

			uNameBox.OnNameChanged(name);
		}

		internal virtual void OnNameColourChanged(Color colour) {
			uNameBox.OnNameColourChanged(colour);
		}

		protected void OnCameraZoomChanged(float zoom, Vector2 _) {
			uNameBox.OnCameraZoomChanged(zoom);
		}

		protected virtual void OnModelPositionChanged(Vector2 position) {
			transform.position = position;
			SetMoveSprite();
		}

		protected virtual void OnModelTileChanged(Tile tile) {
			SetColour(tile.sr.color);
			SetSortingOrder(tile.sr.sortingOrder + (int)SortingOrder.Object);
		}

		protected virtual void OnModelVisibilityChanged(bool visible) {
			SetVisible(visible);
		}

		protected virtual void OnModelHealthChanged(float health) {
			uNameBox.OnHealthChanged(health);
		}

		protected virtual void OnModelDied() {
			// TODO Death logic
		}

		protected virtual void OnDestroy() {
			Unbind();
		}

		protected void SetColour(Color newColour) {
			newColour.a = 1;
			foreach (BodySectionView bodySection in uBodySections) {
				bodySection.SetAmbientColour(newColour);
			}
		}

		public Sprite GetForwardFacingSprite()
		{
			return MoveSprites[0];
		}

		protected int CalculateMoveSpriteIndex() {
			if (!Model.IsMoving) {
				return 0;
			}
			Tile previousTile = MapQuery.Map.GetTileFromPosition(Model.PreviousPosition);
			if (previousTile == Model.NextTile) {
				return 0;
			}
			int moveSpriteIndex = previousTile.SurroundingTiles[EGridConnectivity.EightWay].IndexOf(Model.NextTile);
			if (moveSpriteIndex == -1) {
				moveSpriteIndex = 0;
			}
			moveSpriteIndex = moveSpriteIndices[moveSpriteIndex];
			return moveSpriteIndex;
		}

		private void SetMoveSprite() {
			int moveSpriteIndex = CalculateMoveSpriteIndex();
			uBodySpriteRenderer.sprite = MoveSprites[moveSpriteIndex];
			foreach (BodySectionView bodySection in uBodySections) {
				bodySection.SetMoveSprite(moveSpriteIndex);
			}
		}

		private void SetVisible(bool visible) {
			gameObject.SetActive(visible);
		}

		private void SetSortingOrder(int sortingOrder) {
			uBodySpriteRenderer.sortingOrder = sortingOrder;
			foreach (BodySectionView bodySectionView in uBodySections) {
				bodySectionView.SetSortingOrder(uBodySpriteRenderer.sortingOrder);
			}
		}
	}
}
