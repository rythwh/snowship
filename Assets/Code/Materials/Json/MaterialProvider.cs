using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Snowship.NEntity;
using UnityEngine;
using VContainer.Unity;

namespace Snowship.NMaterial
{
	public interface IMaterialProvider
	{
		MaterialRegistry Registry { get; }
		UniTask WhenReadyAsync(CancellationToken cancellationToken);
	}

	[UsedImplicitly]
	public class MaterialProvider : IAsyncStartable, IMaterialProvider
	{
		private readonly ItemTraitFactoryRegistry traitFactoryRegistry;
		private readonly string fileName;

		public MaterialRegistry Registry { get; private set; }
		private UniTaskCompletionSource readyTcs;

		public MaterialProvider(ItemTraitFactoryRegistry traitFactoryRegistry)
		{
			this.traitFactoryRegistry = traitFactoryRegistry;
			fileName = "BaseGame/Data/materials.json"; // TODO Modding - Create different "sources" for loading multiple materials files
			readyTcs = new UniTaskCompletionSource();
		}

		public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
		{
			string path = Path.Combine(Application.streamingAssetsPath, fileName);

			string json = await File.ReadAllTextAsync(path, cancellation);
			BuildRegistry(json);

			readyTcs.TrySetResult();
		}

		public UniTask WhenReadyAsync(CancellationToken cancellationToken)
		{
			return readyTcs.Task.AttachExternalCancellation(cancellationToken);
		}

		private void BuildRegistry(string json)
		{
			List<JsonMaterial> jsonMaterials = JsonConvert.DeserializeObject<List<JsonMaterial>>(json);
			JsonMaterialLoader loader = new JsonMaterialLoader(traitFactoryRegistry);

			List<Material> materials = new();
			foreach (JsonMaterial jsonMaterial in jsonMaterials) {
				Material material = loader.JsonToMaterial(jsonMaterial);
				materials.Add(material);
				Debug.Log($"Loaded material Id:{material.Id} Class:{material.ClassId} Val:{material.Value} Vol:{material.Volume} W:{material.Weight} with {material.Modifiers.Count} modifiers ({string.Join(" ", material.Modifiers)}) and {material.Traits.Count} traits ({string.Join(" ", material.Traits)})");
			}

			Registry = new MaterialRegistry(materials);
		}
	}
}
