using System;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public static class JsonEditorStyles
	{
		private static readonly GUIStyleState redState = new GUIStyleState {
			textColor = Color.softRed
		};
		public static readonly GUIStyle ErrorLabelStyle = new(EditorStyles.largeLabel) {
			normal = redState,
			hover = redState,
			focused = redState,
			active = redState
		};

		public static readonly GUIStyle PlusIconStyle = new GUIStyle(EditorStyles.iconButton) {
			imagePosition = ImagePosition.ImageOnly,
			alignment = TextAnchor.MiddleCenter,
			fixedWidth = 22,
			fixedHeight = 22
		};

		public static readonly GUIStyle ListBoxStyle = new GUIStyle("box") {
			normal = {
				background = Texture2D.grayTexture
			}
		};

		public static readonly Func<bool, GUIStyle> ListButtonStyle = valid => new GUIStyle(EditorStyles.miniButton) {
			alignment = TextAnchor.MiddleLeft,
			normal = {
				textColor = valid ? Color.white : Color.softRed
			},
			focused = {
				textColor = valid ? Color.white : Color.darkRed
			}
		};
	}
}
