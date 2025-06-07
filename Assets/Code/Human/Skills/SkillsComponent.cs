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
						human,
						skillPrefab,
						true,
						0
					));
			}
		}
	}
}