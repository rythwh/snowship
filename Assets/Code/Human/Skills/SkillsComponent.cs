using System.Collections.Generic;
using Snowship.NHuman;

namespace Snowship.NColonist
{
	public class SkillsComponent
	{
		private readonly List<SkillInstance> skills = new();

		public SkillsComponent(Human human) {
			foreach (SkillPrefab skillPrefab in SkillPrefab.skillPrefabs) {
				skills.Add(
					new SkillInstance(
						skillPrefab,
						true,
						0
					)
				);
			}
		}

		public SkillInstance GetSkillFromEnum(ESkill type) {
			return skills.Find(s => s.prefab.type == type);
		}

		public SkillInstance GetSkillFromJobType(string jobType) {
			return skills.Find(s => s.prefab.affectedJobTypes.ContainsKey(jobType));
		}

		public float GetJobSkillMultiplier(string jobType) {
			SkillInstance skill = GetSkillFromJobType(jobType);
			if (skill != null) {
				return 1 * (-(1f / (skill.prefab.affectedJobTypes[jobType] * skill.Level + 1)) + 1);
			}
			return 1.0f;
		}

		public IReadOnlyList<SkillInstance> AsList() {
			return skills;
		}
	}
}