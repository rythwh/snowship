using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NUtilities;
using Snowship.Selectable;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIActionsPanelPresenter : UIPresenter<UIActionsPanelView>
	{
		private readonly SelectionManager selectionM;
		private readonly JobRegistry jobRegistry;

		private readonly List<ITreeButton> childButtons = new();

		public UIActionsPanelPresenter(UIActionsPanelView view, SelectionManager selectionM, JobRegistry jobRegistry) : base(view)
		{
			this.selectionM = selectionM;
			this.jobRegistry = jobRegistry;
		}

		public override async UniTask OnCreate() {
			await CreateButtons();
		}

		public override void OnClose() {
		}

		private async UniTask CreateButtons() {
			foreach (UIActionButtonData buttonData in View.ButtonsData.buttons) {
				UIActionGroupButtonElement button = new(buttonData.buttonName, buttonData.buttonSprite);
				await button.OpenAsync(View.ButtonsLayoutGroup.transform);
				button.OnButtonClicked += () => SetButtonChildrenActive(button);
				childButtons.Add(button);

				switch (buttonData.buttonName) {
					case "Build":
						await CreateBuildButtons(button);
						break;
					case "Terraform":
						await CreateTerraformButtons(button);
						break;
					case "Farm":
						await CreateFarmButtons(button);
						break;
					case "Remove":
						await CreateRemoveButtons(button);
						break;
				}
			}
		}

		private void SetButtonChildrenActive(ITreeButton buttonToBeActive) {
			foreach (ITreeButton childButton in childButtons) {
				childButton.SetChildElementsActive(childButton == buttonToBeActive && !buttonToBeActive.ChildElementsActiveState);
			}
		}

		private async UniTask CreateGroupButtons(ITreeButton parentButton, IGroupItem baseGroupItem, Action<ITreeButton, IGroupItem> handleLastChildrenAction) {
			await CreateGroupButtons(parentButton, new List<IGroupItem> { baseGroupItem }, handleLastChildrenAction);
		}

		private async UniTask CreateGroupButtons(ITreeButton parentButton, List<IGroupItem> baseGroupItems, Action<ITreeButton, IGroupItem> handleLastChildrenAction) {

			foreach (IGroupItem group in baseGroupItems) {

				ITreeButton groupButton;
				if (baseGroupItems.Count == 1) {
					groupButton = parentButton;
				} else {
					groupButton = await ((UIActionGroupButtonElement)parentButton).AddChildGroupButton(group.Name, group.Icon);
					groupButton.SetChildElementsActive(false);
					groupButton.OnButtonClicked += () => parentButton.SetChildSiblingChildElementsActive(groupButton);
				}

				foreach (IGroupItem subGroup in group.Children) {
					ITreeButton subGroupButton;
					if (group.Children.Count == 1) {
						subGroupButton = groupButton;
					} else {
						subGroupButton = await ((UIActionGroupButtonElement)groupButton).AddChildGroupButton(subGroup.Name, subGroup.Icon);
						subGroupButton.SetChildElementsActive(false);
						subGroupButton.OnButtonClicked += () => groupButton.SetChildSiblingChildElementsActive(subGroupButton);
					}

					foreach (IGroupItem item in subGroup.Children) {
						UIActionItemButtonElement itemButton = await ((UIActionGroupButtonElement)subGroupButton).AddChildItemButton(item.Name, item.Icon);
						itemButton.SetChildElementsActive(false);

						handleLastChildrenAction(itemButton, item);
					}
				}
			}
		}

		// Build Buttons

		private async UniTask CreateBuildButtons(ITreeButton button) {
			List<IGroupItem> groups = ObjectPrefabGroup.GetObjectPrefabGroups()
				.Where(group => group.type is not (ObjectPrefabGroup.ObjectGroupEnum.None or ObjectPrefabGroup.ObjectGroupEnum.Command or ObjectPrefabGroup.ObjectGroupEnum.Farm or ObjectPrefabGroup.ObjectGroupEnum.Terraform))
				.Cast<IGroupItem>()
				.ToList();

			await CreateGroupButtons(button, groups, SetupIndividualBuildButton);
		}

		private async void SetupIndividualBuildButton(ITreeButton button, IGroupItem item) {

			if (button is not UIActionItemButtonElement prefabButton) {
				return;
			}
			if (item is not ObjectPrefab prefab) {
				return;
			}

			if (prefab.variations.Count > 0) {
				foreach (Variation variation in prefab.variations) {
					UIActionItemButtonElement variationButton = await prefabButton.AddVariation(prefab, variation);
					variationButton.OnButtonClicked += () => prefabButton.SetVariation(prefab, variation, variationButton);
				}
				prefabButton.SetVariation(prefab, prefab.variations.First(), null);
			}

			BuildJobParams buildJobParams = new(prefab);
			prefabButton.OnButtonClicked += () => selectionM.SetSelectedJob<BuildJob, BuildJobDefinition>(buildJobParams);
		}

		// Terraform Buttons

		private async UniTask CreateTerraformButtons(ITreeButton button) {
			JobGroup group = jobRegistry.GetJobGroup("Terraform");

			await CreateGroupButtons(button, group, SetupIndividualTerraformButton);
		}

		private void SetupIndividualTerraformButton(ITreeButton button, IGroupItem item) {
			if (button is not UIActionItemButtonElement jobButton) {
				return;
			}
			if (item is not IJobDefinition jobDefinition) {
				return;
			}

			jobButton.OnButtonClicked += () => selectionM.SetSelectedJob(jobDefinition);
		}

		// Farm Buttons

		private async UniTask CreateFarmButtons(ITreeButton button) {
			JobGroup group = jobRegistry.GetJobGroup("Farm");

			await CreateGroupButtons(button, group, SetupIndividualFarmButton);
		}

		private void SetupIndividualFarmButton(ITreeButton button, IGroupItem item) {
			if (button is not UIActionItemButtonElement jobButton) {
				return;
			}
			if (item is not IJobDefinition jobDefinition) {
				return;
			}

			jobButton.OnButtonClicked += () => selectionM.SetSelectedJob(jobDefinition);
		}

		// Remove Buttons

		private async UniTask CreateRemoveButtons(ITreeButton button) {
			JobGroup group = jobRegistry.GetJobGroup("Remove");

			await CreateGroupButtons(button, group, SetupIndividualRemoveButton);
		}

		private void SetupIndividualRemoveButton(ITreeButton button, IGroupItem item) {
			if (button is not UIActionItemButtonElement jobButton) {
				return;
			}
			if (item is not IJobDefinition jobDefinition) {
				return;
			}

			jobButton.OnButtonClicked += () => selectionM.SetSelectedJob(jobDefinition);
		}
	}
}
