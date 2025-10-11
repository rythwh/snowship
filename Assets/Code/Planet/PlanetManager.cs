using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NMap;
using Snowship.NMap.Generation;
using Snowship.NMap.NTile;

namespace Snowship.NPlanet
{
	[UsedImplicitly]
	public class PlanetManager
	{
		private readonly IMapGenerator mapGenerator;
		private readonly TileManager tileM;

		public Planet planet;
		public PlanetTile selectedPlanetTile;
		private readonly PlanetMapDataValues planetMapDataValues = new PlanetMapDataValues();

		public PlanetManager(IMapGenerator mapGenerator, TileManager tileM)
		{
			this.mapGenerator = mapGenerator;
			this.tileM = tileM;
		}

		public void SetSelectedPlanetTile(PlanetTile planetTile) {
			selectedPlanetTile = planetTile;
		}

		public async UniTask<Planet> CreatePlanet(CreatePlanetData createPlanetData) {

			MapData planetData = new MapData(
				createPlanetData.Seed,
				createPlanetData.Size,
				PlanetMapDataValues.ActualMap,
				PlanetMapDataValues.PlanetTemperature,
				createPlanetData.TemperatureRange,
				createPlanetData.Distance,
				PlanetMapDataValues.AverageTemperature,
				PlanetMapDataValues.AveragePrecipitation,
				planetMapDataValues.terrainTypeHeights,
				planetMapDataValues.surroundingPlanetTileHeightDirections,
				PlanetMapDataValues.River,
				planetMapDataValues.surroundingPlanetTileRivers,
				PlanetMapDataValues.PreventEdgeTouching,
				createPlanetData.WindDirection,
				planetMapDataValues.planetTilePosition
			);

			planet = new Planet(planetData, new MapContext(tileM.TilePrefab));
			MapGenContext context = new MapGenContext(planet, planetData, planetData.mapSeed);
			await mapGenerator.Run(context);
			planet.planetTiles.AddRange(planet.tiles.ConvertAll(t => new PlanetTile(planet, t)));
			return planet;
		}
	}
}
