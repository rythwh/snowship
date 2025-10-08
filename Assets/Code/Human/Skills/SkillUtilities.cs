using System.Linq;

namespace Snowship.NColonist
{
	public static class SkillUtilities
	{
		private static IColonistQuery ColonistQuery => GameManager.Get<IColonistQuery>();

		public static SkillInstance GetBestColonistAtSkill(SkillPrefab skill) {

			Colonist firstColonist = ColonistQuery.Colonists.FirstOrDefault();
			if (firstColonist == null) {
				return null;
			}
			SkillInstance highestSkillInstance = firstColonist.Skills.AsList().FirstOrDefault(s => s.prefab == skill);
			if (highestSkillInstance == null) {
				return null;
			}
			float highestSkillValue = highestSkillInstance.CalculateTotalSkillLevel();

			foreach (Colonist otherColonist in ColonistQuery.Colonists.Skip(1)) {
				SkillInstance otherColonistSkillInstance = otherColonist.Skills.AsList().FirstOrDefault(s => s.prefab == skill);
				if (otherColonistSkillInstance == null) {
					continue;
				}
				float otherColonistSkillValue = otherColonistSkillInstance.CalculateTotalSkillLevel();
				if (otherColonistSkillValue > highestSkillValue) {
					highestSkillValue = otherColonistSkillValue;
					highestSkillInstance = otherColonistSkillInstance;
				}
			}

			return highestSkillInstance;
		}
	}
}
