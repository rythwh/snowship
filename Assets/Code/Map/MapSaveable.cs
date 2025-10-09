using Snowship.NPersistence;

namespace Snowship.NMap
{
	[Saveable("Map", 1)]
	public partial class Map : ISaveable
	{
		public string Id { get; set; }

		private sealed class MapDto
		{

		}

		public object Save(SaveContext context) {
			context.Store(Id, this);
			return new MapDto();
		}

		public void Load(LoadContext context, object data, int typeVersion) {
			throw new System.NotImplementedException();
		}

		public void OnAfterLoad(LoadContext context) {
			throw new System.NotImplementedException();
		}
	}
}
