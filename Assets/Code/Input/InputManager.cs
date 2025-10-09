using System;
using TMPro;
using UnityEngine.EventSystems;
using VContainer.Unity;

namespace Snowship.NInput {
	public class InputManager : IStartable, ITickable {

		private InputSystemActions inputSystemActions;
		public InputSystemActions InputSystemActions {
			get => inputSystemActions ??= new InputSystemActions();
			private set => inputSystemActions = value;
		}

		public bool PointerOverUI { get; private set; }

		public event Action<InputSystemActions> OnInputSystemEnabled;
		public event Action<InputSystemActions> OnInputSystemDisabled;

		public void Start() {
			inputSystemActions = new InputSystemActions();

			EnableInputSystem();
		}

		public void Tick() {
			PointerOverUI = IsPointerOverUI();
		}

		private void EnableInputSystem() {
			inputSystemActions.Enable();
			OnInputSystemEnabled?.Invoke(inputSystemActions);
		}

		private void DisableInputSystem() {
			inputSystemActions.Disable();
			OnInputSystemDisabled?.Invoke(inputSystemActions);
		}

		public bool IsPlayerTyping() {
			return EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
		}

		public bool IsPointerOverUI() {
			return EventSystem.current.IsPointerOverGameObject();
		}
	}
}
