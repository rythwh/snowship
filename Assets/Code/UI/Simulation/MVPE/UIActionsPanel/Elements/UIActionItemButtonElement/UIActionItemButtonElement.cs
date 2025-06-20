﻿using System;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Snowship.NUI
{
	public class UIActionItemButtonElement : UIElement<UIActionItemButtonElementComponent>, ITreeButton
	{
		private string itemName;
		private Sprite itemIcon;

		public bool ChildElementsActiveState { get; private set; } = false;

		private bool hasVariations = false;

		public event Action OnButtonClicked;

		public UIActionItemButtonElement(string itemName, Sprite itemIcon) {
			this.itemName = itemName;
			this.itemIcon = itemIcon;
		}

		protected override void OnCreate() {
			base.OnCreate();

			Component.SetItemName(itemName);
			Component.SetItemIcon(itemIcon);

			Component.OnButtonClicked += OnComponentButtonClicked;

			SetChildElementsActive(ChildElementsActiveState);
		}

		private void OnComponentButtonClicked(PointerEventData eventData) {
			if (eventData.button == PointerEventData.InputButton.Left) {
				OnButtonClicked?.Invoke();
			}
			SetChildElementsActive(!ChildElementsActiveState && hasVariations && eventData.button == PointerEventData.InputButton.Right);
		}

		public void SetChildSiblingChildElementsActive(ITreeButton childButtonToBeActive) {
			throw new NotImplementedException();
		}

		public void SetChildElementsActive(bool active) {
			ChildElementsActiveState = active;
			Component.SetChildElementsActive(active);
		}

		public UIActionItemButtonElement AddVariation(ObjectPrefab prefab, Variation variation) {
			hasVariations = true;
			return Component.AddVariation(prefab, variation);
		}

		public void SetVariation(ObjectPrefab prefab, Variation variation, UIActionItemButtonElement variationButton) {
			prefab.SetVariation(variation);

			itemName = prefab.selectedVariation.instanceName;
			itemIcon = prefab.GetBaseSpriteForVariation(variation);

			Component.SetItemName(itemName);
			Component.SetItemIcon(itemIcon);

			// TODO Refresh Required Resources

			SetChildElementsActive(false);
			variationButton?.SetChildElementsActive(false);
		}
	}
}