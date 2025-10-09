using VContainer;

namespace Snowship.NResource
{
	public static class ResourceServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<ResourceProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<ResourceLoader>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<ResourceManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
