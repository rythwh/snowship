using VContainer;
using VContainer.Unity;

namespace Snowship.NColonist
{
	public static class ColonistServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<ColonistProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.RegisterEntryPoint<ColonistManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
