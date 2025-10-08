using VContainer;

namespace Snowship.NMap
{
	public static class MapServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<MapProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<MapManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
