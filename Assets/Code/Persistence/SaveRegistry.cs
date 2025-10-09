using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Snowship.NPersistence
{
	public sealed class SaveTypeInfo
	{
		public string Key { get; }
		public Type Type { get; }
		public int Version { get; }

		public SaveTypeInfo(string key, Type type, int version) {
			Key = key;
			Type = type;
			Version = version;
		}
	}

	public static class SaveRegistry
	{
		private static readonly Dictionary<string, SaveTypeInfo> keyToInfo = new();
		private static readonly Dictionary<Type, SaveTypeInfo> typeToInfo = new();

		private static bool initialized;

		public static void EnsureInitialized() {
			if (initialized) {
				return;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies) {
				Type[] types = GetTypesSafe(assembly);
				foreach (Type type in types) {
					if (!typeof(ISaveable).IsAssignableFrom(type)) {
						continue;
					}
					object[] attributes = type.GetCustomAttributes(typeof(SaveableAttribute), false);
					if (attributes.Length <= 0) {
						continue;
					}
					if (attributes.FirstOrDefault() is not SaveableAttribute saveableAttribute) {
						continue;
					}

					SaveTypeInfo saveTypeInfo = new SaveTypeInfo(
						saveableAttribute.TypeKey,
						type,
						saveableAttribute.Version
					);

					keyToInfo[saveableAttribute.TypeKey] = saveTypeInfo;
					typeToInfo[type] = saveTypeInfo;
				}
			}

			initialized = true;
		}

		public static SaveTypeInfo GetInfo(string typeKey) {
			EnsureInitialized();
			return keyToInfo[typeKey];
		}

		public static SaveTypeInfo GetInfo(Type type) {
			EnsureInitialized();
			return typeToInfo[type];
		}

		private static Type[] GetTypesSafe(Assembly assembly) {
			try {
				return assembly.GetTypes();
			} catch (ReflectionTypeLoadException ex) {
				return ex.Types.Where(x => x != null).ToArray();
			}
		}
	}

	// Ensures object keys in dictionaries are compared by identity.
	internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
		public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
		public int GetHashCode(object obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
	}
}
