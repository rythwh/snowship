using Newtonsoft.Json;

namespace Snowship.NMaterial
{
	public sealed class JsonMaterialAmount
	{
		public const string IdPropertyName = "id";
		public const string AmountPropertyName = "amount";

		[JsonProperty(IdPropertyName)] public string Id { get; set; }
		[JsonProperty(AmountPropertyName)] public int Amount { get; set; }
	}
}
