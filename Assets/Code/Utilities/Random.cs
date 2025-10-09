using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Snowship
{
	public static class Random
	{
		private static Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex(GenerateInitialSeed());

		private static uint GenerateInitialSeed() {
			byte[] b = new byte[4];
			RandomNumberGenerator.Fill(b);
			uint seed = BitConverter.ToUInt32(b, 0);
			if (seed == 0u) {
				seed = 1u;
			}
			if (seed == uint.MaxValue) {
				seed = 1u;
			}
			return seed;
		}

		public static void Seed(uint seed) {
			uint safeSeed = seed == 0u ? 1u : seed;
			random = Unity.Mathematics.Random.CreateFromIndex(safeSeed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Range(int minInclusive, int maxExclusive) {
			if (minInclusive > maxExclusive) {
				(minInclusive, maxExclusive) = (maxExclusive, minInclusive);
			}
			return random.NextInt(minInclusive, maxExclusive);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Range(float minInclusive, float maxExclusive) {
			if (minInclusive > maxExclusive) {
				(minInclusive, maxExclusive) = (maxExclusive, minInclusive);
			}
			return random.NextFloat(minInclusive, maxExclusive);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unit() {
			return random.NextFloat(); // [0,1)
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Chance(float probability) {
			if (probability <= 0f) {
				return false;
			}
			if (probability >= 1f) {
				return true;
			}
			return random.NextFloat() < probability;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool NextBool() {
			return random.NextInt(2) == 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign() {
			return NextBool() ? 1 : -1;
		}

		// Fisher–Yates
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			for (int i = n - 1; i > 0; i--) {
				int j = Range(0, i + 1);
				(list[i], list[j]) = (list[j], list[i]);
			}
		}


		// --------- IReadOnlyList<T> ---------
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRandomElement<T>(this IReadOnlyList<T> list, out T element) {
			if (list is { Count: > 0 }) {
				element = list[Range(0, list.Count)];
				return true;
			}
			element = default(T);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RandomElement<T>(this IReadOnlyList<T> list) {
			if (TryRandomElement(list, out T element)) {
				return element;
			}
			throw new InvalidOperationException("Provided List is null or empty.");
		}

		// --------- HashSet<T> ---------
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRandomElement<T>(this HashSet<T> set, out T element) {
			if (set == null || set.Count == 0) {
				element = default(T);
				return false;
			}

			int target = Range(0, set.Count);
			int i = 0;
			foreach (T item in set) {
				if (i == target) {
					element = item;
					return true;
				}
				i++;
			}

			element = default(T);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RandomElement<T>(this HashSet<T> hashSet) {
			if (TryRandomElement(hashSet, out T element)) {
				return element;
			}
			throw new InvalidOperationException("Provided HashSet is null or empty.");
		}

		// --------- Array T[] ---------
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRandomElement<T>(this T[] array, out T element) {
			if (array == null || array.Length == 0) {
				element = default(T);
				return false;
			}
			element = array[Range(0, array.Length)];
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RandomElement<T>(this T[] array) {
			if (TryRandomElement(array, out T element)) {
				return element;
			}
			throw new InvalidOperationException("Provided Array is null or empty.");
		}

		// --------- IEnumerable<T> ---------
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRandomElement<T>(this IEnumerable<T> seq, out T element) {
			element = default(T);

			if (seq == null) {
				return false;
			}

			int seen = 0;
			foreach (T item in seq) {
				seen++;
				if (Range(0, seen) == 0) {
					element = item;
				}
			}

			if (seen == 0) {
				return false;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RandomElement<T>(this IEnumerable<T> seq) {
			if (seq is IReadOnlyList<T> list) {
				return RandomElement(list);
			}
			if (seq is HashSet<T> hashSet) {
				return RandomElement(hashSet);
			}
			if (TryRandomElement(seq, out T element)) {
				return element;
			}
			throw new InvalidOperationException("Provided Sequence is null or empty.");
		}

		// --------- Enum ---------
		private static class EnumCache<T> where T : struct, Enum
		{
			public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RandomElement<T>() where T : struct, Enum {
			T[] values = EnumCache<T>.Values;
			return values.RandomElement();
		}
	}
}
