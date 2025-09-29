using System.Collections.Generic;

namespace Snowship.Persistence
{
	public interface ISaveRootProvider
	{
		IEnumerable<ISaveable> GetAllSaveables();
	}
}
