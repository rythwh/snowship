using System.Collections.Generic;
using JetBrains.Annotations;

namespace Snowship.NEntity
{
	[UsedImplicitly]
	public class EntityRegistry
	{
		private readonly List<Entity> entities = new();
		public IReadOnlyList<Entity> Entities => entities;

		public Entity Create()
		{
			Entity entity = new();
			entities.Add(entity);
			return entity;
		}
	}
}
