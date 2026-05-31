using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snowship.NEntity
{
	public sealed class JsonEntity
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("category")] public string Category { get; set; }
		[JsonProperty("sub_category")] public string SubCategory { get; set; }
		[JsonProperty("is_abstract")] public bool IsAbstract { get; set; }

		[JsonProperty("components")] public List<JsonEntityComponent> Components { get; set; } = new();
		[JsonProperty("variations")] public List<JsonEntityVariation> Variations { get; set; } = new();
	}
}
