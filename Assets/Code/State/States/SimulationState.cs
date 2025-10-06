using System;
using Cysharp.Threading.Tasks;
using Snowship.NUI;
using VContainer;

namespace Snowship.NState.States
{
	public class SimulationState : State<EState>
	{
		public override EState Type => EState.Simulation;
		public override EState[] ValidNextStates { get; } = { EState.PauseMenu, EState.Saving, EState.QuitToMenu, EState.QuitToDesktop };

		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition { get; } = {
			async resolver => await resolver.Resolve<UIManager>().OpenViewAsync<UISimulation>()
		};
	}
}
