using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaravanManager : BaseManager {

	public override void Update() {
		foreach (Trader trader in traders) {
			trader.Update();
		}
	}

	public List<Trader> traders = new List<Trader>(); // TODO Remove Caravan from map and delete its traders from this list

	public class Trader : HumanManager.Human {

		public Caravan caravan;

		public Trader(TileManager.Tile spawnTile, float startingHealth, Caravan caravan) : base(spawnTile, startingHealth) {
			this.caravan = caravan;

			SetNameColour(UIManager.GetColour(UIManager.Colours.LightPurple));
			obj.transform.SetParent(GameObject.Find("TraderParent").transform, false);
			MoveToTile(GameManager.colonistM.colonists[0].overTile, false);

			GameManager.caravanM.traders.Add(this);
		}
	}
	private float traderTimer = 0; // The time since the last trader visited
	private static readonly int traderTimeMin = 1440; // Traders will only come once every traderTimeMin in-game minutes
	private static readonly int traderTimeMax = 10080; // Traders will definitely come if the traderTimer is greater than traderTimeMax

	public void UpdateTraders() {
		traderTimer += GameManager.timeM.deltaTime;
		if (traderTimer > traderTimeMin && GameManager.timeM.minuteChanged) {
			if (UnityEngine.Random.Range(0f, 1f) < ((traderTimer - traderTimeMin) / (traderTimeMax - traderTimeMin))) {
				traderTimer = 0;
				SpawnCaravan(CaravanTypeEnum.Foot, 4);
			}
		}
	}

	public List<Caravan> caravans = new List<Caravan>();

	public void SpawnCaravan(CaravanTypeEnum caravanType, int maxNumTraders) {
		List<TileManager.Tile> validSpawnTiles = null;
		if (caravanType == CaravanTypeEnum.Foot || caravanType == CaravanTypeEnum.Wagon) {
			validSpawnTiles = GameManager.colonyM.colony.map.edgeTiles.Where(tile => tile.walkable && !TileManager.liquidWaterEquivalentTileTypes.Contains(tile.tileType.type)).ToList();
		} else if (caravanType == CaravanTypeEnum.Boat) {
			validSpawnTiles = new List<TileManager.Tile>(); // TODO Implement boat caravans
		}
		if (validSpawnTiles.Count > 0) {
			TileManager.Tile targetSpawnTile = validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)];

			List<TileManager.Tile> edgeTilesConnectedToTargetSpawnTile = new List<TileManager.Tile>();

			List<TileManager.Tile> spawnTilesFrontier = new List<TileManager.Tile>() { targetSpawnTile };
			List<TileManager.Tile> spawnTilesChecked = new List<TileManager.Tile>();
			while (spawnTilesFrontier.Count > 0) {
				TileManager.Tile currentTile = spawnTilesFrontier[0];
				spawnTilesFrontier.RemoveAt(0);
				spawnTilesChecked.Add(currentTile);
				edgeTilesConnectedToTargetSpawnTile.Add(currentTile);
				foreach (TileManager.Tile nTile in currentTile.horizontalSurroundingTiles) {
					if (!spawnTilesChecked.Contains(nTile) && validSpawnTiles.Contains(nTile)) {
						spawnTilesFrontier.Add(nTile);
					}
				}
			}

			if (edgeTilesConnectedToTargetSpawnTile.Count > 0) {
				int numTraders = UnityEngine.Random.Range(1, maxNumTraders + 1);
				List<TileManager.Tile> edgeTilesCloseToTargetSpawnTile = edgeTilesConnectedToTargetSpawnTile.Where(tile => Vector2.Distance(tile.obj.transform.position, targetSpawnTile.obj.transform.position) <= numTraders * 2).ToList();
				if (edgeTilesCloseToTargetSpawnTile.Count > 0) {
					if (edgeTilesCloseToTargetSpawnTile.Count < numTraders) {
						numTraders = edgeTilesCloseToTargetSpawnTile.Count;
					}
					Caravan caravan = new Caravan(numTraders, CaravanTypeEnum.Foot, edgeTilesCloseToTargetSpawnTile);
					caravans.Add(caravan);

					GameManager.uiM.SetCaravanElements();
				}
			}
		}
	}

	public enum CaravanTypeEnum { Foot, Wagon, Boat };

	public class Caravan {

		public List<Trader> traders = new List<Trader>();
		public int amount;
		public CaravanTypeEnum caravanType;

		public string originLocation; // This should be changed to a "Location" object (to be created in TileManager?) that stores info

		public ResourceManager.Inventory inventory;

		public List<ResourceManager.TradeResourceAmount> resourcesToTrade = new List<ResourceManager.TradeResourceAmount>();

		public List<ResourceManager.ConfirmedTradeResourceAmount> confirmedResourcesToTrade = new List<ResourceManager.ConfirmedTradeResourceAmount>();

		public Caravan(int amount, CaravanTypeEnum caravanType, List<TileManager.Tile> spawnTiles) {
			this.amount = amount;
			this.caravanType = caravanType;
			for (int i = 0; i < amount && spawnTiles.Count > 0; i++) {
				TileManager.Tile spawnTile = spawnTiles[UnityEngine.Random.Range(0, spawnTiles.Count)];
				SpawnTrader(spawnTile);
				spawnTiles.Remove(spawnTile);
			}

			inventory = new ResourceManager.Inventory(null, null, int.MaxValue);

			foreach (ResourceManager.Resource resource in GameManager.resourceM.resources) {
				if (UnityEngine.Random.Range(0, 100) < 50) {
					inventory.ChangeResourceAmount(resource, UnityEngine.Random.Range(0, 50));
				}
			}
		}

		public void SpawnTrader(TileManager.Tile spawnTile) {
			traders.Add(new Trader(spawnTile, 1f, this));
		}

		public List<ResourceManager.TradeResourceAmount> GetTradeResourceAmounts() {
			List<ResourceManager.TradeResourceAmount> tradeResourceAmounts = new List<ResourceManager.TradeResourceAmount>();
			tradeResourceAmounts.AddRange(resourcesToTrade);

			List<ResourceManager.ResourceAmount> caravanResourceAmounts = inventory.resources;
			List<ResourceManager.ResourceAmount> colonyResourceAmounts = GameManager.resourceM.GetFilteredResources(true, false, true, false);

			foreach (ResourceManager.ResourceAmount resourceAmount in caravanResourceAmounts) {
				ResourceManager.TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new ResourceManager.TradeResourceAmount(resourceAmount.resource, resourceAmount.amount, DeterminePriceForResource(resourceAmount.resource), this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			foreach (ResourceManager.ResourceAmount resourceAmount in colonyResourceAmounts) {
				ResourceManager.TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new ResourceManager.TradeResourceAmount(resourceAmount.resource, 0, DeterminePriceForResource(resourceAmount.resource), this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			tradeResourceAmounts = tradeResourceAmounts.OrderByDescending(tra => tra.caravanAmount).ThenByDescending(tra => tra.resource.GetAvailableAmount()).ThenBy(tra => tra.resource.name).ToList();

			return tradeResourceAmounts;
		}

		public bool DetermineImportanceForResource(ResourceManager.Resource resource) {
			return false; // TODO This needs to be implemented once the originLocation is properly implemented (see above)
		}

		public ResourceManager.Resource.Price DeterminePriceForResource(ResourceManager.Resource resource) {
			// TODO This needs to be implemented once the originLocation is properly implemented and use DetermineImportanceForResource(...)
			//return new ResourceManager.Resource.Price(UnityEngine.Random.Range(0, 100), UnityEngine.Random.Range(0, 100), UnityEngine.Random.Range(0, 100));
			return resource.price;
		}

		public void SetSelectedResource(ResourceManager.TradeResourceAmount tradeResourceAmount) {
			ResourceManager.TradeResourceAmount existingTradeResourceAmount = resourcesToTrade.Find(tra => tra.resource == tradeResourceAmount.resource);
			if (existingTradeResourceAmount == null) {
				resourcesToTrade.Add(tradeResourceAmount);
			} else {
				if (existingTradeResourceAmount.GetTradeAmount() == 0) {
					resourcesToTrade.Remove(tradeResourceAmount);
				} else {
					existingTradeResourceAmount.SetTradeAmount(tradeResourceAmount.GetTradeAmount());
				}
			}
		}

		public void ConfirmTrade() {
			foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				confirmedResourcesToTrade.Add(new ResourceManager.ConfirmedTradeResourceAmount(tradeResourceAmount, tradeResourceAmount.GetTradeAmount()));
			}
			resourcesToTrade.Clear();
		}
	}
}