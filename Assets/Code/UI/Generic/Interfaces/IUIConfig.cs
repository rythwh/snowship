using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Snowship.NUI
{
	public interface IUIConfig {
		public bool Closeable { get; }
		public UniTask<(IUIView view, IUIPresenter presenter, LifetimeScope scope)> Open(Transform parent, LifetimeScope parentScope);
		public void OnClose();
	}

	public interface IUIConfigParameters : IUIConfig
	{
		public UniTask<(IUIView view, IUIPresenter presenter, LifetimeScope scope)> Open(Transform parent, IUIParameters parameters, LifetimeScope parentScope);
	}
}
