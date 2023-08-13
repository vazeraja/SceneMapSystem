#if UNITY_EDITOR

using System;
using UnityEditor;

namespace TNS.SceneSystem.Editor
{
    public class SceneAssetPostprocessor : AssetPostprocessor
    {
        public static event Action<string> OnImported;

        private static void OnPostprocessAllAssets( string[] importedAssets,
            string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
        {
            foreach ( var path in importedAssets ) {
                var assetAtPath = AssetDatabase.LoadAssetAtPath<SceneAsset>( path );
                if ( assetAtPath != null ) {
                    OnImported?.Invoke( path );
                }
            }
        }
    }
}

#endif