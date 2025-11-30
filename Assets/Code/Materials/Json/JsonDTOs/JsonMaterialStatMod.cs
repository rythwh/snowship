using Newtonsoft.Json;

namespace Snowship.NMaterial
{
	public sealed class JsonMaterialStatMod
	{
		[JsonProperty("stat")] public EStat Stat { get; set; }
		[JsonProperty("operation")] public EStatOperation Operation { get; set; }
		[JsonProperty("value")] public float Value { get; set; }
	}
}
