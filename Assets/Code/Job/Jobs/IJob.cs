﻿using Snowship.NHuman;
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
		IGroupItem Group { get; }
		IGroupItem SubGroup { get; }
		Human Worker { get; }
		void AssignWorker(Human human);
		EJobState ChangeJobState(EJobState newState);
		bool CanBeAssigned();
		void Close();
	}
}