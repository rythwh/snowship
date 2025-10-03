using System.Threading;

// ReSharper disable All

namespace Snowship.NUtilities
{
	public static class IdProvider<T>
	{
		private static int nextId;

		public static int Next()
		{
			return Interlocked.Increment(ref nextId);
		}
	}

	public static class IdUtility
	{
		public static int NextIdFor<T>()
		{
			return IdProvider<T>.Next();
		}
	}
}
