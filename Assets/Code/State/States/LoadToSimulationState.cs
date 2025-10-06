using System;
using Cysharp.Threading.Tasks;
using Snowship.NUI;
using VContainer;

namespace Snowship.NState.States
{
	public class LoadToSimulationState : State<EState>
	{
		public override EState Type => EState.LoadToSimulation;
		public override EState[] ValidNextStates { get; } = { EState.Simulation };

		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition { get; } = {
			async resolver => await resolver.Resolve<UIManager>().OpenViewAsync<UILoadingScreen>()
		};
	}
}
