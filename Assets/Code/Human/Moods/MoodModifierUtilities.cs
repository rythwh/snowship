using System;
using System.Collections.Generic;
using Snowship.NHuman;

namespace Snowship.NColonist {
	public static class MoodModifierUtilities {

		public static readonly Dictionary<MoodModifierGroupEnum, Action<Human>> moodModifierFunctions = new() {
			{
				MoodModifierGroupEnum.Death, delegate(Human colonist) {
					// TODO Implement colonists viewing deaths and being sad
				}
			}, {
				MoodModifierGroupEnum.Rest, delegate(Human human) {
					NeedInstance restNeed = human.needs.Find(ni => ni.prefab.type == ENeed.Rest);
					if (restNeed.GetValue() >= restNeed.prefab.critValue) {
						human.Moods.AddMoodModifier(MoodModifierEnum.Exhausted);
					} else if (restNeed.GetValue() >= restNeed.prefab.maxValue) {
						human.Moods.AddMoodModifier(MoodModifierEnum.Tired);
					} else {
						human.Moods.RemoveMoodModifier(MoodModifierEnum.Exhausted);
						human.Moods.RemoveMoodModifier(MoodModifierEnum.Tired);
					}
				}
			}, /* {
				MoodModifierGroupEnum.Water, delegate(Colonist colonist) {
					NeedInstance waterNeed = colonist.needs.Find(ni => ni.prefab.type == ENeed.Water);
					if (waterNeed.GetValue() >= waterNeed.prefab.critValue) {
						colonist.AddMoodModifier(MoodModifierEnum.Dehydrated);
					} else if (waterNeed.GetValue() >= waterNeed.prefab.maxValue) {
						colonist.AddMoodModifier(MoodModifierEnum.Thirsty);
					} else {
						colonist.RemoveMoodModifier(MoodModifierEnum.Dehydrated);
						colonist.RemoveMoodModifier(MoodModifierEnum.Thirsty);
					}
				}
			}, */ {
				MoodModifierGroupEnum.Food, delegate(Human human) {
					NeedInstance foodNeed = human.Needs.Find(ni => ni.prefab.type == ENeed.Food);
					if (foodNeed.GetValue() >= foodNeed.prefab.critValue) {
						human.Moods.AddMoodModifier(MoodModifierEnum.Starving);
					} else if (foodNeed.GetValue() >= foodNeed.prefab.maxValue) {
						human.Moods.AddMoodModifier(MoodModifierEnum.Hungry);
					} else {
						human.Moods.RemoveMoodModifier(MoodModifierEnum.Starving);
						human.Moods.RemoveMoodModifier(MoodModifierEnum.Hungry);
					}
				}
			}, {
				MoodModifierGroupEnum.Inventory, delegate(Human human) {
					if (human.Inventory.UsedWeight() > human.Inventory.maxWeight) {
						human.Moods.AddMoodModifier(MoodModifierEnum.Overencumbered);
					} else {
						human.Moods.RemoveMoodModifier(MoodModifierEnum.Overencumbered);
					}
				}
			}
		};

	}
}