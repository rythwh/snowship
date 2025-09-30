using System.Linq;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NHuman;

namespace Snowship.NUI.UITab
{
	[UsedImplicitly]
	public class UIBiographyTab : UITabElement<UIBiographyTabComponent>
	{
		private readonly Human human;

		public UIBiographyTab(Human human) {
			this.human = human;
		}

		protected override async void OnCreate() {
			base.OnCreate();

			foreach (NeedInstance need in human.Needs.AsList().OrderByDescending(need => need.GetValue())) {
				await Component.AddNeedElement(need);
			}
			foreach (MoodModifierInstance mood in human.Moods.MoodModifiers) {
				await Component.AddMoodElement(mood);
			}
			foreach (SkillInstance skill in human.Skills.AsList()) {
				await Component.AddSkillElement(skill);
			}

			human.Moods.OnMoodAdded += OnMoodAdded;
			human.Moods.OnMoodChanged += Component.OnMoodChanged;
			human.Moods.OnMoodRemoved += OnMoodRemoved;
		}

		protected override void OnClose() {
			base.OnClose();

			human.Moods.OnMoodAdded -= OnMoodAdded;
			human.Moods.OnMoodChanged -= Component.OnMoodChanged;
			human.Moods.OnMoodRemoved -= OnMoodRemoved;

			foreach (MoodModifierInstance mood in human.Moods.MoodModifiers) {
				OnMoodRemoved(mood);
			}
		}

		private void OnMoodAdded(MoodModifierInstance mood) {
			Component.OnMoodAdded(mood);
			mood.OnTimerAtZero += OnMoodRemoved;
		}

		private void OnMoodRemoved(MoodModifierInstance mood) {
			mood.OnTimerAtZero -= OnMoodAdded;
			Component.OnMoodRemoved(mood);
		}
	}
}
