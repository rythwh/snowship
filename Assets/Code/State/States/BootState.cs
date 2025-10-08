using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Snowship.NState.States
{
	public class BootState : State<EState>
	{
		public override EState Type => EState.Boot;
		public override EState[] ValidNextStates { get; } = { EState.MainMenu };
		public override Func<IObjectResolver, UniTask>[] ActionsOnTransition => null;
	}
}
