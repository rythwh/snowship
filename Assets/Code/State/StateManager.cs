using System;
using Cysharp.Threading.Tasks;
using Snowship.NUI;
using UnityEngine;

namespace Snowship.NState {
	public partial class StateManager : IManager {
		private State<EState> state;

		public EState State => state.Type;

		public bool StateChangeLocked { get; private set; }

		public event Action<(EState previousState, EState newState)> OnStateChanged;

		public async void OnCreate() {
			await TransitionToState(EState.Boot);
			await TransitionToState(EState.MainMenu);
		}

		public async UniTask TransitionToState(EState newState, ETransitionUIAction transitionUIAction = ETransitionUIAction.Close) {

			Debug.Log($"{state?.Type} -> {newState} ({transitionUIAction})");

			if (StateChangeLocked) {
				return;
			}

			state ??= states[newState];

			if (state.Type == newState) {
				Debug.LogWarning($"Trying to transition to the state we're already in: {state.Type} -> {newState}");
				return;
			}

			if (!state.ValidNextStates.Contains(newState)) {
				Debug.LogWarning($"Trying to transition to a illegal next state: {state.Type} -> {newState}.");
				return;
			}

			EState previousState = state.Type;
			state = states[newState];

			switch (transitionUIAction) {
				case ETransitionUIAction.Nothing:
					break;
				case ETransitionUIAction.Hide:
					GameManager.Get<UIManager>().ToggleAllViews();
					break;
				case ETransitionUIAction.Close:
					GameManager.Get<UIManager>().CloseAllViews();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(transitionUIAction), transitionUIAction, null);
			}

			foreach (Func<UniTask> action in state.ActionsOnTransition) {
				await action();
			}

			OnStateChanged?.Invoke((previousState, state.Type));
		}

		public void LockStateChange() {
			StateChangeLocked = true;
		}

		public void UnlockStateChange() {
			StateChangeLocked = false;
		}
	}

}
