using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snowship.NEntity
{
	public sealed class JsonEntity
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("components")] public List<JsonComponent> Components { get; set; }
	}
}
