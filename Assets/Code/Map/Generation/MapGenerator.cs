using Cysharp.Threading.Tasks;
using Snowship.NState;
using Snowship.NTime;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		// private readonly List<IMapGenStep> steps = new();

		// public MapGenerator Add(IMapGenStep step) {
		// 	steps.Add(step);
		// 	return this;
		// }

		public async UniTask Run(MapGenContext context) {
			// foreach (IMapGenStep step in steps) {
			// 	step.Run(context);
			// }

			await CreateMap(context);
		}

		private async UniTask CreateMap(MapGenContext context) {

			if (context.Data.actualMap) {
				await GameManager.Get<StateManager>().TransitionToState(EState.LoadToSimulation);
			}

			#region Tiles

			CreateTiles(context);
			SetSurroundingTiles(context);
			SetMapEdgeTiles(context);
			SetSortedMapEdgeTiles(context);

			#endregion

			#region Heightmap

			GenerateTerrain(context);
			AverageTileHeights(context);

			if (context.Data.preventEdgeTouching) {
				PreventEdgeTouching(context);
			}

			if (context.Data.actualMap) {
				SmoothHeightWithSurroundingPlanetTiles(context);
			}

			#endregion

			#region Regions

			SetTileRegions(context, true, false);
			ReduceNoise(context);

			#endregion Regions

			#region Rivers

			if (context.Data.actualMap) {
				CreateLargeRivers(context);
				SetTileRegions(context, false, false);
				ReduceNoise(context);
			}

			SetTileRegions(context, false, true);

			DetermineDrainageBasins(context);
			CreateRivers(context);

			#endregion Rivers

			#region Biomes

			CalculateTemperature(context);
			AverageTileTemperatures(context);
			CalculateWindDirections(context);
			CalculatePrecipitation(context);
			context.Data.primaryWindDirection = primaryWindDirection;

			SetBiomes(context);

			#endregion

			#region Region Blocks

			CreateRegionBlocks(context);

			#endregion

			if (context.Data.actualMap) {

				SetRoofs(context);

				SetCoastalWater(context);

				CreateResourceVeins(context);

				context.Map.RecalculateLighting(context.Map.tiles, false);
				context.Map.DetermineVisibleRegionBlocks();
				context.Map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
			}

			context.Map.Bitmasking(context.Map.tiles, false, false);

			context.Map.Created = true;

			if (context.Data.actualMap) {
				await GameManager.Get<StateManager>().TransitionToState(EState.Simulation);
			}
		}
	}
}
