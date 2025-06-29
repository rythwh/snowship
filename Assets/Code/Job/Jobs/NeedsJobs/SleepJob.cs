﻿using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Rest", "Sleep")]
	public class SleepJobDefinition : JobDefinition<SleepJob>
	{
		public SleepJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
			Returnable = false;
		}
	}

	public class SleepJob : Job<SleepJobDefinition>
	{
		private Colonist colonist;
		private NeedInstance sleepNeed;
		private float startSleepNeedValue;
		private float sleepTime;
		private Bed bed;

		public SleepJob(Tile tile) : base(tile) {
			Description = "Sleeping.";
		}

		protected override void OnJobTaken() {
			base.OnJobTaken();

			bed = Bed.Beds
				.Where(b => b.Occupant == null && b.tile.region == Worker.Tile.region)
				.OrderByDescending(b => b.prefab.restComfortAmount)
				.ThenByDescending(b => PathManager.RegionBlockDistance(Worker.Tile.regionBlock, b.tile.regionBlock, true, true, false))
				.FirstOrDefault();

			if (bed != null) {
				ChangeTile(bed.tile);
			}

			sleepNeed = Worker.Needs.Get(ENeed.Rest);
			startSleepNeedValue = sleepNeed.GetValue();
			sleepTime = SimulationDateTime.DayLengthSeconds * (8 / 24f) - (bed?.prefab.restComfortAmount).GetValueOrDefault(0);
		}

		protected override void OnJobStarted() {
			base.OnJobStarted();

			bed.StartSleeping(Worker);
		}

		protected override void OnJobInProgress() {
			base.OnJobInProgress();

			sleepNeed?.ChangeValue(-(startSleepNeedValue / sleepTime));
		}

		protected override void OnJobReturned() {
			base.OnJobReturned();

			bed?.StopSleeping();
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (bed != null) {
				bed.StopSleeping();
				colonist.Moods.AddMoodModifier(bed.prefab.restComfortAmount >= 10 ? MoodModifierEnum.WellRested : MoodModifierEnum.Rested);
			} else {
				colonist.Moods.AddMoodModifier(MoodModifierEnum.SleptOnTheGround);
			}
		}
	}
}