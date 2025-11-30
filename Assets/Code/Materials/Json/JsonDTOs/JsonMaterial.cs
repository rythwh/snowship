using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snowship.NMaterial
{
	public sealed class JsonMaterial
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("class")] public string ClassId { get; set; }

		[JsonProperty("weight")] public int Weight { get; set; }
		[JsonProperty("volume")] public int Volume { get; set; }
		[JsonProperty("value")] public int Value { get; set; }

		[JsonProperty("modifiers")] public List<JsonMaterialStatMod> Modifiers { get; set; } = new();
		[JsonProperty("traits")] public List<JsonMaterialTrait> Traits { get; set; } = new();
	}
}
