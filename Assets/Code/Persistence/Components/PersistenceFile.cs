using System;
using System.Collections.Generic;

namespace Snowship.NPersistence
{
	[Serializable]
	public sealed class PersistenceFile
	{
		public int Version = 1;
		public List<ObjectRecord> Objects = new();
		public List<string> Roots = new();
	}
}
