﻿using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	internal abstract class PriorityResourceElement {

		public enum Type {
			Resource,
			Fuel
		}

		public Type type;

		public Resource resource;
		public CraftingObject craftingObject;

		public GameObject panel;

		public PriorityResourceElement(
			Type type,
			Resource resource,
			Transform parent,
			CraftingObject craftingObject
		) {
			this.type = type;
			this.resource = resource;
			this.craftingObject = craftingObject;

			panel = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/SelectPriorityResource-Panel"), parent, false);

			panel.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resource.image;
			panel.transform.Find("ResourceName-Text").GetComponent<Text>().text = resource.name;

			panel.transform.Find("Details-Panel/EnergyValue-Text").GetComponent<Text>().text = (type == Type.Resource ? resource.craftingEnergy : resource.fuelEnergy) + " energy";

			panel.GetComponent<Button>().onClick.AddListener(delegate {
				if (type == Type.Resource) {
					craftingObject.ToggleResource(resource, 1);
				} else if (type == Type.Fuel) {
					craftingObject.ToggleFuel(resource, 1);
				}

				Update();

				if (type == Type.Resource) {
					UpdatePriorityButtonText(craftingObject.GetCraftableResourceFromResource(resource) == null ? 0 : 1);
				} else if (type == Type.Fuel) {
					UpdatePriorityButtonText(craftingObject.GetFuelFromFuelResource(resource) == null ? 0 : 1);
				}
			});

			UpdatePriorityButtonText(0);
			if (type == Type.Resource) {
				CraftableResourceInstance craftableResource = craftingObject.GetCraftableResourceFromResource(resource);
				if (craftableResource != null) {
					UpdatePriorityButtonText(craftableResource.priority.Get());
				}
			} else if (type == Type.Fuel) {
				PriorityResourceInstance fuel = craftingObject.GetFuelFromFuelResource(resource);
				if (fuel != null) {
					UpdatePriorityButtonText(fuel.priority.Get());
				}
			}

			panel.transform.Find("Priority-Button").GetComponent<HandleClickScript>().Initialize(
				delegate {
					ChangePriority(1);
				},
				null,
				delegate {
					ChangePriority(-1);
				}
			);

			Update();
		}

		public virtual void Update() {
			panel.transform.Find("Details-Panel/AvailableAmount-Text").GetComponent<Text>().text = resource.GetAvailableAmount() + " available";

			panel.transform.Find("Details-Panel/AvailableAmount-Text").GetComponent<Text>().color =
				resource.GetAvailableAmount() > 0
					? ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen)
					: ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
		}

		public void UpdatePriorityButtonText(int priority) {
			panel.transform.Find("Priority-Button/Text").GetComponent<Text>().text = priority > 0 ? priority.ToString() : string.Empty;

			panel.transform.Find("Priority-Button/Text").GetComponent<Text>().color = Color.Lerp(
				ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen),
				ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed),
				(priority - 1f) / (PriorityResourceInstance.priorityMax - 1f)
			);
		}

		public abstract void ChangePriority(int amount);

		public void Remove() {
			MonoBehaviour.Destroy(panel);
		}
	}
}