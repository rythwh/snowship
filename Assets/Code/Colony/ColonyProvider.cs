using System;
using JetBrains.Annotations;

namespace Snowship.NColony
{
	[UsedImplicitly]
	public class ColonyProvider : IColonyEvents
	{
		public event Action<Colony> OnColonyCreated;

		public void InvokeOnColonyCreated(Colony colony) => OnColonyCreated?.Invoke(colony);
	}

	public interface IColonyEvents
	{
		event Action<Colony> OnColonyCreated;

		void InvokeOnColonyCreated(Colony colony);
	}
}
