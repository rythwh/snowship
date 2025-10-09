using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Snowship.NResource
{
	[UsedImplicitly]
	public class ResourceProvider : IResourceQuery
	{
		public Dictionary<EResource, Resource> Resources { get; } = new();
		public Dictionary<Resource.ResourceClassEnum, List<Resource>> ResourceClassToResources { get; } = new();

		public List<Resource> GetResources() {
			return Resources.Values.ToList();
		}

		public Resource GetResourceByString(string resourceString) {
			return GetResourceByEnum((EResource)Enum.Parse(typeof(EResource), resourceString));
		}

		public Resource GetResourceByEnum(EResource resourceType) {
			return Resources[resourceType];
		}

		public List<Resource> GetResourcesInClass(Resource.ResourceClassEnum resourceClass) {
			return ResourceClassToResources[resourceClass];
		}


	}

	public interface IResourceQuery
	{
		Dictionary<EResource, Resource> Resources { get; }
		Dictionary<Resource.ResourceClassEnum, List<Resource>> ResourceClassToResources { get; }

		List<Resource> GetResources();
		Resource GetResourceByString(string resourceString);
		Resource GetResourceByEnum(EResource resourceType);
		List<Resource> GetResourcesInClass(Resource.ResourceClassEnum resourceClass);
	}
}
