using Cysharp.Threading.Tasks;
using Snowship.NCamera;
using Snowship.NColony;
using Snowship.NMap.Generation;
using Snowship.NMap.NTile;
using Snowship.NState;
using Snowship.NTime;

namespace Snowship.NMap
{
	public class MapManager
	{
		private readonly IMapWrite mapWrite;
		private readonly IMapQuery mapQuery;
		private readonly IMapEvents mapEvents;
		private readonly ICameraEvents cameraEvents;
		private readonly StateManager stateM;
		private readonly TileManager tileM;
		private readonly TimeManager timeM;

		public MapManager(
			IMapWrite mapWrite,
			IMapQuery mapQuery,
			IMapEvents mapEvents,
			ICameraEvents cameraEvents,
			IColonyEvents colonyEvents,
			StateManager stateM,
			TileManager tileM,
			TimeManager timeM
		) {
			this.mapWrite = mapWrite;
			this.mapQuery = mapQuery;
			this.mapEvents = mapEvents;
			this.cameraEvents = cameraEvents;
			this.stateM = stateM;
			this.tileM = tileM;
			this.timeM = timeM;

			colonyEvents.OnColonyCreated += OnColonyCreated;
		}

		private async void OnColonyCreated(Colony _) {
			await CreateMap(mapQuery.MapData);
		}

		public async UniTask CreateMap(MapData mapData) {

			await stateM.TransitionToState(EState.LoadToSimulation);

			Map map = new Map(mapData, new MapContext(tileM.TilePrefab));
			mapWrite.SetMap(map);
			MapGenContext context = new(map, mapData, mapData.mapSeed);
			MapGenerator mapGenerator = new();
			await mapGenerator.Run(context);

			InitializeMap();

			await stateM.TransitionToState(EState.Simulation);
		}

		private void InitializeMap() {

			mapQuery.Map.DetermineVisibleRegionBlocks();

			cameraEvents.OnCameraPositionChanged += mapQuery.Map.OnCameraPositionChanged;
			cameraEvents.OnCameraZoomChanged += mapQuery.Map.OnCameraZoomChanged;
			timeM.OnTimeChanged += mapQuery.Map.OnTimeChanged;

			mapEvents.InvokeOnMapCreated();
		}
	}
}
