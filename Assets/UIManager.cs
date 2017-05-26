using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class UIManager : MonoBehaviour {

	public string SplitByCapitals(string combinedString) {
		var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])",
				 RegexOptions.IgnorePatternWhitespace);
		return r.Replace(combinedString," ");
	}

	private GameObject GM;
	private TileManager tm;
	private CameraManager cm;

	private GameObject mainMenu;

	public int mapSize;

	void Awake() {
		GM = GameObject.Find("GM");
		tm = GM.GetComponent<TileManager>();
		cm = GM.GetComponent<CameraManager>();

		GameObject.Find("PlayButton").GetComponent<Button>().onClick.AddListener(delegate { PlayButton(); });

		GameObject.Find("MapSize-Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateMapSizeText(); });
		GameObject.Find("MapSize-Slider").GetComponent<Slider>().value = 4;

		mainMenu = GameObject.Find("MainMenu-BackgroundPanel");

		
	}

	public void PlayButton() {
		string mapSeedString = GameObject.Find("MapSeedInput-Text").GetComponent<Text>().text;
		int mapSeed = 0;
		if (string.IsNullOrEmpty(mapSeedString)) {
			mapSeed = -1;
		} else {
			foreach (char c in mapSeedString) {
				mapSeed += c;
			}
		}
		print(mapSeed);

		print(mapSize);

		tm.Initialize(mapSize,mapSeed);
		mainMenu.SetActive(false);
	}

	public void UpdateMapSizeText() {
		mapSize = Mathf.RoundToInt(GameObject.Find("MapSize-Slider").GetComponent<Slider>().value * 50);
		GameObject.Find("MapSizeValue-Text").GetComponent<Text>().text = mapSize.ToString();
	}
}
