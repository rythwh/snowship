using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NLife;

namespace Snowship.NHuman
{
	public class HumanData : LifeData
	{
		public Gender Gender { get; }
		public List<(ESkill, float)> Skills { get; }

		public HumanData() {
			Name = GameManager.Get<HumanManager>().GetName(Gender);
		}
	}
}
