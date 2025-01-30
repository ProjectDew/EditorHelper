namespace EditorHelper
{
	using System;
	using System.IO;
	using System.Text;
	using System.Linq;
	using System.Reflection;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public class InspectorBuilder : EditorWindow
	{
		[MenuItem ("Assets/Generate Custom Inspector")]
		public static void OpenInspectorBuilder ()
		{
			InspectorBuilder window = GetWindow<InspectorBuilder> ();

			window.titleContent = new ("Inspector Builder");
			window.minSize = new (350, 300);
		}

		private static readonly string templateNotFoundError = "A template file for the custom inspector wasn't found. Make sure there is a document in the project with this name: \"{0}\".";
		private static readonly string canEditMultipleObjectsText = ", CanEditMultipleObjects";

		[SerializeField]
		private string namespaceName;
		
		[SerializeField]
		private string folderPath;
		
		[SerializeField]
		private bool canEditMultipleObjects;

		private Type[] types;
		private string[] typeNames;

		private int selectedIndex;

		private SerializedObject serializedObject;

		private SerializedProperty namespaceNameProp;
		private SerializedProperty folderPathProp;
		private SerializedProperty canEditMultipleObjectsProp;

		public class ClassInfo
		{
			public ClassInfo (FieldInfo[] classFields)
			{
				nonArrayInfo = new ();
				arrayInfo = new ();

				for (int i = 0; i < classFields.Length; i++)
				{
					Type fieldType = classFields[i].FieldType;

					if (fieldType.IsArray && fieldType.GetElementType () != typeof (string))
						arrayInfo.Add (classFields[i]);
					else
						nonArrayInfo.Add (classFields[i]);
				}
			}

			private readonly List<FieldInfo> nonArrayInfo;
			private readonly List<FieldInfo> arrayInfo;

			public FieldInfo[] GetNonArrayInfo () => nonArrayInfo.ToArray ();

			public FieldInfo[] GetArrayInfo () => arrayInfo.ToArray ();
		}

		private void OnEnable ()
		{
			serializedObject = new (this);

			namespaceNameProp = serializedObject.FindProperty ("namespaceName");
			folderPathProp = serializedObject.FindProperty ("folderPath");
			canEditMultipleObjectsProp = serializedObject.FindProperty ("canEditMultipleObjects");

			FindInspectableTypes ();
		}

		private void OnGUI ()
		{
			serializedObject.Update ();

			GUILayout.Space (10);

			using (new EditorGUILayout.HorizontalScope ())
			{
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("INSPECTOR BUILDER", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace ();
			}

			GUILayout.Space (10);

			GUILayout.Label ("Set up inspector", EditorStyles.boldLabel);
			
			GUILayout.Space (5);

			selectedIndex = EditorGUILayout.Popup ("Class type", selectedIndex, typeNames);

			GUILayout.Space (5);

			EditorGUILayout.PropertyField (canEditMultipleObjectsProp, new GUIContent ("Can edit multiple objects"));

			GUILayout.Space (5);

			EditorGUILayout.PropertyField (namespaceNameProp, new GUIContent ("Namespace (optional)"));

			GUILayout.Space (20);

			GUILayout.Label ("Create inspector", EditorStyles.boldLabel);
			
			GUILayout.Space (5);

			EditorGUILayout.HelpBox ($"If there is a class named \"{typeNames[selectedIndex]}Inspector\" in the selected folder, this action will overwrite it.", MessageType.Warning);

			GUILayout.Space (5);

			EditorGUILayout.PropertyField (folderPathProp, new GUIContent ("Folder path"));

			if (folderPathProp.stringValue == null || folderPathProp.stringValue.Length == 0)
				folderPathProp.stringValue = "Assets/Editor/";

			GUILayout.Space (5);

			if (GUILayout.Button ("Generate inspector"))
				GenerateCustomInspector ();

			serializedObject.ApplyModifiedProperties ();
		}

		private void FindInspectableTypes ()
		{
			types =
				AppDomain.CurrentDomain.GetAssemblies ()
				.Where (assembly => !assembly.FullName.Contains ("UnityEngine") && !assembly.FullName.Contains ("UnityEditor"))
				.Where (assembly => !assembly.FullName.Contains ("Unity.") && !assembly.FullName.Contains ("System."))
				.SelectMany (assembly => assembly.GetTypes ())
				.Where (type => type.BaseType == typeof (MonoBehaviour) || type.BaseType == typeof (ScriptableObject))
				.ToArray ();

			typeNames = new string[types.Length];

			for (int i = 0; i < types.Length; i++)
				typeNames[i] = types[i].Name;
		}

		private void GenerateCustomInspector ()
		{
			FieldInfo[] classFields = GetClassFields ();

			if (classFields == null)
				return;

			ClassInfo classInfo = new (classFields);

			bool addNamespace = namespaceName != null && namespaceName.Length > 0;

			string templateName = addNamespace ? "NamespaceCustomInspectorTemplate" : "NoNamespaceCustomInspectorTemplate";
			string searchFilter = $"{templateName}_DONTDELETEFROMPROJECT";

			TextAsset template = GetTemplate (searchFilter);

			StringBuilder sb = new (template.text);

			string variable0 = namespaceName;
			string variable1 = typeNames[selectedIndex];
			string variable2 = canEditMultipleObjects ? canEditMultipleObjectsText : string.Empty;

			FieldInfo[] nonArrayInfo = classInfo.GetNonArrayInfo ();
			FieldInfo[] arrayInfo = classInfo.GetArrayInfo ();

			StringBuilder fieldsContent = new ();
			
			sb.Replace ("{0}", variable0);
			sb.Replace ("{1}", variable1);
			sb.Replace ("{2}", variable2);
			sb.Replace ("{3}", GetFieldTexts (fieldsContent, nonArrayInfo, templateName, 3));
			sb.Replace ("{4}", GetFieldTexts (fieldsContent, arrayInfo, templateName, 4));
			sb.Replace ("{5}", GetFieldTexts (fieldsContent, nonArrayInfo, templateName, 5));
			sb.Replace ("{6}", GetFieldTexts (fieldsContent, arrayInfo, templateName, 6));
			sb.Replace ("{7}", GetFieldTexts (fieldsContent, nonArrayInfo, templateName, 7));
			sb.Replace ("{8}", GetFieldTexts (fieldsContent, arrayInfo, templateName, 8));
			
			if (!Directory.Exists (folderPath))
				Directory.CreateDirectory (folderPath);

			string fileExtension = ".cs";
			string fileName = string.Concat (typeNames[selectedIndex], "Inspector", fileExtension);

			string filePath = Path.Combine (folderPath, fileName);

			File.WriteAllText (filePath, sb.ToString ());

			AssetDatabase.Refresh ();
		}

		private TextAsset GetTemplate (string searchFilter)
		{
			TextAsset template = Assets.Find<TextAsset> (searchFilter);

			if (template == null)
			{
				string errorMessage = string.Format (templateNotFoundError, searchFilter);
				Debug.LogError (errorMessage);
			}

			return template;
		}

		private FieldInfo[] GetClassFields ()
		{
			Type type = types[selectedIndex];

			if (type == null)
				return null;

			FieldInfo[] allFields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			List<FieldInfo> fields = new ();

			for (int i = 0; i < allFields.Length; i++)
				if (allFields[i].IsPublic || allFields[i].IsDefined (typeof (SerializeField)))
					fields.Add (allFields[i]);

			return fields.ToArray ();
		}

		private string GetFieldTexts (StringBuilder sb, FieldInfo[] fieldInfo, string templateName, int formatIndex)
		{
			string searchFilter = $"{templateName}{formatIndex}_DONTDELETEFROMPROJECT";
			
			TextAsset template = GetTemplate (searchFilter);

			sb.Clear ();

			for (int i = 0; i < fieldInfo.Length; i++)
			{
				sb.Append (template.text);
				sb.Replace ("{0}", fieldInfo[i].Name);
			}

			return sb.ToString ();
		}
	}
}
