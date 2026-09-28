using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NEntity
{
	public class BuildingComponent : IComponent
	{
		public int Width { get; }
		public int Height { get; }
		public int Integrity { get; }
		public bool BlocksLight { get; }
		public bool Walkable { get; }
		public float WalkSpeed { get; }
		public float Flammability { get; }

		public BuildingComponent(int width, int height, int integrity, bool blocksLight, bool walkable, float walkSpeed, float flammability)
		{
			Width = width;
			Height = height;
			Integrity = integrity;
			BlocksLight = blocksLight;
			Walkable = walkable;
			WalkSpeed = walkSpeed;
			Flammability = flammability;
		}

		public IEnumerable<Vector2Int> GetFootprint(int rotation)
		{
			int w = rotation % 2 == 0 ? Width : Height;
			int h = rotation % 2 == 0 ? Height : Width;

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					yield return new Vector2Int(x, y);
				}
			}
		}
	}
}
