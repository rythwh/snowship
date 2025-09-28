using System;
using System.Collections.Generic;

namespace Snowship.Persistence
{
	[Serializable]
	public sealed class PersistenceFile
	{
		public int Version = 1;
		public List<ObjectRecord> Objects = new();
		public List<string> Roots = new();
	}
}