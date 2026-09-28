using UnityEngine;

namespace Snowship.NEntity
{
	public class EntityView : MonoBehaviour
	{
		private Entity entity;
		private SpriteRenderer spriteRenderer;

		public void Initialize(Entity entity)
		{
			this.entity = entity;
			spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer == null) {
				spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			}
			UpdateView();
		}

		public void UpdateView()
		{
			if (entity == null) return;

			if (entity.TryGet(out WorldLocationComponent location)) {
				transform.position = new Vector3(location.Tile.PositionGrid.x + 0.5f, location.Tile.PositionGrid.y + 0.5f, 0);
				// TODO: handle rotation if needed
			}

			if (entity.TryGet(out RenderingComponent rendering)) {
				if (!string.IsNullOrEmpty(rendering.SpritePath)) {
					Sprite sprite = Resources.Load<Sprite>(rendering.SpritePath);
					if (sprite != null) {
						spriteRenderer.sprite = sprite;
					}
				}

				if (spriteRenderer.sprite != null) {
					float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
					float height = spriteRenderer.sprite.rect.height / pixelsPerUnit;
					float width = spriteRenderer.sprite.rect.width / pixelsPerUnit;

					Vector2 actualPivot = spriteRenderer.sprite.pivot / spriteRenderer.sprite.rect.size;

					float offsetX = (rendering.PivotX - actualPivot.x) * width;
					float offsetY = (rendering.PivotY - actualPivot.y) * height;

					spriteRenderer.transform.localPosition = new Vector3(-offsetX, -offsetY, 0);
				}
			}
		}
	}
}
