using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Snowship.NCaravan;
using Snowship.NResource;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UITradeMenuView : UIView {

		[Header("Buttons")]
		[SerializeField] private Button confirmTradeButton;
		[SerializeField] private Button closeButton;

		[Header("Title Texts")]
		[SerializeField] private TMP_Text tradeCaravanAffiliationText;
		[SerializeField] private TMP_Text tradeCaravanDescriptionText;
		[SerializeField] private TMP_Text sellingResourceGroupText;

		[Header("Resource Lists")]
		[SerializeField] private VerticalLayoutGroup tradeResourcesLayoutGroup;
		[SerializeField] private VerticalLayoutGroup confirmedTradeResourcesLayoutGroup;

		[Header("Value Texts")]
		[SerializeField] private TMP_Text tradeResourcesCaravanText;
		[SerializeField] private TMP_Text tradeValueCaravanText;
		[SerializeField] private TMP_Text tradeResourcesColonyText;
		[SerializeField] private TMP_Text tradeValueColonyText;

		[SerializeField] private TMP_Text tradeValueDifferenceText;
		[SerializeField] private TMP_Text tradeFairnessText;

		private readonly List<UITradeResourceElement> tradeResourceElements = new();
		private readonly List<UIConfirmedTradeResourceElement> confirmedTradeResourceElements = new();
		private readonly List<UITradeResourceElement> removedTradeResourceElements = new();

		public event Action OnConfirmTradeButtonClicked;
		public event Action OnCloseButtonClicked;

		public override void OnOpen() {
			confirmTradeButton.onClick.AddListener(() => OnConfirmTradeButtonClicked?.Invoke());
			closeButton.onClick.AddListener(() => OnCloseButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}

		public void SetCaravanInformation(Caravan caravan) {

			if (caravan == null) {
				return;
			}

			string locationName = caravan.location.Name;
			string wealth = caravan.location.wealth.ToLower();
			string citySize = caravan.location.citySize.ToLower();
			string resourceRichness = caravan.location.resourceRichness.ToLower();
			string biome = TileManager.Biome.GetBiomeByEnum(caravan.location.biomeType).name.ToLower();

			tradeCaravanAffiliationText.SetText($"Trade Caravan of {locationName}");
			tradeCaravanDescriptionText.SetText($"{locationName} is a {wealth} {citySize} with {resourceRichness} resources in a {biome} climate.");
			sellingResourceGroupText.SetText($"Selling {caravan.resourceGroup.name}");
		}

		public void CreateTradeResourceElements(List<TradeResourceAmount> tradeResourceAmounts) {

			// Remove any old-existing elements
			foreach (UITradeResourceElement tradeResourceElement in tradeResourceElements) {
				tradeResourceElement.Close();
			}
			tradeResourceElements.Clear();

			// Create new elements
			foreach (TradeResourceAmount tradeResourceAmount in tradeResourceAmounts) {
				UITradeResourceElement tradeResourceElement = new(tradeResourceAmount);
				tradeResourceElement.Open(tradeResourcesLayoutGroup.transform).Forget();
				tradeResourceElement.OnTradeResourceElementShouldBeRemoved += OnTradeResourceElementShouldBeRemoved;
				tradeResourceElements.Add(tradeResourceElement);
			}
		}

		private void OnTradeResourceElementShouldBeRemoved(UITradeResourceElement tradeResourceElement) {
			removedTradeResourceElements.Add(tradeResourceElement);
		}

		public void AddTradeResourceElementIfMissing(
			Resource resource,
			int caravanAmount,
			int colonyAmount,
			Caravan caravan
		) {
			if (tradeResourceElements.Find(tre => tre.tradeResourceAmount.resource == resource) != null) {
				return;
			}

			UITradeResourceElement tradeResourceElement = new(
				new TradeResourceAmount(resource, caravanAmount, colonyAmount, caravan)
			);
			tradeResourceElement.Open(tradeResourcesLayoutGroup.transform).Forget();
			tradeResourceElements.Add(tradeResourceElement);
		}

		public void CreateConfirmedTradeResourceElements(
			IOrderedEnumerable<ConfirmedTradeResourceAmount> colonyConfirmedTradeResourceAmounts,
			IOrderedEnumerable<ConfirmedTradeResourceAmount> caravanConfirmedTradeResourceAmounts
		) {
			// Remove any old-existing elements
			foreach (UIConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
				confirmedTradeResourceElement.Close();
			}
			confirmedTradeResourceElements.Clear();

			// Create new elements
			foreach (ConfirmedTradeResourceAmount confirmedTradeResourceAmount in colonyConfirmedTradeResourceAmounts) {
				UIConfirmedTradeResourceElement confirmedTradeResourceElement = new(confirmedTradeResourceAmount);
				confirmedTradeResourceElement.Open(confirmedTradeResourcesLayoutGroup.transform).Forget();
				confirmedTradeResourceElements.Add(confirmedTradeResourceElement);
			}

			foreach (ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravanConfirmedTradeResourceAmounts) {
				UIConfirmedTradeResourceElement confirmedTradeResourceElement = new(confirmedTradeResourceAmount);
				confirmedTradeResourceElement.Open(confirmedTradeResourcesLayoutGroup.transform).Forget();
				confirmedTradeResourceElements.Add(confirmedTradeResourceElement);
			}
		}

		public void RefreshAfterTradeCompleted() {
			foreach (UITradeResourceElement tradeResourceElement in tradeResourceElements) {
				tradeResourceElement.OnTradeAmountChanged(string.Empty);
				tradeResourceElement.tradeResourceAmount.UpdateCaravanAmount();
			}
		}

		public void RemoveRemovedTradeResourceElements() {
			foreach (UITradeResourceElement tradeResourceElement in removedTradeResourceElements) {
				tradeResourceElement.Close();
			}
			removedTradeResourceElements.Clear();
		}
	}
}