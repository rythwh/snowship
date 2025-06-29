﻿using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NPlanet {
	class PlanetMapDataValues {
		public const bool ActualMap = false;
		public const bool PlanetTemperature = true;
		public const float AverageTemperature = -1;
		public const float AveragePrecipitation = -1;
		public readonly Dictionary<TileTypeGroup.TypeEnum, float> terrainTypeHeights = new Dictionary<TileTypeGroup.TypeEnum, float> {
			{ TileTypeGroup.TypeEnum.Water, 0.40f },
			{ TileTypeGroup.TypeEnum.Stone, 0.75f }
		};
		public readonly List<int> surroundingPlanetTileHeightDirections = null;
		public const bool River = false;
		public readonly List<int> surroundingPlanetTileRivers = null;
		public const bool PreventEdgeTouching = true;
		public readonly Vector2 planetTilePosition = Vector2.zero;
	}
}
