using VContainer;

namespace Snowship.NColony
{
	public static class ColonyServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<ColonyProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<ColonyManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
