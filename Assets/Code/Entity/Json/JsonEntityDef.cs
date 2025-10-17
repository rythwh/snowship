using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snowship.NEntity
{
	public sealed class JsonEntityDef
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("components")] public List<JsonComponentDef> Components { get; set; }
	}
}
