﻿using System.Collections.Generic;
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

		public override void OnCreate() {
			base.OnCreate();

			moodViewButton.onClick.AddListener(() => SetMoodPanelActive(true));
			moodPanelCloseButton.onClick.AddListener(() => SetMoodPanelActive(false));

			SetMoodPanelActive(false);
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

		public void AddNeedElement(NeedInstance need) {
			UINeedElement needElement = new(need);
			needElement.Open(needsListVerticalLayoutGroup.transform).Forget();
			needElements.Add(needElement);
		}

		public void AddSkillElement(SkillInstance skill) {
			UISkillElement skillElement = new(skill);
			skillElement.Open(skillsListVerticalLayoutGroup.transform).Forget();
			skillElements.Add(skillElement);
		}

		private void SetMoodPanelActive(bool active) {
			moodPanel.SetActive(active);
		}

		public void AddMoodElement(MoodModifierInstance mood) {
			UIMoodElement moodElement = new(mood);
			moodElement.Open(moodListVerticalLayoutGroup.transform).Forget();
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

		public void OnMoodAdded(MoodModifierInstance mood) {
			AddMoodElement(mood);
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
