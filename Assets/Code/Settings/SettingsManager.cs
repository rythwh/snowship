using Snowship.NPersistence;

namespace Snowship.NSettings
{
	public class SettingsManager : IManager
	{
		public SettingsState SettingsState { get; set; } = new();
	}
}
