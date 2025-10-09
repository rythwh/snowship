using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NInput;
using Snowship.NLife;
using Snowship.NMap;
using Snowship.NMap.NTile;
using Snowship.NSelection;
using Snowship.NState;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Snowship.NHuman
{
	[UsedImplicitly]
	public sealed class HumanManager : IInitializable, IAsyncStartable, ITickable
	{
		private readonly IHumanQuery humanQuery;
		private readonly IHumanEvents humanEvents;
		private readonly IHumanWrite humanWrite;

		private readonly ICameraWrite cameraWrite;
		private readonly ICameraQuery cameraQuery;

		private readonly InputManager inputM;
		private readonly IMapQuery mapQuery;
		private readonly SelectionManager selectionM;
		private readonly IStateQuery stateQuery;

		private GameObject humanViewPrefab;

		private readonly Dictionary<Gender, string[]> names = new();

		public readonly List<List<Sprite>> humanMoveSprites = new();

		public Human selectedHuman;
		private GameObject selectionIndicator;

		public HumanManager(
			IHumanQuery humanQuery,
			IHumanEvents humanEvents,
			IHumanWrite humanWrite,
			ICameraWrite cameraWrite,
			ICameraQuery cameraQuery,
			InputManager inputM,
			IMapQuery mapQuery,
			SelectionManager selectionM,
			IStateQuery stateQuery
		) {
			this.humanQuery = humanQuery;
			this.humanEvents = humanEvents;
			this.humanWrite = humanWrite;
			this.cameraWrite = cameraWrite;
			this.cameraQuery = cameraQuery;
			this.inputM = inputM;
			this.mapQuery = mapQuery;
			this.selectionM = selectionM;
			this.stateQuery = stateQuery;
		}

		public void Initialize() {
			SkillPrefab.CreateColonistSkills(); // TODO (Solution: Use string references which can be converted to the correct Prefab obj when needed) Skills must currently be ahead of professions to determine skill-profession relationship
			NeedPrefab.CreateColonistNeeds();
			MoodModifierGroup.CreateMoodModifiers();
		}

		public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken()) {
			await LoadHumanPrefab();
			LoadNames();
			LoadSprites();
		}

		public void Tick() {
			if (stateQuery.State == EState.Simulation) {
				SetSelectedHumanFromClick();
			}
			if (Input.GetKey(KeyCode.F) && selectedHuman != null) {
				cameraWrite.SetPosition(selectedHuman.Position, false);
			}
		}

		private async UniTask LoadHumanPrefab() {
			AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Game/Human");
			handle.ReleaseHandleOnCompletion();
			humanViewPrefab = await handle;
		}

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

			HumanView humanView = Object.Instantiate(humanViewPrefab, tile.PositionWorld, Quaternion.identity).GetComponent<HumanView>();
			humanView.gameObject.AddComponent<TViewModule>();

			// TODO Move into Bind()
			humanView.moveSprites = humanMoveSprites[human.BodyTypeProperties[BodySection.Skin]];

			humanView.Bind(human);

			humanWrite.AddHuman<THuman>(human, humanView);

			return human;
		}

		public string GetName(Gender gender) {
			string chosenName = names[gender][Random.Range(0, names[gender].Length)];
			chosenName = char.ToUpper(chosenName[0]) + chosenName[1..].ToLower();
			return chosenName;
		}

		private void SetSelectedHumanFromClick() {
			if (Input.GetMouseButtonDown(0) && !inputM.IsPointerOverUI()) {
				Vector2 mousePosition = cameraQuery.ScreenToWorld(Input.mousePosition);
				List<Human> validHumans = humanQuery.Humans.Where(human => Vector2.Distance(human.Position, mousePosition) < 0.5f).ToList();
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
					colonist.PlayerMoveToTile(mapQuery.Map.GetTileFromPosition(mousePosition));
				}
			}
			if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
				SetSelectedHuman(null);
			}
		}

		public void SetSelectedHuman(Human human) {

			selectedHuman = human;

			SetSelectedHumanIndicator();

			humanEvents.InvokeHumanSelected(selectedHuman);
		}

		private void SetSelectedHumanIndicator() {

			if (selectedHuman == null) {
				selectionIndicator.SetActive(false);
				selectionIndicator.transform.SetParent(null);
				return;
			}

			HumanView humanView = humanQuery.GetHumanView(selectedHuman);

			if (selectionIndicator == null) {
				selectionIndicator = Object.Instantiate(selectionM.SelectionIndicatorPrefab, humanView.transform, false);
				selectionIndicator.name = "SelectedHumanIndicator";
				selectionIndicator.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.UI;
			} else {
				selectionIndicator.transform.SetParent(humanView.transform, false);
			}

			selectionIndicator.SetActive(true);
		}
	}
}
