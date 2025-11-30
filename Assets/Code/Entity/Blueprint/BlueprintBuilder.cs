using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class BlueprintBuilder
	{
		private readonly JsonEntityLoader loader;
		private readonly MaterialRegistry materialRegistry;
		private readonly CostService costService;
		private readonly LocationService locationService;
		private readonly JsonEntityRegistry objectRegistry;

		public BlueprintBuilder(
			JsonEntityLoader loader,
			MaterialRegistry materialRegistry,
			CostService costService,
			LocationService locationService,
			JsonEntityRegistry objectRegistry
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
			JsonEntity def = objectRegistry.Get(objectId);
			Entity entity = loader.BuildFromDef(entityManager, def);

			if (entity.TryGet(out MaterialOptions options)) {
				Material candidate = materialRegistry.Get(materialId);
				if (!options.IsAllowed(candidate)) {
					throw new InvalidOperationException($"Material '{materialId}' not allowed for object '{objectId}'");
				}
			}

			Material material = materialRegistry.Get(materialId);
			entity.AddComponent(new MaterialC(material));

			List<Ingredient> needed = costService.ComputeFinalCost(entity);
			entity.AddComponent(new RequiredResourcesC(needed));
			entity.AddComponent(new CBlueprintMarker());

			locationService.MoveToTile(entity, tile, rotation);
			return entity;
		}
	}
}
