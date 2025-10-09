using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Snowship.NUI
{
	public class UIConfig<TView, TPresenter> : IUIConfig
		where TView : IUIView
		where TPresenter : IUIPresenter
	{
		public virtual bool Closeable { get; protected set; } = true;

		private string AddressableKey() {
			return $"UI/{GetType().Name}";
		}

		public async UniTask<(IUIView view, IUIPresenter presenter, LifetimeScope scope)> Open(Transform parent, LifetimeScope parentScope) {
			GameObject viewPrefab = await GetViewPrefab();
			GameObject viewGameObject = Object.Instantiate(viewPrefab, parent, false);
			TView viewComponent = viewGameObject.GetComponent<TView>();
			viewComponent.Instance = viewGameObject;

			(TPresenter presenter, LifetimeScope scope) = CreatePresenter(viewComponent, parentScope);

			return (viewComponent, presenter, scope);
		}

		protected async UniTask<GameObject> GetViewPrefab() {

			string addressableKey = AddressableKey();

			if (UIManager.CachedComponents.TryGetValue(addressableKey, out GameObject viewPrefabGameObject)) {
				return viewPrefabGameObject;
			}

			AsyncOperationHandle<GameObject> viewPrefabOperationHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
			viewPrefabOperationHandle.ReleaseHandleOnCompletion();
			viewPrefabGameObject = await viewPrefabOperationHandle;
			UIManager.CachedComponents.Add(addressableKey, viewPrefabGameObject);
			return viewPrefabGameObject;
		}

		private (TPresenter presenter, LifetimeScope scope) CreatePresenter(TView view, LifetimeScope parentScope) {

			LifetimeScope scope = parentScope.CreateChild(builder => {
				builder.RegisterInstance(view).AsSelf().As<TView>();
				builder.Register<TPresenter>(Lifetime.Scoped).AsSelf();
			});

			TPresenter presenter = scope.Container.Resolve<TPresenter>();

			return (presenter, scope);
		}

		public void OnClose() {
		}
	}

	// ReSharper disable once UnusedTypeParameter
	public class UIConfig<TView, TPresenter, TParameters> : UIConfig<TView, TPresenter>, IUIConfigParameters
		where TView : IUIView
		where TPresenter : IUIPresenter
		where TParameters : IUIParameters
	{
		public async UniTask<(IUIView view, IUIPresenter presenter, LifetimeScope scope)> Open(Transform parent, IUIParameters parameters, LifetimeScope parentScope) {
			GameObject viewPrefab = await GetViewPrefab();
			GameObject viewGameObject = Object.Instantiate(viewPrefab, parent, false);
			TView viewComponent = viewGameObject.GetComponent<TView>();
			viewComponent.Instance = viewGameObject;

			(TPresenter presenter, LifetimeScope scope) = CreatePresenter(viewComponent, parameters, parentScope);

			return (viewComponent, presenter, scope);
		}

		private (TPresenter presenter, LifetimeScope scope) CreatePresenter(TView view, IUIParameters parameters, LifetimeScope parentScope) {

			LifetimeScope scope = parentScope.CreateChild(builder => {
				builder.RegisterInstance(view).AsSelf().As<TView>();
				builder.RegisterInstance(parameters).AsSelf().As<IUIParameters>();
				builder.Register<TPresenter>(Lifetime.Scoped).AsSelf();
			});

			TPresenter presenter = scope.Container.Resolve<TPresenter>();

			return (presenter, scope);
		}
	}
}
