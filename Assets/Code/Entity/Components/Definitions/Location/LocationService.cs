using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Snowship.NMap.NTile;
using UnityEngine;

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
			entity.Remove<OccupiedTilesComponent>();
		}

		public WorldLocationComponent MoveToTile(Entity entity, Tile tile, int rotation)
		{
			WorldLocationComponent worldLocation;
			if (entity.TryGet(out worldLocation)) {
				worldLocation.Set(tile, rotation);
			} else {
				RemoveAnyLocation(entity);
				worldLocation = entity.AddComponent(new WorldLocationComponent(tile, rotation));
			}

			if (entity.TryGet(out BuildingComponent building)) {
				List<Tile> occupied = new();
				foreach (Vector2Int pos in building.GetFootprint(rotation)) {
					Tile t = tile.map.GetTileFromPosition(tile.PositionGrid.x + pos.x, tile.PositionGrid.y + pos.y);
					if (t != null) {
						occupied.Add(t);
					}
				}

				entity.Remove<OccupiedTilesComponent>();
				entity.AddComponent(new OccupiedTilesComponent(occupied));
			}

			return worldLocation;
		}

		public void Despawn(Entity entity)
		{
			RemoveAnyLocation(entity);
		}
	}
}
