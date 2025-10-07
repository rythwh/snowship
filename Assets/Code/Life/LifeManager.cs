using System.Collections.Generic;
using Snowship.NMap;
using UnityEngine;

namespace Snowship.NLife
{
	public class LifeManager : Manager
	{
		public readonly Dictionary<Life, LifeView<Life>> Life = new();

		private MapManager MapM => GameManager.Get<MapManager>();

		public override void OnGameSetupComplete() {
			base.OnGameSetupComplete();

			MapM.OnMapCreated += OnMapCreated;
		}

		private void OnMapCreated(Map map) {
			MapM.Map.LightingUpdated += OnMapLightingUpdated;
		}

		private void OnMapLightingUpdated(Color _) {
			foreach (LifeView<Life> lifeView in Life.Values) {
				lifeView.ApplyTileColour();
			}
		}
	}
}
