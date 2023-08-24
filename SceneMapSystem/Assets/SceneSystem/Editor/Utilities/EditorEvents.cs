using System;
using UnityEditor;

namespace TNS.SceneSystem.Editor
{
    public class EditorEvents
    {
        private static SceneMapEditorWindow m_Window => EditorWindow.focusedWindow as SceneMapEditorWindow;

        public event Action AssetSaved;
        public event Action AssetInitialized;

        public event Action<SceneMapAsset.DataType> ItemSelected;
        public event Action<int> CollectionSelected;
        public event Action<SceneCollection> CollectionRemoved;
        public event Action<SceneCollection> CollectionCreated;
        public event Action<int> SceneSelected;
        public event Action<SceneReference> SceneReferenceRemoved;
        public event Action<SceneReference> SceneReferenceCreated;
        public event Action<SceneCollection, SceneTransitionParameter> ParameterRemoved;
        public event Action<SceneCollection, SceneTransitionParameter> ParameterCreated;

        public event Action<SceneTransition> TransitionSelected;
        public event Action<SceneTransition> TransitionRemoved;
        public event Action<SceneTransition> TransitionCreated;

        public event Action<SceneMapAsset.DataType, string> ItemLabelChanged;

        public void Clear()
        {
            ItemSelected = null;

            CollectionSelected = null;
            CollectionRemoved = null;
            CollectionCreated = null;

            SceneSelected = null;
            SceneReferenceRemoved = null;
            SceneReferenceCreated = null;

            ParameterRemoved = null;
            ParameterCreated = null;

            TransitionSelected = null;
            TransitionRemoved = null;
            TransitionCreated = null;

            ItemLabelChanged = null;
        }

        public void TriggerAssetInitialized()
        {
            AssetInitialized?.Invoke();
        }

        public void TriggerItemSelected( SceneMapAsset.DataType type )
        {
            ItemSelected?.Invoke( type );
            SceneMapUtility.RebuildWindows();
        }

        public void TriggerCollectionSelected( int index )
        {
            CollectionSelected?.Invoke( index );
            SceneMapUtility.RebuildWindows();
        }

        public void TriggerSceneSelected( int index )
        {
            SceneSelected?.Invoke( index );
            SceneMapUtility.RebuildWindows();
        }

        public void TriggerCollectionRemoved( SceneCollection c )
        {
            CollectionRemoved?.Invoke( c );
            m_Window.SaveAndRebuild();
        }

        public void TriggerCollectionCreated( SceneCollection c )
        {
            CollectionCreated?.Invoke( c );
            m_Window.SaveAndRebuild();
        }

        public void TriggerSceneReferenceRemoved( SceneReference scene )
        {
            SceneReferenceRemoved?.Invoke( scene );
            m_Window.SaveAndRebuild();
        }

        public void TriggerSceneReferenceCreated( SceneReference scene )
        {
            SceneReferenceCreated?.Invoke( scene );
            m_Window.SaveAndRebuild();
        }

        public void TriggerParameterRemoved( SceneCollection collection, SceneTransitionParameter parameter )
        {
            ParameterRemoved?.Invoke( collection, parameter );
            m_Window.SaveAndRebuild();
        }

        public void TriggerParameterCreated( SceneCollection collection, SceneTransitionParameter parameter )
        {
            ParameterCreated?.Invoke( collection, parameter );
            m_Window.SaveAndRebuild();
        }

        public void TriggerLabelChanged( SceneMapAsset.DataType type, string text )
        {
            ItemLabelChanged?.Invoke( type, text );
            m_Window.SaveAndRebuild();
        }

        public void TriggerTransitionSelected( SceneTransition transition )
        {
            TransitionSelected?.Invoke( transition );
        }

        public void TriggerTransitionRemoved( SceneTransition transition )
        {
            TransitionRemoved?.Invoke( transition );
            m_Window.SaveAndRebuild();
        }

        public void TriggerTransitionCreated( SceneTransition transition )
        {
            TransitionCreated?.Invoke( transition );
            m_Window.SaveAndRebuild();
        }

        public void TriggerSaveEvent()
        {
            if ( m_Window == null ) return;
            
            AssetSaved?.Invoke();
            m_Window.SaveAndRebuild(showCallerName: true);
        }
    }
}