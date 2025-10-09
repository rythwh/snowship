using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;

namespace Snowship.NMap.NTile
{
	public class TileManager : IAsyncStartable
	{
		public GameObject TilePrefab { get; private set; }

		public static readonly Dictionary<int, List<List<int>>> nonWalkableSurroundingTilesComparatorMap = new Dictionary<int, List<List<int>>> {
			{ 0, new List<List<int>> { new List<int> { 4, 1, 5, 2 }, new List<int> { 7, 3, 6, 2 } } },
			{ 1, new List<List<int>> { new List<int> { 4, 0, 7, 3 }, new List<int> { 5, 2, 6, 3 } } },
			{ 2, new List<List<int>> { new List<int> { 5, 1, 4, 0 }, new List<int> { 6, 3, 7, 0 } } },
			{ 3, new List<List<int>> { new List<int> { 6, 2, 5, 1 }, new List<int> { 7, 0, 4, 1 } } }
		};

		public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken()) {
			await LoadTilePrefab();
		}

		private async UniTask LoadTilePrefab() {
			AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Game/Tile");
			handle.ReleaseHandleOnCompletion();
			TilePrefab = await handle;
		}
	}
}
