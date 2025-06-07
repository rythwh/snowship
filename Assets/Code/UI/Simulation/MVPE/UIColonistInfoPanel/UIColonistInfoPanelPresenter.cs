using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIColonistInfoPanelPresenter : UIPresenter<UIColonistInfoPanelView, UIColonistInfoPanelParameters>
	{
		private readonly Human human;
		private readonly Inventory inventory;

		public UIColonistInfoPanelPresenter(UIColonistInfoPanelView view, UIColonistInfoPanelParameters parameters) : base(view, parameters) {
			human = Parameters.Human;
			inventory = human.Inventory;
		}

		public override void OnCreate() {

			View.OnTabSelected += OnTabSelected;

			OnTabSelected(View.ButtonToTabMap.First().button);

			View.SetColonistInformation(
				human.moveSprites[0],
				$"{human.Name} ({human.gender.ToString()[0]})",
				$"Colonist of {GameManager.Get<ColonyManager>().colony.name}" // TODO Make affiliation accessible by Human and use that here
			);

			SetupNeedsSkillsTab();

			human.Jobs.OnJobChanged += OnJobChanged;
			OnJobChanged(human.Jobs.ActiveJob);

			View.SetupHealthSlider((0, 1), human.Health, true);
			View.OnHealthChanged(human.Health);
			human.OnHealthChanged += View.OnHealthChanged;

			View.SetupMoodSlider((0, 100), human.Moods.EffectiveMood, true);
			View.OnMoodChanged(human.Moods.EffectiveMood, human.Moods.MoodModifiersSum);
			human.Moods.OnMoodAdded += OnMoodAdded;
			human.Moods.OnMoodChanged += View.OnMoodChanged;
			human.Moods.OnMoodRemoved += OnMoodRemoved;

			View.SetupInventorySliders(inventory);
			View.OnEmptyInventoryButtonClicked += OnEmptyInventoryButtonClicked;
			inventory.OnInventoryChanged += View.OnInventoryChanged;

			SetupInventoryTab();
			inventory.OnResourceAmountAdded += View.OnInventoryResourceAmountAdded;
			inventory.OnResourceAmountRemoved += View.OnInventoryResourceAmountRemoved;
			inventory.OnReservedResourcesAdded += View.OnInventoryReservedResourcesAdded;
			inventory.OnReservedResourcesRemoved += View.OnInventoryReservedResourcesRemoved;

			SetupClothingTab();
			human.OnClothingChanged += View.OnHumanClothingChanged;
			View.SetClothingSelectionPanelActive(false);
		}

		public override void OnClose() {
			base.OnClose();

			View.OnTabSelected -= OnTabSelected;

			human.Jobs.OnJobChanged -= OnJobChanged;

			human.OnHealthChanged -= View.OnHealthChanged;

			human.Moods.OnMoodAdded -= OnMoodAdded;
			human.Moods.OnMoodChanged -= View.OnMoodChanged;
			human.Moods.OnMoodRemoved -= OnMoodRemoved;

			View.OnEmptyInventoryButtonClicked -= OnEmptyInventoryButtonClicked;
			inventory.OnInventoryChanged -= View.OnInventoryChanged;

			inventory.OnResourceAmountAdded -= View.OnInventoryResourceAmountAdded;
			inventory.OnResourceAmountRemoved -= View.OnInventoryResourceAmountRemoved;
			inventory.OnReservedResourcesAdded -= View.OnInventoryReservedResourcesAdded;
			inventory.OnReservedResourcesRemoved -= View.OnInventoryReservedResourcesRemoved;

			human.OnClothingChanged -= View.OnHumanClothingChanged;

			foreach (MoodModifierInstance mood in human.Moods.MoodModifiers) {
				OnMoodRemoved(mood);
			}
		}

		private void OnTabSelected(Button tabButton) {
			foreach ((Button button, GameObject tab) mapping in View.ButtonToTabMap) {
				mapping.tab.SetActive(mapping.button == tabButton);
			}
		}

		private void OnJobChanged(IJob job) {
			View.SetCurrentActionText(job?.Description);
			View.SetStoredActionText(string.Empty);
		}

		private void OnMoodAdded(MoodModifierInstance mood) {
			View.OnMoodAdded(mood);
			mood.OnTimerAtZero += OnMoodRemoved;
		}

		private void OnMoodRemoved(MoodModifierInstance mood) {
			mood.OnTimerAtZero -= OnMoodAdded;
			View.OnMoodRemoved(mood);
		}

		private void SetupNeedsSkillsTab() {
			foreach (NeedInstance need in human.Needs.AsList().OrderByDescending(need => need.GetValue())) {
				View.AddNeedElement(need);
			}
			foreach (MoodModifierInstance mood in human.Moods.MoodModifiers) {
				View.AddMoodElement(mood);
			}
			foreach (SkillInstance skill in human.Skills.AsList()) {
				View.AddSkillElement(skill);
			}
		}

		private void SetupInventoryTab() {
			foreach (ResourceAmount resourceAmount in inventory.resources) {
				View.OnInventoryResourceAmountAdded(resourceAmount);
			}
		}

		private void OnEmptyInventoryButtonClicked() {
			if (human is not Colonist colonist) {
				return;
			}
			colonist.EmptyInventory(colonist.FindValidContainersToEmptyInventory());
		}

		private void SetupClothingTab() {
			foreach (KeyValuePair<BodySection, Clothing> clothingMapping in human.clothes) {
				UIClothingButtonElement clothingButton = View.CreateClothingButton(clothingMapping.Key, clothingMapping.Value);
				clothingButton.OnButtonClicked += OnClothingButtonClicked;
			}
		}

		private void OnClothingButtonClicked(BodySection bodySection) {

			View.SetClothingSelectionPanelActive(true);
			View.SetClothesAvailableTitleTextPanelActive(false);
			View.SetClothesTakenTitleTextPanelActive(false);

			View.SetDisrobeButtonTarget(bodySection);

			List<Clothing> clothesByAppearance = Clothing.GetClothesByAppearance(bodySection);
			foreach (Clothing clothing in clothesByAppearance) {
				if (clothing.GetWorldTotalAmount() <= 0) {
					continue;
				}

				UIClothingElement clothingElement = null;

				if (clothing.GetAvailableAmount() > 0) {
					clothingElement = View.CreateAvailableClothingElement(clothing);
					View.SetClothesAvailableTitleTextPanelActive(true);
				} else if (human.clothes[bodySection] == null || clothing.name != human.clothes[bodySection].name) {
					clothingElement = View.CreateTakenClothingElement(clothing);
					View.SetClothesTakenTitleTextPanelActive(true);
				}

				if (clothingElement != null) {
					clothingElement.OnButtonClicked += OnClothingElementClicked;
				}
			}
		}

		private void OnClothingElementClicked(Clothing clothing) {
			human.ChangeClothing(clothing.prefab.BodySection, clothing);
			View.SetClothingSelectionPanelActive(false);
		}
	}
}
