using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceManager : MonoBehaviour {

	public enum ResourceGroups { Natural, Materials };

	public enum Resources { Dirt, Stone, Logs, Firewood };

	public List<ResourceGroup> resourceGroups = new List<ResourceGroup>();

	public class ResourceGroup {

		public ResourceGroups type;
		public string name;

		public List<Resources> resourceTypes = new List<Resources>();
		public List<Resource> resources = new List<Resource>();

		public ResourceGroup(List<string> resourceGroupData, ResourceManager rm) {
			type = (ResourceGroups)System.Enum.Parse(typeof(ResourceGroups),resourceGroupData[0]);
			name = type.ToString();

			List<string> resourceData = resourceGroupData[1].Split('`').ToList();
			foreach (string resourceString in resourceData) {
				print(resourceString);
				Resource resource = new Resource(resourceString.Split('/').ToList(),this,rm);
				resourceTypes.Add(resource.type);
				resources.Add(resource);
				rm.resources.Add(resource);
			}
		}
	}

	public List<Resource> resources = new List<Resource>();

	public class Resource {
		public Resources type;
		public string name;
		
		public ResourceGroup resourceGroup;

		public int value;

		public Resource(List<string> resourceData,ResourceGroup resourceGroup, ResourceManager rm) {
			type = (Resources)System.Enum.Parse(typeof(Resources),resourceData[0]);
			name = type.ToString();

			this.resourceGroup = resourceGroup;

			value = int.Parse(resourceData[1]);

			print(name + " " + value + " " + resourceGroup.name);
		}
	}

	public void CreateResources() {
		List<string> resourceGroupsDataString = UnityEngine.Resources.Load<TextAsset>(@"Data/resources").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('~').ToList();
		foreach (string resourceGroupDataString in resourceGroupsDataString) {
			print(resourceGroupDataString);
			List<string> resourceGroupData = resourceGroupDataString.Split(':').ToList();
			ResourceGroup resourceGroup = new ResourceGroup(resourceGroupData,this);
			resourceGroups.Add(resourceGroup);
		}
	}

	public class ResourceAmount {
		public Resource resource;
		public int amount;
		public ResourceAmount(Resource resource,int amount) {
			this.resource = resource;
			this.amount = amount;
		}
	}

	public class ReservedResources {
		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public ColonistManager.Colonist colonist;

		public ReservedResources(List<ResourceAmount> resourcesToReserve,ColonistManager.Colonist colonistReservingResources) {
			resources.AddRange(resourcesToReserve);
			colonist = colonistReservingResources;
		}
	}

	public class Container {
		public Inventory inventory;
	}

	public class Inventory {
		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public List<ReservedResources> reservedResources = new List<ReservedResources>();

		public ColonistManager.Colonist colonist;
		public Container container;

		public Inventory(ColonistManager.Colonist colonist, Container container) {
			this.colonist = colonist;
			this.container = container;
		}

		public void ChangeResourceAmount(Resource resource,int amount) {
			ResourceAmount existingResourceAmount = resources.Find(ra => ra.resource == resource);
			if (existingResourceAmount != null) {
				if (amount >= 0 || (amount - existingResourceAmount.amount) >= 0) {
					print("Added an additional " + amount + " of " + resource.name + " to " + colonist.name);
					existingResourceAmount.amount += amount;
				} else {
					Debug.LogError("Trying to remove " + amount + " of " + resource.name + " on " + colonist.name + " when only " + existingResourceAmount.amount + " of that resource exist in this inventory");
				}
			} else {
				if (amount > 0) {
					print("Adding " + resource.name + " to " + colonist.name + " with a starting amount of " + amount);
					resources.Add(new ResourceAmount(resource,amount));
				} else if (amount < 0) {
					Debug.LogError("Trying to remove " + amount + " of " + resource.name + " that doesn't exist in " + colonist.name);
				}
			}
			if (existingResourceAmount.amount == 0) {
				print("Removed " + existingResourceAmount.resource.name + " from " + colonist.name + " as its amount was 0");
				resources.Remove(existingResourceAmount);
			} else if (existingResourceAmount.amount < 0) {
				Debug.LogError("There is a negative amount of " + resource.name + " on " + colonist.name + " with " + existingResourceAmount.amount);
			} else {
				print(colonist.name + " now has " + existingResourceAmount.amount + " of " + existingResourceAmount.resource.name);
			}
		}

		public void ReserveResources(List<ResourceAmount> resourcesToReserve) {

		}
	}
}
