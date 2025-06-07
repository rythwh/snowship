using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NProfession
{
	public class ProfessionsComponent
	{
		private readonly List<ProfessionInstance> professions = new();

		public ProfessionsComponent() {
			foreach (ProfessionPrefab professionPrefab in ProfessionPrefab.professionPrefabs) {
				professions.Add(
					new ProfessionInstance(
						professionPrefab,
						Mathf.RoundToInt(ProfessionPrefab.professionPrefabs.Count / 2f)
					)
				);
			}
		}

		public ProfessionInstance GetProfessionFromType(string type) {
			return professions.Find(p => p.prefab.type == type);
		}
	}
}