using System.Linq;

namespace Snowship.NColonist
{
	public static class SkillUtilities
	{
		private static ColonistManager ColonistM => GameManager.Get<ColonistManager>();

		public static SkillInstance GetBestColonistAtSkill(SkillPrefab skill) {

			Colonist firstColonist = ColonistM.Colonists.FirstOrDefault();
			if (firstColonist == null) {
				return null;
			}
			SkillInstance highestSkillInstance = firstColonist.Skills.AsList().FirstOrDefault(s => s.prefab == skill);
			if (highestSkillInstance == null) {
				return null;
			}
			float highestSkillValue = highestSkillInstance.CalculateTotalSkillLevel();

			foreach (Colonist otherColonist in ColonistM.Colonists.Skip(1)) {
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
