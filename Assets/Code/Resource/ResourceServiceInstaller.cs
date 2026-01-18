using Snowship.NMaterial;
using VContainer;
using VContainer.Unity;

namespace Snowship.NResource
{
	public static class ResourceServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<ResourceProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.RegisterEntryPoint<ResourceLoader>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<ResourceManager>(Lifetime.Singleton).AsSelf();

			builder.RegisterEntryPoint<MaterialRegistry>(Lifetime.Singleton).AsSelf();
		}
	}
}
