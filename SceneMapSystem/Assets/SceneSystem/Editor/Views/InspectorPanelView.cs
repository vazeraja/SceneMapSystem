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
        private readonly List<SceneCollectionInspectorView> m_SceneCollectionInspectors;
        private readonly List<SceneTransitionInspectorView> m_SceneTransitionInspectors;

        public InspectorPanelView( SceneMapEditorWindow window, VisualElement root )
        {
            m_Root = root;
            m_Window = window;

            m_ContentContainer = root.Q<VisualElement>( "inspector-content__content-container" );
            m_SceneSettingsInspectors = new List<SceneSettingsInspectorView>();
            m_SceneCollectionInspectors = new List<SceneCollectionInspectorView>();
            m_SceneTransitionInspectors = new List<SceneTransitionInspectorView>();

            GUIUtility.Events.AssetInitialized += OnAssetInitialized;
            GUIUtility.Events.SceneSelected += OnSceneSelected;
            GUIUtility.Events.SceneReferenceRemoved += OnSceneRemoved;
            GUIUtility.Events.CollectionSelected += OnCollectionSelected;
            GUIUtility.Events.CollectionRemoved += OnCollectionRemoved;
            GUIUtility.Events.TransitionSelected += OnTransitionSelected;
            GUIUtility.Events.TransitionRemoved += OnTransitionRemoved;
        }

        ~InspectorPanelView()
        {
            GUIUtility.Events.AssetInitialized -= OnAssetInitialized;
            GUIUtility.Events.SceneSelected -= OnSceneSelected;
            GUIUtility.Events.SceneReferenceRemoved -= OnSceneRemoved;
            GUIUtility.Events.CollectionSelected -= OnCollectionSelected;
            GUIUtility.Events.CollectionRemoved -= OnCollectionRemoved;
            GUIUtility.Events.TransitionSelected -= OnTransitionSelected;
            GUIUtility.Events.TransitionRemoved -= OnTransitionRemoved;
        }

        private void OnAssetInitialized()
        {
            m_ContentContainer.Clear();
            CreateInspectors();
        }

        private void OnSceneSelected( int index )
        {
            HideInspectors();
            DisplaySceneInspector( m_Window.SelectedCollection._Scenes[index] );
        }

        private void OnCollectionSelected( int index )
        {
            HideInspectors();

            DisplayCollectionInspector( m_Window.SelectedCollection );
        }

        private void OnTransitionSelected( SceneTransition transition )
        {
            HideInspectors();
            DisplayTransitionInspector( transition );
        }

        private void OnSceneRemoved( SceneReference scene ) => RemoveSceneInspector( FindInspectorForScene( scene ) );
        private void OnCollectionRemoved( SceneCollection collection ) => RemoveCollectionInspector( FindInspectorForCollection( collection ) );
        private void OnTransitionRemoved( SceneTransition transition ) => RemoveTransitionInspector( FindInspectorForTransition( transition ) );

        public void CreateInspectors()
        {
            Debug.Log( "create inspectors " );
            for ( var cIndex = 0; cIndex < m_Window.SceneMap.SceneCollections.Count; cIndex++ )
            {
                var collection = m_Window.SceneMap.SceneCollections[cIndex];
                CreateCollectionInspector( collection, cIndex );

                for ( var sIndex = 0; sIndex < collection.scenes.Count; sIndex++ )
                {
                    var scene = collection.scenes[sIndex];
                    CreateSceneInspector( scene, cIndex, sIndex );
                }

                for ( var tIndex = 0; tIndex < collection.sceneTransitions.Count; tIndex++ )
                {
                    var transition = collection.sceneTransitions[tIndex];
                    CreateTransitionInspector( transition, cIndex, tIndex );
                }
            }

            HideInspectors();
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

        private void CreateCollectionInspector( SceneCollection collection, int cIndex )
        {
            var inspector = new SceneCollectionInspectorView();
            inspector.name = $"{collection.name}__inspector-view";
            inspector.Initialize( m_Window, collection );

            var sceneCollectionListProp = m_Window.SerializedSceneMap.FindProperty( nameof( SceneMapAsset._SceneCollections ) );
            var sceneCollectionProp = sceneCollectionListProp.GetArrayElementAtIndex( cIndex );
            
            inspector.Bind( sceneCollectionProp );

            m_SceneCollectionInspectors.Add( inspector );
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

        private void RemoveCollectionInspector( SceneCollectionInspectorView inspector )
        {
            m_SceneCollectionInspectors.Remove( inspector );
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

        private void DisplayCollectionInspector( SceneCollection collection )
        {
            var inspector = FindInspectorForCollection( collection );
            if ( inspector == null )
            {
                var cIndex = m_Window.SelectedCollectionIndex;
                var sIndex = m_Window.SelectedSceneIndex;
                CreateCollectionInspector( collection, cIndex );
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

        private SceneCollectionInspectorView FindInspectorForCollection( SceneCollection collection )
        {
            return m_SceneCollectionInspectors.FirstOrDefault( inspector => inspector.m_SceneCollection.id == collection.id );
        }

        private SceneTransitionInspectorView FindInspectorForTransition( SceneTransition transition )
        {
            return m_SceneTransitionInspectors.FirstOrDefault( inspector => inspector.m_SceneTransition.m_ID == transition.m_ID );
        }

        public void HideInspectors()
        {
            HideSceneInspectors();
            HideCollectionInspectors();
            HideTransitionInspectors();
        }

        private void HideSceneInspectors()
        {
            foreach ( var sceneSettingsInspector in m_SceneSettingsInspectors )
            {
                GUIUtility.SetVisibility( sceneSettingsInspector, false );
            }
        }

        private void HideCollectionInspectors()
        {
            foreach ( var sceneCollectionInspector in m_SceneCollectionInspectors )
            {
                GUIUtility.SetVisibility( sceneCollectionInspector, false );
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