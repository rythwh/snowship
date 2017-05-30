using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

	public int pauseTimeModifier = 0;
	public int timeModifier = 0;

	public float deltaTime;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			timeModifier -= 1;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			timeModifier += 1;
		}
		timeModifier = Mathf.Clamp(timeModifier,0,5);
		pauseTimeModifier = Mathf.Clamp(timeModifier,0,5);
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (timeModifier != 0) {
				pauseTimeModifier = timeModifier;
				timeModifier = 0;
			} else {
				if (pauseTimeModifier == 0) {
					timeModifier = 1;
				} else {
					timeModifier = pauseTimeModifier;
				}
			}
		}
		deltaTime = Time.deltaTime * timeModifier;
	}
}
