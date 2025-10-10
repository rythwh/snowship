using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.EventSystems;
using VContainer.Unity;

namespace Snowship.NInput
{
	[UsedImplicitly]
	public class InputManager : IInitializable, ITickable {

		private InputSystemActions inputSystemActions;
		public InputSystemActions InputSystemActions => inputSystemActions ??= new InputSystemActions();

		public bool PointerOverUI { get; private set; }

		public event Action<InputSystemActions> OnInputSystemEnabled;
		public event Action<InputSystemActions> OnInputSystemDisabled;

		public void Initialize() {
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
