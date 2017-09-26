using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

	private UIManager uiM;
	private TileManager tileM;
	private DebugManager debugM;

	void Awake() {
		uiM = GetComponent<UIManager>();
		tileM = GetComponent<TileManager>();
		debugM = GetComponent<DebugManager>();
	}

	public int pauseTimeModifier = 0;
	public int timeModifier = 0;

	public float deltaTime;

	private float timer = 0;

	private int minute = 0;
	private int hour = 8;
	private int day = 1;
	private int month = 1;
	private int year = 1;

	public bool isDay;

	public bool minuteChanged;

	void Update() {
		if (tileM.generated) {
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
			timeModifier = Mathf.Clamp(timeModifier, 0, 5);
			pauseTimeModifier = Mathf.Clamp(pauseTimeModifier, 0, 5);
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

			timer += deltaTime;
			minuteChanged = false;
			if (timer >= 1) {
				minute += 1;
				timer = 0;
				if (minute % 10 == 0) {
					tileM.map.SetTileBrightness(GetTileBrightnessTime());
				}
				minuteChanged = true;
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

			uiM.UpdateDateTimeInformation(minute, hour, day, month, year, isDay);
		}
	}

	public int Get12HourTime() {
		return Mathf.FloorToInt(1 + (12 - (1 - hour)) % 12);
	}

	public int GetHour() {
		return hour;
	}

	public float GetTileBrightnessTime() {
		return (float)System.Math.Round(hour + (minute / 60f),2);
	}

	public void SetTime(float time) {
		hour = Mathf.FloorToInt(time);
		minute = Mathf.RoundToInt((time - hour) * 60);
	}
}
