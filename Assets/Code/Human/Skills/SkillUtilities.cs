using System.Linq;

namespace Snowship.NColonist
{
	public static class SkillUtilities
	{
		public static SkillInstance GetBestColonistAtSkill(SkillPrefab skill) {

			Colonist firstColonist = Colonist.colonists.FirstOrDefault();
			if (firstColonist == null) {
				return null;
			}
			SkillInstance highestSkillInstance = firstColonist.Skills.AsList().FirstOrDefault(s => s.prefab == skill);
			if (highestSkillInstance == null) {
				return null;
			}
			float highestSkillValue = highestSkillInstance.CalculateTotalSkillLevel();

			foreach (Colonist otherColonist in Colonist.colonists.Skip(1)) {
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