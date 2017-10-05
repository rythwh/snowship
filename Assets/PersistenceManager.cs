using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

public class PersistenceManager : MonoBehaviour {

	private int gameVersion;
	private int saveVersion;

	public int GetGameVersion() {
		return gameVersion;
	}

	public int GetSaveVersion() {
		return saveVersion;
	}

	private CameraManager cameraM; // Save the camera zoom and position
	private ColonistManager colonistM; // Save all instances of colonists, humans, life, etc.
	private JobManager jobM; // Save all non-taken jobs
	private TileManager tileM; // Save the map data
	private TimeManager timeM; // Save the date/time
	private UIManager uiM; // Save the planet data

	void Awake() {
		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();
		jobM = GetComponent<JobManager>();
		tileM = GetComponent<TileManager>();
		timeM = GetComponent<TimeManager>();
		uiM = GetComponent<UIManager>();
	}

	public void SaveSettings() {
		string settingsFilePath = Application.persistentDataPath + "/Settings/settings.txt";
		FileStream settingsFile = new FileStream(settingsFilePath, FileMode.OpenOrCreate);
		File.WriteAllText(@settingsFilePath, string.Empty);
	}

	public void LoadSettings() {

	}

	/* https://stackoverflow.com/questions/13266496/easily-write-a-whole-class-instance-to-xml-file-and-read-back-in */

	public string GenerateSaveFileName() {
		System.DateTime now = System.DateTime.Now;
		string dateTime = now.Year + "y" + now.Month + "m" + now.Day + "d" + now.Hour + "h" + now.Minute + "m" + now.Second + "s" + now.Millisecond + "m";
		string fileName = Application.persistentDataPath + "/Saves/snowship-save-" + uiM.colonyName + "-" + dateTime + ".snowship";
		return fileName;
	}

	public void SaveGame(string fileName) {

		FileStream file = new FileStream(fileName, FileMode.Create);
		//FileStream file = new FileStream(Application.persistentDataPath + "/Saves/snowship-save" + System.DateTime.Now.ToUniversalTime() + ".snowship",FileMode.Create);

		// 1: Save the map data

		// 2: Save the colonist data

		// 3: Save the job data

		// 4: Save the time data

		// 5: Save the camera data

		// 6: Save the planet data
	}

	public void LoadGame() {
		// 1: Load the map data

		// 2: Load the colonist data

		// 3: Load the job data

		// 4: Load the time data

		// 5: Load the camera data

		// 6: Load the planet data
	}
}
