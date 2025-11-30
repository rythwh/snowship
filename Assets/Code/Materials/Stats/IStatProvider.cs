using System.Collections.Generic;

namespace Snowship.NMaterial
{
	public interface IStatProvider
	{
		void CollectModifiers(List<StatModifier> modifiers);
	}
}
