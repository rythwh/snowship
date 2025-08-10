using Snowship.NMap;
using Cysharp.Threading.Tasks;
using Snowship.NColonist;
using Snowship.NPlanet;

namespace Snowship.NColony {
	public class ColonyManager : IManager {

		public Colony colony;

		private MapManager MapM => GameManager.Get<MapManager>();
		private ColonistManager ColonistM => GameManager.Get<ColonistManager>();
		private PlanetManager PlanetM => GameManager.Get<PlanetManager>();

		public void CreateColony(CreateColonyData data) {

			MapData mapData = new MapData(
				PlanetM.planet.MapData,
				data.Seed,
				data.Size,
				true,
				false,
				0,
				0,
				data.PlanetTile.tile.temperature,
				data.PlanetTile.tile.GetPrecipitation(),
				data.PlanetTile.terrainTypeHeights,
				data.PlanetTile.surroundingPlanetTileHeightDirections,
				data.PlanetTile.isRiver,
				data.PlanetTile.surroundingPlanetTileRivers,
				false,
				PlanetM.planet.primaryWindDirection,
				data.PlanetTile.tile.position
			);

			colony = new Colony(data.Name);

			MapM.Initialize(mapData, MapInitializeType.NewMap).Forget();
		}

		public void SetupNewColony() {
			ColonistM.SpawnStartColonists(3);
		}
	}
}
