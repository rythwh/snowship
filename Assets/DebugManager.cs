using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class DebugManager : MonoBehaviour {

	private ColonistManager colonistM;
	private ResourceManager resourceM;
	private UIManager uiM;
	private TileManager tileM;

	private void GetScriptReferences() {
		colonistM = GetComponent<ColonistManager>();
		resourceM = GetComponent<ResourceManager>();
		uiM = GetComponent<UIManager>();
		tileM = GetComponent<TileManager>();
	}

	private GameObject debugIndicator;
	private GameObject debugInputObj;
	private InputField debugInput;

	private GameObject debugConsole;
	private GameObject debugConsoleList;

	void Awake() {
		GetScriptReferences();

		debugIndicator = GameObject.Find("DebugIndicator-Image");

		debugInputObj = debugIndicator.transform.Find("DebugCommand-Input").gameObject;
		debugInput = debugInputObj.GetComponent<InputField>();
		debugInput.onEndEdit.AddListener(delegate { ParseCommandInput(); });

		debugConsole = debugIndicator.transform.Find("DebugCommandsList-ScrollPanel").gameObject;
		debugConsoleList = debugConsole.transform.Find("DebugCommandsList-Panel").gameObject;

		debugIndicator.SetActive(false);

		CreateCommandFunctions();
	}

	public bool debugMode;

	void Update() {
		if (tileM.generated && (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote))) {
			debugMode = !debugMode;
			ToggleDebugUI();
		}
	}

	private void ToggleDebugUI() {
		debugIndicator.SetActive(debugMode);
	}

	private enum Commands {
		help,                   // help (commandName)						-- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command
		clear,					// clear									-- clear the console
		selectcolonist,         // selectcolonist <name>					-- sets the selected colonist to the first colonist named <name>
		sethealth,              // sethealth <amount>						-- sets the health of the selected colonist to <amount>
		setneed,                // setneed <name> <amount>					-- sets the need named <name> on the selected colonist to <amount>
		changeinvamt,           // changeinvamt <resourceName> <amount> (allColonists) (allContainers)
		//																	-- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container,
		//																	   or all colonists (allColonists = true), or all containers (allContainers = true)
		selectcontainer,        // selectcontainer <index>					-- sets the selected container to the container at <index> in the list of containers
		reserveresource,        // reserveresource <resourceName> <amount>	-- reserve the resource named <resourceName> for <amount> by the selected colonist at the selected container
		selecttile,             // selecttile <x(,rangeX)> <y,(rangeY)>		-- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)
		selecttilemouse,        // selecttilemouse							-- toggle continous selection of the tile at the mouse position
		changetiletype,         // changetiletype <tileType>				-- changes the tile type of the selected tile to <tileType>
		changetileobj,          // changetileobj <tileObj>					-- changes the tile object of the selected tile to <tileObj>
		campos,                 // campos <x> <y>							-- sets the camera position to <x> <y>
		camzoom,                // camzoom <zoom>							-- sets the camera orthographic size to <zoom>
		togglesuncycle,         // togglesuncycle							-- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)
		settime,                // settime <24hour> <minute>				-- sets the in-game time to <24hour> <minute>, changes brightness accordingly provided sun cycle is enabled (togglesuncycle)
		viewnormal,             // viewnormal								-- sets the map overlay to normal gameplay settings
		viewregions,            // viewregions								-- sets the map overlay to colour individual regions (region colour is random)
		viewheightmap,          // viewheightmap							-- sets the map overlay to colour each tile depending on its height between 0 and 1 (black = 0, white = 1)
		viewprecipitation,      // viewprecipitation						-- sets the map overlay to colour each tile depending on its precipitation between 0 and 1 (black = 0, white = 1)
		viewtemperature,        // viewtemperature							-- sets the map overlay to colour each tile depending on its temperature (darker = colder, lighter = warmer)
		viewbiomes,             // viewbiomes								-- sets the map overlay to colour individual biomes (colours correspond to specific biomes)
		viewdrainagebasins,     // viewdrainagebasins						-- sets the map overlay to colour individual drainage basins (drainage basin colour is random)
		viewrivers,             // viewrivers (index)						-- without (index): highlight all rivers. with (index): highlight the river at (index) in the list of rivers
		viewwalkspeed,          // viewwalkspeed							-- sets the map overlay to colour each tile depending on its walk speed (black = 0, white = 1)
		viewregionblocks,       // viewregionblocks							-- sets the map overlay to colour individual region blocks (region block colour is random)
		viewsquareregionblocks, // viewsquareregionblocks					-- sets the map overlay to colour invididual square region blocks (square region block colour is random)
		viewtabott,             // viewtabott								-- highlights all tiles that affect the brightness of the selected tile through the day (black = earlier, white = later)
		viewtttabo,             // viewtttabo								-- highlights all tiles that the selected tile affects the brightness of through the day (black = earlier, white = later)
		save,                   // save (filename)							-- without (filename): saves the name normally. with (filename): saves the game to the specified file name. will overwrite
		load                    // load (filename)							-- without (filename): loads the most recent save. with (filename): loads the file named (filename). will not save first
	};

	private Dictionary<Commands, string> commandHelpOutputs = new Dictionary<Commands, string>() {
		{Commands.help,"help (commandName) -- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command" },
		{Commands.clear,"clear -- clear the console" },
		{Commands.selectcolonist,"selectcolonist <name> -- sets the selected colonist to the first colonist named <name>" },
		{Commands.sethealth,"sethealth <amount> -- sets the health of the selected colonist to <amount> (value between 0 (0%) -> 1 (100%))" },
		{Commands.setneed,"setneed <name> <amount> -- sets the need named <name> on the selected colonist to <amount> (value between 0 (0%) -> 100 (100%))" },
		{Commands.changeinvamt,"changeinvamt <resourceName> <amount> (allColonists) (allContainers) -- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container, or all colonists (allColonists = true), or all containers (allContainers = true)" },
		{Commands.selectcontainer,"selectcontainer <index> -- sets the selected container to the container at <index> in the list of containers" },
		{Commands.reserveresource,"reserveresource <resourceName> <amount> -- reserve the resource named <resourceName> for <amount> by the selected colonist at the selected container" },
		{Commands.selecttile,"selecttile <x(,rangeX)> <y,(rangeY)> -- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)" },
		{Commands.selecttilemouse,"selecttilemouse -- continuously select the tile under the mouse until this command is run again" },
		{Commands.changetiletype,"changetiletype <tileType> -- changes the tile type of the selected tile to <tileType>" },
		{Commands.changetileobj,"changetileobj <tileObj> -- changes the tile object of the selected tile to <tileObj>" },
		{Commands.campos,"campos <x> <y> -- sets the camera position to <x> <y>" },
		{Commands.camzoom,"camzoom <zoom> -- sets the camera orthographic size to <zoom>" },
		{Commands.togglesuncycle,"togglesuncycle -- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)" },
		{Commands.settime,"settime <24hour> <minute> -- sets the in-game time to <24hour> <minute>, changes brightness accordingly provided sun cycle is enabled (togglesuncycle)" },
		{Commands.viewnormal,"viewnormal -- sets the map overlay to normal gameplay settings" },
		{Commands.viewregions,"viewregions -- sets the map overlay to colour individual regions (region colour is random)" },
		{Commands.viewheightmap,"viewheightmap -- sets the map overlay to colour each tile depending on its height between 0 and 1 (black = 0, white = 1)" },
		{Commands.viewprecipitation,"viewprecipitation -- sets the map overlay to colour each tile depending on its precipitation between 0 and 1 (black = 0, white = 1)" },
		{Commands.viewtemperature,"viewtemperature -- sets the map overlay to colour each tile depending on its temperature (darker = colder, lighter = warmer)" },
		{Commands.viewbiomes,"viewbiomes -- sets the map overlay to colour individual biomes (colours correspond to specific biomes)" },
		{Commands.viewdrainagebasins,"viewdrainagebasins -- sets the map overlay to colour individual drainage basins (drainage basin colour is random)" },
		{Commands.viewrivers,"viewrivers (index) -- without (index): highlight all rivers. with (index): highlight the river at (index) in the list of rivers" },
		{Commands.viewwalkspeed,"viewwalkspeed -- sets the map overlay to colour each tile depending on its walk speed (black = 0, white = 1)" },
		{Commands.viewregionblocks,"viewregionblocks -- sets the map overlay to colour individual region blocks (region block colour is random)" },
		{Commands.viewsquareregionblocks,"viewsquareregionblocks -- sets the map overlay to colour invididual square region blocks (square region block colour is random)" },
		{Commands.viewtabott,"viewtabott -- highlights all tiles that affect the brightness of the selected tile through the day (black = earlier, white = later)" },
		{Commands.viewtttabo,"viewtttabo -- highlights all tiles that the selected tile affects the brightness of through the day (black = earlier, white = later)" },
		{Commands.save,"save (filename) -- without (filename): saves the name normally. with (filename): saves the game to the specified file name. will overwrite" },
		{Commands.load,"load (filename) -- without (filename): loads the most recent save. with (filename): loads the file named (filename). will not save first" }
	};

	public void ParseCommandInput() {
		string commandString = debugInput.text;
		OutputToConsole(commandString);

		if (string.IsNullOrEmpty(commandString)) {
			return;
		}

		debugInput.text = string.Empty;

		List<string> commandStringSplit = commandString.Split(' ').ToList();
		if (commandStringSplit.Count > 0) {
			Commands selectedCommand;
			try {
				selectedCommand = (Commands)Enum.Parse(typeof(Commands), commandStringSplit[0]);
			} catch {
				OutputToConsole("Error running command: " + commandString);
				return;
			}
			commandFunctions[selectedCommand](selectedCommand,commandStringSplit.Skip(1).ToList());
		}
	}

	private float charsPerLine = 53;
	private int textBoxSizePerLine = 11;

	private void OutputToConsole(string outputString) {
		GameObject outputTextBox = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/CommandOutputText-Panel"), debugConsoleList.transform, false);

		int lines = Mathf.CeilToInt(outputString.Length / charsPerLine);

		outputTextBox.GetComponent<LayoutElement>().minHeight = lines * textBoxSizePerLine;
		outputTextBox.GetComponent<RectTransform>().sizeDelta = new Vector2(outputTextBox.GetComponent<RectTransform>().sizeDelta.x, lines * textBoxSizePerLine);

		outputTextBox.transform.Find("Text").GetComponent<Text>().text = outputString;
	}

	private Dictionary<Commands, Action<Commands,List<string>>> commandFunctions = new Dictionary<Commands, Action<Commands,List<string>>>();

	private void CreateCommandFunctions() {
		commandFunctions.Add(Commands.help, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (KeyValuePair<Commands, string> commandHelpKVP in commandHelpOutputs) {
					OutputToConsole(commandHelpKVP.Value);
				}
			} else if (parameters.Count == 1) {
				Commands parameterCommand;
				try {
					parameterCommand = (Commands)Enum.Parse(typeof(Commands), parameters[0]);
				} catch {
					OutputToConsole("ERROR: Unable to get help with command: " + parameters[0]);
					return;
				}
				if ((int)parameterCommand < commandHelpOutputs.Count && (int)parameterCommand >= 0) {
					OutputToConsole(commandHelpOutputs[parameterCommand]);
				} else {
					OutputToConsole("ERROR: Command index is out of range: " + parameters[0]);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.clear, delegate (Commands selectedCommand, List<string> parameters) {
			foreach (Transform child in debugConsoleList.transform) {
				Destroy(child.gameObject);
			}
			debugConsoleList.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
			debugConsole.transform.Find("Scrollbar").GetComponent<Scrollbar>().value = 0;
		});
		commandFunctions.Add(Commands.selectcolonist, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				ColonistManager.Colonist selectedColonist = colonistM.colonists.Find(colonist => colonist.name == parameters[0]);
				if (selectedColonist != null) {
					colonistM.SetSelectedColonist(selectedColonist);
				} else {
					OutputToConsole("ERROR: Invalid colonist name: " + parameters[0]);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.sethealth, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				float newHealth = 0;
				if (float.TryParse(parameters[0], out newHealth)) {
					if (newHealth >= 0 && newHealth <= 1) {
						if (colonistM.selectedColonist != null) {
							colonistM.selectedColonist.health = newHealth;
						} else {
							OutputToConsole("ERROR: No colonist selected.");
						}
					} else {
						OutputToConsole("ERROR: Invalid range on health value (float from 0 to 1).");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse health value as float.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.setneed, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				ColonistManager.NeedPrefab needPrefab = colonistM.needPrefabs.Find(need => need.name == parameters[0]);
				if (needPrefab != null) {
					if (colonistM.selectedColonist != null) {
						ColonistManager.NeedInstance needInstance = colonistM.selectedColonist.needs.Find(need => need.prefab == needPrefab);
						if (needInstance != null) {
							float newNeedValue = 0;
							if (float.TryParse(parameters[1], out newNeedValue)) {
								if (newNeedValue >= 0 && newNeedValue <= 100) {
									needInstance.value = newNeedValue;
								} else {
									OutputToConsole("ERROR: Invalid range on need value (float from 0 to 100).");
								}
							} else {
								OutputToConsole("ERROR: Unable to parse need value as float.");
							}
						} else {
							OutputToConsole("ERROR: Unable to find specified need on selected colonist.");
						}
					} else {
						OutputToConsole("ERROR: No colonist selected.");
					}
				} else {
					OutputToConsole("ERROR: Invalid need name.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.changeinvamt, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				ResourceManager.Resource resource = resourceM.resources.Find(r => r.name == parameters[0]);
				if (resource != null) {
					if (colonistM.selectedColonist != null || uiM.selectedContainer != null) {
						if (colonistM.selectedColonist != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								colonistM.selectedColonist.inventory.ChangeResourceAmount(resource, amount);
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("No colonist selected, skipping.");
						}
						if (uiM.selectedContainer != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								colonistM.selectedColonist.inventory.ChangeResourceAmount(resource, amount);
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("No container selected, skipping.");
						}
					} else {
						OutputToConsole("ERROR: No colonist or container selected.");
					}
				} else {
					OutputToConsole("ERROR: Invalid resource name.");
				}
			} else if (parameters.Count == 3) {
				bool allColonists = false;
				if (bool.TryParse(parameters[2], out allColonists)) {
					ResourceManager.Resource resource = resourceM.resources.Find(r => r.name == parameters[0]);
					if (resource != null) {
						int amount = 0;
						if (int.TryParse(parameters[1], out amount)) {
							foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
								colonist.inventory.ChangeResourceAmount(resource, amount);
							}
						} else {
							OutputToConsole("ERROR: Unable to parse resource amount as int.");
						}
					} else {
						OutputToConsole("ERROR: Invalid resource name.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allColonists parameter as bool.");
				}
			} else if (parameters.Count == 4) {
				bool allContainers = false;
				if (bool.TryParse(parameters[3], out allContainers)) {
					ResourceManager.Resource resource = resourceM.resources.Find(r => r.name == parameters[0]);
					if (resource != null) {
						int amount = 0;
						if (int.TryParse(parameters[1], out amount)) {
							foreach (ResourceManager.Container container in resourceM.containers) {
								container.inventory.ChangeResourceAmount(resource, amount);
							}
						} else {
							OutputToConsole("ERROR: Unable to parse resource amount as int.");
						}
					} else {
						OutputToConsole("ERROR: Invalid resource name.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allContainers parameter as bool.");
				}
				bool allColonists = false;
				if (bool.TryParse(parameters[2], out allColonists)) {
					ResourceManager.Resource resource = resourceM.resources.Find(r => r.name == parameters[0]);
					if (resource != null) {
						int amount = 0;
						if (int.TryParse(parameters[1], out amount)) {
							foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
								colonist.inventory.ChangeResourceAmount(resource, amount);
							}
						} else {
							OutputToConsole("ERROR: Unable to parse resource amount as int.");
						}
					} else {
						OutputToConsole("ERROR: Invalid resource name.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allColonists parameter as bool.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
	}
}
