using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;
using UnityEngine;

namespace Snowship.NJob
{
	public interface IJobParams
	{
		List<Func<Tile, int, bool>> SelectionConditions => null;
		Sprite JobPreviewSprite => null;

		int SetRotation(int rotation) {
			return 0;
		}

		void UpdateJobPreviewSprite() {
		}
	}
}
