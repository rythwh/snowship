using VContainer;
using VContainer.Unity;

namespace Snowship.NUI
{
	public static class UIServiceInstaller
	{
		public static void Install(IContainerBuilder builder) {
			builder.RegisterEntryPoint<UIManager>(Lifetime.Singleton).AsSelf();

			// Main Menu
			builder.Register<UIMainMenuPresenter>(Lifetime.Singleton).AsSelf();

			builder.Register<UICreateColonyPresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UICreatePlanetPresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UILoadColonyPresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UILoadSavePresenter>(Lifetime.Singleton).AsSelf();

			builder.Register<UISettingsPresenter>(Lifetime.Singleton).AsSelf();

			// Loading
			builder.Register<UILoadingScreenPresenter>(Lifetime.Scoped).AsSelf();

			// Simulation
			builder.Register<UISimulationPresenter>(Lifetime.Singleton).AsSelf();

			builder.Register<UIDebugConsolePresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UIPauseMenuPresenter>(Lifetime.Singleton).AsSelf();

			builder.Register<UITradeMenuPresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UIActionsPanelPresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UIDateTimePresenter>(Lifetime.Singleton).AsSelf();
			builder.Register<UIHumanInfoPanelPresenter>(Lifetime.Singleton).AsSelf();
		}
	}
}
