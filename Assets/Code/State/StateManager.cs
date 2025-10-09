using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NState.States;
using Snowship.NUI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Snowship.NState
{
	[UsedImplicitly]
	public class StateManager : IStartable, IStateEvents, IStateQuery
	{
		private readonly IObjectResolver resolver;
		private readonly UIManager uiM;

		private Dictionary<EState, State<EState>> states = new();

		private State<EState> state;

		public EState? State => state?.Type;

		public bool StateChangeLocked { get; private set; }

		public event Action<(EState previousState, EState newState)> OnStateChanged;

		public StateManager(
			IObjectResolver resolver,
			UIManager uiM
		) {
			this.resolver = resolver;
			this.uiM = uiM;

			states.Add(EState.Boot, new BootState());
			states.Add(EState.MainMenu, new MainMenuState());
			states.Add(EState.LoadToSimulation, new LoadToSimulationState());
			states.Add(EState.Simulation, new SimulationState());
			states.Add(EState.PauseMenu, new PauseMenuState());
			states.Add(EState.Saving, new SavingState());
			states.Add(EState.QuitToMenu, new QuitToMenuState());
			states.Add(EState.QuitToDesktop, new QuitToDesktopState());
		}

		public void Start() {
			SetInitialState();
		}

		private async void SetInitialState() {
			await TransitionToState(EState.Boot);
			await TransitionToState(EState.MainMenu);
		}

		public async UniTask TransitionToState(EState newState, ETransitionUIAction transitionUIAction = ETransitionUIAction.Close) {

			Debug.Log($"{state?.Type} -> {newState} ({transitionUIAction})");

			if (StateChangeLocked) {
				return;
			}

			if (state == null) {
				state = states[newState];
			} else {
				if (state.Type == newState) {
					Debug.LogWarning($"Trying to transition to the state we're already in: {state.Type} -> {newState}");
					return;
				}

				if (!state.ValidNextStates.Contains(newState)) {
					Debug.LogWarning($"Trying to transition to a illegal next state: {state.Type} -> {newState}.");
					return;
				}
			}

			EState previousState = state.Type;
			state = states[newState];

			switch (transitionUIAction) {
				case ETransitionUIAction.Nothing:
					break;
				case ETransitionUIAction.Hide:
					uiM.ToggleAllViews();
					break;
				case ETransitionUIAction.Close:
					uiM.CloseAllViews();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(transitionUIAction), transitionUIAction, null);
			}

			if (state.ActionsOnTransition != null) {
				foreach (Func<IObjectResolver, UniTask> action in state.ActionsOnTransition) {
					await action(resolver);
				}
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
