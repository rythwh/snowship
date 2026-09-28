using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NEntity
{
	public sealed class JsonEntity : IGroupItem
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("category")] public string Category { get; set; }
		[JsonProperty("sub_category")] public string SubCategory { get; set; }
		[JsonProperty("is_abstract")] public bool IsAbstract { get; set; }

		[JsonProperty("components")] public List<JsonEntityComponent> Components { get; set; } = new();
		[JsonProperty("variations")] public List<JsonEntityVariation> Variations { get; set; } = new();

		[JsonIgnore]
		public Sprite Icon {
			get {
				if (icon == null) {
					JsonEntityComponent rendering = Components.FirstOrDefault(c => c.Type == EEntityComponent.Rendering);
					if (rendering != null && rendering.Data != null && rendering.Data.TryGetValue("sprite_path", out Newtonsoft.Json.Linq.JToken path)) {
						icon = Resources.Load<Sprite>(path.ToString());
					}
				}
				return icon;
			}
			set => icon = value;
		}
		private Sprite icon;

		[JsonIgnore]
		public List<IGroupItem> Children => null;
	}
}
