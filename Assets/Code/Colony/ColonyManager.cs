using Snowship.NMap;
using Snowship.NMap.Tile;
using Cysharp.Threading.Tasks;
using Snowship.NColonist;
using Snowship.NPersistence;
using Snowship.NPlanet;
using UnityEngine;

namespace Snowship.NColony {
	public class ColonyManager : IManager {

		private readonly PColony pColony = new PColony();

		private MapManager MapM => GameManager.Get<MapManager>();
		private ColonistManager ColonistM => GameManager.Get<ColonistManager>();
		private PersistenceManager PersistenceM => GameManager.Get<PersistenceManager>();
		private PlanetManager PlanetM => GameManager.Get<PlanetManager>();

		public void SetupNewColony(Colony colony, bool initialized) {
			if (!initialized) {
				this.colony = colony;

				MapM.Initialize(colony.mapData, MapInitializeType.NewMap).Forget();
			} else {
				ColonistM.SpawnStartColonists(3);

				pColony.CreateColony(colony);
				PersistenceM.CreateSave(colony).Forget();
			}
		}

		public Colony CreateColony(CreateColonyData createColonyData) {
			Colony colony = new Colony(
				createColonyData.Name,
				new MapData(
					PlanetM.planet.mapData,
					createColonyData.Seed,
					createColonyData.Size,
					true,
					false,
					0,
					0,
					createColonyData.PlanetTile.tile.temperature,
					createColonyData.PlanetTile.tile.GetPrecipitation(),
					createColonyData.PlanetTile.terrainTypeHeights,
					createColonyData.PlanetTile.surroundingPlanetTileHeightDirections,
					createColonyData.PlanetTile.isRiver,
					createColonyData.PlanetTile.surroundingPlanetTileRivers,
					false,
					PlanetM.planet.primaryWindDirection,
					createColonyData.PlanetTile.tile.position
				)
			);
			return colony;
		}

		public void LoadColony(Colony colony, bool initialized) {
			if (!initialized) {
				this.colony = colony;

				Random.InitState(colony.mapData.mapSeed);

				//TileM.Initialize(colony, TileManager.MapInitializeType.LoadMap);
			}
		}

		public Colony colony;

		public void SetColony(Colony colony) {
			if (this.colony != null && this.colony.map != null) {
				foreach (Tile tile in this.colony.map.tiles) {
					MonoBehaviour.Destroy(tile.obj);
				}
			}

			this.colony = colony;
		}
	}
}
