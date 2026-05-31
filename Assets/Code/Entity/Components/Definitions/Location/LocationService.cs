using System;
using JetBrains.Annotations;
using Snowship.NMap.NTile;

namespace Snowship.NEntity
{
	[UsedImplicitly]
	public class LocationService
	{
		private static readonly Type[] locationTypes = {
			typeof(WorldLocationComponent),
			typeof(InventoryLocationComponent),
			typeof(AttachedToEntityComponent)
		};

		public bool TryGetLocation(Entity entity, out ILocation location)
		{
			foreach (Type locationType in locationTypes) {
				if (!entity.TryGet(locationType, out IComponent component)) {
					continue;
				}
				location = (ILocation)component;
				return true;
			}

			location = null;
			return false;
		}

		public void RemoveAnyLocation(Entity entity)
		{
			foreach (Type locationType in locationTypes) {
				entity.Remove(locationType);
			}
		}

		public WorldLocationComponent MoveToTile(Entity entity, Tile tile, int rotation)
		{
			if (entity.TryGet(out WorldLocationComponent worldLocation)) {
				worldLocation.Set(tile, rotation);
				return worldLocation;
			}

			RemoveAnyLocation(entity);
			return entity.AddComponent(new WorldLocationComponent(tile, rotation));
		}

		public void Despawn(Entity entity)
		{
			RemoveAnyLocation(entity);
		}
	}
}
