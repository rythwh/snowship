using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Snowship.NMaterial;

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

		public string TryGetString(string name, string defaultValue)
		{
			if (data.TryGetValue(name, out JToken t)) {
				return t.Value<string>();
			}
			return defaultValue;
		}

		public int TryGetInt(string name, int defaultValue)
		{
			if (data.TryGetValue(name, out JToken t)) {
				return t.Value<int>();
			}
			return defaultValue;
		}

		public float TryGetFloat(string name, float defaultValue)
		{
			if (data.TryGetValue(name, out JToken token)) {
				return token.Type is JTokenType.Float or JTokenType.Integer
					? token.Value<float>()
					: float.Parse(token.Value<string>(), CultureInfo.InvariantCulture);
			}
			return defaultValue;
		}

		public bool TryGetBool(string key, bool defaultValue)
		{
			if (data.TryGetValue(key, out JToken token)) {
				return token.Value<bool>();
			}
			return defaultValue;
		}

		public TEnum TryGetEnum<TEnum>(string key) where TEnum : Enum
		{
			int enumInt = TryGetInt(key, 0);
			return (TEnum)Enum.Parse(typeof(TEnum), enumInt.ToString(), true);
		}

		public JArray TryGetArray(string key)
		{
			if (data.TryGetValue(key, out JToken token)) {
				return token as JArray;
			}
			return null;
		}

		public T TryGetObject<T>(string key) where T : JObject
		{
			if (data.TryGetValue(key, out JToken token)) {
				return token.Value<T>();
			}
			return null;
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

		public string PathStr(string key, string value, string defaultValue)
		{
			if (!TryGetObject(key, out JObject jObject) || !jObject.TryGetValue(value, out JToken jToken)) {
				return defaultValue;
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
