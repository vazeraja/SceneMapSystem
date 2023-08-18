#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class InspectorPanelView
    {
        private readonly VisualElement m_Root;
        private readonly VisualElement m_ContentContainer;
        private readonly SceneMapEditorWindow m_Window;

        private SerializedProperty SceneCollectionListProp => m_Window.SerializedSceneMap.FindProperty( nameof( SceneMapAsset._SceneCollections ) );
        private SerializedProperty SceneCollectionProp => SceneCollectionListProp.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );
        private SerializedProperty SceneReferenceListProp => SceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._Scenes ) );
        private SerializedProperty SceneTransitionListProp => SceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._SceneTransitions ) );

        private readonly RibbonFoldout m_SceneSettingsRibbonFoldout;
        private readonly RibbonFoldout m_LoadingSettingsRibbonFoldout;

        private readonly SceneSettingsInspectorView m_SceneSettingsInspectorView;
        private readonly SceneTransitionInspectorView m_SceneTransitionInspectorView;

        public InspectorPanelView( SceneMapEditorWindow window, VisualElement root )
        {
            m_Root = root;
            m_Window = window;

            m_ContentContainer = root.Q<VisualElement>( "inspector-content__content-container" );
            m_SceneTransitionInspectorView = root.Q<SceneTransitionInspectorView>();
            m_SceneSettingsInspectorView = root.Q<SceneSettingsInspectorView>();

            m_SceneTransitionInspectorView.Initialize( window );
            m_SceneSettingsInspectorView.Initialize( window );

            GUIUtility.Events.SceneSelected += OnSceneSelected;
            GUIUtility.Events.CollectionSelected += OnCollectionSelected;
            GUIUtility.Events.TransitionSelected += OnTransitionSelected;
        }

        private void OnTransitionSelected( int index )
        {
            m_SceneSettingsInspectorView.style.display = DisplayStyle.None;
            m_SceneTransitionInspectorView.style.display = DisplayStyle.Flex;
            m_SceneTransitionInspectorView.Display( SceneTransitionListProp.GetArrayElementAtIndex( index ) );
        }

        private void OnSceneSelected( int index )
        {
            m_SceneTransitionInspectorView.style.display = DisplayStyle.None;
            m_SceneSettingsInspectorView.style.display = DisplayStyle.Flex;
            m_SceneSettingsInspectorView.Display( SceneReferenceListProp.GetArrayElementAtIndex( index )
                .FindPropertyRelative( nameof( SceneReference._SceneSettings ) ) );
        }

        private void OnCollectionSelected( int index )
        {
            ClearContent();
        }

        public void ClearContent()
        {
            m_SceneTransitionInspectorView.style.display = DisplayStyle.None;
            m_SceneSettingsInspectorView.style.display = DisplayStyle.None;
        }
    }
}
#endif