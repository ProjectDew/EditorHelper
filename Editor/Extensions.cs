namespace EditorHelper
{
	using System;
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;

	using Object = UnityEngine.Object;

	public static class Extensions
	{
		public static Type GetElementType (this SerializedProperty property)
		{
			Type classType = property.serializedObject.targetObject.GetType ();
			FieldInfo fieldInfo = GetFieldInfo (classType, property.propertyPath);

			if (fieldInfo == null)
			{
				Debug.LogError (string.Format ("The field wasn't found. Property path: {0}", property.propertyPath));
				return null;
			}

			Type fieldType = fieldInfo.FieldType;

			bool isArray = property.isArray && property.propertyType != SerializedPropertyType.String;

			return isArray ? fieldType.GetElementType () : fieldType;
		}

		public static T[] GetArray<T> (this SerializedProperty property)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return new T[0];
			}
			
			T[] targetArray = new T[property.arraySize];

			for (int i = 0; i < targetArray.Length; i++)
				targetArray[i] = (T)property.GetArrayElementAtIndex (i).boxedValue;

			return targetArray;
		}

		public static T[] GetArray<T> (this SerializedProperty property, int length)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !ValueIsZeroOrPositive (property, length, "Length", out errorMessage))
			{
				Debug.LogError (errorMessage);
				return new T[0];
			}

			T[] targetArray = new T[length];

			for (int i = 0; i < targetArray.Length; i++)
				targetArray[i] = (T)property.GetArrayElementAtIndex (i).boxedValue;

			return targetArray;
		}

		public static void CopyTo (this SerializedProperty property, object[] destinationArray, int startIndex)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !ValueIsZeroOrPositive (property, startIndex, "Index", out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			for (int i = 0; i < property.arraySize; i++)
			{
				int destinationIndex = startIndex + i;
				destinationArray[destinationIndex] = property.GetArrayElementAtIndex (i).boxedValue;
			}
		}

		public static SerializedProperty GetPropertyAt (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return null;
			}

			return property.GetArrayElementAtIndex (index);
		}

		public static object GetFirstInstanceOf (this SerializedProperty property, object element)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return null;
			}
			
			for (int i = 0; i < property.arraySize; i++)
				if (element.Equals (property.GetArrayElementAtIndex (i).boxedValue))
					return property.GetArrayElementAtIndex (i).boxedValue;

			return null;
		}

		public static int IndexOf (this SerializedProperty property, object element)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return -1;
			}
			
			for (int i = 0; i < property.arraySize; i++)
			{
				if (element.Equals (property.GetArrayElementAtIndex (i).boxedValue))
					return i;
			}

			return -1;
		}

		public static bool Contains (this SerializedProperty property, object element)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return false;
			}
			
			for (int i = 0; i < property.arraySize; i++)
				if (element.Equals (property.GetArrayElementAtIndex (i).boxedValue))
					return true;

			return false;
		}

		public static void Add (this SerializedProperty property, object element)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}
			
			int index = property.arraySize;

			property.arraySize++;

			property.GetArrayElementAtIndex (index).boxedValue = GetValidObject (property, element);
		}

		public static void Insert (this SerializedProperty property, int index, object newElement)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !ValueIsZeroOrPositive (property, index, "Index", out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			object nextElement;
			
			property.arraySize++;

			newElement = GetValidObject (property, newElement);
			
			for (int i = index; i < property.arraySize; i++)
			{
				nextElement = property.GetArrayElementAtIndex (i).boxedValue;
				property.GetArrayElementAtIndex (i).boxedValue = newElement;
				newElement = nextElement;
			}
		}

		public static bool Remove (this SerializedProperty property, object element)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return false;
			}
			
			for (int i = 0; i < property.arraySize; i++)
			{
				if (!element.Equals (property.GetArrayElementAtIndex (i).boxedValue))
					continue;
				
				property.DeleteArrayElementAtIndex (i);

				return true;
			}

			return false;
		}

		public static void RemoveAt (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.DeleteArrayElementAtIndex (index);
		}

		public static void RemoveAll (this SerializedProperty property, object element)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}
			
			for (int i = 0; i < property.arraySize; i++)
			{
				if (!element.Equals (property.GetArrayElementAtIndex (i).boxedValue))
					continue;
				
				property.DeleteArrayElementAtIndex (i);
			}
		}

		public static void Move (this SerializedProperty property, int indexFrom, int indexTo)
		{
			if (!PropertyIsAnArray (property, out string errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}
			
			if (!IndexIsInsideBounds (property, indexFrom, property.arraySize, out errorMessage) || !IndexIsInsideBounds (property, indexTo, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			if (indexFrom == indexTo)
				return;
				
			property.MoveArrayElement (indexFrom, indexTo);
		}

		public static AnimationCurve AnimationCurve (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).animationCurveValue;
		}

		public static bool Bool (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).boolValue;
		}

		public static BoundsInt BoundsInt (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).boundsIntValue;
		}

		public static Bounds Bounds (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).boundsValue;
		}

		public static Color Color (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).colorValue;
		}

		public static double Double (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).doubleValue;
		}

		public static float Float (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).floatValue;
		}

		public static Gradient Gradient (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).gradientValue;
		}

		public static Hash128 Hash128 (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).hash128Value;
		}

		public static int Int (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).intValue;
		}

		public static long Long (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).longValue;
		}

		public static Object ObjectReference (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).objectReferenceValue;
		}

		public static Quaternion Quaternion (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).quaternionValue;
		}

		public static RectInt RectInt (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).rectIntValue;
		}

		public static Rect Rect (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).rectValue;
		}

		public static string String (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return string.Empty;
			}

			return property.GetArrayElementAtIndex (index).stringValue;
		}

		public static uint UInt (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).uintValue;
		}

		public static ulong ULong (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).ulongValue;
		}

		public static Vector2Int Vector2Int (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).vector2IntValue;
		}

		public static Vector2 Vector2 (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).vector2Value;
		}

		public static Vector3Int Vector3Int (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).vector3IntValue;
		}

		public static Vector3 Vector3 (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).vector3Value;
		}

		public static Vector4 Vector4 (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).vector4Value;
		}

		public static object BoxedValue (this SerializedProperty property, int index)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return default;
			}

			return property.GetArrayElementAtIndex (index).boxedValue;
		}

		public static void Set (this SerializedProperty property, int index, AnimationCurve value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).animationCurveValue = value;
		}

		public static void Set (this SerializedProperty property, int index, bool value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).boolValue = value;
		}

		public static void Set (this SerializedProperty property, int index, BoundsInt value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).boundsIntValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Bounds value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).boundsValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Color value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).colorValue = value;
		}

		public static void Set (this SerializedProperty property, int index, double value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).doubleValue = value;
		}

		public static void Set (this SerializedProperty property, int index, float value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).floatValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Gradient value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).gradientValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Hash128 value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).hash128Value = value;
		}

		public static void Set (this SerializedProperty property, int index, int value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).intValue = value;
		}

		public static void Set (this SerializedProperty property, int index, long value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).longValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Object value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).objectReferenceValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Quaternion value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).quaternionValue = value;
		}

		public static void Set (this SerializedProperty property, int index, RectInt value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).rectIntValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Rect value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).rectValue = value;
		}

		public static void Set (this SerializedProperty property, int index, string value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).stringValue = value;
		}

		public static void Set (this SerializedProperty property, int index, uint value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).uintValue = value;
		}

		public static void Set (this SerializedProperty property, int index, ulong value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).ulongValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Vector2Int value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).vector2IntValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Vector2 value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).vector2Value = value;
		}

		public static void Set (this SerializedProperty property, int index, Vector3Int value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).vector3IntValue = value;
		}

		public static void Set (this SerializedProperty property, int index, Vector3 value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).vector3Value = value;
		}

		public static void Set (this SerializedProperty property, int index, Vector4 value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).vector4Value = value;
		}

		public static void SetBoxedValue (this SerializedProperty property, int index, object value)
		{
			if (!PropertyIsAnArray (property, out string errorMessage) || !IndexIsInsideBounds (property, index, property.arraySize, out errorMessage))
			{
				Debug.LogError (errorMessage);
				return;
			}

			property.GetArrayElementAtIndex (index).boxedValue = GetValidObject (property, value);
		}

		private static bool PropertyIsAnArray (SerializedProperty property, out string errorMessage)
		{
			errorMessage = string.Format ("The property is not an array. Property: {0}", property.propertyPath);
			return property.isArray;
		}

		private static bool ValueIsZeroOrPositive (SerializedProperty property, int value, string valueName, out string errorMessage)
		{
			errorMessage = string.Format ("{0} must be a non-negative number. {0}: {1}, property path: {2}",
				valueName, value.ToString (), property.propertyPath);

			return value >= 0;
		}

		private static bool IndexIsInsideBounds (SerializedProperty property, int index, int arrayLength, out string errorMessage)
		{
			errorMessage = string.Format ("Index is outside the bounds of the array. Index: {0}, array length: {1}, property path: {2}",
				index.ToString (), arrayLength.ToString (), property.propertyPath);

			return index >= 0 && index < arrayLength;
		}

		private static FieldInfo GetFieldInfo (Type classType, string fieldPath)
		{
			FieldInfo fieldInfo = null;

			while (classType != null && fieldInfo == null)
			{
				fieldInfo = classType.GetField (fieldPath, BindingFlags.NonPublic | BindingFlags.Instance);
				classType = classType.BaseType;
			}

			return fieldInfo;
		}

		private static object GetValidObject (SerializedProperty property, object value)
		{
			if (value == null)
			{
				Type fieldType = property.GetElementType ();

				if (fieldType == typeof (string))
					return string.Empty;

				if (!typeof (Object).IsAssignableFrom (fieldType))
					value = Activator.CreateInstance (fieldType);
			}

			return value;
		}
	}
}
