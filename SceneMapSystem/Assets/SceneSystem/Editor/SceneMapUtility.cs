using UnityEditor;
using UnityEngine;

namespace TNS.SceneSystem.Editor
{
    public static class SceneMapUtility
    {
        // Example string to use when reloading a corrupted .scenemap
        // @formatter:off
        public const string DefaultLayout = "{\n    \"_Name\": \"SceneMapB\",\n    \"_DefaultCollection\": {\n        \"name\": \"\",\n        \"id\": \"\",\n        \"defaultScene\": {\n            \"active\": \"True\",\n            \"name\": \"\",\n            \"id\": \"\",\n            \"path\": \"\",\n            \"sceneSettingsJson\": {\n                \"_Type\": \"Global\",\n                \"_Mode\": \"Single\",\n                \"_LoadingScenePath\": \"\",\n                \"_LoadingParameters\": {\n                    \"useLoadingScreen\": \"False\",\n                    \"threadPriority\": \"Low\",\n                    \"secureLoad\": \"False\",\n                    \"interpolate\": \"False\",\n                    \"interpolationSpeed\": \"0\"\n                }\n            },\n            \"nodePositionJson\": {\n                \"x\": \"0\",\n                \"y\": \"0\",\n                \"width\": \"0\",\n                \"height\": \"0\"\n            }\n        },\n        \"scenes\": [],\n        \"sceneTransitions\": [],\n        \"parameters\": []\n    },\n    \"_Collections\": [\n        {\n            \"name\": \"Level 1\",\n            \"id\": \"9f2d9072-d88d-486f-bb55-3c609615f270\",\n            \"defaultScene\": {\n                \"active\": \"True\",\n                \"name\": \"\",\n                \"id\": \"\",\n                \"path\": \"\",\n                \"sceneSettingsJson\": {\n                    \"_Type\": \"Global\",\n                    \"_Mode\": \"Single\",\n                    \"_LoadingScenePath\": \"\",\n                    \"_LoadingParameters\": {\n                        \"useLoadingScreen\": \"False\",\n                        \"threadPriority\": \"Low\",\n                        \"secureLoad\": \"False\",\n                        \"interpolate\": \"False\",\n                        \"interpolationSpeed\": \"0\"\n                    }\n                },\n                \"nodePositionJson\": {\n                    \"x\": \"0\",\n                    \"y\": \"0\",\n                    \"width\": \"0\",\n                    \"height\": \"0\"\n                }\n            },\n            \"scenes\": [\n                {\n                    \"active\": \"True\",\n                    \"name\": \"AN Demo\",\n                    \"id\": \"d257c912-f242-424a-8140-79f86a8b6d5a\",\n                    \"path\": \"Assets/AZURE Nature/Demo/AN_Demo.unity\",\n                    \"sceneSettingsJson\": {\n                        \"_Type\": \"Global\",\n                        \"_Mode\": \"Single\",\n                        \"_LoadingScenePath\": \"\",\n                        \"_LoadingParameters\": {\n                            \"useLoadingScreen\": \"False\",\n                            \"threadPriority\": \"Low\",\n                            \"secureLoad\": \"False\",\n                            \"interpolate\": \"False\",\n                            \"interpolationSpeed\": \"0\"\n                        }\n                    },\n                    \"nodePositionJson\": {\n                        \"x\": \"14\",\n                        \"y\": \"1\",\n                        \"width\": \"0\",\n                        \"height\": \"0\"\n                    }\n                }\n            ],\n            \"sceneTransitions\": [],\n            \"parameters\": []\n        }\n    ]\n}";
        // @formatter:on
        
        public static void AddSceneToBuildSettings( string path )
        {
            var original = EditorBuildSettings.scenes;

            for ( var index = 0; index < original.Length; index++ )
            {
                var editorBuildSettingsScene = original[index];
                if ( editorBuildSettingsScene.path == path )
                {
                    return;
                }
            }

            var sceneToAdd = new EditorBuildSettingsScene( path, true );
            ArrayHelpers.Append( ref original, sceneToAdd );

            EditorBuildSettings.scenes = original;

            RebuildWindows();
        }

        public static void RemoveSceneFromBuildSettings( string path )
        {
            var original = EditorBuildSettings.scenes;
            var indexToRemove = -1;

            for ( var index = 0; index < original.Length; index++ )
            {
                var editorBuildSettingsScene = original[index];
                if ( editorBuildSettingsScene.path == path )
                {
                    indexToRemove = index;
                }
            }

            if ( indexToRemove == -1 ) return;

            ArrayHelpers.EraseAt( ref original, indexToRemove );

            EditorBuildSettings.scenes = original;

            RebuildWindows();
        }
        
        public static void ToggleEnabledInBuildSettings( string path, bool value )
        {
            var original = EditorBuildSettings.scenes;

            foreach ( var editorBuildSettingsScene in original ) {
                if ( editorBuildSettingsScene.path == path ) {
                    editorBuildSettingsScene.enabled = value;
                }
            }

            EditorBuildSettings.scenes = original;

            RebuildWindows();
        }
        
        public static void RebuildWindows()
        {
            if ( !EditorWindow.HasOpenInstances<SceneMapEditor>() ) return;
            var windows = Resources.FindObjectsOfTypeAll<SceneMapEditor>();
            foreach ( var window in windows ) {
                window.RebuildLists();
            }
        }
    }
}