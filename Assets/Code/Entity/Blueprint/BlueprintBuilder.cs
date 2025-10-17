using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;

namespace Snowship.NEntity
{
	public class BlueprintBuilder
	{
		private readonly JsonEntityLoader loader;
		private readonly MaterialRegistry materialRegistry;
		private readonly CostService costService;
		private readonly LocationService locationService;
		private readonly ObjectDefRegistry objectRegistry;

		public BlueprintBuilder(
			JsonEntityLoader loader,
			MaterialRegistry materialRegistry,
			CostService costService,
			LocationService locationService,
			ObjectDefRegistry objectRegistry
		)
		{
			this.loader = loader;
			this.materialRegistry = materialRegistry;
			this.costService = costService;
			this.locationService = locationService;
			this.objectRegistry = objectRegistry;
		}

		public Entity CreateBlueprint(EntityManager entityManager, string objectId, string materialId, Tile tile, int rotation)
		{
			JsonEntityDef def = objectRegistry.Get(objectId);
			Entity entity = loader.BuildFromDef(entityManager, def);

			if (entity.TryGet(out MaterialOptions options)) {
				MaterialDef candidate = materialRegistry.Get(materialId);
				if (!options.IsAllowed(candidate)) {
					throw new InvalidOperationException($"Material '{materialId}' not allowed for object '{objectId}'");
				}
			}

			MaterialDef material = materialRegistry.Get(materialId);
			entity.AddComponent(new CMaterial(material));

			List<Ingredient> needed = costService.ComputeFinalCost(entity);
			entity.AddComponent(new CRequiredResources(needed));
			entity.AddComponent(new CBlueprintMarker());

			locationService.MoveToTile(entity, tile, rotation);
			return entity;
		}
	}
}
