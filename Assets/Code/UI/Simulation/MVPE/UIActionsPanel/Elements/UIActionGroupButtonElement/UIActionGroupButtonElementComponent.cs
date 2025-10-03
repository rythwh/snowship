using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIActionGroupButtonElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text groupNameText;
		[SerializeField] private Image groupIconImage;
		[SerializeField] private LayoutElement groupIconLayoutElement;

		[SerializeField] private Button button;
		[SerializeField] private GameObject selectedIndicator;

		[SerializeField] private LayoutGroup subGroupsLayoutGroup;
		public LayoutGroup SubGroupsLayoutGroup => subGroupsLayoutGroup;

		[SerializeField] private int heightWithIcon = 40;

		public event Action OnButtonClicked;

		public override UniTask OnCreate() {
			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			button.onClick.RemoveListener(() => OnButtonClicked?.Invoke());
		}

		public void SetGroupIcon(Sprite groupIcon) {
			bool hasGroupIcon = groupIcon;
			groupIconImage.sprite = groupIcon;
			groupIconLayoutElement.preferredWidth = groupIconLayoutElement.preferredHeight * (groupIcon?.rect.width / groupIcon?.rect.height) ?? 0;
			if (hasGroupIcon) {
				Vector2 sizeDelta = RectTransform.sizeDelta;
				sizeDelta.y = heightWithIcon;
				RectTransform.sizeDelta = sizeDelta;
			}
		}

		public void SetGroupName(string groupName) {
			groupNameText.SetText(groupName);
		}

		public void SetChildElementsActive(bool active) {
			subGroupsLayoutGroup.gameObject.SetActive(active);
			selectedIndicator.SetActive(active);
		}
	}
}
