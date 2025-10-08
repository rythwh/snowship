using VContainer;

namespace Snowship.NHuman
{
	public static class HumanServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<HumanProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<HumanManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
