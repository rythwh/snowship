﻿using Snowship.NResource;
using Snowship.NUI;
using Snowship.NUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmedTradeResourceElementComponent : UIElementComponent {

	[SerializeField] private Image background;
	[SerializeField] private Image resourceImage;
	[SerializeField] private TMP_Text resourceNameText;
	[SerializeField] private TMP_Text collectedVsRemainingAmountText;

	public void SetResource(ConfirmedTradeResourceAmount resourceAmount) {
		resourceImage.sprite = resourceAmount.resource.image;
		resourceNameText.text = resourceAmount.resource.name;

		UpdateCollectedAmount(resourceAmount);
	}

	public void UpdateCollectedAmount(ConfirmedTradeResourceAmount resourceAmount) {
		int collected = Mathf.Abs(resourceAmount.tradeAmount - resourceAmount.amountRemaining);
		int remaining = Mathf.Abs(resourceAmount.tradeAmount);
		collectedVsRemainingAmountText.text = $"{collected} / {remaining}";

		background.color = ColourUtilities.GetColour(
			resourceAmount.amountRemaining == 0
				? ColourUtilities.EColour.LightGreen
				: ColourUtilities.EColour.LightGrey200
		);
	}
}
