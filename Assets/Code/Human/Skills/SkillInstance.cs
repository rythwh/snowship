using System;
using UnityEngine;

namespace Snowship.NColonist {
	public class SkillInstance {

		public readonly SkillPrefab prefab;

		public int Level { get; set; }
		public float CurrentExperience { get; set; }
		public float NextLevelExperience { get; set; }

		public event Action<int> OnLevelChanged;
		public event Action<float, float> OnExperienceChanged;

		public SkillInstance(SkillPrefab prefab, bool randomStartingLevel, int startingLevel) {
			this.prefab = prefab;

			if (randomStartingLevel) {
				//level = UnityEngine.Random.Range((colonist.profession.primarySkill != null && colonist.profession.primarySkill.type == prefab.type ? Mathf.RoundToInt(colonist.profession.skillRandomMaxValues[prefab] / 2f) : 0), colonist.profession.skillRandomMaxValues[prefab]);
				Level = UnityEngine.Random.Range(0, 7);
			} else {
				Level = startingLevel;
			}

			CurrentExperience = UnityEngine.Random.Range(0, 100);
			NextLevelExperience = CalculateNextLevelExperience();
		}

		public void AddExperience(float amount) {
			Debug.Log($"Adding experience: {CurrentExperience} + {amount} -> {NextLevelExperience} {Level}");
			CurrentExperience += amount;
			while (CurrentExperience >= NextLevelExperience) {
				Level += 1;
				CurrentExperience -= NextLevelExperience;
				NextLevelExperience = CalculateNextLevelExperience();
				Debug.Log($"Added experience: {CurrentExperience} + {amount} -> {NextLevelExperience} {Level}");
				OnLevelChanged?.Invoke(Level);
			}
			OnExperienceChanged?.Invoke(CurrentExperience, NextLevelExperience);
		}

		private int CalculateNextLevelExperience() {
			return 100 + 10 * Level;
		}

		public float CalculateTotalSkillLevel() {
			return Level + CurrentExperience / NextLevelExperience;
		}
	}
}