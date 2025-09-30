using System.Drawing;
using Cysharp.Threading.Tasks;
using Snowship.NState;
using Snowship.NTime;
using Snowship.NUI;

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

			// Tiles

			await UpdateProgress(context, "Tiles", "Creating Tiles");
			CreateTiles(context);
			SetSurroundingTiles(context);
			SetMapEdgeTiles(context);
			SetSortedMapEdgeTiles(context);

			// Heightmap

			await UpdateProgress(context, "Terrain", "Generating Noise");
			GenerateTerrain(context);
			await UpdateProgress(context, substate: "Smoothing Noise");
			AverageTileHeights(context);

			if (context.Data.preventEdgeTouching) {
				PreventEdgeTouching(context);
			}

			if (context.Data.actualMap) {
				await UpdateProgress(context, substate: "Applying Planetary Context");
				SmoothHeightWithSurroundingPlanetTiles(context);
			}

			// Regions

			await UpdateProgress(context, "Regions", "Determining Regions");
			SetTileRegions(context, true, false);
			await UpdateProgress(context, substate: "Removing Region Noise");
			ReduceNoise(context);

			// Rivers

			if (context.Data.actualMap) {
				await UpdateProgress(context, "Rivers", "Creating Planetary Rivers");
				CreateLargeRivers(context);
				await UpdateProgress(context, substate: "Determining Regions");
				SetTileRegions(context, false, false);
				await UpdateProgress(context, substate: "Removing Region Noise");
				ReduceNoise(context);
			}

			await UpdateProgress(context, substate: "Determining Regions");
			SetTileRegions(context, false, true);

			await UpdateProgress(context, substate: "Calculating Local Drainage Basins");
			DetermineDrainageBasins(context);
			await UpdateProgress(context, substate: "Creating Local Rivers");
			CreateRivers(context);

			// Biomes

			await UpdateProgress(context, "Biomes", "Calculating Temperatures");
			CalculateTemperature(context);
			await UpdateProgress(context, substate: "Smoothing Temperatures");
			AverageTileTemperatures(context);
			await UpdateProgress(context, substate: "Calculating Wind");
			CalculateWindDirections(context);
			await UpdateProgress(context, substate: "Calculating Precipitation");
			CalculatePrecipitation(context);
			context.Data.primaryWindDirection = primaryWindDirection;

			await UpdateProgress(context, substate: "Determining Biomes");
			SetBiomes(context);

			// Region Blocks

			await UpdateProgress(context, "Regions", "Creating Region Blocks");
			CreateRegionBlocks(context);

			if (context.Data.actualMap) {

				// Roofs
				await UpdateProgress(context, "Roofs", "Determining Roofs");
				SetRoofs(context);

				// Biomes
				await UpdateProgress(context, "Biomes", "Determining Coastal Waters");
				SetCoastalWater(context);

				// Resources
				await UpdateProgress(context, "Resources", "Generating Resources");
				CreateResourceVeins(context);

				// Lighting
				await UpdateProgress(context, "Lighting", "Calculating Lighting");
				context.Map.RecalculateLighting(context.Map.tiles, false);
				context.Map.DetermineVisibleRegionBlocks();
				context.Map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
			}

			await UpdateProgress(context, "Finishing Touches", string.Empty, false);
			context.Map.RedrawTiles(context.Map.tiles, false, false);

			context.Map.Created = true;

			await UpdateProgress(context, "Map Generation Completed", "One moment please...");

			if (context.Data.actualMap) {
				await GameManager.Get<StateManager>().TransitionToState(EState.Simulation);
			}
		}

		private async UniTask UpdateProgress(MapGenContext context, string state = null, string substate = null, bool redraw = true) {
			if (!context.Data.actualMap) {
				return;
			}
			context.SetNewState(state, substate);
			if (redraw) {
				context.Map.RedrawTiles(context.Map.tiles, false, false);
			}
			await UniTask.NextFrame();
		}
	}
}
