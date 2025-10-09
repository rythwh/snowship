using VContainer;
using VContainer.Unity;

namespace Snowship.NColony
{
	public static class ColonyServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<ColonyProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.RegisterEntryPoint<ColonyManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
