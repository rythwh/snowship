﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class PersistenceManager : MonoBehaviour {

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

	/* https://stackoverflow.com/questions/13266496/easily-write-a-whole-class-instance-to-xml-file-and-read-back-in */

	public void Save() {
		// 1: Save the map data

		// 2: Save the colonist data

		// 3: Save the job data

		// 4: Save the time data

		// 5: Save the camera data

		// 6: Save the planet data
	}

	public void Load() {
		// 1: Load the map data

		// 2: Load the colonist data

		// 3: Load the job data

		// 4: Load the time data

		// 5: Load the camera data

		// 6: Load the planet data
	}
}