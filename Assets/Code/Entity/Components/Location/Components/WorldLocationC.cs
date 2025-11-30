using Snowship.NMap.NTile;

namespace Snowship.NEntity
{
	public class WorldLocationC : ILocation
	{
		public Tile Tile { get; private set; }
		public int Rotation { get; private set; }

		public WorldLocationC(Tile tile, int rotation)
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
