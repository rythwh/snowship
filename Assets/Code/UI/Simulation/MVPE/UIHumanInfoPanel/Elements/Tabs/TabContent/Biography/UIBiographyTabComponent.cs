using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NColonist;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Snowship.NUtilities.ColourUtilities;

namespace Snowship.NUI.UITab
{
	public class UIBiographyTabComponent : UITabElementComponent
	{
		[Header("Needs Sub-Panel")]
		[SerializeField] private TMP_Text moodCurrentText;
		[SerializeField] private Button moodViewButton;
		[SerializeField] private TMP_Text moodModifiersSumText;
		[SerializeField] private VerticalLayoutGroup needsListVerticalLayoutGroup;

		[Header("Moods Sub-Panel")]
		[SerializeField] private GameObject moodPanel;
		[SerializeField] private Button moodPanelCloseButton;
		[SerializeField] private VerticalLayoutGroup moodListVerticalLayoutGroup;

		[Header("Skills Sub-Panel")]
		[SerializeField] private VerticalLayoutGroup skillsListVerticalLayoutGroup;

		private readonly List<UINeedElement> needElements = new();
		private readonly List<UIMoodElement> moodElements = new();
		private readonly List<UISkillElement> skillElements = new();

		public override UniTask OnCreate() {

			moodViewButton.onClick.AddListener(() => SetMoodPanelActive(true));
			moodPanelCloseButton.onClick.AddListener(() => SetMoodPanelActive(false));

			SetMoodPanelActive(false);

			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			base.OnClose();

			foreach (UINeedElement needElement in needElements) {
				needElement.Close();
			}
			needElements.Clear();

			foreach (UISkillElement skillElement in skillElements) {
				skillElement.Close();
			}
			skillElements.Clear();
		}

		public async UniTask AddNeedElement(NeedInstance need) {
			UINeedElement needElement = new(need);
			await needElement.OpenAsync(needsListVerticalLayoutGroup.transform);
			needElements.Add(needElement);
		}

		public async UniTask AddSkillElement(SkillInstance skill) {
			UISkillElement skillElement = new(skill);
			await skillElement.OpenAsync(skillsListVerticalLayoutGroup.transform);
			skillElements.Add(skillElement);
		}

		private void SetMoodPanelActive(bool active) {
			moodPanel.SetActive(active);
		}

		public async UniTask AddMoodElement(MoodModifierInstance mood) {
			UIMoodElement moodElement = new(mood);
			await moodElement.OpenAsync(moodListVerticalLayoutGroup.transform);
			moodElements.Add(moodElement);
		}

		public void OnMoodChanged(float effectiveMood, float moodModifiersSum) {
			moodCurrentText.SetText($"{Mathf.RoundToInt(effectiveMood)}%");
			moodCurrentText.color = Color.Lerp(
				GetColour(EColour.DarkRed),
				GetColour(EColour.DarkGreen),
				effectiveMood / 100
			);

			moodModifiersSumText.SetText($"{(moodModifiersSum > 0 ? '+' : string.Empty)}{moodModifiersSum}%");
			moodModifiersSumText.color = moodModifiersSum switch {
				> 0 => GetColour(EColour.LightGreen),
				< 0 => GetColour(EColour.LightRed),
				_ => GetColour(EColour.DarkGrey50)
			};
		}

		public async void OnMoodAdded(MoodModifierInstance mood) {
			await AddMoodElement(mood);
		}

		public void OnMoodRemoved(MoodModifierInstance mood) {
			UIMoodElement moodElementToRemove = moodElements.Find(moodElement => moodElement.Mood == mood);
			if (moodElementToRemove == null) {
				return;
			}
			moodElements.Remove(moodElementToRemove);
			moodElementToRemove.Close();
		}
	}
}
