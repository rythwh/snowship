using System;
using Snowship.NHuman;
using UnityEngine;

namespace Snowship.NColonist {
	public class NeedInstance {

		public Human human;
		public NeedPrefab prefab;
		private float value = 0;
		private int roundedValue = 0;

		public event Action<float, int> OnValueChanged;

		public NeedInstance(Human human, NeedPrefab prefab) {
			this.human = human;
			this.prefab = prefab;
		}

		public float GetValue() {
			return value;
		}

		public void SetValue(float newValue) {
			float oldValue = value;
			value = newValue;
			value = Mathf.Clamp(value, 0, prefab.clampValue);
			roundedValue = Mathf.RoundToInt(value / prefab.clampValue * 100);
			if (Mathf.RoundToInt(oldValue / prefab.clampValue * 100) != roundedValue) {
				OnValueChanged?.Invoke(value, roundedValue);
			}
		}

		public void ChangeValue(float amount) {
			//Debug.Log($"{prefab.name} : {value} + {amount}");
			SetValue(value + amount);
		}

		public int GetRoundedValue() {
			return roundedValue;
		}

	}
}