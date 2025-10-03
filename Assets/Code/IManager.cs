public abstract class Manager : IManager
{
	public bool Created { get; internal set; } = false;
	public bool PostGameSetupCompleted { get; internal set; } = false;

	public virtual void OnCreate() {
	}

	public virtual void OnGameSetupComplete() {
	}

	public virtual void OnUpdate() {
	}

	public virtual void OnClose() {
	}
}

public interface IManager
{
	void OnCreate();
	void OnGameSetupComplete();
	void OnUpdate();
	void OnClose();
}
