using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NCaravan;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NResource.NInventory;
using Snowship.NUI.UITab;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIHumanInfoPanelPresenter : UIPresenter<UIHumanInfoPanelView, UIHumanInfoPanelParameters>
	{
		private readonly Human human;
		private readonly HumanView humanView;
		private readonly Inventory inventory;
		private int OpenedTabIndex;

		private readonly List<(IUITabElement tab, UITabButton button)> tabAndButtonGroups = new();

		public event Action<int> OnTabIndexOpened;

		public UIHumanInfoPanelPresenter(UIHumanInfoPanelView view, UIHumanInfoPanelParameters parameters) : base(view, parameters) {
			human = Parameters.Human;
			humanView = Parameters.HumanView;
			OpenedTabIndex = Parameters.OpenedTabIndex;
			inventory = human.Inventory;
		}

		public override async UniTask OnCreate() {

			View.SetGeneralInformation(
				humanView.moveSprites[0],
				$"{human.Name} ({human.Gender.ToString()[0]})",
				$"{human.Title} of {human.OriginLocation.Name}"
			);

			human.Jobs.OnJobChanged += OnJobChanged;
			OnJobChanged(human.Jobs.ActiveJob);

			View.SetupHealthSlider((0, 1), human.Health, true);
			View.OnHealthChanged(human.Health);
			human.HealthChanged += View.OnHealthChanged;

			View.SetupMoodSlider((0, 100), human.Moods.EffectiveMood, true);
			View.OnMoodChanged(human.Moods.EffectiveMood, human.Moods.MoodModifiersSum);
			human.Moods.OnMoodChanged += View.OnMoodChanged;

			View.SetupInventorySliders(inventory);

			inventory.OnInventoryChanged += View.OnInventoryChanged;

			await CreateTabs();
		}

		private async UniTask CreateTabs() {
			// TODO Should make tabs from some type of registry, or data on the actual "human" object(?)
			await AddTabAsync<UIBiographyTab>("Biography");
			await AddTabAsync<UIInventoryTab>("Inventory");
			await AddTabAsync<UIClothingTab>("Clothing");
			if (human is Trader) {
				await AddTabAsync<UITradeTab>("Trade");
			}

			// Open last opened tab
			if (tabAndButtonGroups.Count <= OpenedTabIndex) {
				OpenedTabIndex = 0;
			}
			IUITabElement tab = tabAndButtonGroups[OpenedTabIndex].tab;
			OpenTab(tab);
		}

		public override void OnClose() {
			base.OnClose();

			human.HealthChanged -= View.OnHealthChanged;
			human.Jobs.OnJobChanged -= OnJobChanged;
			human.Moods.OnMoodChanged -= View.OnMoodChanged;
			human.OnClothingChanged -= View.OnHumanClothingChanged;
			inventory.OnInventoryChanged -= View.OnInventoryChanged;

			RemoveTabs();
		}

		private async UniTask<IUITabElement> AddTabAsync<TTab>(string buttonText) where TTab : class, IUITabElement {
			IUITabElement tab = Activator.CreateInstance(typeof(TTab), human, humanView) as IUITabElement;
			UITabButton button = new UITabButton();

			tabAndButtonGroups.Add((tab, button));

			button.ButtonClicked += () => OpenTab(tab);

			await View.CreateTab(tab, button, buttonText);

			return tab;
		}

		private void OpenTab(IUITabElement tab) {
			for (int i = 0; i < tabAndButtonGroups.Count; i++) {
				(IUITabElement tab, UITabButton button) group = tabAndButtonGroups[i];

				bool isActiveTab = group.tab == tab;

				group.tab.SetActive(isActiveTab);
				group.button.ShowConnector(isActiveTab);

				if (isActiveTab) {
					OnTabIndexOpened?.Invoke(i);
				}
			}
		}

		private void RemoveTabs()
		{
			foreach ((IUITabElement tab, UITabButton button) group in tabAndButtonGroups) {
				group.tab.Close();
				group.button.Close();
			}
			tabAndButtonGroups.Clear();
		}

		private void OnJobChanged(IJob job) {
			View.SetCurrentActionText(job?.Description);
			View.SetStoredActionText(string.Empty);
		}
	}
}
