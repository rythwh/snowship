using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NInput;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NUtilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Snowship.NHuman
{
	public class HumanManager : Manager
	{
		public readonly List<Human> humans = new();

		private readonly Dictionary<Life.Gender, string[]> names = new();

		public readonly List<List<Sprite>> humanMoveSprites = new();

		public Human selectedHuman;
		private GameObject selectionIndicator;
		public event Action<Human> OnHumanSelected;

		private CameraManager CameraM => GameManager.Get<CameraManager>();
		private InputManager InputM => GameManager.Get<InputManager>();
		private ResourceManager ResourceM => GameManager.Get<ResourceManager>();
		private MapManager MapM => GameManager.Get<MapManager>();

		public void CreateNames() {
			foreach (Life.Gender gender in Enum.GetValues(typeof(Life.Gender))) {
				names.Add(gender, Resources.Load<TextAsset>($"Data/names-{gender.ToString().ToLower()}").text.Split(' '));
			}
		}

		public void CreateHumanSprites() {
			for (int i = 0; i < 3; i++) {
				List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
				humanMoveSprites.Add(innerHumanMoveSprites);
			}
		}

		public override void OnUpdate() {
			if (MapM.MapState == MapState.Generated) {
				SetSelectedHumanFromClick();
			}
			if (Input.GetKey(KeyCode.F) && selectedHuman != null) {
				CameraM.SetCameraPosition(selectedHuman.obj.transform.position, false);
			}
		}

		public string GetName(Life.Gender gender) {
			string[] genderNames = names[gender];
			string[] validNames = genderNames.Where(name => humans.All(human => !string.Equals(human.Name, name, StringComparison.CurrentCultureIgnoreCase))).ToArray();

			string chosenName = validNames.Length > 0
				? validNames[Random.Range(0, validNames.Length)]
				: genderNames[Random.Range(0, genderNames.Length)];
			chosenName = char.ToUpper(chosenName[0]) + chosenName[1..].ToLower();
			return chosenName;
		}

		private void SetSelectedHumanFromClick() {
			if (Input.GetMouseButtonDown(0) && !InputM.IsPointerOverUI()) {
				Vector2 mousePosition = CameraM.camera.ScreenToWorldPoint(Input.mousePosition);
				List<Human> validHumans = humans.Where(human => Vector2.Distance(human.obj.transform.position, mousePosition) < 0.5f).ToList();
				Human humanToSelect = null;
				switch (validHumans.Count) {
					case 1:
						humanToSelect = validHumans.First();
						break;
					case > 1: {
						bool previousHumanIsSelected = false;
						foreach (Human human in validHumans) {
							if (previousHumanIsSelected) {
								humanToSelect = human;
								break;
							}
							if (human == selectedHuman) {
								previousHumanIsSelected = true;
							}
						}
						if (!previousHumanIsSelected) {
							humanToSelect = validHumans.FirstOrDefault();
						}
						break;
					}
				}
				if (humanToSelect != null) {
					SetSelectedHuman(humanToSelect);
				} else if (selectedHuman is Colonist colonist) {
					colonist.PlayerMoveToTile(MapM.Map.GetTileFromPosition(mousePosition));
				}
			}
			if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
				SetSelectedHuman(null);
			}
		}

		public void SetSelectedHuman(Human human) {

			selectedHuman = human;

			SetSelectedHumanIndicator();

			OnHumanSelected?.Invoke(selectedHuman);
		}

		private void SetSelectedHumanIndicator() {

			if (selectedHuman == null) {
				selectionIndicator.SetActive(false);
				selectionIndicator.transform.SetParent(null);
				return;
			}

			if (selectionIndicator == null) {
				selectionIndicator = Object.Instantiate(ResourceM.selectionIndicator, selectedHuman.obj.transform, false);
				selectionIndicator.name = "SelectedHumanIndicator";
				selectionIndicator.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.UI;
			} else {
				selectionIndicator.transform.SetParent(selectedHuman.obj.transform);
			}

			selectionIndicator.transform.localPosition = Vector3.zero;
			selectionIndicator.SetActive(true);
		}
	}
}
