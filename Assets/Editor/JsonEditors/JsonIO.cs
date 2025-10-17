using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	internal static class JsonIO
	{
		public static T Load<T>(string path)
		{
			if (!File.Exists(path)) {
				throw new FileNotFoundException("JSON file not found", path);
			}

			string text = File.ReadAllText(path);
			JsonSerializerSettings settings = new() {
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			};
			return JsonConvert.DeserializeObject<T>(text, settings);
		}

		public static void Save<T>(string path, T data)
		{
			string dir = Path.GetDirectoryName(path) ?? Application.dataPath;
			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}

			JsonSerializerSettings settings = new() {
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			};

			string text = JsonConvert.SerializeObject(data, settings);
			File.WriteAllText(path, text);
			AssetDatabase.Refresh();
		}

		public static string PickJsonFile(string title, string defaultPath)
		{
			string path = EditorUtility.OpenFilePanel(title, string.IsNullOrEmpty(defaultPath) ? Application.dataPath : Path.GetDirectoryName(defaultPath), "json");
			return path;
		}

		public static string PickFolder(string title, string defaultPath)
		{
			string path = EditorUtility.OpenFolderPanel(title, string.IsNullOrEmpty(defaultPath) ? Application.dataPath : defaultPath, "");
			return path;
		}

		public static string SaveFile(string title, string defaultName, string defaultDir)
		{
			string dir = string.IsNullOrEmpty(defaultDir) ? Application.dataPath : defaultDir;
			string path = EditorUtility.SaveFilePanel(title, dir, defaultName, "json");
			return path;
		}
	}
}
