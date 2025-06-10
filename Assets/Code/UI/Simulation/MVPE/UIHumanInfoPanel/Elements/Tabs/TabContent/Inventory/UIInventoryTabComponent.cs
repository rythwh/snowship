using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.UITab
{
	public class UIInventoryTabComponent : UITabElementComponent
	{
		[SerializeField] private Button emptyInventoryButton;
		[SerializeField] private GridLayoutGroup inventoryListGridLayoutGroup;

		public event Action OnEmptyInventoryButtonClicked;

		private readonly List<UIReservedResourcesElement> reservedResourcesElements = new();
		private readonly List<UIResourceAmountElement> inventoryResourceAmountElements = new();

		public override void OnCreate() {
			base.OnCreate();

			emptyInventoryButton.onClick.AddListener(() => OnEmptyInventoryButtonClicked?.Invoke());
		}

		protected override void OnClose() {
			base.OnClose();

			foreach (UIResourceAmountElement resourceAmountElement in inventoryResourceAmountElements) {
				resourceAmountElement.Close();
			}
			inventoryResourceAmountElements.Clear();
		}

		public void OnInventoryResourceAmountAdded(ResourceAmount resourceAmount) {
			UIResourceAmountElement resourceAmountElement = new(resourceAmount);
			resourceAmountElement.Open(inventoryListGridLayoutGroup.transform).Forget();
			inventoryResourceAmountElements.Add(resourceAmountElement);
		}

		public void OnInventoryResourceAmountRemoved(ResourceAmount resourceAmount) {
			UIResourceAmountElement resourceAmountElementToRemove = inventoryResourceAmountElements.Find(e => e.ResourceAmount == resourceAmount);
			inventoryResourceAmountElements.Remove(resourceAmountElementToRemove);
			resourceAmountElementToRemove.Close();
		}

		public void OnInventoryReservedResourcesAdded(ReservedResources reservedResources) {
			UIReservedResourcesElement reservedResourcesElement = new(reservedResources);
			reservedResourcesElement.Open(inventoryListGridLayoutGroup.transform).Forget();
			reservedResourcesElements.Add(reservedResourcesElement);
		}

		public void OnInventoryReservedResourcesRemoved(ReservedResources reservedResources) {
			UIReservedResourcesElement reservedResourcesElement = reservedResourcesElements.Find(rre => rre.ReservedResources == reservedResources);
			if (reservedResourcesElement == null) {
				return;
			}
			reservedResourcesElements.Remove(reservedResourcesElement);
			reservedResourcesElement.Close();
		}
	}
}
