using VContainer;

namespace Snowship.NCamera
{
	public static class CameraServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.Register<CameraProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<CameraManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
