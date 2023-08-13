#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace TNS.SceneSystem.Editor
{
    public static class SerializedPropertyHelpers
    {
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