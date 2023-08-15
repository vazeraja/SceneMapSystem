#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TNS.SceneSystem.Editor
{
    public static class SerializedPropertyHelpers
    {
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.Next(false);
            }
 
            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;
 
                    yield return currentProperty.Copy();
                }
                while (currentProperty.Next(false));
            }
        }
        
        // Show a PropertyField with a greyed-out default text if the field is empty and not being edited.
        // This is meant to communicate the fact that filling these properties is optional and that Unity will
        // use reasonable defaults if left empty.
        public static void PropertyFieldWithDefaultText( this SerializedProperty prop, GUIContent label, string defaultText )
        {
            GUI.SetNextControlName( label.text );
            var rt = GUILayoutUtility.GetRect( label, GUI.skin.textField );

            EditorGUI.PropertyField( rt, prop, label );
            if ( string.IsNullOrEmpty( prop.stringValue ) && GUI.GetNameOfFocusedControl() != label.text &&
                 Event.current.type == EventType.Repaint ) {
                using ( new EditorGUI.DisabledScope( true ) ) {
                    rt.xMin += EditorGUIUtility.labelWidth;
                    GUI.skin.textField.Draw( rt, new GUIContent( defaultText ), false, false, false, false );
                }
            }
        }
    }
}

#endif