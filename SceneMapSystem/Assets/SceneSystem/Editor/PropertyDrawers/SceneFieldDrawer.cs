#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    [CustomPropertyDrawer( typeof( Scene ), true )]
    public class SceneFieldDrawer : PropertyDrawer
    {
        // public override VisualElement CreatePropertyGUI( SerializedProperty property )
        // {
        //     var pathProp = property.FindPropertyRelative( "_Path" );
        //     var nameProp = property.FindPropertyRelative( "_Name" );
        //     var indexProp = property.FindPropertyRelative( "_BuildIndex" );
        //
        //     var root = new VisualElement();
        //     // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( GUIUtility.SceneFieldDrawerUxmlPath );
        //     // visualTree.CloneTree( root );
        //
        //     var field = new ObjectField();
        //     field.objectType = typeof( SceneAsset );
        //     field.value = AssetDatabase.LoadAssetAtPath<SceneAsset>( pathProp.stringValue );
        //     field.label = property.displayName;
        //     field.BindProperty( property );
        //     field.RegisterValueChangedCallback( OnValueChanged );
        //
        //     void OnValueChanged( ChangeEvent<Object> evt )
        //     {
        //         if ( evt.newValue == evt.previousValue ) return;
        //
        //         var newScene = evt.newValue;
        //         nameProp.stringValue = newScene == null ? null : newScene.name;
        //
        //         var newPath = newScene == null ? null : AssetDatabase.GetAssetPath( newScene );
        //         pathProp.stringValue = newPath;
        //
        //         var intValue = newPath == null ? -1 : SceneUtility.GetBuildIndexByScenePath( newPath );
        //         indexProp.intValue = intValue;
        //
        //         GUIUtility.Events.TriggerSaveEvent();
        //     }
        //
        //     root.Add( field );
        //     return root;
        // }

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            // EditorGUI.BeginProperty( position, label, property );
            
            var pathProp = property.FindPropertyRelative( "_Path" );
            var nameProp = property.FindPropertyRelative( "_Name" );
            var indexProp = property.FindPropertyRelative( "_BuildIndex" );
            
            var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>( pathProp.stringValue );
            
            using ( var scope = new EditorGUI.ChangeCheckScope() )
            {
                var newScene = (SceneAsset) EditorGUI.ObjectField( position, label, oldScene, typeof( SceneAsset ), true );
                if ( scope.changed )
                {
                    nameProp.stringValue = newScene == null ? null : newScene.name;
            
                    var newPath = newScene == null ? null : AssetDatabase.GetAssetPath( newScene );
                    pathProp.stringValue = newPath;
            
                    var intValue = newPath == null ? -1 : SceneUtility.GetBuildIndexByScenePath( newPath );
                    indexProp.intValue = intValue;
                }
            }
            
            // EditorGUI.EndProperty();
        }
    }
}
#endif