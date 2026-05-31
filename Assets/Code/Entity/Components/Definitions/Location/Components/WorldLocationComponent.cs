using Snowship.NMap.NTile;

namespace Snowship.NEntity
{
	public class WorldLocationComponent : ILocation
	{
		public Tile Tile { get; private set; }
		public int Rotation { get; private set; }

		public WorldLocationComponent(Tile tile, int rotation)
		{
			Tile = tile;
			Rotation = rotation;
		}

		public void Set(Tile tile, int rotation)
		{
			Tile = tile;
			Rotation = rotation;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
