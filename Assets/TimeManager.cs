using System;
using UnityEngine;

public class TimeManager : BaseManager {

	public static readonly int dayLengthSeconds = 1440; // Number of seconds in 1 in-game day

	private bool paused;

	private int pauseTimeModifier = 0;
	private int timeModifier = 0;

	public int GetTimeModifier() {
		return timeModifier;
	}

	public float deltaTime;

	private float timer = 0;

	private int minute = 0;
	private int hour = 8;
	private int day = 1;
	private int month = 1;
	private int year = 1;

	public bool isDay;

	public bool minuteChanged;

	public float tileBrightnessTime = 0;

	public override void Update() {
		tileBrightnessTime = CalculateTileBrightnessTime();
		if (GameManager.tileM.mapState == TileManager.MapState.Generated) {
			if (Input.GetKeyDown(KeyCode.Alpha1) && !GameManager.uiM.pauseMenu.activeSelf && !GameManager.uiM.playerTyping) {
				if (timeModifier > 0) {
					timeModifier -= 1;
					if (timeModifier <= 0) {
						timeModifier = 1;
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && !GameManager.uiM.pauseMenu.activeSelf && !GameManager.uiM.playerTyping) {
				timeModifier += 1;
			}
			timeModifier = Mathf.Clamp(timeModifier, 0, 3);
			if (timeModifier > 0 && paused) {
				TogglePause();
			}
			pauseTimeModifier = Mathf.Clamp(pauseTimeModifier, 0, 5);
			if (Input.GetKeyDown(KeyCode.Space) && !GameManager.uiM.pauseMenu.activeSelf && !GameManager.uiM.playerTyping) {
				TogglePause();
			}
			deltaTime = Time.deltaTime * timeModifier;

			timer += deltaTime;
			minuteChanged = false;
			if (timer >= 1) {
				minute += 1;
				timer = 0;
				if (minute % 10 == 0) {
					GameManager.colonyM.colony.map.SetTileBrightness(tileBrightnessTime, false);
				}
				minuteChanged = true;
				if (minute >= 60) {
					hour += 1;
					minute = 0;
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

			GameManager.uiM.UpdateDateTimeInformation(minute, hour, day, month, year, isDay);
		}
	}

	public void TogglePause() {
		paused = !paused;
		if (paused) {
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

	public void SetPaused(bool newPausedState) {
		paused = !newPausedState;
		TogglePause();
	}

	public bool GetPaused() {
		return paused;
	}

	public int Get12HourTime() {
		return Mathf.FloorToInt(1 + (12 - (1 - hour)) % 12);
	}

	private float CalculateTileBrightnessTime() {
		return (float)Math.Round(hour + (minute / 60f), 2);
	}

	public void SetTime(float time) {
		hour = Mathf.FloorToInt(time);
		minute = Mathf.RoundToInt((time - hour) * 60);
		Update();
	}

	public string GetDateString() {
		return (day + "," + month + "," + year);
	}

	public int GetMinute() {
		return minute;
	}

	public void SetMinute(int minute) {
		this.minute = minute;
	}

	public int GetHour() {
		return hour;
	}

	public void SetHour(int hour) {
		this.hour = hour;
	}

	public int GetDay() {
		return day;
	}

	public void SetDay(int day) {
		this.day = day;
	}

	public int GetMonth() {
		return month;
	}

	public void SetMonth(int month) {
		this.month = month;
	}

	public int GetYear() {
		return year;
	}

	public void SetYear(int year) {
		this.year = year;
	}
}