using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Core.Utils;
using UnityEditor;
using UnityEngine;

namespace Editor.Core
{
    public static class BaseEditor
    {
        public const string KeyName = "Key";
        public const string ValueName = "Value";
        
        /// <summary>
        /// property path for the disabled script object selector on top of every MonoBehaviour/ScriptableObject
        /// </summary>
        public const string ScriptPropertyPath = "m_Script";

        public const string SerializedListName = "serializedList";
        private const int Margin = 10;
        
        public const float LabelWidth = 125f;
        
        public const float SmallButtonWidth = 35f;

        public static GUIStyle Box = new GUIStyle(GUI.skin.box) { margin = new RectOffset(Margin, Margin, Margin, Margin) };

        public static void DrawEnumDict<TKey, TValue>(SerializedProperty dictProperty, Action<SerializedProperty> onDrawValue) where TKey : Enum
        {
            GUIStyle marginStyle = new GUIStyle(GUI.skin.label) {margin = new RectOffset(Margin, Margin, 0, 0)};
            // do whatever you want with this style, e.g.:

            EditorGUILayout.BeginVertical(marginStyle);

            SerializedProperty pairListProperty = dictProperty.FindPropertyRelative(SerializedListName);

            foreach (SerializedProperty property in pairListProperty)
            {
                SerializedProperty keyProperty = property.FindPropertyRelative(KeyName);

                if (keyProperty.enumValueIndex == -1) continue;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField(Utils.GetDisplayName($"{keyProperty.enumNames[keyProperty.enumValueIndex]}"),
                    EditorStyles.boldLabel);

                onDrawValue?.Invoke(property);

                EditorGUILayout.EndVertical();
            }

            List<TKey> allKeys = ((TKey[]) Utils.GetEnumValues(typeof(TKey))).ToList();

            bool saveDict = false;
            
            var dictionary = (GenericDictionary<TKey, TValue>) dictProperty.GetValue();

            if (dictionary.Any(pair => !allKeys.Contains(pair.Key)))
            {
                dictionary.RemoveAll(pair => !allKeys.Contains(pair.Key));

                saveDict = true;
            }

            foreach (TKey key in allKeys)
            {
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, default);

                    saveDict = true;
                }
            }

            if (saveDict) dictProperty.SetValue(dictionary);
            
