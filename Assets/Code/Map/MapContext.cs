using UnityEngine;

namespace Snowship.NMap
{
	public class MapContext
	{
		public GameObject TilePrefab { get; }

		public MapContext(GameObject tilePrefab) {
			TilePrefab = tilePrefab;
		}
	}
}
