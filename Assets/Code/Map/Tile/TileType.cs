using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NPersistence;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

public class TileType {

	public enum TypeEnum {
		DirtWater, GrassWater, ColdGrassWater, DryGrassWater, SandWater, SnowIce, StoneIce, StoneWater, ClayWater,
		DirtHole, SandHole, SnowHole, StoneHole,
		Dirt, Mud, DirtGrass, DirtThinGrass, DirtDryGrass, Grass, ThickGrass, ColdGrass, DryGrass, Sand, Snow, SnowStone, StoneThinGrass, StoneSand, StoneSnow, Clay,
		Andesite, Basalt, Diorite, Granite, Kimberlite, Obsidian, Rhyolite, Chalk, Claystone, Coal, Flint, Lignite, Limestone, Sandstone, Anthracite, Marble, Quartz, Slate,
		IronOre, GoldOre, SilverOre, BronzeOre, CopperOre
	};

	public enum ClassEnum {
		LiquidWater,
		Dirt,
		Plantable
	};

	public enum PropertyEnum {
		TileType,
		Type,
		Classes,
		WalkSpeed,
		Walkable,
		Buildable,
		Bitmasking,
		BlocksLight,
		ResourceRanges
	}

	public static readonly List<TileType> tileTypes = new List<TileType>();

	public readonly TileTypeGroup.TypeEnum groupType;

	public readonly TypeEnum type;
	public readonly string name;

	public readonly Dictionary<ClassEnum, bool> classes;

	public readonly float walkSpeed;

	public readonly bool walkable;
	public readonly bool buildable;

	public readonly bool bitmasking;

	public readonly bool blocksLight;

	public readonly List<ResourceRange> resourceRanges;

	public readonly List<Sprite> baseSprites = new List<Sprite>();
	public readonly List<Sprite> bitmaskSprites = new List<Sprite>();
	public readonly List<Sprite> riverSprites = new List<Sprite>();

