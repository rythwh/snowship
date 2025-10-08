using System;

namespace Snowship.NMap
{
	public class MapProvider : IMapQuery, IMapWrite, IMapEvents
	{
		public Map Map { get; private set; }
		public MapData MapData { get; private set; }

		public event Action<Map> OnMapCreated;

		public void SetMap(Map map) {
			Map = map;
		}

		public void SetMapData(MapData mapData) {
			MapData = mapData;
		}

		public void InvokeOnMapCreated() {
			OnMapCreated?.Invoke(Map);
		}
	}

	public interface IMapEvents
	{
		event Action<Map> OnMapCreated;
		void InvokeOnMapCreated();
	}

	public interface IMapWrite
	{
		void SetMap(Map map);
		void SetMapData(MapData mapData);
	}

	public interface IMapQuery
	{
		Map Map { get; }
		MapData MapData { get; }
	}
}
