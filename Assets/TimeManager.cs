using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

	private UIManager uiM;

	void Awake() {
		uiM = GetComponent<UIManager>();
	}

	public int pauseTimeModifier = 0;
	public int timeModifier = 0;

	public float deltaTime;

	private float timer = 0;

	private int minute = 0;
	private int hour = 6;
	private int day = 1;
	private int month = 1;
	private int year = 1;

	public bool isDay;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			if (timeModifier > 0) {
				timeModifier -= 1;
				if (timeModifier <= 0) {
					timeModifier = 1;
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			timeModifier += 1;
		}
		timeModifier = Mathf.Clamp(timeModifier,0,5);
		pauseTimeModifier = Mathf.Clamp(pauseTimeModifier,0,5);
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

		timer += 1f * deltaTime;
		if (timer >= 1) {
			minute += 1;
			timer = 0;
			if (minute >= 60) {
				minute = 1;
				hour += 1;
				if (hour >= 24) {
					day += 1;
					hour = 0;
					if (day > 30) {
						month += 1;
						day = 1;
						if (month > 12) {
							year += 1;
							month = 1;
						}
					}
				}
			}
		}
		isDay = (hour >= 6 && hour <= 18);

		uiM.UpdateDateTimeInformation(minute,hour,day,month,year,isDay);
	}

	public int Get12HourTime() {
		return Mathf.FloorToInt(1 + (12 - (1 - hour)) % 12);
	}
}
