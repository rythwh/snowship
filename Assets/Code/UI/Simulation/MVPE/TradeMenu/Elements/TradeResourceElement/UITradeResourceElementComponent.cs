﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UITradeResourceElementComponent : UIElementComponent {

		[SerializeField] private InputField tradeAmountInputField;
		[SerializeField] private TMP_Text tradeAmountInputFieldText;
		[SerializeField] private Button clearButton;

		[Header("Caravan")]
		[SerializeField] private Image caravanResourceImage;
		[SerializeField] private TMP_Text caravanResourceName;
		[SerializeField] private TMP_Text caravanResourcePrice;
		[SerializeField] private TMP_Text caravanAmountText;

		[SerializeField] private Button buyOneButton;
		[SerializeField] private Button buyAllButton;

		[Header("Colony")]
		[SerializeField] private Button sellOneButton;
		[SerializeField] private Button sellAllButton;

		[SerializeField] private Image colonyResourceImage;
		[SerializeField] private TMP_Text colonyResourceName;
		[SerializeField] private TMP_Text colonyResourcePrice;
		[SerializeField] private TMP_Text colonyAmountText;

		public event Action<string> OnTradeAmountChanged;
		public event Action OnClearButtonClicked;
		public event Action<bool> OnBuyIncreaseButtonClicked;
		public event Action<bool> OnSellIncreaseButtonClicked;

		public override void OnCreate() {
			tradeAmountInputField.onEndEdit.AddListener(amountString => OnTradeAmountChanged?.Invoke(amountString));
			clearButton.onClick.AddListener(() => OnClearButtonClicked?.Invoke());

			buyOneButton.onClick.AddListener(() => OnBuyIncreaseButtonClicked?.Invoke(false));
			buyAllButton.onClick.AddListener(() => OnBuyIncreaseButtonClicked?.Invoke(true));

			sellOneButton.onClick.AddListener(() => OnSellIncreaseButtonClicked?.Invoke(false));
			sellAllButton.onClick.AddListener(() => OnSellIncreaseButtonClicked?.Invoke(true));
		}

		protected override void OnClose() {
			tradeAmountInputField.onEndEdit.RemoveListener(amountString => OnTradeAmountChanged?.Invoke(amountString));
			clearButton.onClick.RemoveListener(() => OnClearButtonClicked?.Invoke());

			buyOneButton.onClick.RemoveListener(() => OnBuyIncreaseButtonClicked?.Invoke(false));
			buyAllButton.onClick.RemoveListener(() => OnBuyIncreaseButtonClicked?.Invoke(true));

			sellOneButton.onClick.RemoveListener(() => OnSellIncreaseButtonClicked?.Invoke(false));
			sellAllButton.onClick.RemoveListener(() => OnSellIncreaseButtonClicked?.Invoke(true));
		}

		public void SetTradeAmountTextWithoutNotify(string text) {
			tradeAmountInputField.SetTextWithoutNotify(text);
		}

		public void SetTradeAmountText(string text) {
			tradeAmountInputField.text = text;
		}

		public void SetTradeAmountTextColour(Color colour) {
			tradeAmountInputFieldText.color = colour;
		}

		public void SetResourceImageSprite(Sprite resourceSprite) {
			caravanResourceImage.sprite = resourceSprite;
			colonyResourceImage.sprite = resourceSprite;
		}

		public void SetResourceName(string resourceName) {
			caravanResourceName.SetText(resourceName);
			colonyResourceName.SetText(resourceName);
		}

		public void SetResourcePrice(string resourcePrice) {
			caravanResourcePrice.SetText(resourcePrice);
			colonyResourcePrice.SetText(resourcePrice);
		}

		public void SetCaravanAmount(int caravanAmount) {
			caravanAmountText.SetText(caravanAmount == 0 ? string.Empty : caravanAmount.ToString());
		}

		public void SetColonyAmount(int colonyAmount) {
			colonyAmountText.SetText(colonyAmount == 0 ? string.Empty : colonyAmount.ToString());
		}

		public void SetClearButtonInteractable(bool interactable) {
			clearButton.interactable = interactable;
		}
	}
}
