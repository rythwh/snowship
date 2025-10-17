using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Snowship.NEntity
{
	public sealed class JsonArgs
	{
		private readonly JObject data;
		public JToken DataToken => data;

		public JsonArgs(JObject data)
		{
			this.data = data ?? new JObject();
		}

		public string Str(string name, string def)
		{
			if (data.TryGetValue(name, out JToken t)) {
				return t.Value<string>();
			}
			return def;
		}

		public int Int(string name, int def)
		{
			if (data.TryGetValue(name, out JToken t)) {
				return t.Value<int>();
			}
			return def;
		}

		public float Float(string name, float def)
		{
			if (data.TryGetValue(name, out JToken t)) {
				return t.Type is JTokenType.Float or JTokenType.Integer
					? t.Value<float>()
					: float.Parse(t.Value<string>(), CultureInfo.InvariantCulture);
			}
			return def;
		}

		public Dictionary<EStat, float> StatMapFromSelf()
		{
			Dictionary<EStat, float> dict = new(data.Count);
			foreach (KeyValuePair<string, JToken> kv in data) {
				if (!Enum.TryParse(kv.Key, true, out EStat id)) {
					continue;
				}
				float v = kv.Value.Type is JTokenType.Float or JTokenType.Integer
					? kv.Value.Value<float>()
					: float.Parse(kv.Value.Value<string>(), CultureInfo.InvariantCulture);
				dict[id] = v;
			}
			return dict;
		}

		public IReadOnlyList<string> PathStringList(string key, string value)
		{
			if (!TryGetObject(key, out JObject o) || !o.TryGetValue(value, out JToken t) || t.Type != JTokenType.Array) {
				return Array.Empty<string>();
			}
			JArray array = (JArray)t;
			List<string> list = new(array.Count);
			foreach (JToken token in array) {
				list.Add(token.Value<string>());
			}
			return list;
		}

		public string PathStr(string key, string value, string def)
		{
			if (!TryGetObject(key, out JObject jObject) || !jObject.TryGetValue(value, out JToken jToken)) {
				return def;
			}
			return jToken.Value<string>();
		}

		public JObject PathObject(string key, string value)
		{
			if (!TryGetObject(key, out JObject jObject)) {
				return null;
			}
			if (!jObject.TryGetValue(value, out JToken jToken) || jToken.Type != JTokenType.Object) {
				return null;
			}
			return (JObject)jToken;
		}

		public static List<Ingredient> ReadIngredientsArray(JArray ingredientsArray)
		{
			List<Ingredient> ingredients = new(ingredientsArray.Count);
			foreach (JToken token in ingredientsArray) {
				JObject jsonIngredient = (JObject)token;
				string id = (jsonIngredient["id"] ?? throw new InvalidOperationException($"Invalid id in ingredient def {jsonIngredient}")).Value<string>();
				int amount = (jsonIngredient["amount"] ?? throw new InvalidOperationException($"Invalid value in ingredient def {jsonIngredient}")).Value<int>();
				ingredients.Add(new Ingredient(id, amount));
			}
			return ingredients;
		}

		private bool TryGetObject(string key, out JObject obj)
		{
			if (!data.TryGetValue(key, out JToken token) || token.Type != JTokenType.Object) {
				obj = null;
				return false;
			}
			obj = (JObject)token;
			return true;
		}
	}
}
