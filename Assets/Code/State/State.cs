using System;
using Cysharp.Threading.Tasks;
using Snowship.NUI;
using VContainer;

namespace Snowship.NState {
	public abstract class State<TStateEnum> where TStateEnum : Enum
	{
		public abstract TStateEnum Type { get; }
		public abstract TStateEnum[] ValidNextStates { get; }
		public abstract Func<IObjectResolver, UniTask>[] ActionsOnTransition { get; }
	}
}
