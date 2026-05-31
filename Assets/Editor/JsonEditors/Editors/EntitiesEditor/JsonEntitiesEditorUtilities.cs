using Snowship.NEntity;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public static class JsonEntitiesEditorUtilities
	{
		public static bool IsEntityValid(JsonEntity entity)
		{
			return
				!string.IsNullOrWhiteSpace(entity.Id)
				&& !string.IsNullOrWhiteSpace(entity.Category);
		}

		public static GUIContent GetComponentImage(EEntityComponent component)
		{
			switch (component) {
				case EEntityComponent.BaseRecipe:
					return EditorGUIUtility.IconContent("d_ShaderInclude Icon");
				case EEntityComponent.BaseStats:
					return EditorGUIUtility.IconContent("d_NetworkIdentity Icon");
				case EEntityComponent.MaterialOptions:
					return EditorGUIUtility.IconContent("d_Material Icon");
				case EEntityComponent.Building:
					return EditorGUIUtility.IconContent("d_AudioReverbFilter Icon");
				case EEntityComponent.Bed:
					return EditorGUIUtility.IconContent("d_DataMode.Authoring@2x");
				case EEntityComponent.Container:
					return EditorGUIUtility.IconContent("d_ModelImporter Icon");
				case EEntityComponent.Crafting:
					return EditorGUIUtility.IconContent("CustomCollider2D Icon");
				case EEntityComponent.Plant:
					return EditorGUIUtility.IconContent("d_BillboardAsset Icon");
				case EEntityComponent.Quality:
					return EditorGUIUtility.IconContent("d_Favorite Icon");
				default:
					return EditorGUIUtility.IconContent("d_Help@2x");
			};
		}
	}
}
