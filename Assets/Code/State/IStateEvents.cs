using System;

namespace Snowship.NState
{
	public interface IStateEvents
	{
		event Action<(EState previousState, EState newState)> OnStateChanged;
	}
}
