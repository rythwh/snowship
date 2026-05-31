using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Snowship.NEntity
{
	public sealed class JsonEntityComponent
	{
		[JsonProperty("type")] public EEntityComponent Type { get; set; }
		[JsonProperty("data")] public JObject Data { get; set; }
	}
}
