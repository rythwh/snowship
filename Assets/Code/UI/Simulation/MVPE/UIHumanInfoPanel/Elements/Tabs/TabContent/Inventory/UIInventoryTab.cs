﻿using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NHuman;
using Snowship.NResource;

namespace Snowship.NUI.UITab
{
	[UsedImplicitly]
	public class UIInventoryTab : UITabElement<UIInventoryTabComponent>
	{
		private readonly Human human;

		public UIInventoryTab(Human human) {
			this.human = human;
		}

		protected override void OnCreate() {
			base.OnCreate();

			foreach (ResourceAmount resourceAmount in human.Inventory.resources) {
				Component.OnInventoryResourceAmountAdded(resourceAmount);
			}

			Component.OnEmptyInventoryButtonClicked += OnEmptyInventoryButtonClicked;

			human.Inventory.OnResourceAmountAdded += Component.OnInventoryResourceAmountAdded;
			human.Inventory.OnResourceAmountRemoved += Component.OnInventoryResourceAmountRemoved;
			human.Inventory.OnReservedResourcesAdded += Component.OnInventoryReservedResourcesAdded;
			human.Inventory.OnReservedResourcesRemoved += Component.OnInventoryReservedResourcesRemoved;
		}

		protected override void OnClose() {
			base.OnClose();

			Component.OnEmptyInventoryButtonClicked -= OnEmptyInventoryButtonClicked;

			human.Inventory.OnResourceAmountAdded -= Component.OnInventoryResourceAmountAdded;
			human.Inventory.OnResourceAmountRemoved -= Component.OnInventoryResourceAmountRemoved;
			human.Inventory.OnReservedResourcesAdded -= Component.OnInventoryReservedResourcesAdded;
			human.Inventory.OnReservedResourcesRemoved -= Component.OnInventoryReservedResourcesRemoved;
		}

		private void OnEmptyInventoryButtonClicked() {
			if (human is not Colonist colonist) {
				return;
			}
			colonist.EmptyInventory(colonist.FindValidContainersToEmptyInventory());
		}
	}
}
