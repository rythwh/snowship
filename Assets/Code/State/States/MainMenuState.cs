using System;
using Cysharp.Threading.Tasks;
using Snowship.NUI;
using VContainer;

namespace Snowship.NState.States
{
	public class MainMenuState : State<EState>
	{
		public override EState Type => EState.MainMenu;
		public override EState[] ValidNextStates { get; } = { EState.LoadToSimulation, EState.QuitToDesktop };

		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition { get; } = {
			async resolver => await resolver.Resolve<UIManager>().OpenViewAsync<UIMainMenu>()
		};
	}
}
