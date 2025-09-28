using System;

namespace Snowship.Persistence
{
	[Serializable]
	public sealed class ObjectRecord
	{
		public string Id;
		public string TypeKey;
		public int TypeVersion;
		public object Data;
	}
}