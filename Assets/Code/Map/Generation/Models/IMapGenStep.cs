namespace Snowship.NMap.Generation
{
	public interface IMapGenStep
	{
		void Run(MapGenContext context);
	}
}
