namespace EditorHelper
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

	public class SerializedArray
    {
		public SerializedArray (SerializedProperty property) => Initialize (null, property);

		public SerializedArray (SerializedObject serializedObject, string propertyPath) => Initialize (null, serializedObject.FindProperty (propertyPath));
		
		public SerializedArray (string headerLabel, SerializedProperty property) => Initialize (headerLabel, property);
		
		public SerializedArray (string headerLabel, SerializedObject serializedObject, string propertyPath) => Initialize (headerLabel, serializedObject.FindProperty (propertyPath));
		
		private SerializedProperty property;
		private string headerLabel;
		
		private ReorderableList list;

		public void Draw ()
		{
			if (property == null || list == null)
				return;

			using (new EditorGUILayout.HorizontalScope ())
			{
				float sizeWidth = 40;
				float sizeHeight = EditorGUIUtility.singleLineHeight - 1;

				property.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup (property.isExpanded, headerLabel);

				property.arraySize = EditorGUILayout.IntField (property.arraySize, GUILayout.Width (sizeWidth), GUILayout.Height (sizeHeight));

				EditorGUILayout.EndFoldoutHeaderGroup ();
			}

			if (property.isExpanded)
				list.DoLayoutList ();
		}

		private void Initialize (string headerLabel, SerializedProperty property)
		{
			this.property = property;

			if (!property.isArray)
			{
				Debug.Log (string.Format ("The property is not an array. Property: {0}", property.propertyPath));
				return;
			}

			this.headerLabel = (headerLabel == null || headerLabel.Length == 0) ? property.displayName : headerLabel;

			list = new (property.serializedObject, property, draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: false);

			list.drawElementCallback = DrawArrayElement;
			list.onAddCallback = AddElement;

			list.multiSelect = true;
		}

		private void DrawArrayElement (Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex (index);

			float labelWidth = 15;
			float buttonWidth = 20;

			float height = EditorGUIUtility.singleLineHeight;
			
			float padding = 5;

			float labelOffset = labelWidth + padding;
			float buttonOffset = buttonWidth + padding;

			float propertyPosX = rect.x + labelOffset + buttonOffset;
			float propertyWidth = rect.width - labelOffset - buttonOffset * 2;

			if (property.arraySize > 10)
				labelWidth += padding;

			Rect indexRect = new (rect.x, rect.y, labelWidth, height);
			Rect buttonAddRect = new (rect.x + labelOffset, rect.y, buttonWidth, height);
			Rect propertyRect = new (propertyPosX, rect.y, propertyWidth, height);
			Rect buttonRemoveRect = new (rect.width + buttonWidth, rect.y, buttonWidth, height);

			EditorGUI.LabelField (indexRect, index.ToString ());

			if (GUI.Button (buttonAddRect, "+"))
				property.Insert (index, default);

			EditorGUI.PropertyField (propertyRect, element, GUIContent.none);

			if (GUI.Button (buttonRemoveRect, "-"))
				ReorderableList.defaultBehaviours.DoRemoveButton (list);
		}

		private void AddElement (ReorderableList list)
		{
			property.Add (default);
		}
	}
}
