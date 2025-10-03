using System.Collections.Generic;
using Snowship.NCamera;
using Snowship.NMap.Models.Structure;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NMap
{
	public partial class Map
	{
		private readonly List<RegionBlock> visibleRegionBlocks = new List<RegionBlock>();
		private RegionBlock centreRegionBlock;
		private int lastOrthographicSize = -1;

		// TODO Refactor to instead calculate by taking the rect of the camera view frustum, and grab the region blocks within those bounds
		internal void DetermineVisibleRegionBlocks() {
			Camera camera = GameManager.Get<CameraManager>().camera;
			RegionBlock newCentreRegionBlock = GetTileFromPosition(camera.transform.position).squareRegionBlock;
			if (newCentreRegionBlock == centreRegionBlock && Mathf.RoundToInt(camera.orthographicSize) == lastOrthographicSize) {
				return;
			}
			visibleRegionBlocks.Clear();
			lastOrthographicSize = Mathf.RoundToInt(camera.orthographicSize);
			centreRegionBlock = newCentreRegionBlock;
			float maxVisibleRegionBlockDistance = camera.orthographicSize * ((float)Screen.width / Screen.height);
			List<RegionBlock> frontier = new List<RegionBlock> { centreRegionBlock };
			List<RegionBlock> checkedBlocks = new List<RegionBlock> { centreRegionBlock };
			while (frontier.Count > 0) {
				RegionBlock currentRegionBlock = frontier[0];
				frontier.RemoveAt(0);
				visibleRegionBlocks.Add(currentRegionBlock);
				float currentRegionBlockCameraDistance = Vector2.Distance(currentRegionBlock.averagePosition, camera.transform.position);
				foreach (RegionBlock nBlock in currentRegionBlock.surroundingRegionBlocks) {
					if (checkedBlocks.Contains(nBlock)) {
						continue;
					}
					if (currentRegionBlockCameraDistance <= maxVisibleRegionBlockDistance) {
						frontier.Add(nBlock);
					} else {
						visibleRegionBlocks.Add(nBlock);
					}
					checkedBlocks.Add(nBlock);
				}
			}
			UpdateGlobalLighting(GameManager.Get<TimeManager>().Time.TileBrightnessTime, false);
		}
	}
}
