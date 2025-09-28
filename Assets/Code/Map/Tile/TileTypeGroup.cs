using System;
using System.Collections.Generic;
using Snowship.NUtilities;

namespace Snowship.NMap.NTile
{
	public class TileTypeGroup {

		public enum TypeEnum {
			Water,
			Hole,
			Ground,
			Stone
		}

		public enum PropertyEnum {
			TileTypeGroup,
			Type,
			DefaultTileType,
			TileTypes
		}

		public static readonly List<TileTypeGroup> tileTypeGroups = new List<TileTypeGroup>();

		public readonly TypeEnum type;
		public readonly string name;

		public readonly TileType.TypeEnum defaultTileType;

		public readonly List<TileType> tileTypes;

		public TileTypeGroup(TypeEnum type, TileType.TypeEnum defaultTileType, List<TileType> tileTypes) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.defaultTileType = defaultTileType;

			this.tileTypes = tileTypes;
		}

		public static TileTypeGroup GetTileTypeGroupByString(string tileTypeGroupString) {
			return GetTileTypeGroupByEnum((TypeEnum)Enum.Parse(typeof(TypeEnum), tileTypeGroupString));
		}

		public static TileTypeGroup GetTileTypeGroupByEnum(TypeEnum tileTypeGroupEnum) {
			return tileTypeGroups.Find(tileTypeGroup => tileTypeGroup.type == tileTypeGroupEnum);
		}
	}
}
