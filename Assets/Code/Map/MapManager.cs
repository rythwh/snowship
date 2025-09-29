using Cysharp.Threading.Tasks;
using Snowship.NCamera;
using Snowship.NColony;
using Snowship.NMap.Generation;

namespace Snowship.NMap
{
	public class MapManager : IManager
	{
		public Map Map { get; private set; }

		private CameraManager CameraM => GameManager.Get<CameraManager>();
		private ColonyManager ColonyM => GameManager.Get<ColonyManager>();

		public MapState MapState = MapState.Nothing;

		public async UniTask CreateMap(MapData mapData, MapInitializeType mapInitializeType) {

			MapState = MapState.Generating;

			Map = new Map(mapData);
			MapGenContext context = new(Map, mapData, mapData.mapSeed);

			MapGenerator mapGenerator = new();
			await mapGenerator.Run(context);

			MapState = MapState.Generated;

			if (mapInitializeType == MapInitializeType.NewMap) {
				ColonyM.SetupNewColony();
			}

			Map.DetermineVisibleRegionBlocks();

			CameraM.OnCameraPositionChanged += Map.OnCameraPositionChanged;
			CameraM.OnCameraZoomChanged += Map.OnCameraZoomChanged;
		}
	}
}
