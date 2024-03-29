﻿using System.Collections.Generic;
using Snowship.NJob;
using Snowship.NUtilities;

namespace Snowship.NCaravan {
	public class Trader : HumanManager.Human {

		public Caravan caravan;

		public TileManager.Tile leaveTile;

		public List<ResourceManager.TradingPost> tradingPosts;

		public Trader(TileManager.Tile spawnTile, float startingHealth, Caravan caravan) : base(spawnTile, startingHealth) {
			this.caravan = caravan;

			obj.transform.SetParent(GameManager.resourceM.traderParent.transform, false);
		}

		public override void SetName(string name) {
			base.SetName(name);

			SetNameColour(ColourUtilities.GetColour(ColourUtilities.Colours.LightPurple100));
		}

		public override void Update() {
			base.Update();

			if (tradingPosts != null && tradingPosts.Count > 0) {
				ResourceManager.TradingPost tradingPost = tradingPosts[0];
				if (path.Count <= 0) {
					MoveToTile(tradingPost.zeroPointTile, false);
				}

				if (overTile == tradingPost.zeroPointTile) {
					foreach (ResourceManager.ReservedResources rr in tradingPost.GetInventory().TakeReservedResources(this)) {
						foreach (ResourceManager.ResourceAmount ra in rr.resources) {
							caravan.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);

							ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount = caravan.confirmedResourcesToTrade.Find(crtt => crtt.resource == ra.resource);
							if (confirmedTradeResourceAmount != null) {
								confirmedTradeResourceAmount.amountRemaining += ra.amount;
							}
						}
					}

					tradingPosts.RemoveAt(0);
				}

				if (tradingPosts.Count <= 0) {
					Job job = new Job(
						JobPrefab.GetJobPrefabByName("CollectResources"),
						tradingPost.tile,
						GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectResources),
						null,
						0
					) {
						transferResources = new List<ResourceManager.ResourceAmount>()
					};
					foreach (ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade) {
						if (confirmedTradeResourceAmount.tradeAmount > 0) {
							tradingPost.GetInventory().ChangeResourceAmount(confirmedTradeResourceAmount.resource, confirmedTradeResourceAmount.tradeAmount, false);
							confirmedTradeResourceAmount.amountRemaining = 0;
							job.transferResources.Add(new ResourceManager.ResourceAmount(confirmedTradeResourceAmount.resource, confirmedTradeResourceAmount.tradeAmount));
						}
					}

					caravan.confirmedResourcesToTrade.Clear();
					GameManager.jobM.CreateJob(job);
				}
			}
			else {
				if (path.Count <= 0) {
					Wander(caravan.targetTile, 4);
				}
				else {
					wanderTimer = UnityEngine.Random.Range(10f, 20f);
				}
			}
		}

		public override void Remove() {
			base.Remove();

			if (GameManager.humanM.selectedHuman == this) {
				GameManager.humanM.SetSelectedHuman(null);
			}
		}
	}
}
