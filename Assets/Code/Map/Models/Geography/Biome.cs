using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;
using Snowship.NPersistence;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NMap.Models.Geography
{
	public class Biome {

		public enum PropertyEnum {
			Biome, Type, TileTypes, PlantChances, Ranges, Colour
		}

		public enum RangePropertyEnum {
			Range, Precipitation, Temperature
		}

		public enum TypeEnum {
			IceCap,
			Tundra, WetTundra, PolarWetlands,
			Steppe, BorealForest,
			TemperateWoodlands, TemperateForest, TemperateWetForest, TemperateWetlands,
			Mediterranean,
			SubtropicalScrub, SubtropicalWoodlands, SubtropicalDryForest, SubtropicalForest, SubtropicalWetForest, SubtropicalWetlands,
			TropicalScrub, TropicalWoodlands, TropicalDryForest, TropicalForest, TropicalWetForest, TropicalWetlands,
			PolarDesert, CoolDesert, TemperateDesert, Desert, ExtremeDesert
		};

		public static readonly List<Biome> biomes = new List<Biome>();

		public readonly TypeEnum type;
		public readonly string name;

		public readonly Dictionary<TileTypeGroup.TypeEnum, TileType> tileTypes;

		public readonly Dictionary<Plant.PlantEnum, float> plantChances;

		public readonly List<Range> ranges;

		public readonly Color colour;

		public Biome(
			TypeEnum type,
			Dictionary<TileTypeGroup.TypeEnum, TileType> tileTypes,
			Dictionary<Plant.PlantEnum, float> plantChances,
			List<Range> ranges,
			Color colour
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.tileTypes = tileTypes;

			this.plantChances = plantChances;

			this.ranges = ranges;

			this.colour = colour;
		}

		public static void InitializeBiomes() {
			List<KeyValuePair<string, object>> biomeProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/biomes").text.Split('\n').ToList());
			foreach (KeyValuePair<string, object> biomeProperty in biomeProperties) {
				switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), biomeProperty.Key)) {
					case PropertyEnum.Biome:

						TypeEnum? type = null;
						Dictionary<TileTypeGroup.TypeEnum, TileType> tileTypes = new Dictionary<TileTypeGroup.TypeEnum, TileType>();
						Dictionary<Plant.PlantEnum, float> plantChances = new();
						List<Range> ranges = new List<Range>();
						Color? colour = null;

						foreach (KeyValuePair<string, object> biomeSubProperty in (List<KeyValuePair<string, object>>)biomeProperty.Value) {
							switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), biomeSubProperty.Key)) {
								case PropertyEnum.Type:
									type = (TypeEnum)Enum.Parse(typeof(TypeEnum), (string)biomeSubProperty.Value);
									break;
								case PropertyEnum.TileTypes:
									foreach (KeyValuePair<string, object> tileTypeProperty in (List<KeyValuePair<string, object>>)biomeSubProperty.Value) {
										tileTypes.Add(
											(TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), tileTypeProperty.Key),
											TileType.GetTileTypeByString((string)tileTypeProperty.Value)
										);
									}
									break;
								case PropertyEnum.PlantChances:
									foreach (KeyValuePair<string, object> plantChanceProperty in (List<KeyValuePair<string, object>>)biomeSubProperty.Value) {
										plantChances.Add(
											(Plant.PlantEnum)Enum.Parse(typeof(Plant.PlantEnum), plantChanceProperty.Key),
											float.Parse((string)plantChanceProperty.Value)
										);
									}
									break;
								case PropertyEnum.Ranges:
									foreach (KeyValuePair<string, object> rangeProperty in (List<KeyValuePair<string, object>>)biomeSubProperty.Value) {
										switch ((RangePropertyEnum)Enum.Parse(typeof(RangePropertyEnum), rangeProperty.Key)) {
											case RangePropertyEnum.Range:

												float? pMin = null;
												float? pMax = null;

												float? tMin = null;
												float? tMax = null;

												foreach (KeyValuePair<string, object> rangeSubProperty in (List<KeyValuePair<string, object>>)rangeProperty.Value) {
													switch ((RangePropertyEnum)Enum.Parse(typeof(RangePropertyEnum), rangeSubProperty.Key)) {
														case RangePropertyEnum.Precipitation:

															string[] pString = ((string)rangeSubProperty.Value).Split(',');

															if (pString[0] == "-Inf") {
																pMin = float.MinValue;
															} else {
																pMin = float.Parse(pString[0]);
															}

															if (pString[1] == "Inf") {
																pMax = float.MaxValue;
															} else {
																pMax = float.Parse(pString[1]);
															}

															break;
														case RangePropertyEnum.Temperature:
															string[] tString = ((string)rangeSubProperty.Value).Split(',');

															if (tString[0] == "-Inf") {
																tMin = float.MinValue;
															} else {
																tMin = float.Parse(tString[0]);
															}

															if (tString[1] == "Inf") {
																tMax = float.MaxValue;
															} else {
																tMax = float.Parse(tString[1]);
															}

															break;
														default:
															Debug.LogError("Unknown range sub property: " + rangeSubProperty.Key + " " + rangeSubProperty.Value);
															break;
													}
												}

												ranges.Add(
													new Range(
														pMin.Value,
														pMax.Value,
														tMin.Value,
														tMax.Value
													)
												);

												break;
											default:
												Debug.LogError("Unknown range property: " + rangeProperty.Key + " " + rangeProperty.Value);
												break;
										}
									}
									break;
								case PropertyEnum.Colour:
									colour = ColourUtilities.HexToColor((string)biomeSubProperty.Value);
									break;
								default:
									Debug.LogError("Unknown biome sub property: " + biomeSubProperty.Key + " " + biomeSubProperty.Value);
									break;
							}

						}

						Biome biome = new Biome(
							type.Value,
							tileTypes,
							plantChances,
							ranges,
							colour.Value
						);
						biomes.Add(biome);

						break;
					default:
						Debug.LogError("Unknown biome property: " + biomeProperty.Key + " " + biomeProperty.Value);
						break;
				}
			}
		}

		public static Biome GetBiomeByString(string biomeTypeString) {
			return GetBiomeByEnum((TypeEnum)Enum.Parse(typeof(TypeEnum), biomeTypeString));
		}

		public static Biome GetBiomeByEnum(TypeEnum biomeTypeEnum) {
			return biomes.Find(biome => biome.type == biomeTypeEnum);
		}

		public class Range {

			public readonly float pMin;
			public readonly float pMax;

			public readonly float tMin;
			public readonly float tMax;

			public Range(float pMin, float pMax, float tMin, float tMax) {
				this.pMin = pMin;
				this.pMax = pMax;

				this.tMin = tMin;
				this.tMax = tMax;
			}

			public bool IsInRange(float precipitation, float temperature) {
				if (pMin <= precipitation && pMax > precipitation) {
					if (tMin <= temperature && tMax > temperature) {
						return true;
					}
				}
				return false;
			}
		}
	}
}
