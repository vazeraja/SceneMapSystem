#if UNITY_EDITOR
using System.Collections.Generic;
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

        private VisualElement m_SceneSettingsInspectorsContainer;
        private readonly List<SceneSettingsInspectorView> m_SceneSettingsInspectors;
        private readonly List<SceneTransitionInspectorView> m_SceneTransitionInspectors;

        private IMGUIContainer m_IMGUIContainer;

        public InspectorPanelView( SceneMapEditorWindow window, VisualElement root )
        {
            m_Root = root;
            m_Window = window;

            m_ContentContainer = root.Q<VisualElement>( "inspector-content__content-container" );
            m_SceneTransitionInspectors = new List<SceneTransitionInspectorView>();
            m_SceneSettingsInspectors = new List<SceneSettingsInspectorView>();

            m_IMGUIContainer = new IMGUIContainer();

            GUIUtility.Events.AssetInitialized += OnAssetInitialized;
            GUIUtility.Events.SceneSelected += OnSceneSelected;
            GUIUtility.Events.SceneReferenceRemoved += OnSceneRemoved;
            GUIUtility.Events.CollectionSelected += OnCollectionSelected;
            GUIUtility.Events.CollectionRemoved += OnCollectionRemoved;
            GUIUtility.Events.TransitionSelected += OnTransitionSelected;
            GUIUtility.Events.TransitionRemoved += OnTransitionRemoved;
        }

        private void OnAssetInitialized() { }

        private void OnSceneSelected( SceneReference scene )
        {
            HideInspectors();
            DisplaySceneInspector( scene );
        }

        private void OnCollectionSelected( int index )
        {
            HideInspectors();

            var sceneCollectionListProp = m_Window.SerializedSceneMap.FindProperty( nameof( SceneMapAsset._SceneCollections ) );
            var sceneCollectionProp = sceneCollectionListProp.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );
            var sceneReferenceListProp = sceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._Scenes ) );
            var loadOrder = sceneCollectionProp.FindPropertyRelative( nameof( SceneCollection.m_LoadOrder ) );

            var loadOrderList = new ReorderableList( m_Window.SerializedSceneMap, loadOrder, true, true, false, false );
            loadOrderList.drawElementCallback = DrawFunc;

            void DrawFunc( Rect rect, int i, bool isActive, bool isFocused )
            {
                var sceneReferenceProp = sceneReferenceListProp.GetArrayElementAtIndex( i );
                EditorGUI.DropdownButton( rect, new GUIContent( "yeehaw" ), FocusType.Passive );
            }

            // m_IMGUIContainer.onGUIHandler = () =>
            // {
            //     m_Window.SerializedSceneMap.Update();
            //
            //     using ( var scope = new EditorGUI.ChangeCheckScope() )
            //     {
            //         loadOrderList.DoLayoutList();
            //         if ( scope.changed )
            //         {
            //             Debug.Log( "scope changed" );
            //         }
            //     }
            //
            //     if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            //     {
            //         m_Window.SaveAndRebuild();
            //     }
            // };

            m_ContentContainer.Add( m_IMGUIContainer );
        }

        private void OnTransitionSelected( SceneTransition transition )
        {
            HideInspectors();
            DisplayTransitionInspector( transition );
        }

        private void OnSceneRemoved( SceneReference scene )
        {
            var inspector = FindInspectorForScene( scene );
            RemoveSceneInspector( inspector );
        }

        private void OnCollectionRemoved( SceneCollection collection ) { }

        private void OnTransitionRemoved( SceneTransition transition )
        {
            var inspector = FindInspectorForTransition( transition );
            RemoveTransitionInspector( inspector );
        }

        private void CreateSceneInspector( SceneReference scene, int cIndex, int sIndex )
        {
            var inspector = new SceneSettingsInspectorView();
            inspector.name = $"{scene.name}__inspector-view";
            inspector.Initialize( m_Window, scene );

            var sceneCollectionListProp = m_Window.SerializedSceneMap.FindProperty( nameof( SceneMapAsset._SceneCollections ) );
            var sceneCollectionProp = sceneCollectionListProp.GetArrayElementAtIndex( cIndex );
            var sceneReferenceListProp = sceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._Scenes ) );
            var sceneReferenceProp = sceneReferenceListProp.GetArrayElementAtIndex( sIndex );

            inspector.Bind( sceneReferenceProp );

            m_SceneSettingsInspectors.Add( inspector );
            m_ContentContainer.Add( inspector );
        }

        private void CreateTransitionInspector( SceneTransition transition, int cIndex, int tIndex )
        {
            var inspector = new SceneTransitionInspectorView();
            inspector.name = $"{transition.m_Label}__inspector-view";
            inspector.Initialize( m_Window, transition );

            var sceneCollectionListProp = m_Window.SerializedSceneMap.FindProperty( nameof( SceneMapAsset._SceneCollections ) );
            var sceneCollectionProp = sceneCollectionListProp.GetArrayElementAtIndex( cIndex );
            var sceneTransitionListProp = sceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._SceneTransitions ) );
            var sceneTransitionProp = sceneTransitionListProp.GetArrayElementAtIndex( tIndex );

            inspector.Bind( sceneTransitionProp );

            m_SceneTransitionInspectors.Add( inspector );
            m_ContentContainer.Add( inspector );
        }

        private void RemoveSceneInspector( SceneSettingsInspectorView inspector )
        {
            m_SceneSettingsInspectors.Remove( inspector );
            m_ContentContainer.Remove( inspector );
        }

        private void RemoveTransitionInspector( SceneTransitionInspectorView inspector )
        {
            m_SceneTransitionInspectors.Remove( inspector );
            m_ContentContainer.Remove( inspector );
        }

        private void DisplaySceneInspector( SceneReference scene )
        {
            var inspector = FindInspectorForScene( scene );
            if ( inspector == null )
            {
                var cIndex = m_Window.SelectedCollectionIndex;
                var sIndex = m_Window.SelectedSceneIndex;
                CreateSceneInspector( scene, cIndex, sIndex );
                return;
            }

            GUIUtility.SetVisibility( inspector, true );
        }

        private void DisplayTransitionInspector( SceneTransition transition )
        {
            var inspector = FindInspectorForTransition( transition );
            if ( inspector == null )
            {
                var cIndex = m_Window.SelectedCollectionIndex;
                var tIndex = m_Window.SelectedCollection.sceneTransitions.IndexOfReference( transition );
                CreateTransitionInspector( transition, cIndex, tIndex );
                return;
            }

            GUIUtility.SetVisibility( inspector, true );
        }

        private SceneSettingsInspectorView FindInspectorForScene( SceneReference scene )
        {
            return m_SceneSettingsInspectors.FirstOrDefault( inspector => inspector.m_SceneReference.id == scene.id );
        }

        private SceneTransitionInspectorView FindInspectorForTransition( SceneTransition transition )
        {
            return m_SceneTransitionInspectors.FirstOrDefault( inspector => inspector.m_SceneTransition.m_ID == transition.m_ID );
        }

        public void HideInspectors()
        {
            HideSceneInspectors();
            HideTransitionInspectors();
            m_IMGUIContainer.RemoveFromHierarchy();
        }

        private void HideSceneInspectors()
        {
            foreach ( var sceneSettingsInspector in m_SceneSettingsInspectors )
            {
                GUIUtility.SetVisibility( sceneSettingsInspector, false );
            }
        }

        private void HideTransitionInspectors()
        {
            foreach ( var transitionInspector in m_SceneTransitionInspectors )
            {
                GUIUtility.SetVisibility( transitionInspector, false );
            }
        }
    }
}
#endif