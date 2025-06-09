using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NJob
{
	public interface IJobParams
	{
		List<Func<TileManager.Tile, int, bool>> SelectionConditions => null;
		Sprite JobPreviewSprite => null;

		int SetRotation(int rotation) {
			return 0;
		}

		void UpdateJobPreviewSprite() {
		}
	}
}
