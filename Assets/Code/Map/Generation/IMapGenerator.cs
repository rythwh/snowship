using Cysharp.Threading.Tasks;

namespace Snowship.NMap.Generation
{
	public interface IMapGenerator
	{
		UniTask Run(MapGenContext context);
	}
}