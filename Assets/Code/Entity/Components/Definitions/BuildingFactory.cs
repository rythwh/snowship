using Snowship.NEntity;

namespace Snowship.NEntity
{
	public class BuildingFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Building;

		public IComponent Create(JsonArgs args)
		{
			int width = args.GetIntOrDefault("width", 1);
			int height = args.GetIntOrDefault("height", 1);
			int integrity = args.GetIntOrDefault("integrity", 100);
			bool blocksLight = args.GetBoolOrDefault("blocks_light", false);
			bool walkable = args.GetBoolOrDefault("walkable", true);
			float walkSpeed = args.GetFloatOrDefault("walk_speed", 1f);
			float flammability = args.GetFloatOrDefault("flammability", 0f);

			return new BuildingComponent(width, height, integrity, blocksLight, walkable, walkSpeed, flammability);
		}
	}
}
