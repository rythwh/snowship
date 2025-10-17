using System.Collections.Generic;

namespace Snowship.NEntity
{
	public interface IStatProvider
	{
		void CollectModifiers(List<StatModifier> modifiers);
	}
}
