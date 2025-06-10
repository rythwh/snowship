using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NCaravan;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NUI.UITab;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIHumanInfoPanelPresenter : UIPresenter<UIHumanInfoPanelView, UIHumanInfoPanelParameters>
	{
		private readonly Human human;
		private readonly Inventory inventory;

		private readonly List<(IUITabElement tab, UITabButton button)> tabAndButtonGroups = new();

		public UIHumanInfoPanelPresenter(UIHumanInfoPanelView view, UIHumanInfoPanelParameters parameters) : base(view, parameters) {
			human = Parameters.Human;
			inventory = human.Inventory;
		}

		public override void OnCreate() {

			AddTabAsync<UIBiographyTab>("Biography").Forget();
			AddTabAsync<UIInventoryTab>("Inventory").Forget();
			AddTabAsync<UIClothingTab>("Clothing").Forget();
			if (human is Trader) {
				AddTabAsync<UITradeTab>("Trade").Forget();
			}

			View.SetGeneralInformation(
				human.moveSprites[0],
				$"{human.Name} ({human.gender.ToString()[0]})",
				$"{human.Title} of {human.OriginLocation.Name}"
			);

			human.Jobs.OnJobChanged += OnJobChanged;
			OnJobChanged(human.Jobs.ActiveJob);

			View.SetupHealthSlider((0, 1), human.Health, true);
			View.OnHealthChanged(human.Health);
			human.OnHealthChanged += View.OnHealthChanged;

			View.SetupMoodSlider((0, 100), human.Moods.EffectiveMood, true);
			View.OnMoodChanged(human.Moods.EffectiveMood, human.Moods.MoodModifiersSum);
			human.Moods.OnMoodChanged += View.OnMoodChanged;

			View.SetupInventorySliders(inventory);

			inventory.OnInventoryChanged += View.OnInventoryChanged;
		}

		public override void OnClose() {
			base.OnClose();

			human.Jobs.OnJobChanged -= OnJobChanged;

			human.OnHealthChanged -= View.OnHealthChanged;

			human.Moods.OnMoodChanged -= View.OnMoodChanged;

			inventory.OnInventoryChanged -= View.OnInventoryChanged;

			human.OnClothingChanged -= View.OnHumanClothingChanged;
		}

		private async UniTask AddTabAsync<TTab>(string buttonText) where TTab : class, IUITabElement {
			IUITabElement tab = Activator.CreateInstance(typeof(TTab), human) as IUITabElement;
			UITabButton button = new UITabButton();

			tabAndButtonGroups.Add((tab, button));

			button.ButtonClicked += () => OpenTab(tab);

			await View.CreateTab(tab, button, buttonText);
		}

		private void OpenTab(IUITabElement tab) {
			foreach ((IUITabElement tab, UITabButton button) group in tabAndButtonGroups) {
				bool isActiveTab = group.tab == tab;
				group.tab.SetActive(isActiveTab);
				group.button.ShowConnector(isActiveTab);
			}
		}

		private void OnJobChanged(IJob job) {
			View.SetCurrentActionText(job?.Description);
			View.SetStoredActionText(string.Empty);
		}
	}
}
