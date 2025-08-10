using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.Tile;

namespace Snowship.NResource
{
	public class Container : ObjectInstance, IInventory
	{
		public static readonly List<Container> containers = new();

		public Inventory Inventory { get; private set; }

		public Container(ObjectPrefab prefab, Variation variation, Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {
			Inventory = new Inventory(this, prefab.maxInventoryWeight, prefab.maxInventoryVolume);
		}

		public static List<Container> GetContainersInRegion(Region region) {
			return containers.Where(c => c.tile.region == region).ToList();
		}

		public static Container GetContainerOrChildOnTile(Tile tile) {
			Container container = null;
			if (container == null) {
				container = containers.Find(c => c.tile == tile);
			}
			if (container == null) {
				container = TradingPost.tradingPosts.Find(tp => tp.tile == tile);
			}
			return container;
		}
	}
}