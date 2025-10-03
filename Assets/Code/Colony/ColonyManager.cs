using Snowship.NMap;
using Cysharp.Threading.Tasks;
using Snowship.NColonist;
using Snowship.NPlanet;

namespace Snowship.NColony {
	public class ColonyManager : Manager {

		public Colony colony;

		private MapManager MapM => GameManager.Get<MapManager>();
		private ColonistManager ColonistM => GameManager.Get<ColonistManager>();
		private PlanetManager PlanetM => GameManager.Get<PlanetManager>();

		public async UniTask CreateColony(CreateColonyData data) {

			MapData mapData = new MapData(
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
				PlanetM.planet.MapData.primaryWindDirection,
				data.PlanetTile.tile.PositionGrid
			);

			colony = new Colony(data.Name);

			await MapM.CreateMap(mapData, MapInitializeType.NewMap);
		}

		public void SetupNewColony() {
			ColonistM.SpawnStartColonists(3);
		}
	}
}
