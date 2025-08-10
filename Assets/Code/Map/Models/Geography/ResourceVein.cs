using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Tile;
using Snowship.NPersistence;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NMap.Models.Geography
{
	public class ResourceVein {

		public enum GroupEnum {
			Stone, Coast
		}

		public enum GroupPropertyEnum {
			VeinGroup,
			Type,
			Veins
		}

		public enum PropertyEnum {
			Vein,
			ResourceType,
			TileTypes,
			NumVeinsByMapSize,
			VeinDistance,
			VeinSize,
			VeinSizeRange
		}

		public static readonly List<ResourceVein> resourceVeins = new List<ResourceVein>();

		public EResource resourceType;
		public GroupEnum groupType;
		public Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum> tileTypes;
		public int numVeinsByMapSize = 0;
		public int veinDistance = 0;
		public int veinSize = 0;
		public int veinSizeRange = 0;

		public ResourceVein(
			EResource resourceType,
			GroupEnum groupType,
			Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum> tileTypes,
			int numVeinsByMapSize,
			int veinDistance,
			int veinSize,
			int veinSizeRange
		) {
			this.resourceType = resourceType;
			this.groupType = groupType;
			this.tileTypes = tileTypes;
			this.numVeinsByMapSize = numVeinsByMapSize;
			this.veinDistance = veinDistance;
			this.veinSize = veinSize;
			this.veinSizeRange = veinSizeRange;
		}

		public static readonly Dictionary<EResource, Func<Tile.Tile, bool>> resourceVeinValidTileFunctions = new Dictionary<EResource, Func<Tile.Tile, bool>>() {
			{
				EResource.Clay, delegate(Tile.Tile tile) {
					if (((tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && tile.horizontalSurroundingTiles.Find(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) != null) || (tile.tileType.groupType != TileTypeGroup.TypeEnum.Water)) && (tile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
						if (tile.temperature >= -30) {
							return true;
						}
					}
					return false;
				}
			}, {
				EResource.Coal, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				}
			}, {
				EResource.GoldOre, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				}
			}, {
				EResource.SilverOre, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				}
			}, {
				EResource.BronzeOre, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				}
			}, {
				EResource.IronOre, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				}
			}, {
				EResource.CopperOre, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				}
			}, {
				EResource.Chalk, delegate(Tile.Tile tile) {
					if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						return true;
					}
					return false;
				} }
		};

		public static void InitializeResourceVeins() {
			List<KeyValuePair<string, object>> resourceVeinGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/resource-veins").text.Split('\n').ToList());
			foreach (KeyValuePair<string, object> resourceVeinGroupProperty in resourceVeinGroupProperties) {
				switch ((GroupPropertyEnum)Enum.Parse(typeof(GroupPropertyEnum), resourceVeinGroupProperty.Key)) {
					case GroupPropertyEnum.VeinGroup:

						GroupEnum? groupType = null;

						foreach (KeyValuePair<string, object> resourceVeinGroupSubProperty in (List<KeyValuePair<string, object>>)resourceVeinGroupProperty.Value) {
							switch ((GroupPropertyEnum)Enum.Parse(typeof(GroupPropertyEnum), resourceVeinGroupSubProperty.Key)) {
								case GroupPropertyEnum.Type:
									groupType = (GroupEnum)Enum.Parse(typeof(GroupEnum), (string)resourceVeinGroupSubProperty.Value);
									break;
								case GroupPropertyEnum.Veins:
									foreach (KeyValuePair<string, object> resourceVeinProperty in (List<KeyValuePair<string, object>>)resourceVeinGroupSubProperty.Value) {
										switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), resourceVeinProperty.Key)) {
											case PropertyEnum.Vein:

												EResource? resourceType = null;
												Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum> tileTypes = new Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum>();
												int? numVeinsByMapSize = null;
												int? veinDistance = null;
												int? veinSize = null;
												int? veinSizeRange = null;

												foreach (KeyValuePair<string, object> resourceVeinSubProperty in (List<KeyValuePair<string, object>>)resourceVeinProperty.Value) {
													switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), resourceVeinSubProperty.Key)) {
														case PropertyEnum.ResourceType:
															resourceType = (EResource)Enum.Parse(typeof(EResource), (string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.TileTypes:
															foreach (string tileTypesString in ((string)resourceVeinSubProperty.Value).Split(',')) {
																TileTypeGroup.TypeEnum tileTypeGroup = (TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), tileTypesString.Split(':')[0]);
																TileType.TypeEnum tileType = (TileType.TypeEnum)Enum.Parse(typeof(TileType.TypeEnum), tileTypesString.Split(':')[1]);
																tileTypes.Add(tileTypeGroup, tileType);
															}
															break;
														case PropertyEnum.NumVeinsByMapSize:
															numVeinsByMapSize = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.VeinDistance:
															veinDistance = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.VeinSize:
															veinSize = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.VeinSizeRange:
															veinSizeRange = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown resource vein sub property: " + resourceVeinSubProperty.Key + " " + resourceVeinSubProperty.Value);
															break;
													}
												}

												ResourceVein resourceVein = new ResourceVein(
													resourceType.Value,
													groupType.Value,
													tileTypes,
													numVeinsByMapSize.Value,
													veinDistance.Value,
													veinSize.Value,
													veinSizeRange.Value
												);
												resourceVeins.Add(resourceVein);

												break;
											default:
												Debug.LogError("Unknown resource vein property: " + resourceVeinProperty.Key + " " + resourceVeinProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown resource vein group sub property: " + resourceVeinGroupSubProperty.Key + " " + resourceVeinGroupSubProperty.Value);
									break;
							}
						}
						break;
					default:
						Debug.LogError("Unknown resource vein group property: " + resourceVeinGroupProperty.Key + " " + resourceVeinGroupProperty.Value);
						break;

				}
			}
		}

		public static List<ResourceVein> GetResourceVeinsByGroup(GroupEnum resourceVeinGroupEnum) {
			return resourceVeins.Where(resourceVein => resourceVein.groupType == resourceVeinGroupEnum).ToList();
		}
	}
}
