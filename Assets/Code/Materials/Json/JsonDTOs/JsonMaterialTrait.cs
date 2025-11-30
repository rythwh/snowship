using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Snowship.NMaterial
{
	public sealed class JsonMaterialTrait
	{
		[JsonProperty("type")] public ETrait Type;
		[JsonProperty("data")] public JObject Data;
	}
}
