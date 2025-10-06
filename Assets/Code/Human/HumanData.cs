using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NLife;

namespace Snowship.NHuman
{
	public class HumanData : LifeData
	{
		public List<(ESkill, float)> Skills { get; }

		public HumanData(
			string name,
			Gender gender,
			List<(ESkill, float)> skills
		)
			: base(name, gender)
		{
			Skills = skills;
		}
	}
}
