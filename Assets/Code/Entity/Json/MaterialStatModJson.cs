using Newtonsoft.Json;

namespace Snowship.NEntity
{
	public sealed class MaterialStatModJson
	{
		[JsonProperty("stat")] public string Stat { get; set; }
		[JsonProperty("op")] public string Op { get; set; }
		[JsonProperty("value")] public float Value { get; set; }
	}
}