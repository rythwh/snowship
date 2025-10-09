using System.Collections.Generic;
using JetBrains.Annotations;
using Snowship.NMap;
using UnityEngine;
using VContainer.Unity;

namespace Snowship.NLife
{
	[UsedImplicitly]
	public class LifeManager : IPostStartable
	{
		private readonly IMapQuery mapQuery;
		private readonly IMapEvents mapEvents;

		public readonly Dictionary<Life, LifeView<Life>> Life = new();

		public LifeManager(IMapQuery mapQuery, IMapEvents mapEvents) {
			this.mapQuery = mapQuery;
			this.mapEvents = mapEvents;
		}

		public void PostStart() {
			mapEvents.OnMapCreated += OnMapCreated;
		}

		private void OnMapCreated(Map map) {
			mapQuery.Map.LightingUpdated += OnMapLightingUpdated;
		}

		private void OnMapLightingUpdated(Color _) {
			foreach (LifeView<Life> lifeView in Life.Values) {
				lifeView.ApplyTileColour();
			}
		}
	}
}
