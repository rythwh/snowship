using Code.Simulation;
using Snowship.NCamera;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NInput;
using Snowship.NJob;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NMap.NTile;
using Snowship.NPlanet;
using Snowship.NResource;
using Snowship.NSettings;
using Snowship.NState;
using Snowship.NTime;
using Snowship.NUI;
using Snowship.Persistence;
using Snowship.Selectable;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Snowship
{
	public class GameLifetimeScope : LifetimeScope
	{
		[SerializeField] private SharedReferences SharedReferences;

		protected override void Configure(IContainerBuilder builder) {

			builder.RegisterInstance(SharedReferences);

			builder.RegisterEntryPoint<ServiceLocatorBridge>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<GameManager>(Lifetime.Singleton).AsSelf();

			CameraServiceInstaller.Install(builder);

			builder.Register<CaravanManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

			ColonistServiceInstaller.Install(builder);
			ColonyServiceInstaller.Install(builder);

			builder.Register<DebugManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

			HumanServiceInstaller.Install(builder);

			builder.Register<InputManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

			JobServiceInstaller.Install(builder);

			builder.Register<LifeManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

			MapServiceInstaller.Install(builder);

			builder.Register<PersistenceManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<PlanetManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

			ResourceServiceInstaller.Install(builder);

			builder.Register<SelectionManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<SettingsManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<SimulationManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<StateManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<TileManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<TimeManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<UIManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
			builder.Register<UniverseManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
		}
	}
}
