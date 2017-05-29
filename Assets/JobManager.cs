using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JobManager : MonoBehaviour {

	public List<Job> jobs = new List<Job>();

	public class Job {
		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public bool accessible;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab, ColonistManager cm) {
			this.tile = tile;
			this.prefab = prefab;

			accessible = false;
			foreach (ColonistManager.Colonist colonist in cm.colonists) {
				if (colonist.overTile.region == tile.region) {
					accessible = true;
					break;
				}
			}
		}

		public void SetColonist(ColonistManager.Colonist colonist) {
			this.colonist = colonist;
		}
	}

	public ResourceManager.TileObjectPrefab selectedPrefab;

	void Update() {
		CreateJobs();
		GiveJobsToColonists();
	}

	public void CreateJobs() {
		/*
		if (selectedPrefab != null) {
			Vector2 mousePosition = GetComponent<CameraManager>().cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile startTile = null;
			if (Input.GetMouseButtonDown(0)) {
				startTile = GetComponent<TileManager>().GetTileFromPosition(mousePosition);
			}
			if (startTile != null) {
				for (int y = (startTile.obj.transform.position.x - mousePosition.x > 0 ? startTile.obj.transform.position.x : 
		}
		*/
	}

	public void GiveJobsToColonists() {
		List<Job> jobsToRemove = new List<Job>();
		foreach (Job job in jobs) {
			if (job.accessible) {
				List<ColonistManager.Colonist> sortedColonists = GetComponent<ColonistManager>().colonists.Where(c => c.job == null).OrderBy(c => Vector2.Distance(c.overTile.obj.transform.position,job.tile.obj.transform.position)).ToList();
				foreach (ColonistManager.Colonist colonist in sortedColonists) {
					colonist.SetJob(job);
					jobsToRemove.Add(job);
				}
			}
		}
		foreach (Job job in jobsToRemove) {
			jobs.Remove(job);
		}
		jobsToRemove.Clear();
	}
}
