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

		private ICameraQuery CameraQuery => GameManager.Get<ICameraQuery>();

		// TODO Refactor to instead calculate by taking the rect of the camera view frustum, and grab the region blocks within those bounds
		internal void DetermineVisibleRegionBlocks() {
			RegionBlock newCentreRegionBlock = GetTileFromPosition(CameraQuery.CurrentPosition).squareRegionBlock;
			if (newCentreRegionBlock == centreRegionBlock && Mathf.RoundToInt(CameraQuery.CurrentZoom) == lastOrthographicSize) {
				return;
			}
			visibleRegionBlocks.Clear();
			lastOrthographicSize = Mathf.RoundToInt(CameraQuery.CurrentZoom);
			centreRegionBlock = newCentreRegionBlock;
			float maxVisibleRegionBlockDistance = CameraQuery.CurrentZoom * ((float)Screen.width / Screen.height);
			List<RegionBlock> frontier = new List<RegionBlock> { centreRegionBlock };
			List<RegionBlock> checkedBlocks = new List<RegionBlock> { centreRegionBlock };
			while (frontier.Count > 0) {
				RegionBlock currentRegionBlock = frontier[0];
				frontier.RemoveAt(0);
				visibleRegionBlocks.Add(currentRegionBlock);
				float currentRegionBlockCameraDistance = Vector2.Distance(currentRegionBlock.averagePosition, CameraQuery.CurrentPosition);
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
			UpdateGlobalLighting(GameManager.Get<TimeManager>().Time.DecimalHour, false);
		}
	}
}
