#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TNS.SceneSystem.Editor
{
    [InitializeOnLoad]
    public static class SceneAssetImporter
    {
        static SceneAssetImporter()
        {
            SceneAssetPostprocessor.OnImported += OnImported;
        }

        private static void OnImported( string path )
        {
            if ( EditorWindow.HasOpenInstances<SceneMapEditorWindow>() ) {
                var windows = Resources.FindObjectsOfTypeAll<SceneMapEditorWindow>();
                foreach ( var window in windows ) {
                    TryUpdateSceneAssetReference( window.SceneMap, path );
                    window.SaveChangesToAsset();
                }
            }
            else {
                var mapAssets = FindAssetsByType<SceneMapAsset>();
                foreach ( var asset in mapAssets ) {
                    TryUpdateSceneAssetReference( asset, path );
                    asset.SaveChangesToAsset();
                }
            }
        }

        private static void TryUpdateSceneAssetReference( SceneMapAsset asset, string newPath )
        {
            var sceneReferencesByPath = asset.SceneReferencesByPath;
            if ( sceneReferencesByPath.Count < 1 ) return;

            var pathsInMap = sceneReferencesByPath.Select( kvp => kvp.Key ).ToList();
            // var except = pathsInMap.Except( EditorBuildSettings.scenes.Select( s => s.path ) );

            var allSceneAssetPaths = GetAssetsOfType( typeof( SceneAsset ), ".unity" ).Select( AssetDatabase.GetAssetPath );

            var intersection = pathsInMap.Intersect( allSceneAssetPaths );
            var except = pathsInMap.Except( intersection ).ToList();
            if ( !except.Any() ) return;

            if ( !sceneReferencesByPath.TryGetValue( except.First(), out var sceneReference ) ) return;
            sceneReference._Scene = new Scene( newPath );
            sceneReference._Scene.Rebind( newPath );
        }
        
        public static IEnumerable<SceneAsset> GetAllSceneAssets()
        {
            return GetAssetsOfType( typeof( SceneAsset ), ".unity" ).Select( asset => asset as SceneAsset );
        }
        
        public static IEnumerable<Object> GetAssetsOfType( Type type, string fileExtension )
        {
            var tempObjects = new List<Object>();
            DirectoryInfo directory = new DirectoryInfo( Application.dataPath );
            var goFileInfo = directory.GetFiles( "*" + fileExtension, SearchOption.AllDirectories );

            int goFileInfoLength = goFileInfo.Length;
            for ( int i = 0; i < goFileInfoLength; i++ ) {
                var tempGoFileInfo = goFileInfo[i];
                if ( tempGoFileInfo == null )
                    continue;

                var tempFilePath = tempGoFileInfo.FullName;
                tempFilePath = tempFilePath.Replace( @"\", "/" ).Replace( Application.dataPath, "Assets" );

                // Debug.Log( tempFilePath );

                var tempGO = AssetDatabase.LoadAssetAtPath( tempFilePath, typeof( Object ) );
                if ( tempGO == null ) {
                    Debug.LogWarning( "Skipping Null" );
                    continue;
                }

                if ( tempGO.GetType() != type ) {
                    Debug.LogWarning( "Skipping " + tempGO.GetType() );
                    continue;
                }

                tempObjects.Add( tempGO );
            }

            return tempObjects.ToArray();
        }
        
        public static IEnumerable<T> FindAssetsByType<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets( $"t:{typeof( T )}" );
            foreach ( var t in guids ) {
                var assetPath = AssetDatabase.GUIDToAssetPath( t );
                var asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
                if ( asset != null ) {
                    yield return asset;
                }
            }
        }
        
    }
}

#endif