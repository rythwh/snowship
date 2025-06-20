﻿using TMPro;
using UnityEngine;

namespace Snowship.NUI
{
	public class UISimulationView : UIView {

		[Header("General")]
		[SerializeField] private TMP_Text disclaimerText;

		public override void OnOpen() {

		}

		public override void OnClose() {

		}

		public void SetDisclaimerText(string text) {
			disclaimerText.text = text;
		}

	}
}