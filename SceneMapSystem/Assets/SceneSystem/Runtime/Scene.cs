#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct Scene : IDisposable, ICloneable
    {
        [SerializeField] internal string _Name;
        [SerializeField] internal string _Path;
        [SerializeField] internal int _BuildIndex;

        public string Name => _Name;
        public string Path => _Path;
        public int BuildIndex => _BuildIndex;

        public Scene( string path )
        {
            if ( string.IsNullOrEmpty( path ) )
            {
                _Name = default;
                _Path = default;
                _BuildIndex = -1;
                return;
            }

            _Path = path;
            _Name = AssetDatabase.LoadAssetAtPath<SceneAsset>( _Path ).name;
            _BuildIndex = SceneManager.GetBuildIndexByScenePath( _Path );
        }

        public void Dispose() { }

        private Scene Clone()
        {
            var clone = new Scene( _Path );
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

#if UNITY_EDITOR
        public void Rebind( string path = null, bool displayLog = false )
        {
            if ( !string.IsNullOrEmpty( path ) )
            {
                _Path = path;
            }

            if ( string.IsNullOrEmpty( _Path ) && _BuildIndex < 0 )
            {
                Debug.Log( "Please reassign the scene field. Cannot rebind fields as there is nothing to rebind" );
                return;
            }

            if ( string.IsNullOrEmpty( _Path ) )
            {
                _Path = SceneManager.GetScenePathByBuildIndex( _BuildIndex );
            }

            // if ( !EditorBuildSettings.scenes.Select( s => s.path ).Contains( _Path ) ) {
            //     Debug.Log( $"This scene {_Path} is not added or does not exist in build settings and cannot be auto rebound" );
            //     return;
            // }

            _Name = AssetDatabase.LoadAssetAtPath<SceneAsset>( _Path ).name;
            _BuildIndex = SceneManager.GetBuildIndexByScenePath( _Path );

            var log = new StringBuilder();
            log.AppendLine();
            log.AppendLine( $"Name: {_Name}" );
            log.AppendLine( $"Path: {_Path}" );
            log.AppendLine( $"BuildIndex: {_BuildIndex}" );

            if ( displayLog ) Debug.Log( log );
        }
#endif
    }
}