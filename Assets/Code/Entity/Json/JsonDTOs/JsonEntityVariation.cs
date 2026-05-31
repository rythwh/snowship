using System.Collections.Generic;
using Newtonsoft.Json;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class JsonEntityVariation
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("name")] public string Name { get; set; }

		[JsonProperty("required_materials")] public List<JsonMaterialAmount> RequiredMaterials { get; set; }
		[JsonProperty("returned_materials")] public List<JsonMaterialRange> ReturnedMaterials { get; set; }

		[JsonProperty("components")] public List<JsonEntityComponent> Components { get; set; } = new();
	}
}