	public TileType(TileTypeGroup.TypeEnum groupType, TypeEnum type, Dictionary<ClassEnum, bool> classes, float walkSpeed, bool walkable, bool buildable, bool bitmasking, bool blocksLight, List<ResourceRange> resourceRanges) {
		this.groupType = groupType;

		this.type = type;
		name = StringUtilities.SplitByCapitals(type.ToString());

		this.classes = classes;

		this.walkSpeed = walkSpeed;

		this.walkable = walkable;
		this.buildable = buildable;

		this.bitmasking = bitmasking;

		this.blocksLight = blocksLight;

		this.resourceRanges = resourceRanges;

		baseSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + type + "/" + type + "-base").ToList();
		bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + type + "/" + type + "-bitmask").ToList();

		if (classes[ClassEnum.LiquidWater]) {
			riverSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + type + "/" + type + "-river").ToList();
		}
	}

	public static void InitializeTileTypes() {
		List<KeyValuePair<string, object>> tileTypeGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/tile-types").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> tileTypeGroupProperty in tileTypeGroupProperties) {
			switch ((TileTypeGroup.PropertyEnum)Enum.Parse(typeof(TileTypeGroup.PropertyEnum), tileTypeGroupProperty.Key)) {
				case TileTypeGroup.PropertyEnum.TileTypeGroup:

					TileTypeGroup.TypeEnum? groupType = null;
					TypeEnum? defaultTileType = null;
					List<TileType> tileTypes = new List<TileType>();

					foreach (KeyValuePair<string, object> tileTypeGroupSubProperty in (List<KeyValuePair<string, object>>)tileTypeGroupProperty.Value) {
						switch ((TileTypeGroup.PropertyEnum)Enum.Parse(typeof(TileTypeGroup.PropertyEnum), tileTypeGroupSubProperty.Key)) {
							case TileTypeGroup.PropertyEnum.Type:
								groupType = (TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), (string)tileTypeGroupSubProperty.Value);
								break;
							case TileTypeGroup.PropertyEnum.DefaultTileType:
								defaultTileType = (TypeEnum)Enum.Parse(typeof(TypeEnum), (string)tileTypeGroupSubProperty.Value);
								break;
							case TileTypeGroup.PropertyEnum.TileTypes:
								foreach (KeyValuePair<string, object> tileTypeProperty in (List<KeyValuePair<string, object>>)tileTypeGroupSubProperty.Value) {
									switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), tileTypeProperty.Key)) {
										case PropertyEnum.TileType:

											TypeEnum? type = null;
											Dictionary<ClassEnum, bool> classes = new Dictionary<ClassEnum, bool>();
											float? walkSpeed = null;
											bool? walkable = null;
											bool? buildable = null;
											bool? bitmasking = null;
											bool? blocksLight = null;
											List<ResourceRange> resourceRanges = new();

											foreach (ClassEnum tileTypeClassEnum in Enum.GetValues(typeof(ClassEnum))) {
												classes.Add(tileTypeClassEnum, false);
											}

											foreach (KeyValuePair<string, object> tileTypeSubProperty in (List<KeyValuePair<string, object>>)tileTypeProperty.Value) {
												switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), tileTypeSubProperty.Key)) {
													case PropertyEnum.Type:
														type = (TypeEnum)Enum.Parse(typeof(TypeEnum), (string)tileTypeSubProperty.Value);
														break;
													case PropertyEnum.Classes:
														foreach (string tileTypeClassString in ((string)tileTypeSubProperty.Value).Split(',')) {
															classes[(ClassEnum)Enum.Parse(typeof(ClassEnum), tileTypeClassString)] = true;
														}
														break;
													case PropertyEnum.WalkSpeed:
														walkSpeed = float.Parse((string)tileTypeSubProperty.Value);
														break;
													case PropertyEnum.Walkable:
														walkable = bool.Parse((string)tileTypeSubProperty.Value);
														break;
													case PropertyEnum.Buildable:
														buildable = bool.Parse((string)tileTypeSubProperty.Value);
														break;
													case PropertyEnum.Bitmasking:
														bitmasking = bool.Parse((string)tileTypeSubProperty.Value);
														break;
													case PropertyEnum.BlocksLight:
														blocksLight = bool.Parse((string)tileTypeSubProperty.Value);
														break;
													case PropertyEnum.ResourceRanges:
														foreach (string resourceRangeString in ((string)tileTypeSubProperty.Value).Split(',')) {
															Resource resource = Resource.GetResourceByEnum((EResource)Enum.Parse(typeof(EResource), resourceRangeString.Split(':')[0]));
															int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
															int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
															resourceRanges.Add(new ResourceRange(resource, min, max));
														}
														break;
													default:
														Debug.LogError("Unknown tile type sub property: " + tileTypeSubProperty.Key + " " + tileTypeSubProperty.Value);
														break;
												}
											}

											TileType tileType = new TileType(
												groupType.Value,
												type.Value,
												classes,
												walkSpeed.Value,
												walkable.Value,
												buildable.Value,
												bitmasking.Value,
												blocksLight.Value,
												resourceRanges
											);
											tileTypes.Add(tileType);
											TileType.tileTypes.Add(tileType);

											break;
										default:
											Debug.LogError("Unknown tile type property: " + tileTypeProperty.Key + " " + tileTypeProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown tile type group sub property: " + tileTypeGroupSubProperty.Key + " " + tileTypeGroupSubProperty.Value);
								break;
						}

					}

					TileTypeGroup tileTypeGroup = new TileTypeGroup(
						groupType.Value,
						defaultTileType.Value,
						tileTypes
					);
					TileTypeGroup.tileTypeGroups.Add(tileTypeGroup);

					break;
				default:
					Debug.LogError("Unknown tile type group property: " + tileTypeGroupProperty.Key + " " + tileTypeGroupProperty.Value);
					break;

			}
		}
	}

	public static TileType GetTileTypeByString(string tileTypeString) {
		return GetTileTypeByEnum((TypeEnum)Enum.Parse(typeof(TypeEnum), tileTypeString));
	}

	public static TileType GetTileTypeByEnum(TypeEnum tileTypeEnum) {
		return tileTypes.Find(tileType => tileType.type == tileTypeEnum);
	}

	public static List<TileType> GetTileTypesWithClass(ClassEnum tileTypeClassEnum) {
		return tileTypes.Where(tileType => tileType.classes[tileTypeClassEnum]).ToList();
	}
}