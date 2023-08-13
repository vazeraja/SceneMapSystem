using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TNS.SceneSystem
{
    public class SceneReferenceAsset : ScriptableObject
    {
        [SerializeField] public SceneMapAsset _Asset;
        [NonSerialized] private SceneReference m_SceneReference;
        [SerializeField] internal string _SceneId;

        public SceneMapAsset asset => _Asset;

        public SceneReference sceneReference
        {
            get
            {
                if ( m_SceneReference != null ) return m_SceneReference;
                if ( _Asset == null ) return null;

                m_SceneReference = _Asset.FindScene( _SceneId );

                return m_SceneReference;
            }
        }

        public void LoadScene()
        {
            sceneReference.LoadScene();
        }
        public void LoadSceneAsync()
        {
            sceneReference.LoadSceneAsync();
        }

        public bool IsAssigned() => sceneReference.scene.Path != null;

        public void Set( SceneReference reference )
        {
            if ( reference == null ) {
                _Asset = default;
                _SceneId = default;
                return;
            }

            var map = reference.collection;
            if ( map == null || map.asset == null )
                throw new InvalidOperationException(
                    $"Action '{reference}' must be part of an InputActionAsset in order to be able to create an InputActionReference for it" );

            SetInternal( map.asset, reference );
        }

        private void SetInternal( SceneMapAsset asset, SceneReference reference )
        {
            var actionMap = reference.collection;
            if ( !asset.SceneCollections.ToList().Contains( actionMap ) )
                throw new ArgumentException(
                    $"Action '{reference}' is not contained in asset '{asset}'", nameof( reference ) );

            _Asset = asset;
            _SceneId = reference.id.ToString();
            name = GetDisplayName( reference );

            ////REVIEW: should this dirty the asset if IDs had not been generated yet?
        }

        private static string GetDisplayName( SceneReference reference )
        {
            return !string.IsNullOrEmpty( reference?.collection?.name ) ? $"{reference.collection?.name}/{reference.name}" : reference?.name;
        }
    }
}