using Cysharp.Threading.Tasks;
using Snowship.NMap;
using Snowship.NMap.Generation;

namespace Snowship.NPlanet {
	public class PlanetManager : Manager {

		public Planet planet;
		public PlanetTile selectedPlanetTile;
		private readonly PlanetMapDataValues planetMapDataValues = new PlanetMapDataValues();

		public void SetPlanet(Planet planet) {
			this.planet = planet;
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

			Planet planet = new Planet(planetData);
			MapGenContext context = new MapGenContext(planet, planetData, planetData.mapSeed);
			MapGenerator mapGenerator = new();
			await mapGenerator.Run(context);
			planet.planetTiles.AddRange(planet.tiles.ConvertAll(t => new PlanetTile(planet, t)));

			this.planet = planet;

			return planet;
		}
	}
}
