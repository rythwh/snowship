using JetBrains.Annotations;
using Snowship.NMap;
using Snowship.NColonist;
using Snowship.NPlanet;

namespace Snowship.NColony
{
	[UsedImplicitly]
	public class ColonyManager
	{
		private readonly IColonyEvents colonyEvents;
		private readonly IMapWrite mapWrite;
		private readonly ColonistManager colonistM;
		private readonly PlanetManager planetM;

		public Colony colony;

		public ColonyManager(
			IColonyEvents colonyEvents,
			IMapWrite mapWrite,
			IMapEvents mapEvents,
			ColonistManager colonistM,
			PlanetManager planetM
		) {
			this.colonyEvents = colonyEvents;
			this.mapWrite = mapWrite;
			this.colonistM = colonistM;
			this.planetM = planetM;

			mapEvents.OnMapCreated += SetupNewColony;
		}

		public void CreateColony(CreateColonyData data) {

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
				planetM.planet.MapData.primaryWindDirection,
				data.PlanetTile.tile.PositionGrid
			);

			colony = new Colony(data.Name);
			mapWrite.SetMapData(mapData);
			colonyEvents.InvokeOnColonyCreated(colony);
		}

		public void SetupNewColony(Map _) {
			colonistM.SpawnStartColonists(3);
		}
	}
}
