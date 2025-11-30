using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Snowship.NEntity
{
	public sealed class JsonComponent
	{
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("data")] public JToken Data { get; set; }
	}
}
