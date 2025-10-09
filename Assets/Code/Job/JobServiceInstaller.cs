using VContainer;

namespace Snowship.NJob
{
	public static class JobServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<JobRegistry>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<JobManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
