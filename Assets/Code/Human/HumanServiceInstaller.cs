using VContainer;
using VContainer.Unity;

namespace Snowship.NHuman
{
	public static class HumanServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<HumanProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.RegisterEntryPoint<HumanManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
