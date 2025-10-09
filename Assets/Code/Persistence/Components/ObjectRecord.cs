using System;

namespace Snowship.NPersistence
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
