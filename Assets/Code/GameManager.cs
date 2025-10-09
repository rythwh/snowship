using JetBrains.Annotations;
using VContainer;
using VContainer.Unity;

namespace Snowship
{
	[UsedImplicitly]
	public sealed class ServiceLocatorBridge : IInitializable
	{
		private readonly IObjectResolver container;

		public ServiceLocatorBridge(IObjectResolver container)
		{
			this.container = container;
		}

		public void Initialize()
		{
			GameManager.Initialize(container);
		}
	}

	public class GameManager
	{

		public static readonly (int increment, string text) GameVersion = (3, "2025.1");

		private static IObjectResolver resolver;

		public static void Initialize(IObjectResolver container)
		{
			resolver = container;
		}

		public static TManager Get<TManager>()
		{
			if (resolver != null && resolver.TryResolve(typeof(TManager), out object resolved)) {
				return (TManager)resolved;
			}
			return default;
		}
	}
}
