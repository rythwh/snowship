using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowship
{
	public static class Random
	{
		private static Unity.Mathematics.Random random = new Unity.Mathematics.Random();

		public static void Seed(uint seed) {
			random.state = seed;
		}

		public static int Range(int minInclusive, int maxExclusive) {
			if (minInclusive > maxExclusive) {
				(minInclusive, maxExclusive) = (maxExclusive, minInclusive);
			}
			return random.NextInt(minInclusive, maxExclusive);
		}

		public static float Range(float minInclusive, float maxExclusive) {
			if (minInclusive > maxExclusive) {
				(minInclusive, maxExclusive) = (maxExclusive, minInclusive);
			}
			return random.NextFloat(minInclusive, maxExclusive);
		}

		public static float Unit() {
			return random.NextFloat();
		}

		public static T RandomElement<T>(this List<T> list) {
			if (list.Count <= 0) {
				return default(T);
			}
			return list[Range(0, list.Count)];
		}

		public static T RandomElement<T>(this HashSet<T> hashSet) {
			List<T> list = hashSet.ToList();
			return list.RandomElement();
		}

		public static T RandomElement<T>(this T[] array) {
			return array[Range(0, array.Length)];
		}

		public static T RandomElement<T>(this IEnumerable<T> enumerable) {
			List<T> list = enumerable.ToList();
			return list[Range(0, list.Count)];
		}

		public static T RandomElement<T>() where T : struct, Enum {
			T[] values = (T[])Enum.GetValues(typeof(T));
			return values.RandomElement();
		}
	}
}
