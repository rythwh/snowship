using Snowship.NMap.NTile;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		private void SetRoofs(MapGenContext context) {
			foreach (Tile tile in context.Map.tiles) {
				tile.SetRoof(tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone && tile.height >= context.Data.roofHeightThreshold);
			}
		}
	}
}
