using Snowship.NEntity;
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
using Snowship.NMaterial;
using Snowship.NPlanet;
using Snowship.NResource;
using Snowship.NSettings;
using Snowship.NState;
using Snowship.NTime;
using Snowship.NUI;
using Snowship.NPersistence;
using Snowship.NSelection;
using Snowship.NSimulation;
using Snowship.NUniverse;
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

			builder.RegisterEntryPoint<CaravanManager>(Lifetime.Singleton).AsSelf();

			ColonistServiceInstaller.Install(builder);
			ColonyServiceInstaller.Install(builder);

			builder.RegisterEntryPoint<DebugManager>(Lifetime.Singleton).AsSelf();

			EntityServiceInstaller.Install(builder);
			HumanServiceInstaller.Install(builder);

			builder.RegisterEntryPoint<InputManager>(Lifetime.Singleton).AsSelf();

			JobServiceInstaller.Install(builder);

			builder.RegisterEntryPoint<LifeManager>(Lifetime.Singleton).AsSelf();

			MapServiceInstaller.Install(builder);

			builder.RegisterEntryPoint<PersistenceManager>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<PlanetManager>(Lifetime.Singleton).AsSelf();

			ResourceServiceInstaller.Install(builder);
			MaterialServiceInstaller.Install(builder);

			builder.RegisterEntryPoint<SelectionManager>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<SettingsManager>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<SimulationManager>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<StateManager>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<TileManager>(Lifetime.Singleton).AsSelf();
			builder.RegisterEntryPoint<TimeManager>(Lifetime.Singleton).AsSelf();

			UIServiceInstaller.Install(builder);

			builder.RegisterEntryPoint<UniverseManager>(Lifetime.Singleton).AsSelf();
		}
	}
}
