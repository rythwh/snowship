using Newtonsoft.Json.Linq;

namespace Snowship.NMaterial
{
	public class MaterialTraitDef
	{
		public string Id { get; }
		public JObject Data { get; }

		public MaterialTraitDef(string id, JObject data)
		{
			Id = id;
			Data = data ?? new JObject();
		}
	}
}
