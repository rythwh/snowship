using System;
using System.Collections.Generic;
using Snowship.NEntity;
using Snowship.NMaterial;

namespace Snowship.NEditor
{
	public static class JsonEntitiesEditorComponents
	{
		public static readonly Dictionary<EEntityComponent, Action<JsonEntityComponent, JsonArgs, IReadOnlyList<JsonMaterial>>> ComponentEditors = new() {
			{
				EEntityComponent.Building,
				(component, args, materials) => {
					JsonEditorUtilities.IntField("width", component.Data, args, 1);
					JsonEditorUtilities.IntField("height", component.Data, args, 1);
				}
			}, {
				EEntityComponent.Plant,
				(component, args, materials) => {
					JsonEditorUtilities.MaterialAmountField("harvest_materials", component.Data, materials, args);
				}
			}
		};
	}
}
