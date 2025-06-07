using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;
using Snowship.NTime;

namespace Snowship.NColonist
{
	public class NeedsComponent : IDisposable
	{
		private readonly Human human;

		private readonly Dictionary<ENeed, NeedInstance> needs = new();
		private readonly List<NeedInstance> sortedNeeds = new();

		private TimeManager TimeManager => GameManager.Get<TimeManager>();

		public NeedsComponent(Human human) {
			this.human = human;
			foreach (NeedPrefab needPrefab in NeedPrefab.needPrefabs) {
				NeedInstance needInstance = new(human, needPrefab);
				needs.Add(needPrefab.type, needInstance);
				sortedNeeds.Add(needInstance);
			}
			sortedNeeds = sortedNeeds.OrderBy(need => need.prefab.priority).ToList();

			TimeManager.OnTimeChanged += UpdateNeeds;
		}

		public void Dispose() {
			TimeManager.OnTimeChanged -= UpdateNeeds;
		}

		private void UpdateNeeds(SimulationDateTime _) {
			foreach (NeedInstance need in sortedNeeds) {
				NeedUtilities.CalculateNeedValue(need);
				bool checkNeed = false;
				if (human.Jobs.ActiveJob == null) {
					checkNeed = true;
				} else {
					if (human.Jobs.ActiveJob.Group.Name == "Needs") {
						if (NeedUtilities.jobToNeedMap.TryGetValue(human.Jobs.ActiveJob.Name, out ENeed needType)) {
							if (need.prefab.priority < NeedPrefab.GetNeedPrefabFromEnum(needType).priority) {
								checkNeed = true;
							}
						} else {
							checkNeed = true;
						}
					} else {
						checkNeed = true;
					}
				}
				if (checkNeed) {
					if (NeedUtilities.NeedToReactionFunctionMap[need.prefab.type](need)) {
						break;
					}
				}
			}
		}

		public float CalculateNeedSumWeightedByPriority() {
			return sortedNeeds.Sum(need => need.GetValue() / (need.prefab.priority + 1));
		}

		public NeedInstance Get(ENeed needType) {
			return needs.GetValueOrDefault(needType);
		}
	}
}