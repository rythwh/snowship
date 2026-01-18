using Snowship.NMaterial;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public static class JsonMaterialsEditorUtilities
	{
		public static bool IsMaterialValid(JsonMaterial material)
		{
			return
				!string.IsNullOrWhiteSpace(material.Id)
				&& !string.IsNullOrWhiteSpace(material.ClassId);
		}

		public static GUIContent GetStatModifierImage(EStat stat)
		{
			return stat switch {
				EStat.BuildTime => EditorGUIUtility.IconContent("UnityEditor.AnimationWindow@2x"),
				EStat.Value => EditorGUIUtility.IconContent("d_LightProbes Icon"),
				EStat.WalkSpeed => EditorGUIUtility.IconContent("d_NavMeshAgent Icon"),
				EStat.Integrity => EditorGUIUtility.IconContent("Animation.Record@2x"),
				EStat.Flammability => EditorGUIUtility.IconContent("d_VisualEffectAsset Icon"),
				EStat.Insulation => EditorGUIUtility.IconContent("d_SpeedTreeWindAsset Icon"),
				EStat.Fuel => EditorGUIUtility.IconContent("d_ParticleSystemForceField Icon"),
				_ => EditorGUIUtility.IconContent("d_Help@2x")
			};
		}

		public static GUIContent GetTraitImage(ETrait trait)
		{
			return trait switch {
				ETrait.Fuel => EditorGUIUtility.IconContent("d_ParticleSystemForceField Icon"),
				ETrait.Craftable => EditorGUIUtility.IconContent("BuildProfile Icon"),
				ETrait.Food => EditorGUIUtility.IconContent("d_PlatformEffector2D Icon"),
				ETrait.Clothing => EditorGUIUtility.IconContent("Cloth Icon"),
				_ => EditorGUIUtility.IconContent("d_Help@2x")
			};
		}
	}
}
