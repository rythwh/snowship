using VContainer;
using VContainer.Unity;

namespace Snowship.NJob
{
	public static class JobServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.RegisterEntryPoint<JobRegistry>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<JobManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
