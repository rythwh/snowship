using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Snowship.NEntity
{
	public class EntityViewManager : MonoBehaviour
	{
		private EntityRegistry entityRegistry;
		private readonly Dictionary<Entity, EntityView> entityToView = new();

		[Inject]
		public void Construct(EntityRegistry entityRegistry)
		{
			this.entityRegistry = entityRegistry;
			this.entityRegistry.OnEntityCreated += CreateView;
			this.entityRegistry.OnEntityRemoved += DestroyView;

			foreach (Entity entity in entityRegistry.Entities) {
				CreateView(entity);
			}
		}

		private void CreateView(Entity entity)
		{
			GameObject go = new GameObject($"EntityView_{entity.Id}");
			go.transform.SetParent(transform);
			EntityView view = go.AddComponent<EntityView>();
			view.Initialize(entity);
			entityToView[entity] = view;
		}

		private void DestroyView(Entity entity)
		{
			if (entityToView.TryGetValue(entity, out EntityView view)) {
				Destroy(view.gameObject);
				entityToView.Remove(entity);
			}
		}

		private void OnDestroy()
		{
			if (entityRegistry != null) {
				entityRegistry.OnEntityCreated -= CreateView;
				entityRegistry.OnEntityRemoved -= DestroyView;
			}
		}
	}
}
