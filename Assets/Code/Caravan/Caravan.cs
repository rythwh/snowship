using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NHuman;
using Snowship.NLife;
using Snowship.NMap.NTile;
using Snowship.NMap;
using Snowship.NPath;
using Snowship.NResource;
using Snowship.NResource.NInventory;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NCaravan {
	public class Caravan : IInventoriable, IDisposable
	{
		public List<Trader> traders = new List<Trader>();
		public int numTraders;
		public CaravanType caravanType;

		public Location location;

		public Inventory Inventory { get; }

		public List<TradeResourceAmount> resourcesToTrade = new List<TradeResourceAmount>();

		public List<ConfirmedTradeResourceAmount> confirmedResourcesToTrade = new List<ConfirmedTradeResourceAmount>();

		public Tile targetTile;

		public ResourceGroup resourceGroup;

		public int leaveTimer;
		public const int leaveTimerMax = SimulationDateTime.DayLengthSeconds * 2;
		public bool leaving;

		private const int MinDistinctResources = 1;
		private const float MinDistinctResourceChance = 0.5f;
		private const float MinAvailableAmountModifier = 0.1f;
		private const float MaxAvailableAmountModifier = 0.5f;
		private const int MinMinimumCaravanAmount = 5;
		private const int MaxMinimumCaravanAmount = 15;

		private readonly List<Trader> removeTraders = new List<Trader>();

		private HumanManager HumanM => GameManager.Get<HumanManager>();

		public Caravan() {
			Inventory = new Inventory(this, int.MaxValue, int.MaxValue);
		}

		public Caravan(
			int numTraders,
			CaravanType caravanType,
			List<Tile> spawnTiles,
			Tile targetTile
		) {
			this.numTraders = numTraders;
			this.caravanType = caravanType;
			this.targetTile = targetTile;

			location = Location.GenerateLocation();

			for (int i = 0; i < numTraders && spawnTiles.Count > 0; i++) {
				Tile spawnTile = spawnTiles[Random.Range(0, spawnTiles.Count)];
				SpawnTrader(spawnTile);
				spawnTiles.Remove(spawnTile);
			}

			Inventory = new Inventory(this, int.MaxValue, int.MaxValue);

			resourceGroup = ResourceGroup.GetRandomResourceGroup();
			foreach (Resource resource in resourceGroup.resources.OrderBy(_ => Random.Range(0f, 1f))) { // Randomize resource group list
				int resourceGroupResourceCount = Mathf.Clamp(resourceGroup.resources.Count, MinDistinctResources + 1, int.MaxValue); // Ensure minimum count of (minimumDistinctResources + 1)
				if (Random.Range(0f, 1f) < Mathf.Clamp((resourceGroupResourceCount - Inventory.resources.Count - MinDistinctResources) / (float)(resourceGroupResourceCount - MinDistinctResources), MinDistinctResourceChance, 1f)) { // Decrease chance of additional distinct resources on caravan as distinct resources on caravan increase
					int resourceAvailableAmount = resource.GetAvailableAmount();
					int caravanAmount = Mathf.RoundToInt(Mathf.Clamp(Random.Range(resourceAvailableAmount * MinAvailableAmountModifier, resourceAvailableAmount * MaxAvailableAmountModifier), Random.Range(MinMinimumCaravanAmount, MaxMinimumCaravanAmount), int.MaxValue)); // Ensure a minimum number of the resource on the caravan
					Inventory.ChangeResourceAmount(resource, caravanAmount, false);
				}
			}

			GameManager.Get<TimeManager>().OnTimeChanged += UpdateCaravanLeaveState;
		}

		public void Dispose() {
			GameManager.Get<TimeManager>().OnTimeChanged -= UpdateCaravanLeaveState;
		}

		private void SpawnTrader(Tile spawnTile) {

			Gender gender = Random.RandomElement<Gender>();
			HumanData data = new HumanData(
				HumanM.GetName(gender),
				gender,
				new List<(ESkill, float)>()
			);

			Trader trader = HumanM.CreateHuman<Trader, TraderViewModule>(spawnTile, data);
			traders.Add(trader);

			List<Tile> targetTiles = new List<Tile> { targetTile };
			targetTiles.AddRange(targetTile.SurroundingTiles[EGridConnectivity.FourWay].Where(t => t is { walkable: true, buildable: true }));

			List<Tile> additionalTargetTiles = new List<Tile>();
			foreach (Tile tt in targetTiles) {
				additionalTargetTiles.AddRange(tt.SurroundingTiles[EGridConnectivity.FourWay].Where(t => t is { walkable: true, buildable: true } && !additionalTargetTiles.Contains(t)));
			}

			targetTiles.AddRange(additionalTargetTiles);

			trader.MoveToTile(targetTiles[Random.Range(0, targetTiles.Count)], false);
		}

		public List<TradeResourceAmount> GenerateTradeResourceAmounts() {
			List<TradeResourceAmount> tradeResourceAmounts = new List<TradeResourceAmount>();
			tradeResourceAmounts.AddRange(resourcesToTrade);

			List<ResourceAmount> caravanResourceAmounts = Inventory.resources;

			foreach (ResourceAmount resourceAmount in caravanResourceAmounts) {
				TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.Resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new TradeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, 0, this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			List<ResourceAmount> colonyResourceAmounts = new();
			if (traders.Count > 0) {
				colonyResourceAmounts = TradingPost.GetAvailableResourcesInTradingPostsInRegion(traders.Find(t => t != null).Tile.region);
			}

			foreach (ResourceAmount resourceAmount in colonyResourceAmounts) {
				TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.Resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new TradeResourceAmount(resourceAmount.Resource, 0, resourceAmount.Amount, this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			tradeResourceAmounts = tradeResourceAmounts.OrderBy(tra => tra.resource.name).ToList();

			return tradeResourceAmounts;
		}

		public bool DetermineImportanceForResource(Resource resource) {
			return false; // TODO This needs to be implemented once the originLocation is properly implemented (see above)
		}

		public int DeterminePriceForResource(Resource resource) {
			return resource.price;
		}

		public void SetSelectedResource(TradeResourceAmount tradeResourceAmount) {
			TradeResourceAmount existingTradeResourceAmount = resourcesToTrade.Find(tra => tra.resource == tradeResourceAmount.resource);
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
			if (leaving) {
				leaving = false;
				leaveTimer = 0;
				foreach (Trader trader in traders) {
					trader.MoveToTile(trader.Tile, false);
				}
			}

			foreach (TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				ConfirmedTradeResourceAmount existingConfirmedTradeResourceAmount = confirmedResourcesToTrade.Find(crtt => crtt.resource == tradeResourceAmount.resource);
				if (existingConfirmedTradeResourceAmount != null) {
					existingConfirmedTradeResourceAmount.tradeAmount += tradeResourceAmount.GetTradeAmount();
					existingConfirmedTradeResourceAmount.amountRemaining += tradeResourceAmount.GetTradeAmount();
				} else {
					confirmedResourcesToTrade.Add(new ConfirmedTradeResourceAmount(tradeResourceAmount.resource, tradeResourceAmount.GetTradeAmount()));
				}
			}

			List<ResourceAmount> resourcesToReserve = new();
			foreach (TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				if (tradeResourceAmount.GetTradeAmount() < 0) {
					resourcesToReserve.Add(new ResourceAmount(tradeResourceAmount.resource, Mathf.Abs(tradeResourceAmount.GetTradeAmount())));
				}
			}

			foreach (TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				if (tradeResourceAmount.GetTradeAmount() > 0) {
					Inventory.ChangeResourceAmount(tradeResourceAmount.resource, -tradeResourceAmount.GetTradeAmount(), false);
				}
			}

			resourcesToTrade.Clear();

			List<TradingPost> tradingPostsWithReservedResources = new();

			Trader primaryTrader = traders[0];
			if (primaryTrader != null) {
				foreach (TradingPost tradingPost in TradingPost.GetTradingPostsInRegion(primaryTrader.Tile.region).OrderBy(tp => Path.RegionBlockDistance(primaryTrader.Tile.regionBlock, tp.zeroPointTile.regionBlock, true, true, false))) {
					List<ResourceAmount> resourcesToReserveAtThisTradingPost = new();
					List<ResourceAmount> resourcesToReserveToRemove = new();
					foreach (ResourceAmount resourceToReserve in resourcesToReserve) {
						ResourceAmount resourceAmount = tradingPost.Inventory.resources.Find(r => r.Resource == resourceToReserve.Resource);
						if (resourceAmount != null) {
							int amountToReserve = resourceToReserve.Amount < resourceAmount.Amount ? resourceToReserve.Amount : resourceAmount.Amount;
							resourcesToReserveAtThisTradingPost.Add(new ResourceAmount(resourceToReserve.Resource, amountToReserve));
							resourceToReserve.Amount -= amountToReserve;
							if (resourceToReserve.Amount == 0) {
								resourcesToReserveToRemove.Add(resourceToReserve);
							}
						}
					}

					if (resourcesToReserveAtThisTradingPost.Count > 0) {
						tradingPost.Inventory.ReserveResources(resourcesToReserveAtThisTradingPost, primaryTrader);
						tradingPostsWithReservedResources.Add(tradingPost);
					}

					foreach (ResourceAmount resourceToReserveToRemove in resourcesToReserveToRemove) {
						resourcesToReserve.Remove(resourceToReserveToRemove);
					}

					resourcesToReserveToRemove.Clear();
				}

				if (tradingPostsWithReservedResources.Count > 0) {
					primaryTrader.tradingPosts = tradingPostsWithReservedResources;
				}
			}
		}

		/*
			* Returns:
			*		true - if any of the caravan's traders still exist on the map
			*		false - if all of the caravan's traders have left the map
			*/
		public bool Update() {
			foreach (Trader trader in traders) {
				trader.Update();

				if (trader.Tile == trader.leaveTile) {
					removeTraders.Add(trader);
				}
			}

			foreach (Trader trader in removeTraders) {
				trader.Remove();

				traders.Remove(trader);
			}

			removeTraders.Clear();

			return traders.Count > 0;
		}

		private void UpdateCaravanLeaveState(SimulationDateTime _) {
			if (leaving || confirmedResourcesToTrade.Count > 0) {
				return;
			}

			if (leaveTimer >= leaveTimerMax) {
				leaving = true;
				leaveTimer = 0;

				foreach (Trader trader in traders) {
					List<Tile> validLeaveTiles = GameManager.Get<IMapQuery>().Map.edgeTiles.Where(t => t.region == trader.Tile.region).ToList();
					if (validLeaveTiles.Count > 0) {
						trader.leaveTile = validLeaveTiles[Random.Range(0, validLeaveTiles.Count)];
						trader.MoveToTile(trader.leaveTile, false);
					} else {
						trader.leaveTile = trader.Tile;
						trader.Remove();
					}
				}
			} else {
				leaveTimer += 1;
			}
		}
	}
}