            EditorGUILayout.EndVertical();
        }

        public static void DrawInspectorExcept(this SerializedObject serializedObject, string[] fieldsToSkip)
        {
            serializedObject.Update();
            
            SerializedProperty prop = serializedObject.GetIterator();
            
            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Any(prop.name.Contains)) continue;
 
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                }
                
                while (prop.NextVisible(false));
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// get property path for c# properties
        /// </summary>
        /// <param name="propertyDisplayName">displayName of c# property</param>
        /// <returns></returns>
        public static string GetPropertyName(this string propertyDisplayName)
        {
            return $"<{propertyDisplayName}>k__BackingField";
        }
        
        public static string GetNameFromPropertyName(this string propertyName)
        {
            string name = string.Empty;

            foreach (var c in propertyName)
            {
                if (c == '>')
                {
                    //end of Name
                    break;
                }
                //inside <> braces
                if (c != '<')
                {
                    name += c;
                }
            }

            return name;
        }
        
        #region SerializedPropertyExtensions

        public static SerializedProperty GetParent(this SerializedProperty property)
        {
            var path = property.propertyPath;
            
            int j = path.LastIndexOf('.');
            
            if (j < 0) return null;

            int i = path.IndexOf('.', 0, j);

            if (i < 0 || i == j)
            {
                i = 0;
            }
            
            return property.serializedObject.FindProperty(path.Substring(i, j));
        }
        
        public static SerializedProperty FindSiblingProperty(this SerializedProperty property, string path)
        {
            var parent = property.GetParent();
            
            if (parent == null) return property.serializedObject.FindProperty(path);
            
            return parent.FindPropertyRelative(path);
        }
        
        /// (Extension) Get the value of the serialized property.
        public static object GetValue(this SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            int i = 0;
            while (NextPathComponent(propertyPath, ref i, out var token)) value = GetPathComponentValue(value, token);
            return value;
        }

        /// (Extension) Set the value of the serialized property.
        public static void SetValue(this SerializedProperty property, object value)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

            SetValueNoRecord(property, value);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

        /// (Extension) Set the value of the serialized property, but do not record the change.
        /// The change will not be persisted unless you call SetDirty and ApplyModifiedProperties.
        public static void SetValueNoRecord(this SerializedProperty property, object value)
        {
            string propertyPath = property.propertyPath;
            object container = property.serializedObject.targetObject;

            int i = 0;
            NextPathComponent(propertyPath, ref i, out var deferredToken);
            while (NextPathComponent(propertyPath, ref i, out var token))
            {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }

            Debug.Assert(!container.GetType().IsValueType,
                $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary.  Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
            SetPathComponentValue(container, deferredToken, value);
        }

        // Union type representing either a property name or array element index.  The element
        // index is valid only if propertyName is null.
        struct PropertyPathComponent
        {
            public string PropertyName;
            public int ElementIndex;
        }

        static Regex arrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

        // Parse the next path component from a SerializedProperty.propertyPath.  For simple field/property access,
        // this is just tokenizing on '.' and returning each field/property name.  Array/list access is via
        // the pseudo-property "Array.data[N]", so this method parses that and returns just the array/list index N.
        //
        // Call this method repeatedly to access all path components.  For example:
        //
        //      string propertyPath = "quests.Array.data[0].goal";
        //      int i = 0;
        //      NextPropertyPathToken(propertyPath, ref i, out var component);
        //          => component = { propertyName = "quests" };
        //      NextPropertyPathToken(propertyPath, ref i, out var component) 
        //          => component = { elementIndex = 0 };
        //      NextPropertyPathToken(propertyPath, ref i, out var component) 
        //          => component = { propertyName = "goal" };
        //      NextPropertyPathToken(propertyPath, ref i, out var component) 
        //          => returns false
        static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component)
        {
            component = new PropertyPathComponent();

            if (index >= propertyPath.Length) return false;

            var arrayElementMatch = arrayElementRegex.Match(propertyPath, index);
            if (arrayElementMatch.Success)
            {
                index += arrayElementMatch.Length + 1; // Skip past next '.'
                component.ElementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
                return true;
            }

            int dot = propertyPath.IndexOf('.', index);
            if (dot == -1)
            {
                component.PropertyName = propertyPath.Substring(index);
                index = propertyPath.Length;
            }
            else
            {
                component.PropertyName = propertyPath.Substring(index, dot - index);
                index = dot + 1; // Skip past next '.'
            }

            return true;
        }

        static object GetPathComponentValue(object container, PropertyPathComponent component)
        {
            if (component.PropertyName == null)
                return ((IList) container)[component.ElementIndex];
            else
                return GetMemberValue(container, component.PropertyName);
        }

        static void SetPathComponentValue(object container, PropertyPathComponent component, object value)
        {
            if (component.PropertyName == null)
                ((IList) container)[component.ElementIndex] = value;
            else
                SetMemberValue(container, component.PropertyName, value);
        }

        static object GetMemberValue(object container, string name)
        {
            if (container == null) return null;
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i] is FieldInfo field)
                    return field.GetValue(container);
                else if (members[i] is PropertyInfo property) return property.GetValue(container);
            }

            return null;
        }

        static void SetMemberValue(object container, string name, object value)
        {
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i] is FieldInfo field)
                {
                    field.SetValue(container, value);
                    return;
                }
                else if (members[i] is PropertyInfo property)
                {
                    property.SetValue(container, value);
                    return;
                }
            }

            Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }

        #endregion

        #region DrawCustomSerializedObjects

        public static void DrawClipOverride(SerializedProperty overrideProperty)
        {
            EditorGUILayout.BeginVertical(Box);

            ClipOverride clipOverride = (ClipOverride) overrideProperty.GetValue();

            foreach (var pair in clipOverride.Overrides.ToList())
            {
                string key = pair.Key;
                
                bool contained = ClipOverride.OverrideNames.Contains(key);

                if (!contained)
                {
                    clipOverride.Remove(key);
                    
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(Utils.GetDisplayName(key), GUILayout.MaxWidth(LabelWidth));
                
                EditorGUI.BeginDisabledGroup(true);
                
                EditorGUILayout.ObjectField(clipOverride.Overrides[key], typeof(AnimationClip), false);

                EditorGUI.EndDisabledGroup();
                
                if (GUILayout.Button(new GUIContent("-", "Remove"), GUILayout.MaxWidth(SmallButtonWidth)))
                {
                    clipOverride.Remove(key);
                }
                
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginVertical(Box);
            
            EditorGUILayout.BeginHorizontal();
                
            clipOverride.selectedClipIndex = EditorGUILayout
                .Popup(clipOverride.selectedClipIndex, ClipOverride.OverrideNames, GUILayout.MaxWidth(LabelWidth));
                
            clipOverride.selectedClip = (AnimationClip) EditorGUILayout
                .ObjectField(clipOverride.selectedClip, typeof(AnimationClip), false);

            string selectedClipName = ClipOverride.OverrideNames[clipOverride.selectedClipIndex];

            if (clipOverride.Overrides.ContainsKey(selectedClipName))
            {
                EditorGUILayout.HelpBox($"Can't Add, Selected Clip Name {selectedClipName} already in ClipOverride", MessageType.Warning);
            }
            
            else if (clipOverride.selectedClip == null)
            {
                EditorGUILayout.HelpBox($"Please Select an Animation Clip", MessageType.Warning);
            }

            else
            {
                if (GUILayout.Button(new GUIContent("+", "Add"), GUILayout.MaxWidth(SmallButtonWidth)))
                {
                    clipOverride.Overrides.Add(selectedClipName, clipOverride.selectedClip);

                    //reset add values
                    clipOverride.selectedClipIndex = 0;
                    
                    clipOverride.selectedClip = null;
                }
            }
                
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
            
            overrideProperty.SetValue(clipOverride);
        }

        #endregion

        public static bool GetEnumValue<T>(this SerializedProperty property, out T enumValue) where T : struct, Enum
        {
            enumValue = default;
            
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                return false;
            }

            string enumName = property.enumNames[property.enumValueIndex];

            return Enum.TryParse(enumName, out enumValue);
        }
    }
}