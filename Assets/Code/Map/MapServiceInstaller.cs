using VContainer;
using VContainer.Unity;

namespace Snowship.NMap
{
	public static class MapServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<MapProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.RegisterEntryPoint<MapManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
