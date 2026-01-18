using Newtonsoft.Json.Linq;

namespace Snowship.NMaterial
{
	public class MaterialTrait
	{
		public string Id { get; }
		public JObject Data { get; }

		public MaterialTrait(string id, JObject data)
		{
			Id = id;
			Data = data ?? new JObject();
		}
	}
}
