using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snowship.NEntity
{
	public sealed class MaterialJson
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("costMode")] public string CostMode { get; set; }
		[JsonProperty("replace")] public List<Ingredient> Replace { get; set; }
		[JsonProperty("mods")] public List<MaterialStatModJson> Mods { get; set; }
	}
}