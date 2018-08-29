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
	private CameraManager cameraM;
	private TimeManager timeM;

	private void GetScriptReferences() {
		colonistM = GetComponent<ColonistManager>();
		resourceM = GetComponent<ResourceManager>();
		uiM = GetComponent<UIManager>();
		tileM = GetComponent<TileManager>();
		cameraM = GetComponent<CameraManager>();
		timeM = GetComponent<TimeManager>();
	}

	private GameObject debugPanel;
	private GameObject debugInputObj;
	private InputField debugInput;

	private GameObject debugConsole;
	private GameObject debugConsoleList;

	private Sprite whiteSquare;

	void Awake() {
		GetScriptReferences();

		debugPanel = GameObject.Find("Debug-Panel");

		debugInputObj = debugPanel.transform.Find("DebugCommand-Input").gameObject;
		debugInput = debugInputObj.GetComponent<InputField>();
		debugInput.onEndEdit.AddListener(delegate { ParseCommandInput(); });

		debugConsole = debugPanel.transform.Find("DebugCommandsList-ScrollPanel").gameObject;
		debugConsoleList = debugConsole.transform.Find("DebugCommandsList-Panel").gameObject;

		debugPanel.SetActive(false);

		whiteSquare = resourceM.whiteSquareSprite;

		CreateCommandFunctions();
	}

	public bool debugMode;

	void Update() {
		if (tileM.generated && (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote))) {
			debugMode = !debugMode;
			ToggleDebugUI();
		}
		if (debugMode) {
			if (selecttilemouse_Toggle) {
				selecttilemouse_Update();
			}
			if (togglesuncycle_Toggle) {
				togglesuncycle_Update();
			}
		}
	}

	private void ToggleDebugUI() {
		debugPanel.SetActive(debugMode);
		if (debugMode) {
			debugInput.Select();
		}
	}

	private enum Commands {
		help,                   // help (commandName)						-- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command
		clear,					// clear									-- clear the console
		selecthuman,            // selecthuman <name>						-- sets the selected human to the first human named <name>
		spawncolonists,			// spawncolonists <numToSpawn>				-- spawns <numToSpawn> colonists to join the colony
		spawntraders,			// spawntraders <numToSpawn>				-- spawns <numToSpawn> traders
		sethealth,				// sethealth <amount>						-- sets the health of the selected colonist to <amount>
		setneed,                // setneed <name> <amount>					-- sets the need named <name> on the selected colonist to <amount>
		cia,					// changeinvamt								-- shortcut to "changinvamt"
		changeinvamt,           // changeinvamt || cia <resourceName> <amount> (allColonists) (allContainers)
		//																	-- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container,
		//																	   or all colonists (allColonists = true), or all containers (allContainers = true)
		listresources,			// listresources							-- lists all resources
		selectcontainer,        // selectcontainer <index>					-- sets the selected container to the container at <index> in the list of containers
		selecttiles,            // selecttiles <x(,rangeX)> <y,(rangeY)>	-- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)
		selecttilemouse,		// selecttilemouse							-- toggle continous selection of the tile at the mouse position
		deselecttiles,          // deselecttiles							-- deselect all selected tiles
		deselectunwalkable,		// deselectunwalkabletiles					-- deselect all tiles in the selection that are unwalkable
		deselectwalkable,		// deselectwalkabletiles					-- deselect all tiles in the selection that are walkable
		deselectunbuildable,    // deselectunbuildable						-- deselect all tiles in the selection that are unbuildable
		deselectbuildable,      // deselectbuildable						-- deselect all tiles in the selection that are buildable
		changetiletype,         // changetiletype <tileType>				-- changes the tile type of the selected tile(s) to <tileType>
		listtiletypes,			// listtiletypes							-- lists all tile types
		changetileobj,          // changetileobj <tileObj> (rotationIndex)	-- changes the tile object of the selected tile(s) to <tileObj> with (rotationIndex) (int - value depends on obj)
		removetileobj,			// removetileobj (layer)					-- without (layer): removes all tile objects on the selected tile(s). with (layer): removes the tile object at (layer)
		listtileobjs,			// listtileobjs								-- lists all tile objs
		changetileplant,		// changetileplant <plantType> <small>		-- changes the tile plant of the selected tile(s) to <plantType> and size large if (small = false), otherwise small
		removetileplant,		// removetileplant							-- removes the tile plant of the selected tile(s)
		listtileplants,			// listtileplants							-- lists all tile plants
		campos,                 // campos <x> <y>							-- sets the camera position to <x> <y>
		camzoom,                // camzoom <zoom>							-- sets the camera orthographic size to <zoom>
		togglesuncycle,         // togglesuncycle							-- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)
		settime,                // settime <time>							-- sets the in-game time to <time> (time = float between 0 and 23, decimals represent minutes (0.5 = 30 minutes))
		viewselectedtiles,      // viewselectedtiles						-- sets the map overlay to highlight all selected tiles
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
		viewshadowsfrom,        // viewshadowsfrom							-- highlights all tiles that affect the brightness of the selected tile through the day (green = earlier, pink = later)
		viewshadowsto,          // viewshadowsto							-- highlights all tiles that the selected tile affects the brightness of through the day (green = earlier, pink = later)
		viewblockingtiles,      // viewblockingtiles						-- highlights all tiles that the selected tile blocks shadows from (tiles that have shadows that were cut short because this tile was in the way)
		viewroofs,				// viewroofs								-- higlights all tiles with a roof above it
		save,                   // save (filename)							-- without (filename): saves the name normally. with (filename): saves the game to the specified file name. will overwrite
		load                    // load (filename)							-- without (filename): loads the most recent save. with (filename): loads the file named (filename). will not save first
	};

	private Dictionary<Commands, string> commandHelpOutputs = new Dictionary<Commands, string>() {
		{Commands.help,"help (commandName) -- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command" },
		{Commands.clear,"clear -- clear the console" },
		{Commands.selecthuman,"selecthuman <name> -- sets the selected human to the first human named <name>" },
		{Commands.spawncolonists,"spawncolonists <numToSpawn> -- spawns <numToSpawn> colonists to join the colony" },
		{Commands.spawntraders,"spawntraders <numToSpawn> -- spawns <numToSpawn> traders" },
		{Commands.sethealth,"sethealth <amount> -- sets the health of the selected colonist to <amount> (value between 0 (0%) -> 1 (100%))" },
		{Commands.setneed,"setneed <name> <amount> -- sets the need named <name> on the selected colonist to <amount> (value between 0 (0%) -> 100 (100%))" },
		{Commands.cia,"cia -- shortcut to \"changeinvamt\" -- see that command for more information" },
		{Commands.changeinvamt,"changeinvamt <resourceName> <amount> (allColonists) (allContainers) -- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container, or all colonists (allColonists = true), or all containers (allContainers = true)" },
		{Commands.listresources,"listresources -- lists all resources" },
		{Commands.selectcontainer,"selectcontainer <index> -- sets the selected container to the container at <index> in the list of containers" },
		{Commands.selecttiles,"selecttiles <x(,rangeX)> <y,(rangeY)> -- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)" },
		{Commands.selecttilemouse,"selecttilemouse -- continuously select the tile under the mouse until this command is run again" },
		{Commands.deselecttiles,"deselecttiles -- deselect all selected tiles" },
		{Commands.deselectunwalkable,"deselectunwalkabletiles -- deselect all tiles in the selection that are unwalkable" },
		{Commands.deselectwalkable,"deselectwalkabletiles -- deselect all tiles in the selection that are walkable" },
		{Commands.deselectunbuildable,"deselectunbuildable -- deselect all tiles in the selection that are unbuildable" },
		{Commands.deselectbuildable,"deselectbuildable -- deselect all tiles in the selection that are buildable" },
		{Commands.changetiletype,"changetiletype <tileType> -- changes the tile type of the selected tile to <tileType>" },
		{Commands.listtiletypes,"listtiletypes -- lists all tile types" },
		{Commands.changetileobj,"changetileobj <tileObj> -- changes the tile object of the selected tile to <tileObj>" },
		{Commands.removetileobj,"removetileobj (layer) -- without (layer): removes all tile objects on the selected tile(s). with (layer): removes the tile object at (layer)" },
		{Commands.listtileobjs,"listtileobjs -- lists all tile objs" },
		{Commands.changetileplant,"changetileplant <plantType> <small> -- changes the tile plant of the selected tile(s) to <plantType> and size large if (small = false), otherwise small" },
		{Commands.removetileplant,"removetileplant -- removes the tile plant of the selected tile(s)" },
		{Commands.listtileplants,"listtileplants -- lists all tile plants" },
		{Commands.campos,"campos <x> <y> -- sets the camera position to <x> <y>" },
		{Commands.camzoom,"camzoom <zoom> -- sets the camera orthographic size to <zoom>" },
		{Commands.togglesuncycle,"togglesuncycle -- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)" },
		{Commands.settime,"settime <time> -- sets the in-game time to <time> (time = float between 0 and 23, decimals represent minutes (0.5 = 30 minutes))" },
		{Commands.viewselectedtiles,"viewselectedtiles -- sets the map overlay to highlight all selected tiles" },
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
		{Commands.viewshadowsfrom,"viewshadowsfrom -- highlights all tiles that affect the brightness of the selected tile through the day (green = earlier, pink = later)" },
		{Commands.viewshadowsto,"viewshadowsto -- highlights all tiles that the selected tile affects the brightness of through the day (green = earlier, pink = later)" },
		{Commands.viewblockingtiles,"viewblockingtiles -- highlights all tiles that the selected tile blocks shadows from (tiles that have shadows that were cut short because this tile was in the way)" },
		{Commands.viewroofs,"viewroofs -- higlights all tiles with a roof above it" },
		{Commands.save,"save (filename) -- without (filename): saves the game normally. with (filename): saves the game to the specified file name. will overwrite" },
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
				OutputToConsole("ERROR: Unknown command: " + commandString);
				return;
			}
			commandFunctions[selectedCommand](selectedCommand,commandStringSplit.Skip(1).ToList());
		}

		debugInput.Select();
	}

	private float charsPerLine = 70;
	private int textBoxSizePerLine = 17;

	private void OutputToConsole(string outputString) {
		if (!string.IsNullOrEmpty(outputString)) {
			GameObject outputTextBox = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/CommandOutputText-Panel"), debugConsoleList.transform, false);

			int lines = Mathf.CeilToInt(outputString.Length / charsPerLine);

			outputTextBox.GetComponent<LayoutElement>().minHeight = lines * textBoxSizePerLine;
			outputTextBox.GetComponent<RectTransform>().sizeDelta = new Vector2(outputTextBox.GetComponent<RectTransform>().sizeDelta.x, lines * textBoxSizePerLine);

			outputTextBox.transform.Find("Text").GetComponent<Text>().text = outputString;
		}
	}

	private Dictionary<Commands, Action<Commands,List<string>>> commandFunctions = new Dictionary<Commands, Action<Commands,List<string>>>();

	private List<TileManager.Tile> selectedTiles = new List<TileManager.Tile>();
	private bool selecttilemouse_Toggle;

	private void selecttilemouse_Update() {
		selectedTiles.Clear();
		selectedTiles.Add(uiM.mouseOverTile);
	}

	private bool togglesuncycle_Toggle;
	private float holdHour = 12;

	public void togglesuncycle_Update() {
		tileM.map.SetTileBrightness(holdHour);
	}

	private int viewRiverAtIndex = 0;

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
		commandFunctions.Add(Commands.selecthuman, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				ColonistManager.Human selectedHuman = (ColonistManager.Human)colonistM.colonists.Find(colonist => colonist.name == parameters[0]) ?? colonistM.traders.Find(colonist => colonist.name == parameters[0]);
				if (selectedHuman != null) {
					colonistM.SetSelectedHuman(selectedHuman);
					if (colonistM.selectedHuman == selectedHuman) {
						OutputToConsole("SUCCESS: Selected " + colonistM.selectedHuman.name + ".");
					}
				} else {
					OutputToConsole("ERROR: Invalid colonist name: " + parameters[0]);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.spawncolonists, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				int numberToSpawn = 1;
				if (int.TryParse(parameters[0], out numberToSpawn)) {
					int oldNumColonists = colonistM.colonists.Count;
					colonistM.SpawnColonists(numberToSpawn);
					if (oldNumColonists + numberToSpawn == colonistM.colonists.Count) {
						OutputToConsole("SUCCESS: Spawned " + numberToSpawn + " colonists.");
					} else {
						OutputToConsole("ERROR: Unable to spawn colonists. Spawned " + (colonistM.colonists.Count - oldNumColonists) + " colonists.");
					}
				} else {
					OutputToConsole("ERROR: Invalid number of colonists.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.spawntraders, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				int numberToSpawn = 1;
				if (int.TryParse(parameters[0], out numberToSpawn)) {
					int oldNumTraders = colonistM.traders.Count;
					for (int i = 0; i < numberToSpawn; i++) {
						colonistM.SpawnTrader();
					}
					if (oldNumTraders + numberToSpawn == colonistM.traders.Count) {
						OutputToConsole("SUCCESS: Spawned " + numberToSpawn + " traders.");
					} else {
						OutputToConsole("ERROR: Unable to spawn traders. Spawned " + (colonistM.traders.Count - oldNumTraders) + " traders.");
					}
				} else {
					OutputToConsole("ERROR: Invalid number of colonists.");
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
						if (colonistM.selectedHuman != null) {
							colonistM.selectedHuman.health = newHealth;
							if (colonistM.selectedHuman.health == newHealth) {
								OutputToConsole("SUCCESS: " + colonistM.selectedHuman.name + "'s health is now " + colonistM.selectedHuman.health + ".");
							} else {
								OutputToConsole("ERROR: Unable to change " + colonistM.selectedHuman.name + "'s health.");
							}
						} else {
							OutputToConsole("ERROR: No human selected.");
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
					if (colonistM.selectedHuman != null) {
						if (colonistM.selectedHuman is ColonistManager.Colonist) {
							ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)colonistM.selectedHuman;
							ColonistManager.NeedInstance needInstance = selectedColonist.needs.Find(need => need.prefab == needPrefab);
							if (needInstance != null) {
								float newNeedValue = 0;
								if (float.TryParse(parameters[1], out newNeedValue)) {
									if (newNeedValue >= 0 && newNeedValue <= 100) {
										needInstance.SetValue(newNeedValue);
										if (needInstance.GetValue() == newNeedValue) {
											OutputToConsole("SUCCESS: " + needInstance.prefab.name + " now has value " + needInstance.GetValue() + ".");
										} else {
											OutputToConsole("ERROR: Unable to change " + needInstance.prefab.name + " need value.");
										}
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
							OutputToConsole("ERROR: Selected human is not a colonist.");
						}
					} else {
						OutputToConsole("ERROR: No human selected.");
					}
				} else {
					OutputToConsole("ERROR: Invalid need name.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.cia, delegate (Commands selectedCommand, List<string> parameters) {
			commandFunctions[Commands.changeinvamt](Commands.changeinvamt, parameters);
		});
		commandFunctions.Add(Commands.changeinvamt, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				ResourceManager.Resource resource = resourceM.resources.Find(r => r.type.ToString() == parameters[0]);
				if (resource != null) {
					if (colonistM.selectedHuman != null || uiM.selectedContainer != null) {
						if (colonistM.selectedHuman != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								colonistM.selectedHuman.inventory.ChangeResourceAmount(resource, amount);
								OutputToConsole("SUCCESS: Added " + amount + " " + resource.name + " to " + colonistM.selectedHuman + ".");
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("No colonist selected, skipping.");
						}
						if (uiM.selectedContainer != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								uiM.selectedContainer.inventory.ChangeResourceAmount(resource, amount);
								OutputToConsole("SUCCESS: Added " + amount + " " + resource.name + " to container.");
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("No container selected, skipping.");
						}
					} else {
						int amount = 0;
						if (int.TryParse(parameters[1], out amount)) {
							foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
								colonist.inventory.ChangeResourceAmount(resource, amount);
							}
							foreach (ResourceManager.Container container in resourceM.containers) {
								container.inventory.ChangeResourceAmount(resource, amount);
							}
							OutputToConsole("SUCCESS: Added " + amount + " " + resource.name + " to all colonists and all containers.");
						} else {
							OutputToConsole("ERROR: Unable to parse resource amount as int.");
						}
					}
				} else {
					OutputToConsole("ERROR: Invalid resource name.");
				}
			} else if (parameters.Count == 3) {
				bool allColonists = false;
				if (bool.TryParse(parameters[2], out allColonists)) {
					if (allColonists) {
						ResourceManager.Resource resource = resourceM.resources.Find(r => r.type.ToString() == parameters[0]);
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
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allColonists parameter as bool.");
				}
			} else if (parameters.Count == 4) {
				bool allColonists = false;
				if (bool.TryParse(parameters[2], out allColonists)) {
					if (allColonists) {
						ResourceManager.Resource resource = resourceM.resources.Find(r => r.type.ToString() == parameters[0]);
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
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allColonists parameter as bool.");
				}
				bool allContainers = false;
				if (bool.TryParse(parameters[3], out allContainers)) {
					if (allContainers) {
						ResourceManager.Resource resource = resourceM.resources.Find(r => r.type.ToString() == parameters[0]);
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
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allContainers parameter as bool.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.listresources, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ResourceManager.Resource resource in resourceM.resources) {
					OutputToConsole(resource.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.selectcontainer, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				int index = -1;
				if (int.TryParse(parameters[0], out index)) {
					if (index >= 0 && index < resourceM.containers.Count) {
						uiM.SetSelectedContainer(resourceM.containers[index]);
					} else {
						OutputToConsole("ERROR: Container index out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse container index as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.selecttiles, delegate (Commands selectedCommand, List<string> parameters) {
			int count = selectedTiles.Count;
			if (parameters.Count == 2) {
				int startX = 0;
				if (int.TryParse(parameters[0], out startX)) {
					int startY = 0;
					if (int.TryParse(parameters[1], out startY)) {
						if (startX >= 0 && startY >= 0 && startX < tileM.map.mapData.mapSize && startY < tileM.map.mapData.mapSize) {
							selectedTiles.Add(tileM.map.GetTileFromPosition(new Vector2(startX, startY)));
						} else {
							OutputToConsole("ERROR: Positions out of range.");
						}
					} else {
						if (parameters[1].Split(',').ToList().Count > 0) {
							string startYString = parameters[1].Split(',')[0];
							string endYString = parameters[1].Split(',')[1];
							int endY = 0;
							if (int.TryParse(startYString, out startY) && int.TryParse(endYString, out endY)) {
								if (startX >= 0 && startY >= 0 && endY >= 0 && startX < tileM.map.mapData.mapSize && startY < tileM.map.mapData.mapSize && endY < tileM.map.mapData.mapSize) {
									if (startY <= endY) {
										for (int y = startY; y <= endY; y++) {
											selectedTiles.Add(tileM.map.GetTileFromPosition(new Vector2(startX, y)));
										}
									} else {
										OutputToConsole("ERROR: Starting y position is greater than ending y position.");
									}
								} else {
									OutputToConsole("ERROR: Positions out of range.");
								}
							} else {
								OutputToConsole("ERROR: Unable to parse y positions.");
							}
						} else {
							OutputToConsole("ERROR: Unable to parse y as single position or split into two positions by a comma.");
						}
					}
				} else {
					if (parameters[0].Split(',').ToList().Count > 0) {
						string startXString = parameters[0].Split(',')[0];
						string endXString = parameters[0].Split(',')[1];
						int endX = 0;
						if (int.TryParse(startXString, out startX) && int.TryParse(endXString, out endX)) {
							int startY = 0;
							if (int.TryParse(parameters[1], out startY)) {
								if (startX >= 0 && startY >= 0 && endX >= 0 && startX < tileM.map.mapData.mapSize && startY < tileM.map.mapData.mapSize && endX < tileM.map.mapData.mapSize) {
									if (startX <= endX) {
										for (int x = startX; x <= endX; x++) {
											selectedTiles.Add(tileM.map.GetTileFromPosition(new Vector2(x, startY)));
										}
									} else {
										OutputToConsole("ERROR: Starting x position is greater than ending x position.");
									}
								} else {
									OutputToConsole("ERROR: Positions out of range.");
								}
							} else {
								if (parameters[1].Split(',').ToList().Count > 0) {
									string startYString = parameters[1].Split(',')[0];
									string endYString = parameters[1].Split(',')[1];
									int endY = 0;
									if (int.TryParse(startYString, out startY) && int.TryParse(endYString, out endY)) {
										if (startX >= 0 && startY >= 0 && endX >= 0 && endY >= 0 && startX < tileM.map.mapData.mapSize && startY < tileM.map.mapData.mapSize && endX < tileM.map.mapData.mapSize && endY < tileM.map.mapData.mapSize) {
											if (startX <= endX && startY <= endY) {
												for (int y = startY; y <= endY; y++) {
													for (int x = startX; x <= endX; x++) {
														selectedTiles.Add(tileM.map.GetTileFromPosition(new Vector2(x, y)));
													}
												}
											} else {
												OutputToConsole("ERROR: Starting x or y position is greater than ending x or y position.");
											}
										} else {
											OutputToConsole("ERROR: Positions out of range.");
										}
									} else {
										OutputToConsole("ERROR: Unable to parse y positions.");
									}
								} else {
									OutputToConsole("ERROR: Unable to parse y as single position or split into two positions by a comma.");
								}
							}
						} else {
							OutputToConsole("ERROR: Unable to parse x positions.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse x as single position or split into two positions by a comma.");
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			selectedTiles = selectedTiles.Distinct().ToList();
			OutputToConsole("Selected " + (selectedTiles.Count - count) + " tiles.");
		});
		commandFunctions.Add(Commands.selecttilemouse, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				selecttilemouse_Toggle = !selecttilemouse_Toggle;
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.deselecttiles, delegate (Commands selectedCommand, List<string> parameters) {
			int selectedTilesCount = selectedTiles.Count;
			if (parameters.Count == 0) {
				selectedTiles.Clear();
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + selectedTilesCount + " tiles.");
		});
		commandFunctions.Add(Commands.deselectunwalkable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (!tile.walkable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " unwalkable tiles.");
		});
		commandFunctions.Add(Commands.deselectwalkable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (tile.walkable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " walkable tiles.");
		});
		commandFunctions.Add(Commands.deselectunbuildable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (!tile.tileType.buildable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " unbuildable tiles.");
		});
		commandFunctions.Add(Commands.deselectbuildable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (tile.tileType.buildable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " buildable tiles.");
		});
		commandFunctions.Add(Commands.changetiletype, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 1) {
				TileManager.TileType tileType = tileM.tileTypes.Find(tt => tt.type.ToString() == parameters[0]);
				if (tileType != null) {
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							bool previousWalkableState = tile.walkable;
							tile.SetTileType(tileType, true, true, true, false);
							if (!previousWalkableState && tile.walkable) {
								tileM.map.RemoveTileBrightnessEffect(tile);
							} else if (previousWalkableState && !tile.walkable) {
								tileM.map.DetermineShadowTiles(new List<TileManager.Tile>() { tile }, true);
							}
							counter += 1;
						}
					} else {
						OutputToConsole("No tiles are currently selected.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse tile type.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Changed " + counter + " tile types.");
		});
		commandFunctions.Add(Commands.listtiletypes, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.TileType tileType in tileM.tileTypes) {
					OutputToConsole(tileType.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.changetileobj, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 2) {
				ResourceManager.TileObjectPrefab tileObjectPrefab = resourceM.tileObjectPrefabs.Find(top => top.type.ToString() == parameters[0]);
				if (tileObjectPrefab != null) {
					int rotation = 0;
					if (int.TryParse(parameters[1], out rotation)) {
						if (rotation >= 0 && rotation < tileObjectPrefab.bitmaskSprites.Count) {
							if (selectedTiles.Count > 0) {
								foreach (TileManager.Tile tile in selectedTiles) {
									if (tile.objectInstances.ContainsKey(tileObjectPrefab.layer) && tile.objectInstances[tileObjectPrefab.layer] != null) {
										resourceM.RemoveTileObjectInstance(tile.objectInstances[tileObjectPrefab.layer]);
										tile.RemoveTileObjectAtLayer(tileObjectPrefab.layer);
									}
									tile.SetTileObject(tileObjectPrefab, rotation);
									tile.GetObjectInstanceAtLayer(tileObjectPrefab.layer).FinishCreation();
									counter += 1;
								}
							} else {
								OutputToConsole("No tiles are currently selected.");
							}
						} else {
							OutputToConsole("ERROR: Rotation index is out of range.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse rotation index.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse tile object.");
				}
			} else if (parameters.Count == 1) {
				ResourceManager.TileObjectPrefab tileObjectPrefab = resourceM.tileObjectPrefabs.Find(top => top.type.ToString() == parameters[0]);
				if (tileObjectPrefab != null) {
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							if (tile.objectInstances.ContainsKey(tileObjectPrefab.layer) && tile.objectInstances[tileObjectPrefab.layer] != null) {
								resourceM.RemoveTileObjectInstance(tile.objectInstances[tileObjectPrefab.layer]);
								tile.RemoveTileObjectAtLayer(tileObjectPrefab.layer);
							}
							tile.SetTileObject(tileObjectPrefab, 0);
							tile.GetObjectInstanceAtLayer(tileObjectPrefab.layer).FinishCreation();
							counter += 1;
						}
					} else {
						OutputToConsole("No tiles are currently selected.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse tile object.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			tileM.map.RemoveTileBrightnessEffect(null);
			OutputToConsole("Changed " + counter + " tile objects.");
		});
		commandFunctions.Add(Commands.removetileobj, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						List<ResourceManager.TileObjectInstance> removeTOIs = new List<ResourceManager.TileObjectInstance>();
						foreach (KeyValuePair<int, ResourceManager.TileObjectInstance> toiKVP in tile.objectInstances) {
							removeTOIs.Add(toiKVP.Value);
						}
						foreach (ResourceManager.TileObjectInstance toi in removeTOIs) {
							resourceM.RemoveTileObjectInstance(tile.objectInstances[toi.prefab.layer]);
							tile.RemoveTileObjectAtLayer(toi.prefab.layer);
						}
						counter += 1;
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else if (parameters.Count == 1) {
				int layer = 0;
				if (int.TryParse(parameters[0], out layer)) {
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							if (tile.objectInstances.ContainsKey(layer) && tile.objectInstances[layer] != null) {
								resourceM.RemoveTileObjectInstance(tile.objectInstances[layer]);
								tile.RemoveTileObjectAtLayer(layer);
								counter += 1;
							}
						}
					} else {
						OutputToConsole("No tiles are currently selected.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse layer as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Removed " + counter + " tile objects.");
		});
		commandFunctions.Add(Commands.listtileobjs, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ResourceManager.TileObjectPrefab top in resourceM.tileObjectPrefabs) {
					OutputToConsole(top.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.changetileplant, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 2) {
				ResourceManager.PlantGroup plantGroup = resourceM.plantGroups.Find(pg => pg.type.ToString() == parameters[0]);
				if (plantGroup != null) {
					bool small = false;
					if (bool.TryParse(parameters[1], out small)) {
						if (selectedTiles.Count > 0) {
							foreach (TileManager.Tile tile in selectedTiles) {
								tile.SetPlant(false, new ResourceManager.Plant(plantGroup, tile, false, small, tileM.map.smallPlants,true,null,resourceM));
							tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
								if (tile.plant != null) {
									counter += 1;
								}
							}
						} else {
							OutputToConsole("No tiles are currently selected.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse small bool.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse plant type.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Changed " + counter + " tile plants.");
		});
		commandFunctions.Add(Commands.removetileplant, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						tile.SetPlant(true, null);
						if (tile.plant == null) {
							counter += 1;
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Removed " + counter + " tile plants.");
		});
		commandFunctions.Add(Commands.listtileplants, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ResourceManager.PlantGroup plantGroup in resourceM.plantGroups) {
					OutputToConsole(plantGroup.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.campos, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				int camX = 0;
				if (int.TryParse(parameters[0], out camX)) {
					int camY = 0;
					if (int.TryParse(parameters[1], out camY)) {
						cameraM.SetCameraPosition(new Vector2(camX, camY));
					} else {
						OutputToConsole("ERROR: Unable to parse camera y position as int.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse camera x position as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.camzoom, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				float camZoom = 0;
				if (float.TryParse(parameters[0], out camZoom)) {
					cameraM.SetCameraZoom(camZoom);
				} else {
					OutputToConsole("ERROR: Unable to parse camera zoom as float.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.togglesuncycle, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				holdHour = timeM.tileBrightnessTime;
				togglesuncycle_Toggle = !togglesuncycle_Toggle;
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.settime, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				float time = 0;
				if (float.TryParse(parameters[0], out time)) {
					if (time >= 0 && time < 24) {
						holdHour = time;
						timeM.SetTime(time);
						tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
					} else {
						OutputToConsole("ERROR: Time out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse time as float.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewselectedtiles, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				if (selectedTiles.Count <= 0) {
					OutputToConsole("No tiles are currently selected.");
				} else {
					foreach (TileManager.Tile tile in selectedTiles) {
						tile.sr.sprite = whiteSquare;
						tile.sr.color = Color.blue;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewnormal, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					tile.sr.color = Color.white;
				}
				tileM.map.Bitmasking(tileM.map.tiles);
				tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewregions, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.Region region in tileM.map.regions) {
					region.ColourRegion(resourceM.whiteSquareSprite);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewheightmap, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = new Color(tile.height, tile.height, tile.height, 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewprecipitation, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = new Color(tile.GetPrecipitation(), tile.GetPrecipitation(), tile.GetPrecipitation(), 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewtemperature, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = new Color((tile.temperature + 50f) / 100f, (tile.temperature + 50f) / 100f, (tile.temperature + 50f) / 100f, 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewbiomes, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					tile.sr.sprite = whiteSquare;
					if (tile.biome != null) {
						tile.sr.color = tile.biome.colour;
					} else {
						tile.sr.color = Color.black;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewdrainagebasins, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (KeyValuePair<TileManager.Map.Region, TileManager.Tile> kvp in tileM.map.drainageBasins) {
					foreach (TileManager.Tile tile in kvp.Key.tiles) {
						tile.sr.sprite = whiteSquare;
						tile.sr.color = kvp.Key.colour;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewrivers, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.River river in tileM.map.rivers) {
					foreach (TileManager.Tile tile in river.tiles) {
						tile.sr.sprite = whiteSquare;
						tile.sr.color = Color.blue;
					}
					river.tiles[0].sr.color = Color.red;
					river.tiles[river.tiles.Count - 1].sr.color = Color.green;
				}
			} else if (parameters.Count == 1) {
				if (int.TryParse(parameters[0], out viewRiverAtIndex)) {
					if (viewRiverAtIndex >= 0 && viewRiverAtIndex < tileM.map.rivers.Count) {
						foreach (TileManager.Tile tile in tileM.map.rivers[viewRiverAtIndex].tiles) {
							tile.sr.sprite = whiteSquare;
							tile.sr.color = Color.blue;
						}
						tileM.map.rivers[viewRiverAtIndex].tiles[0].sr.color = Color.red;
						tileM.map.rivers[viewRiverAtIndex].tiles[tileM.map.rivers[viewRiverAtIndex].tiles.Count - 1].sr.color = Color.green;
					} else {
						OutputToConsole("ERROR: River index out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse river index as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewwalkspeed, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					tile.sr.sprite = whiteSquare;
					tile.sr.color = new Color(tile.walkSpeed, tile.walkSpeed, tile.walkSpeed, 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewregionblocks, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.Region region in tileM.map.regionBlocks) {
					region.ColourRegion(resourceM.whiteSquareSprite);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewsquareregionblocks, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.Region region in tileM.map.squareRegionBlocks) {
					region.ColourRegion(resourceM.whiteSquareSprite);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewshadowsfrom, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				tileM.map.SetTileBrightness(12);
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						foreach (KeyValuePair<int, Dictionary<TileManager.Tile, float>> shadowsFromKVP in tile.shadowsFrom) {
							foreach (KeyValuePair<TileManager.Tile, float> sTile in shadowsFromKVP.Value) {
								sTile.Key.SetColour(new Color(shadowsFromKVP.Key / 24f, 1 - (shadowsFromKVP.Key / 24f), shadowsFromKVP.Key / 24f, 1f), 12);
							}
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewshadowsto, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				tileM.map.SetTileBrightness(12);
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						foreach (KeyValuePair<int, List<TileManager.Tile>> shadowsToKVP in tile.shadowsTo) {
							foreach (TileManager.Tile shadowsToTile in shadowsToKVP.Value) {
								shadowsToTile.SetColour(new Color(shadowsToKVP.Key / 24f, 1 - (shadowsToKVP.Key / 24f), shadowsToKVP.Key / 24f, 1f), 12);
							}
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewblockingtiles, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				tileM.map.SetTileBrightness(12);
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						foreach (KeyValuePair<int, List<TileManager.Tile>> blockingShadowsFromKVP in tile.blockingShadowsFrom) {
							foreach (TileManager.Tile blockingShadowsFromTile in blockingShadowsFromKVP.Value) {
								blockingShadowsFromTile.SetColour(Color.red, 12);
							}
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewroofs, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in tileM.map.tiles) {
					if (tile.roof) {
						tile.sr.sprite = whiteSquare;
						tile.sr.color = Color.red;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.load, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				OutputToConsole("Currently has no function.");
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.save, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				OutputToConsole("Currently has no function.");
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
	}
}
