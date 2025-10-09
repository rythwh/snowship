using System.Collections.Generic;

namespace Snowship.NPersistence
{
	public interface ISaveRootProvider
	{
		IEnumerable<ISaveable> GetAllSaveables();
	}
}
