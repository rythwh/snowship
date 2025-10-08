using VContainer;

namespace Snowship.NColonist
{
	public static class ColonistServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<ColonistProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<ColonistManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
