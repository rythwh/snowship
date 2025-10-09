using VContainer;
using VContainer.Unity;

namespace Snowship.NCamera
{
	public static class CameraServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<CameraProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.RegisterEntryPoint<CameraManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
