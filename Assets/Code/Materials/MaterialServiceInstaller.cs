using Snowship.NEntity;
using VContainer;
using VContainer.Unity;

namespace Snowship.NMaterial
{
	public static class MaterialServiceInstaller
	{
		public static void Install(IContainerBuilder builder)
		{
			builder.Register<FuelFactory>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<CraftableFactory>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<ItemTraitFactoryRegistry>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

			builder.RegisterEntryPoint<MaterialProvider>(Lifetime.Singleton).AsSelf();
		}
	}
}
