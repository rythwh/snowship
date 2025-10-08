using Snowship;
using VContainer;
using VContainer.Unity;

public sealed class ServiceLocatorBridge : IInitializable
{
	private readonly IObjectResolver container;

	public ServiceLocatorBridge(IObjectResolver container) {
		this.container = container;
	}

	public void Initialize() {
		GameManager.Initialize(container);
	}
}

public class GameManager {

	public static readonly (int increment, string text) GameVersion = (3, "2025.1");

	public static SharedReferences SharedReferences { get; private set; }

	private static IObjectResolver resolver;

	public GameManager(SharedReferences sharedReferences) {
		SharedReferences = sharedReferences;
	}

	public static void Initialize(IObjectResolver container) {
		resolver = container;
	}

	public static TManager Get<TManager>() {
		if (resolver != null && resolver.TryResolve(typeof(TManager), out object resolved)) {
			return (TManager)resolved;
		}
		return default;
	}
}
