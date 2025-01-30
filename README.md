# EditorHelper

Some helpful utilities for the Unity Editor.

- ***SerializedArray***: a wrapper for array properties that adds a couple of quality of life features.

  - ***Draw***: displays the array in the Inspector/Window.

- ***InspectorBuilder***: an Editor window that can be used to generate a basic inspector for the desired MonoBehaviour or ScriptableObject. The inspector created with this tool uses the SerializedArray class to display arrays.

- ***Assets***: a class that performs operations related to the assets folder.

  - ***Find***: returns the first Unity object that matches the search filter.
  - ***Find&lt;T&gt;***: returns the first Unity object of the desired type that matches the search filter.
  - ***FindAll (2 overloads)***: returns all the Unity objects that match the search filter.
  - ***FindAll&lt;T&gt; (2 overloads)***: returns all the Unity objects of the desired type that match the search filter.
  - ***GetCurrentFolder***: returns the folder that is currently selected (or the assets folder if none is selected).
  - ***CreateScriptableObject (2 overloads)***: instantiates an ScriptableObject of the desired type and saves it as an asset.

- ***SearchOption***: an enum that can be provided to the Find, Find&lt;T&gt;, FindAll and FindAll&lt;T&gt; methods in the Assets class to filter the search. It has the following values:

  - All
  - CurrentFolder
  - CurrentFolderAndSubfolders
  - CurrentSelection

- ***Extensions***: a list of extension methods for serialized properties.

  - ***GetElementType***: returns the type of the field contained in a SerializedProperty. If it's an array, returns the type of its elements.
  - ***GetArray&lt;T&gt; (2 overloads)***: returns the array contained in a SerializedProperty.
  - ***CopyTo***: copies the array in the SerializedProperty to the desired array starting at the given index.
  - ***GetPropertyAt***: returns the SerializedProperty at the given index.
  - ***GetFirstInstanceOf***: returns the first matching object in an array SerializedProperty.
  - ***IndexOf***: returns the index of a given object in an array SerializedProperty.
  - ***Contains***: returns true if the object provided is in the array SerializedProperty.
  - ***Add***: adds the desired object at the end of an array SerializedProperty.
  - ***Insert***: inserts the desired object at the given index of an array SerializedProperty.
  - ***Remove***: removes the object provided from an array SerializedProperty.
  - ***RemoveAt***: removes the element at the given index of an array SerializedProperty.
  - ***RemoveAll***: removes from an array SerializedProperty all the elements that match the object provided.
  - ***Move***: moves an element of an array SerializedProperty from one index to another.
  - ***AnimationCurve***: returns the AnimationCurve value at the given index of an array SerializedProperty.
  - ***Bool***: returns the bool value at the given index of an array SerializedProperty.
  - ***BoundsInt***: returns the BoundsInt value at the given index of an array SerializedProperty.
  - ***Bounds***: returns the Bounds value at the given index of an array SerializedProperty.
  - ***Color***: returns the Color value at the given index of an array SerializedProperty.
  - ***Double***: returns the double value at the given index of an array SerializedProperty.
  - ***Float***: returns the float value at the given index of an array SerializedProperty.
  - ***Gradient***: returns the Gradient value at the given index of an array SerializedProperty.
  - ***Hash128***: returns the hash128 value at the given index of an array SerializedProperty.
  - ***Int***: returns the int value at the given index of an array SerializedProperty.
  - ***Long***: returns the long value at the given index of an array SerializedProperty.
  - ***Object***: returns the Unity Object value at the given index of an array SerializedProperty.
  - ***Quaternion***: returns the Quaternion value at the given index of an array SerializedProperty.
  - ***RectInt***: returns the RectInt value at the given index of an array SerializedProperty.
  - ***Rect***: returns the Rect value at the given index of an array SerializedProperty.
  - ***String***: returns the string value at the given index of an array SerializedProperty.
  - ***UInt***: returns the UInt value at the given index of an array SerializedProperty.
  - ***ULong***: returns the ULong value at the given index of an array SerializedProperty.
  - ***Vector2Int***: returns the Vector2Int value at the given index of an array SerializedProperty.
  - ***Vector2***: returns the Vector2 value at the given index of an array SerializedProperty.
  - ***Vector3Int***: returns the Vector3Int value at the given index of an array SerializedProperty.
  - ***Vector3***: returns the Vector3 value at the given index of an array SerializedProperty.
  - ***Vector4***: returns the Vector4 value at the given index of an array SerializedProperty.
  - ***BoxedValue***: returns the boxed value at the given index of an array SerializedProperty.
  - ***Set (23 overloads)***: assings the desired value to the element at the given index of an array SerializedProperty.
  - ***SetBoxedValue***: assings the desired boxed value to the element at the given index of an array SerializedProperty.
