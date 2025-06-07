using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NColonist
{
	public class MoodsComponent
	{
		private readonly Human human;

		private static readonly (int Min, int Max) MoodRange = (0, 100);

		public float BaseMood { get; set; } = 100;
		public float MoodModifiersSum { get; private set; } = 0;
		public float EffectiveMood { get; set; } = 100;

		public readonly List<MoodModifierInstance> MoodModifiers = new();

		public event Action<MoodModifierInstance> OnMoodAdded;
		public event Action<MoodModifierInstance> OnMoodRemoved;
		public event Action<float, float> OnMoodChanged;

		public MoodsComponent(Human human) {
			this.human = human;

			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
		}

		private void OnTimeChanged(SimulationDateTime obj) {
			UpdateMoodModifiers();
			UpdateMood();
		}

		public void UpdateMoodModifiers() {
			foreach (MoodModifierGroup moodModifierGroup in MoodModifierGroup.moodModifierGroups) {
				MoodModifierUtilities.moodModifierFunctions[moodModifierGroup.type](human);
			}
		}

		public void AddMoodModifier(MoodModifierEnum moodModifierEnum) {
			MoodModifierPrefab prefab = MoodModifierGroup.GetMoodModifierPrefabFromEnum(moodModifierEnum);
			MoodModifierInstance sameGroupMoodModifier = MoodModifiers.Find(mm => prefab.group.type == mm.Prefab.group.type);
			if (sameGroupMoodModifier != null) {
				if (sameGroupMoodModifier.Prefab.type == moodModifierEnum) {
					sameGroupMoodModifier.Timer = prefab.effectLengthSeconds;
					return;
				}
				RemoveMoodModifier(sameGroupMoodModifier.Prefab.type);
			}
			MoodModifierInstance moodToAdd = new(prefab);
			MoodModifiers.Add(moodToAdd);
			OnMoodAdded?.Invoke(moodToAdd);
		}

		public void RemoveMoodModifier(MoodModifierEnum moodModifierEnum) {
			MoodModifierInstance moodToRemove = MoodModifiers.Find(findMoodModifier => findMoodModifier.Prefab.type == moodModifierEnum);
			if (moodToRemove == null) {
				return;
			}
			MoodModifiers.Remove(moodToRemove);
			OnMoodRemoved?.Invoke(moodToRemove);
		}

		public void UpdateMood() {

			float previousMood = EffectiveMood;

			BaseMood = Mathf.Clamp(Mathf.RoundToInt(MoodRange.Max - human.Needs.CalculateNeedSumWeightedByPriority()), 0, 100);

			MoodModifiersSum = MoodModifiers.Sum(hM => hM.Prefab.effectAmount);

			float targetMood = Mathf.Clamp(BaseMood + MoodModifiersSum, 0, 100);
			float moodChangeAmount = (targetMood - EffectiveMood) / (EffectiveMood <= 0f ? 1f : EffectiveMood);
			EffectiveMood += moodChangeAmount /* * GameManager.Get<TimeManager>().Time.DeltaTime*/;
			EffectiveMood = Mathf.Clamp(EffectiveMood, 0, 100);

			if (Mathf.RoundToInt(previousMood) != Mathf.RoundToInt(EffectiveMood)) {
				OnMoodChanged?.Invoke(EffectiveMood, MoodModifiersSum);
			}
		}

		public List<MoodModifierInstance> FindMoodModifierByGroupEnum(MoodModifierGroupEnum moodModifierGroupEnum, int polarity) {
			List<MoodModifierInstance> moodModifiersInGroup = MoodModifiers.Where(moodModifier => moodModifier.Prefab.group.type == moodModifierGroupEnum).ToList();
			if (polarity != 0) {
				moodModifiersInGroup = moodModifiersInGroup.Where(moodModifier => moodModifier.Prefab.effectAmount < 0 == polarity < 0).ToList();
			}
			return moodModifiersInGroup;
		}

		public MoodModifierInstance FindMoodModifierByEnum(MoodModifierEnum moodModifierEnum) {
			return MoodModifiers.Find(moodModifier => moodModifier.Prefab.type == moodModifierEnum);
		}
	}
}