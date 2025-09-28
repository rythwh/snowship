using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace Snowship.Persistence
{
	public class PersistenceManager : IManager
	{
		private readonly List<ISaveRootProvider> providers = new List<ISaveRootProvider>();

		public void AddProviders(IEnumerable<ISaveRootProvider> providersToAdd) {
			providers.AddRange(providersToAdd);
		}

		public void AddProviderAtRuntime(ISaveRootProvider provider) {
			providers.Add(provider);
		}

		public async UniTaskVoid SaveAsync(string path) {
			SaveRegistry.EnsureInitialized();

			// 1) Ask every provider for its objects and deduplicate by reference
			List<ISaveable> unique = CollectUniqueSaveables();

			// 2) Prepare contexts and assemble ObjectRecord list
			SaveContext saveContext = new SaveContext();
			List<ObjectRecord> records = new(unique.Count);

			// First pass: ensure every object has a stable ID and register with context (for cross-refs during Save()).
			foreach (ISaveable saveable in unique) {
				if (string.IsNullOrEmpty(saveable.Id)) {
					saveable.Id = Guid.NewGuid().ToString("N");
				}
				saveContext.Store(saveable.Id, saveable);
			}

			// Second pass: call Save() to produce DTOs now that IDs exist for all cross-refs
			foreach (ISaveable saveable in unique) {
				SaveTypeInfo info = SaveRegistry.GetInfo(saveable.GetType());
				string typeKey = info.Key;
				int typeVersion = info.Version;

				object data = saveable.Save(saveContext);

				ObjectRecord record = new ObjectRecord() {
					Id = saveable.Id,
					TypeKey = typeKey,
					TypeVersion = typeVersion,
					Data = data
				};
				records.Add(record);
			}

			// 3) Build the file and write JSON
			PersistenceFile file = new PersistenceFile {
				Version = 1,
				Objects = records
			};

			string json = JsonConvert.SerializeObject(
				file,
				Formatting.Indented,
				new JsonSerializerSettings { // TODO Why recreate each time?
					TypeNameHandling = TypeNameHandling.None,
					NullValueHandling = NullValueHandling.Ignore
				}
			);

			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
				Directory.CreateDirectory(directory);
			}

			await using StreamWriter writer = new StreamWriter(path);
			await writer.WriteAsync(json);
		}

		public async UniTask<bool> LoadAsync(string path) {
			if (!File.Exists(path)) {
				return false;
			}

			SaveRegistry.EnsureInitialized();

			// 1) Read file
			using StreamReader reader = new StreamReader(path);
			string json = await reader.ReadToEndAsync();

			PersistenceFile file = JsonConvert.DeserializeObject<PersistenceFile>(json);

			// 2) Phase A - create/locate instances and call Load() to fill simple fields
			Dictionary<string, object> idToObject = new(StringComparer.Ordinal);
			LoadContext loadContext = new LoadContext(idToObject);

			foreach (ObjectRecord record in file.Objects) {
				Type type = SaveRegistry.GetInfo(record.TypeKey).Type;

				// Create the instance.
				ISaveable instance = LocateOrCreateInstance(record.Id, type);

				// Register instance by ID before calling Load so others can resolve it during their Load if needed
				idToObject[record.Id] = instance;

				// Fill simple fields from DTO. Do NOT resolve references here.
				instance.Load(loadContext, record.Data, record.TypeVersion);

				// Defer cross-references fixups until every object has run Load.
				loadContext.EnqueuePostLoad(instance);
			}

			// 3) Phase B - resolve cross-references (cyclic-dependencies are safe now)
			loadContext.RunPostLoad();
			return true;
		}

		// Ask all providers for entities, merge, and de-duplicate by reference identity.
		private List<ISaveable> CollectUniqueSaveables() {

			// Gather from every provider (modules are free to overlap; de-duplicate handles it).
			List<ISaveable> saveablesWithDuplicates = new();
			foreach (ISaveRootProvider provider in providers) {
				IEnumerable<ISaveable> range = provider.GetAllSaveables();
				if (range == null) {
					continue;
				}
				saveablesWithDuplicates.AddRange(range);
			}

			// De-duplicate by instance ID (same reference -> single save record).
			List<ISaveable> uniqueSaveables = new();
			HashSet<object> seen = new(ReferenceEqualityComparer.Instance);
			foreach (ISaveable saveable in saveablesWithDuplicates) {
				if (saveable == null) {
					continue;
				}
				if (seen.Add(saveable)) {
					uniqueSaveables.Add(saveable);
				}
			}

			return uniqueSaveables;
		}

		// Construct or obtain an instance for a given Type and Id.
		private ISaveable LocateOrCreateInstance(string id, Type type) {
			if (Activator.CreateInstance(type) is not ISaveable instance) {
				throw new InvalidOperationException($"Type {type.FullName} does not implement ISaveable");
			}

			instance.Id = id;

			return instance;
		}
	}
}
