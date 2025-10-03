using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Snowship.NColony;
using Snowship.NMap.NTile;
using Snowship.NPlanet;
using Snowship.NUI;
using UnityEngine;

namespace Snowship.NState {
	public partial class StateManager : Manager {
		private State<EState> state;

		public EState? State => state?.Type;

		public bool StateChangeLocked { get; private set; }

		public event Action<(EState previousState, EState newState)> OnStateChanged;

		public override void OnCreate() {
			SetInitialState();
		}

		private async void SetInitialState() {
			await TransitionToState(EState.Boot);
			await TransitionToState(EState.MainMenu);

			#if UNITY_EDITOR
			if (PlayerPrefs.GetInt("DebugSettings/Quick Start", 0) == 0) {
				return;
			}
			Planet planet = await GameManager.Get<PlanetManager>().CreatePlanet(new CreatePlanetData());

			const string colonyName = "Lumia";
			int seed = UnityEngine.Random.Range(0, int.MaxValue);
			const int size = 100;
			List<PlanetTile> filteredPlanetTiles = planet.planetTiles.Where(pt => pt.tile.tileType.classes[TileType.ClassEnum.Dirt]).ToList();
			PlanetTile planetTile = filteredPlanetTiles.ElementAt(UnityEngine.Random.Range(0, filteredPlanetTiles.Count));

			await GameManager.Get<ColonyManager>().CreateColony(new CreateColonyData(colonyName, seed, size, planetTile));
			#endif
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
					GameManager.Get<UIManager>().ToggleAllViews();
					break;
				case ETransitionUIAction.Close:
					GameManager.Get<UIManager>().CloseAllViews();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(transitionUIAction), transitionUIAction, null);
			}

			if (state.ActionsOnTransition != null) {
				foreach (Func<UniTask> action in state.ActionsOnTransition) {
					await action();
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
