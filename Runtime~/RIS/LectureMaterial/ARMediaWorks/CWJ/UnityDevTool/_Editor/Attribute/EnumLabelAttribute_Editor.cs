﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace CWJ.EditorOnly
{
    [CustomPropertyDrawer(typeof(EnumLabelAttribute))]
    public class EnumLabelAttribute_Editor : PropertyDrawer
    {
        private Dictionary<string, string> customEnumNames = new Dictionary<string, string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetUpCustomEnumNames(property, property.enumNames);

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginChangeCheck();
                string[] displayedOptions = property.enumNames
                        .Where(enumName => customEnumNames.ContainsKey(enumName))
                        .Select<string, string>(enumName => customEnumNames[enumName])
                        .ToArray();
                int selectedIndex = EditorGUI.Popup(position, enumLabelAttribute.label, property.enumValueIndex, displayedOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    property.enumValueIndex = selectedIndex;
                }
            }
        }

        private EnumLabelAttribute enumLabelAttribute => attribute as EnumLabelAttribute;

        public void SetUpCustomEnumNames(SerializedProperty property, string[] enumNames)
        {
            Type type = property.serializedObject.targetObject.GetType();
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumLabelAttribute), false);
                foreach (EnumLabelAttribute customAttribute in customAttributes)
                {
                    Type enumType = fieldInfo.FieldType;

                    foreach (string enumName in enumNames)
                    {
                        FieldInfo field = enumType.GetField(enumName);
                        if (field == null) continue;
                        EnumLabelAttribute[] attrs = field.GetCustomAttributes(customAttribute.GetType(), false) as EnumLabelAttribute[];

                        if (!customEnumNames.ContainsKey(enumName))
                        {
                            foreach (EnumLabelAttribute labelAttribute in attrs)
                            {
                                customEnumNames.Add(enumName, labelAttribute.label);
                            }
                        }
                    }
                }
            }
        }
    }
}