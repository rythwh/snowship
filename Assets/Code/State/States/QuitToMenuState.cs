using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Snowship.NState.States
{
	public class QuitToMenuState : State<EState>
	{
		public override EState Type => EState.QuitToMenu;
		public override EState[] ValidNextStates { get; } = { EState.MainMenu };
		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition => null;
	}
}
