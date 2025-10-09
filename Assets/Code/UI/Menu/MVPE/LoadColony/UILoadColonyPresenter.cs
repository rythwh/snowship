using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

using Snowship.NPlanet;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UILoadColonyPresenter : UIPresenter<UILoadColonyView> {
		private readonly UIManager uiM;

		private PlanetViewModule planetViewModule;
		// private PersistenceColony selectedColony;
		// private readonly PColony pColony = new PColony();

		public UILoadColonyPresenter(UILoadColonyView view, UIManager uiM) : base(view) {
			this.uiM = uiM;
		}

		public override UniTask OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;
			View.OnCreateColonyButtonClicked += OnCreateColonyButtonClicked;
			// View.OnLoadColonyButtonClicked += OnLoadColonyButtonClicked;

			// CreateColonyElements();

			CreatePlanetViewModule();

			return UniTask.CompletedTask;
		}

		public override void OnClose() {
			View.OnBackButtonClicked -= OnBackButtonClicked;
			View.OnCreateColonyButtonClicked -= OnCreateColonyButtonClicked;
			// View.OnLoadColonyButtonClicked -= OnLoadColonyButtonClicked;

			ClosePlanetViewModule();
		}

		private void CreatePlanetViewModule() {
			planetViewModule = new PlanetViewModule(View.PlanetViewGridLayoutGroup, View.PlanetTilePrefab);

			planetViewModule.OnPlanetTileClicked += OnPlanetTileClicked;
			// planetViewModule.OnColonyTileClicked += OnColonyTileClicked;
		}

		private void ClosePlanetViewModule() {
			planetViewModule.OnPlanetTileClicked -= OnPlanetTileClicked;
			// planetViewModule.OnColonyTileClicked -= OnColonyTileClicked;

			planetViewModule.DestroyPlanet();
		}

		private void OnBackButtonClicked() {
			uiM.CloseView(this);
		}

		private async void OnCreateColonyButtonClicked() {
			await uiM.OpenViewAsync<UICreateColony>(this, false);
		}

		/*private void OnColonyElementClicked(PersistenceColony colony) {
			SelectColony(colony);
		}*/

		/*private void OnLoadColonyButtonClicked() {
			pColony.ApplyLoadedColony(selectedColony);
		}*/

		/*private void CreateColonyElements() {
			List<PersistenceColony> colonies = pColony.GetPersistenceColonies();

			planetViewModule.DisplayPlanet(planetM.planet, colonies, true);

			foreach (PersistenceColony colony in colonies) {
				UILoadColonyElement loadColonyElement = new(colony);
				loadColonyElement.Open(View.ColonyElementsParent);
				loadColonyElement.OnLoadColonyElementClicked += OnColonyElementClicked;
			}
		}*/

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			// Do nothing
		}

		/*private void OnColonyTileClicked(PersistenceColony colony) {
			SelectColony(colony);
		}*/

		/*private void SelectColony(PersistenceColony colony) {
			selectedColony = colony;

			bool colonyValid = selectedColony != null;
			string loadColonyButtonText = colonyValid ? $"Load {colony.name}" : "Select a Colony to Load";
			View.SetLoadColonyButtonInteractable(colonyValid, loadColonyButtonText);
		}*/
	}
}
