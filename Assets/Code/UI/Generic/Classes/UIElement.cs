using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Snowship.NUI {
	public abstract class UIElement<TComponent> where TComponent : UIElementComponent {
		protected TComponent Component { get; private set; }
		private bool componentCreated = false;

		public async UniTask OpenAsync(Transform parent) {
			await CreateComponent(parent);
			await OnCreate();
		}

		private string AddressableKey() {
			return $"UI/Element/{GetType().Name}";
		}

		private async UniTask CreateComponent(Transform parent) {

			string addressableKey = AddressableKey();

			if (!UIManager.CachedComponents.TryGetValue(addressableKey, out GameObject componentPrefab)) {
				AsyncOperationHandle<GameObject> componentPrefabOperationHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
				componentPrefabOperationHandle.ReleaseHandleOnCompletion();
				componentPrefab = await componentPrefabOperationHandle;
				Debug.Log($"Created element: {addressableKey}");
				UIManager.CachedComponents.Add(addressableKey, componentPrefab);
			}

			Component = Object.Instantiate(componentPrefab, parent, false).GetComponent<TComponent>();
			await Component.OnCreate();
			componentCreated = true;
		}

		protected virtual UniTask OnCreate() {
			return UniTask.CompletedTask;
		}

		protected virtual void OnClose() {
			if (!componentCreated) {
				return;
			}
			Component.Close();
			Object.Destroy(Component.gameObject);
		}

		public void Close() => OnClose();
	}
}
