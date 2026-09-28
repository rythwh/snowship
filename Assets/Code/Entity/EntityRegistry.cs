using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Snowship.NEntity
{
	[UsedImplicitly]
	public class EntityRegistry
	{
		private readonly List<Entity> entities = new();
		public IReadOnlyList<Entity> Entities => entities;

		public event Action<Entity> OnEntityCreated;
		public event Action<Entity> OnEntityRemoved;

		public Entity Create()
		{
			Entity entity = new();
			entities.Add(entity);
			OnEntityCreated?.Invoke(entity);
			return entity;
		}

		public void Remove(Entity entity)
		{
			if (entities.Remove(entity)) {
				OnEntityRemoved?.Invoke(entity);
			}
		}
	}
}
