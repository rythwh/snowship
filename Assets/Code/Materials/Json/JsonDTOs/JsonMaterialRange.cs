using Newtonsoft.Json;

namespace Snowship.NMaterial
{
	public sealed class JsonMaterialRange
	{
		public const string MaterialIdPropertyName = "material_id";
		public const string LowerLimitPropertyName = "lower_limit";
		public const string UpperLimitPropertyName = "upper_limit";

		[JsonProperty(MaterialIdPropertyName)] public string MaterialId { get; set; }
		[JsonProperty(LowerLimitPropertyName)] public int LowerLimit { get; set; }
		[JsonProperty(UpperLimitPropertyName)] public int UpperLimit { get; set; }
	}
}
