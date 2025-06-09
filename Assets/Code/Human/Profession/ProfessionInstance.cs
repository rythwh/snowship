namespace Snowship.NProfession {

	public class ProfessionInstance {

		public readonly ProfessionPrefab prefab;

		private int priority;

		public ProfessionInstance(
			ProfessionPrefab prefab,
			int priority
		) {
			this.prefab = prefab;
			this.priority = priority;
		}

		public int GetPriority() {
			return priority;
		}

		public void SetPriority(int newPriority) {
			if (newPriority > ProfessionPrefab.maxPriority) {
				newPriority = 0;
			}

			if (newPriority < 0) {
				newPriority = ProfessionPrefab.maxPriority;
			}

			priority = newPriority;
		}

		public void IncreasePriority() {
			SetPriority(priority + 1);
		}

		public void DecreasePriority() {
			SetPriority(priority - 1);
		}
	}
}