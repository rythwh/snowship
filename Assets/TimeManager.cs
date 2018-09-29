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

	private bool paused;

	private int pauseTimeModifier = 0;
	private int timeModifier = 0;

	public int GetTimeModifier() {
		return timeModifier;
	}

	public float deltaTime;

	private float timer = 0;

	private int minute = 0;
	private int hour = 7;
	private int day = 1;
	private int month = 1;
	private int year = 1;

	public bool isDay;

	public bool minuteChanged;

	public float tileBrightnessTime = 0;

	void Update() {
		tileBrightnessTime = CalculateTileBrightnessTime();
		if (tileM.generated) {
			if (Input.GetKeyDown(KeyCode.Alpha1) && !uiM.pauseMenu.activeSelf && !debugM.debugMode) {
				if (timeModifier > 0) {
					timeModifier -= 1;
					if (timeModifier <= 0) {
						timeModifier = 1;
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && !uiM.pauseMenu.activeSelf && !debugM.debugMode) {
				timeModifier += 1;
			}
			timeModifier = Mathf.Clamp(timeModifier, 0, 5);
			if (timeModifier > 0 && paused) {
				TogglePause();
			}
			pauseTimeModifier = Mathf.Clamp(pauseTimeModifier, 0, 5);
			if (Input.GetKeyDown(KeyCode.Space) && !uiM.pauseMenu.activeSelf && !debugM.debugMode) {
				TogglePause();
			}
			deltaTime = Time.deltaTime * timeModifier;

			timer += deltaTime;
			minuteChanged = false;
			if (timer >= 1) {
				minute += 1;
				timer = 0;
				if (minute % 10 == 0) {
					tileM.map.SetTileBrightness(tileBrightnessTime);
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

			uiM.UpdateDateTimeInformation(minute, hour, day, month, year, isDay);
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

	public int GetHour() {
		return hour;
	}

	private float CalculateTileBrightnessTime() {
		return (float)System.Math.Round(hour + (minute / 60f), 2);
	}

	public void SetTime(float time) {
		hour = Mathf.FloorToInt(time);
		minute = Mathf.RoundToInt((time - hour) * 60);
	}

	public string GetDateString() {
		return (day + "," + month + "," + year);
	}

	public void SetDate(int day, int month, int year) {
		this.day = day;
		this.month = month;
		this.year = year;
	}
}
