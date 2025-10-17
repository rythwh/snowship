using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class EntityManager
	{
		private readonly List<Entity> entities = new();
		public IEnumerable<Entity> Entities => entities;

		public Entity Create()
		{
			Entity entity = new();
			entities.Add(entity);
			return entity;
		}
	}
}
