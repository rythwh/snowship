using System;

namespace Snowship.Persistence
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class SaveableAttribute : Attribute
	{
		public string TypeKey { get; }
		public int Version { get; }

		public SaveableAttribute(string typeKey, int version) {
			TypeKey = typeKey;
			Version = version;
		}
	}
}
