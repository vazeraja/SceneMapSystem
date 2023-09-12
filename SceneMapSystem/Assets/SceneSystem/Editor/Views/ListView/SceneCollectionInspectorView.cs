#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneCollectionInspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SceneCollectionInspectorView, UxmlTraits> { }

        private SceneMapEditorWindow m_Window;
        public SceneCollection m_SceneCollection;

        private VisualElement m_Root;
        private VisualElement m_Content;
        private VisualElement m_HelpBox;
        private Label m_HelpBoxText;
        private IMGUIContainer m_IMGUIContainer;

        private ReorderableList m_LoadOrderReorderableList;
        private SerializedProperty m_LoadOrderIDListProp;

        public SceneCollectionInspectorView()
        {
            var inspectorTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( GUIUtility.SceneCollectionInspectorUxmlPath );
            m_Root = inspectorTree.Clone();

            m_Content = m_Root.Q<VisualElement>( "content" );
            m_HelpBox = m_Root.Q<VisualElement>( "help-box" );
            m_HelpBoxText = m_Root.Q<Label>( "text" );
            m_IMGUIContainer = m_Root.Q<IMGUIContainer>();

            SetHelpText( "Edit the settings displayed below to control settings related to this scene collection" );

            hierarchy.Add( m_Root );
        }

        public void Initialize( SceneMapEditorWindow window, SceneCollection collection )
        {
            m_Window = window;
            m_SceneCollection = collection;
        }

        public void Bind( SerializedProperty collectionProp )
        {
            m_LoadOrderIDListProp = collectionProp.FindPropertyRelative( nameof( SceneCollection._LoadOrder ) );

            m_LoadOrderReorderableList = new ReorderableList( m_Window.SerializedSceneMap, m_LoadOrderIDListProp, true, true, false, false );
            m_LoadOrderReorderableList.drawElementCallback = ( rect, index, active, focused ) =>
            {
                var ele = m_LoadOrderIDListProp.GetArrayElementAtIndex( index );
                var scene = m_Window.SceneMap.FindScene( ele.stringValue );

                EditorGUI.DropdownButton( rect, new GUIContent( scene.name ), FocusType.Passive );
            };

            UpdateIMGUI();
        }

        public void SetHelpText( string text )
        {
            m_HelpBoxText.text = text;
        }

        private void UpdateIMGUI()
        {
            m_IMGUIContainer.onGUIHandler = () =>
            {
                m_Window.SerializedSceneMap.Update();

                using ( var scope = new EditorGUI.ChangeCheckScope() )
                {
                    m_LoadOrderReorderableList.DoLayoutList();
                    if ( scope.changed )
                        Debug.Log( "scope changed" );
                }
                
                if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
                {
                    m_Window.SaveAndRebuild();
                }
            };
        }
    }
}
#endif