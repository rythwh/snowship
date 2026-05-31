using Snowship.NMaterial;
using VContainer;
using VContainer.Unity;

namespace Snowship.NEntity
{
	public static class EntityServiceInstaller
	{
		public static void Install(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<EntityRegistry>(Lifetime.Singleton).AsSelf();
			builder.Register<LocationService>(Lifetime.Singleton).AsSelf();
			builder.Register<StatService>(Lifetime.Singleton).AsSelf();
			builder.Register<CostService>(Lifetime.Singleton).AsSelf();

			// builder.RegisterInstance(...MaterialRegistry...).AsSelf();

			builder.Register<IJsonComponentFactory, BaseStatsFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, BaseRecipeFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, MaterialOptionsFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, QualityFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, ContainerFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, BedFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, CraftingFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, PlantFactory>(Lifetime.Singleton);
			builder.Register<IJsonComponentFactory, BuildingFactory>(Lifetime.Singleton);

			builder.Register<JsonComponentFactoryRegistry>(Lifetime.Singleton).AsSelf();

			builder.Register<JsonEntityLoader>(Lifetime.Singleton).AsSelf();

			// builder.RegisterInstance(...ObjectDefRegistry...).AsSelf();
		}
	}
}
