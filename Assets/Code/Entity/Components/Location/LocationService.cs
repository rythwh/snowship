using System;
using JetBrains.Annotations;
using Snowship.NMap.NTile;

namespace Snowship.NEntity
{
	[UsedImplicitly]
	public class LocationService
	{
		private static readonly Type[] locationTypes = {
			typeof(WorldLocationC),
			typeof(InventoryLocationC),
			typeof(AttachedToEntityC)
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

		public WorldLocationC MoveToTile(Entity entity, Tile tile, int rotation)
		{
			if (entity.TryGet(out WorldLocationC worldLocation)) {
				worldLocation.Set(tile, rotation);
				return worldLocation;
			}

			RemoveAnyLocation(entity);
			return entity.AddComponent(new WorldLocationC(tile, rotation));
		}

		public void Despawn(Entity entity)
		{
			RemoveAnyLocation(entity);
		}
	}
}
