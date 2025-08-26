using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NMap.Tile;
using Snowship.NHuman;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	public interface IJob
	{
		IJobDefinition Definition { get; }
		Tile Tile { get; }
		EJobState JobState { get; }
		int Layer { get; }
		string Name { get; }
		Sprite Icon { get; }
		string Description { get; }
		List<ResourceAmount> RequiredResources { get; }
		IJob ParentJob { get; }
		IGroupItem Group { get; }
		IGroupItem SubGroup { get; }
		Human Worker { get; }
		List<ContainerPickup> CalculateWorkerResourcePickups(Human worker, List<ResourceAmount> resourcesToPickup);
		void AssignWorker(Human worker);
		EJobState ChangeJobState(EJobState newState);
		bool CanBeAssigned();
		void Close();
	}
}
