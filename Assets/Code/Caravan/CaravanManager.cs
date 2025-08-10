using Snowship.NTime;
using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Tile;
using Cysharp.Threading.Tasks;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NUI;
using UnityEngine;

namespace Snowship.NCaravan {

	public class CaravanManager : IManager, IDisposable {

		public List<Caravan> caravans = new List<Caravan>();
		private readonly List<Caravan> removeCaravans = new List<Caravan>();

		public int caravanTimer = 0; // The time since the last caravan visited
		private const int CaravanTimeMin = SimulationDateTime.DayLengthSeconds * 7; // Caravans will only come once every caravanTimeMin in-game minutes
		private const int CaravanTimeMax = SimulationDateTime.DayLengthSeconds * 27; // Caravans will definitely come if the caravanTimer is greater than caravanTimeMax

		public Caravan selectedCaravan;

		public void OnCreate() {
			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
		}

		public void Dispose() {
			GameManager.Get<TimeManager>().OnTimeChanged -= OnTimeChanged;
		}

		public void OnUpdate() {
			foreach (Caravan caravan in caravans) {
				if (!caravan.Update()) {
					removeCaravans.Add(caravan);
				}
			}

			foreach (Caravan caravan in removeCaravans) {
				if (selectedCaravan == caravan) {
					SetSelectedCaravan(null);
				}

				caravans.Remove(caravan);

				// GameManager.Get<UIManagerOld>().SetCaravanElements();
			}

			removeCaravans.Clear();

			if (selectedCaravan != null && Input.GetMouseButtonDown(1)) {
				SetSelectedCaravan(null);
			}
		}

		private void OnTimeChanged(SimulationDateTime simulationDateTime) {
			UpdateCaravanTimer();
		}

		private void UpdateCaravanTimer() {

			caravanTimer += 1;

			if (caravanTimer <= CaravanTimeMin) {
				return;
			}

			if (UnityEngine.Random.Range(0f, 1f) < (((float)caravanTimer - CaravanTimeMin) / (CaravanTimeMax - CaravanTimeMin))) {
				caravanTimer = 0;
				SpawnCaravan(CaravanType.Foot, 4);
			}
		}

		public void SpawnCaravan(CaravanType caravanType, int maxNumTraders) {

			List<Tile> validSpawnTiles = new List<Tile>();

			switch (caravanType) {
				case CaravanType.Foot or CaravanType.Wagon:
					validSpawnTiles.AddRange(GameManager.Get<ColonyManager>().colony.map.edgeTiles.Where(tile => tile.walkable && !tile.tileType.classes[TileType.ClassEnum.LiquidWater]).ToList());
					break;
				case CaravanType.Boat:
					// TODO Implement boat caravans
					break;
			}

			if (validSpawnTiles.Count <= 0) {
				return;
			}

			Tile targetSpawnTile = validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)];

			List<Tile> edgeTilesConnectedToTargetSpawnTile = new List<Tile>();

			List<Tile> spawnTilesFrontier = new List<Tile>() { targetSpawnTile };
			List<Tile> spawnTilesChecked = new List<Tile>();
			while (spawnTilesFrontier.Count > 0) {
				Tile currentTile = spawnTilesFrontier[0];
				spawnTilesFrontier.RemoveAt(0);
				spawnTilesChecked.Add(currentTile);
				edgeTilesConnectedToTargetSpawnTile.Add(currentTile);
				foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
					if (!spawnTilesChecked.Contains(nTile) && validSpawnTiles.Contains(nTile)) {
						spawnTilesFrontier.Add(nTile);
					}
				}
			}

			if (edgeTilesConnectedToTargetSpawnTile.Count <= 0) {
				return;
			}

			int numTraders = UnityEngine.Random.Range(1, maxNumTraders + 1);
			List<Tile> edgeTilesCloseToTargetSpawnTile = edgeTilesConnectedToTargetSpawnTile.Where(tile => Vector2.Distance(tile.obj.transform.position, targetSpawnTile.obj.transform.position) <= numTraders * 2).ToList();

			if (edgeTilesCloseToTargetSpawnTile.Count <= 0) {
				return;
			}

			if (edgeTilesCloseToTargetSpawnTile.Count < numTraders) {
				numTraders = edgeTilesCloseToTargetSpawnTile.Count;
			}

			List<ObjectInstance> tradingPosts = new();
			foreach (ObjectPrefab prefab in ObjectInstance.ObjectInstances.Keys.Where(op => op.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.TradingPosts)) {
				foreach (ObjectInstance tradingPost in ObjectInstance.ObjectInstances[prefab]) {
					if (tradingPost.zeroPointTile.region == targetSpawnTile.region) {
						tradingPosts.Add(tradingPost);
					}
				}
			}

			ObjectInstance selectedTradingPost = null;
			if (tradingPosts.Count > 0) {
				selectedTradingPost = tradingPosts[UnityEngine.Random.Range(0, tradingPosts.Count)];
			}

			Tile targetTile = null;
			if (selectedTradingPost != null) {
				targetTile = selectedTradingPost.zeroPointTile;
			}
			else {
				List<Tile> validTargetTiles = targetSpawnTile.region.tiles.Where(tile => tile.walkable && !tile.tileType.classes[TileType.ClassEnum.LiquidWater]).ToList();
				targetTile = validTargetTiles[UnityEngine.Random.Range(0, validTargetTiles.Count)];
			}

			AddCaravan(new Caravan(numTraders, CaravanType.Foot, edgeTilesCloseToTargetSpawnTile, targetTile));
		}

		public void AddCaravan(Caravan caravan) {
			caravans.Add(caravan);

			// GameManager.Get<UIManagerOld>().SetCaravanElements();
		}

		public void SetSelectedCaravan(Caravan selectedCaravan) {
			this.selectedCaravan = selectedCaravan;

			GameManager.Get<UIManager>().OpenViewAsync<UITradeMenu>().Forget();
		}
	}
}