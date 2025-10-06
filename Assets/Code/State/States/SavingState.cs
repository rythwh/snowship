using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Snowship.NState.States
{
	public class SavingState : State<EState>
	{
		public override EState Type => EState.Saving;
		public override EState[] ValidNextStates { get; } = { EState.PauseMenu, EState.Simulation };
		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition => null;
	}
}
