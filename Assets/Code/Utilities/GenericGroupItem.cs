using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUtilities
{
	public class GenericGroupItem : IGroupItem
	{
		public string Name { get; }
		public Sprite Icon { get; set; }
		public List<IGroupItem> Children { get; }

		public GenericGroupItem(string name, List<IGroupItem> children = null, Sprite icon = null)
		{
			Name = name;
			Children = children;
			Icon = icon;
		}
	}
}
