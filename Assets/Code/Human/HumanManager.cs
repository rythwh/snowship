using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NInput;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NMap.NTile;
using Snowship.NUtilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Snowship.NHuman
{
	public sealed class HumanManager : Manager
	{
		private readonly List<Human> humans = new();
		private readonly Dictionary<Human, HumanView> humanToViewMap = new();
		private readonly Dictionary<Type, List<Human>> humansByType = new();

		private readonly Dictionary<Gender, string[]> names = new();

		public readonly List<List<Sprite>> humanMoveSprites = new();

		public Human selectedHuman;
		private GameObject selectionIndicator;
		public event Action<Human> OnHumanSelected;

		private CameraManager CameraM => GameManager.Get<CameraManager>();
		private InputManager InputM => GameManager.Get<InputManager>();
		private ResourceManager ResourceM => GameManager.Get<ResourceManager>();
		private MapManager MapM => GameManager.Get<MapManager>();

		public void LoadNames() {
			foreach (Gender gender in Enum.GetValues(typeof(Gender))) {
				names.Add(gender, Resources.Load<TextAsset>($"Data/names-{gender.ToString().ToLower()}").text.Split(' '));
			}
		}

		public void LoadSprites() {
			for (int i = 0; i < 3; i++) {
				List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
				humanMoveSprites.Add(innerHumanMoveSprites);
			}
		}

		public THuman CreateHuman<THuman, TViewModule>(Tile tile, HumanData data)
			where THuman : Human
			where TViewModule : LifeViewModule
		{
			int id = IdUtility.NextIdFor<THuman>();
			Type humanType = typeof(THuman);

			if (Activator.CreateInstance(humanType, id, tile, data) is not THuman human) {
				Debug.LogError($"Human ({humanType}) was null when trying to create.");
				return null;
			}

			HumanView humanView = Object.Instantiate(ResourceM.humanPrefab, tile.PositionWorld, Quaternion.identity).GetComponent<HumanView>();
			humanView.gameObject.AddComponent<TViewModule>();

			// TODO Move into Bind()
			humanView.moveSprites = humanMoveSprites[human.BodyTypeProperties[BodySection.Skin]];

			humanView.Bind(human);

			if (!humansByType.TryAdd(humanType, new List<Human> { human })) {
				humansByType[humanType] ??= new List<Human>();
				humansByType[humanType].Add(human);
			}
			humanToViewMap.Add(human, humanView);
			humans.Add(human);
			return human;
		}

		public ReadOnlyCollection<Human> GetHumans() {
			return humans.AsReadOnly();
		}

		/// <summary>
		/// If the return result of IEnumerable&lt;THuman&gt; is simply iterated over (foreach),
		/// no copy of the original list will be created -> better performance.
		/// If a copy is needed, can call ToList() on the result.
		/// </summary>
		/// <typeparam name="THuman"></typeparam>
		/// <returns>IEnumerable&lt;THuman&gt;</returns>
		public IEnumerable<THuman> GetHumans<THuman>() where THuman : Human {
			if (humansByType.TryGetValue(typeof(THuman), out List<Human> humansOfType)) {
				return humansOfType.Cast<THuman>();
			}
			humansByType[typeof(THuman)] = new List<Human>();
			return humansByType[typeof(THuman)].Cast<THuman>();
		}

		public int CountHumans<THuman>() where THuman : Human {
			if (humansByType.TryGetValue(typeof(THuman), out List<Human> humansOfType)) {
				return humansOfType.Count;
			}
			return 0;
		}

		public THumanView GetHumanView<THuman, THumanView>(THuman human) where THuman : Human where THumanView : HumanView {
			humanToViewMap.TryGetValue(human, out HumanView humanView);
			return humanView as THumanView;
		}

		public HumanView GetHumanView(Human human) {
			humanToViewMap.TryGetValue(human, out HumanView humanView);
			return humanView;
		}

		public override void OnUpdate() {
			if (MapM.MapState == MapState.Generated) {
				SetSelectedHumanFromClick();
			}
			if (Input.GetKey(KeyCode.F) && selectedHuman != null) {
				CameraM.SetCameraPosition(selectedHuman.Position, false);
			}
		}

		public string GetName(Gender gender) {
			string chosenName = names[gender][Random.Range(0, names[gender].Length)];
			chosenName = char.ToUpper(chosenName[0]) + chosenName[1..].ToLower();
			return chosenName;
		}

		private void SetSelectedHumanFromClick() {
			if (Input.GetMouseButtonDown(0) && !InputM.IsPointerOverUI()) {
				Vector2 mousePosition = CameraM.camera.ScreenToWorldPoint(Input.mousePosition);
				List<Human> validHumans = humans.Where(human => Vector2.Distance(human.Position, mousePosition) < 0.5f).ToList();
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

			HumanView humanView = humanToViewMap[selectedHuman];

			if (selectionIndicator == null) {
				selectionIndicator = Object.Instantiate(ResourceM.selectionIndicator, humanView.transform, false);
				selectionIndicator.name = "SelectedHumanIndicator";
				selectionIndicator.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.UI;
			} else {
				selectionIndicator.transform.SetParent(humanView.transform, false);
			}

			selectionIndicator.SetActive(true);
		}

		public void RemoveHuman(Human human) {

			humans.Remove(human);
			humansByType[human.GetType()].Remove(human);

			HumanView humanView = humanToViewMap[human];
			humanView.Unbind();
			Object.Destroy(humanView.gameObject);
			humanToViewMap.Remove(human);

			OnHumanRemoved?.Invoke(human);
		}
	}
}
