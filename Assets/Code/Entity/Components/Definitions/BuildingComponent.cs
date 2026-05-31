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
	}
}
