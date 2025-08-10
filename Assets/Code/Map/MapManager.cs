using Cysharp.Threading.Tasks;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NColony;

namespace Snowship.NMap
{
	public class MapManager : IManager
	{
		public Map Map { get; private set; }
		public Map Planet { get; private set; }

		private CameraManager CameraM => GameManager.Get<CameraManager>();
		private ColonyManager ColonyM => GameManager.Get<ColonyManager>();

		public MapState MapState = MapState.Nothing;

		public async UniTask Initialize(MapData mapData, MapInitializeType mapInitializeType) {

			MapState = MapState.Generating;

			InitializeMap(mapData);
			await PostInitializeMap(mapInitializeType);
		}

		private void InitializeMap(MapData mapData) {
			Map = CreateMap(mapData);
		}

		public async UniTask PostInitializeMap(MapInitializeType mapInitializeType) {
			Map map = ColonyM.colony.map;
			while (!map.Created) {
				await UniTask.NextFrame();
			}

			MapState = MapState.Generated;

			if (mapInitializeType == MapInitializeType.NewMap) {
				ColonyM.SetupNewColony(ColonyM.colony, true);
			}

			map.DetermineVisibleRegionBlocks();

			CameraM.OnCameraPositionChanged += map.OnCameraPositionChanged;
			CameraM.OnCameraZoomChanged += map.OnCameraZoomChanged;
		}

		public Map CreateMap(MapData mapData) {
			return new Map(mapData);
			//await map.CreateMap();
			//return map;
		}
	}
}
