using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NCamera;
using Snowship.NColony;

public class TileManager : IManager {
	public static readonly Dictionary<int, List<List<int>>> nonWalkableSurroundingTilesComparatorMap = new Dictionary<int, List<List<int>>>() {
		{ 0, new List<List<int>>() { new List<int>() { 4, 1, 5, 2 }, new List<int>() { 7, 3, 6, 2 } } },
		{ 1, new List<List<int>>() { new List<int>() { 4, 0, 7, 3 }, new List<int>() { 5, 2, 6, 3 } } },
		{ 2, new List<List<int>>() { new List<int>() { 5, 1, 4, 0 }, new List<int>() { 6, 3, 7, 0 } } },
		{ 3, new List<List<int>>() { new List<int>() { 6, 2, 5, 1 }, new List<int>() { 7, 0, 4, 1 } } }
	};

	public enum MapState {
		Nothing, Generating, Generated
	}

	public MapState mapState = MapState.Nothing;

	public enum MapInitializeType {
		NewMap,
		LoadMap
	}

	public async UniTask Initialize(Colony colony, MapInitializeType mapInitializeType) {

		// GameManager.Get<UIManagerOld>().SetGameUIActive(false);

		mapState = MapState.Generating;

		InitializeMap(colony);
		await PostInitializeMap(mapInitializeType);
	}

	private void InitializeMap(Colony colony) {
		colony.map = CreateMap(colony.mapData);
	}

	public async UniTask PostInitializeMap(MapInitializeType mapInitializeType) {
		Map map = GameManager.Get<ColonyManager>().colony.map;
		while (!map.Created) {
			await UniTask.NextFrame();
		}

		GameManager.Get<TileManager>().mapState = MapState.Generated;

		if (mapInitializeType == MapInitializeType.NewMap) {
			GameManager.Get<ColonyManager>().SetupNewColony(GameManager.Get<ColonyManager>().colony, true);
		}

		map.SetInitialRegionVisibility();
		map.DetermineVisibleRegionBlocks();
		CameraManager cameraM = GameManager.Get<CameraManager>();
		cameraM.OnCameraPositionChanged += map.OnCameraPositionChanged;
		cameraM.OnCameraZoomChanged += map.OnCameraZoomChanged;

		// GameManager.Get<UIManagerOld>().SetGameUIActive(true);
	}

	public Map CreateMap(MapData mapData) {
		return new Map(mapData);
		//await map.CreateMap();
		//return map;
	}
}
