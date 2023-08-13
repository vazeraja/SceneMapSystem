#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace TNS.SceneSystem
{
    public static class SceneManager
    {
        private static bool m_ReinitializeLoadingManager = true;
        private static LoadingManager m_LoadingManagerInstance;

        private static LoadingManager LoadingManager
        {
            get
            {
                m_LoadingManagerInstance ??= new LoadingManager();
            #if UNITY_EDITOR
                // ReSharper disable once InvertIf
                // Support for turning off domain reload
                if ( EditorSettings.enterPlayModeOptionsEnabled && m_ReinitializeLoadingManager ) {
                    m_ReinitializeLoadingManager = false;
                    m_LoadingManagerInstance.ReleaseSceneManagerOperation();
                    m_LoadingManagerInstance = new LoadingManager();
                }
            #endif
                return m_LoadingManagerInstance;
            }
        }

    #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void RegisterPlayModeStateChange()
        {
            EditorApplication.playModeStateChanged += SetLoadingManagerReInitFlagOnExitPlayMode;
        }

        private static void SetLoadingManagerReInitFlagOnExitPlayMode( PlayModeStateChange change )
        {
            switch ( change ) {
                case PlayModeStateChange.EnteredEditMode or PlayModeStateChange.ExitingPlayMode:
                    m_ReinitializeLoadingManager = true;
                    isLoadingInProgress = false;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    m_LoadingManagerInstance = new LoadingManager();
                    break;
            }
        }

    #endif

        internal static void LoadScene( SceneReference sceneReference )
        {
            if ( isLoadingInProgress ) return;
            if ( sceneReference.sceneSettings._LoadingParameters._UseLoadingScreen ) {
                Debug.LogError( $"This SceneReference is set to use its associated loading screen. " +
                                $"Please use (sceneReference.LoadSceneAsync()) instead." );
                return;
            }

            LoadScene( sceneReference._Scene.Path, sceneReference.sceneSettings._Mode );
        }

        internal static AsyncOperation LoadSceneAsync( SceneReference sceneReference, LoadSceneMode mode = LoadSceneMode.Single )
        {
            if ( !sceneReference.sceneSettings._LoadingParameters._UseLoadingScreen )
                return LoadSceneAsync( sceneReference.scene.Name, sceneReference.sceneSettings._Mode );

            LoadingManager.BeginLoad( sceneReference );

            return null;
        }

        public static bool isLoadingInProgress;

        public static event Action onSceneLoadStarted;
        public static event Action<UnityScene> onUnloadOriginScene;
        public static event Action onLoadDestinationScene;
        public static event Action onLoadProgressComplete;
        public static event Action onInterpolatedLoadProgressComplete;
        public static event Action onDestinationSceneActivation;
        public static event Action onUnloadLoadingScreen;
        public static event Action<float> setInterpolatedProgressValue;
        public static event Action<float> setRealtimeProgressValue;

        internal static void TriggerSceneLoadStarted() => onSceneLoadStarted?.Invoke();
        internal static void TriggerUnloadOriginScene( UnityScene scene ) => onUnloadOriginScene?.Invoke( scene );
        internal static void TriggerLoadDestinationScene() => onLoadDestinationScene?.Invoke();
        internal static void TriggerLoadProgressComplete() => onLoadProgressComplete?.Invoke();
        internal static void TriggerInterpolatedLoadProgressComplete() => onInterpolatedLoadProgressComplete?.Invoke();
        internal static void TriggerDestinationSceneActivation() => onDestinationSceneActivation?.Invoke();
        internal static void TriggerUnloadLoadingScreen() => onUnloadLoadingScreen?.Invoke();
        internal static void TriggerInterpolatedProgressValue( float value ) => setInterpolatedProgressValue?.Invoke( value );
        internal static void TriggerRealtimeProgressValue( float value ) => setRealtimeProgressValue?.Invoke( value );

        // @formatter:off
        public static int sceneCount => UnitySceneManager.sceneCount;
        public static int loadedSceneCount => UnitySceneManager.loadedSceneCount;
        public static int sceneCountInBuildSettings => UnitySceneManager.sceneCountInBuildSettings;
        
        public static UnityScene GetActiveScene() => UnitySceneManager.GetActiveScene();
        public static void SetActiveScene( UnityScene unityScene ) => UnitySceneManager.SetActiveScene( unityScene );
        public static UnityScene GetSceneByPath( string path ) => UnitySceneManager.GetSceneByPath( path );
        public static UnityScene GetSceneByName( string name ) => UnitySceneManager.GetSceneByName( name );
        public static UnityScene GetSceneByBuildIndex( int index ) => UnitySceneManager.GetSceneByBuildIndex( index );
        public static UnityScene GetSceneAt( int index ) => UnitySceneManager.GetSceneAt( index );
        public static UnityScene CreateScene( string name, CreateSceneParameters parameters ) => UnitySceneManager.CreateScene( name, parameters );
        public static UnityScene CreateScene( string name ) => UnitySceneManager.CreateScene( name );
        public static void MergeScenes( UnityScene source, UnityScene destination ) => UnitySceneManager.MergeScenes( source, destination );
        public static void MoveGameObjectToScene( GameObject go, UnityScene scene ) => UnitySceneManager.MoveGameObjectToScene( go, scene );
        
        public static string GetScenePathByBuildIndex(int buildIndex) => SceneUtility.GetScenePathByBuildIndex(buildIndex);
        public static int GetBuildIndexByScenePath(string scenePath) => SceneUtility.GetBuildIndexByScenePath(scenePath);
        
        public static event UnityAction<UnityScene, LoadSceneMode> sceneLoaded
        {
            add => UnitySceneManager.sceneLoaded += value;
            remove => UnitySceneManager.sceneLoaded -= value;
        }
        public static event UnityAction<UnityScene> sceneUnloaded
        {
            add => UnitySceneManager.sceneUnloaded += value;
            remove => UnitySceneManager.sceneUnloaded -= value;
        }
        public static event UnityAction<UnityScene, UnityScene> activeSceneChanged
        {
            add => UnitySceneManager.activeSceneChanged += value;
            remove => UnitySceneManager.activeSceneChanged -= value;
        }
        
        public static bool IsSceneInBuild( string sceneName ) => GetScenesInBuild().Contains( sceneName );
        public static void SetActiveScene( Scene field ) => SetActiveScene( GetSceneByPath( field.Path ) );
        
        public static void LoadScene( string path, LoadSceneMode mode ) => UnitySceneManager.LoadScene( path, mode );
        public static void LoadScene( string sceneName ) => UnitySceneManager.LoadScene( sceneName );
        public static UnityScene LoadScene( string sceneName, LoadSceneParameters lcp ) => UnitySceneManager.LoadScene( sceneName, lcp );
        public static void LoadScene( int sceneBuildIndex, LoadSceneMode mode ) => UnitySceneManager.LoadScene( sceneBuildIndex, mode );
        public static void LoadScene( int sceneBuildIndex ) => UnitySceneManager.LoadScene( sceneBuildIndex );
        public static UnityScene LoadScene( int sceneBuildIndex, LoadSceneParameters lcp ) => UnitySceneManager.LoadScene( sceneBuildIndex, lcp );
        
        public static AsyncOperation LoadSceneAsync( int buildIndex, LoadSceneMode mode ) => UnitySceneManager.LoadSceneAsync( buildIndex, mode );
        public static AsyncOperation LoadSceneAsync( int buildIndex ) => UnitySceneManager.LoadSceneAsync( buildIndex );
        public static AsyncOperation LoadSceneAsync( int buildIndex, LoadSceneParameters lcp ) => UnitySceneManager.LoadSceneAsync( buildIndex, lcp );
        public static AsyncOperation LoadSceneAsync( string sceneName, LoadSceneMode mode ) => UnitySceneManager.LoadSceneAsync( sceneName, mode );
        public static AsyncOperation LoadSceneAsync( string sceneName ) => UnitySceneManager.LoadSceneAsync( sceneName );
        public static AsyncOperation LoadSceneAsync( string sceneName, LoadSceneParameters lcp ) => UnitySceneManager.LoadSceneAsync( sceneName, lcp );
        
        public static AsyncOperation UnloadSceneAsync( UnityScene scene ) => UnitySceneManager.UnloadSceneAsync( scene );
        public static AsyncOperation UnloadSceneAsync( string sceneName ) => UnitySceneManager.UnloadSceneAsync( sceneName );
        public static AsyncOperation UnloadSceneAsync( int buildIndex ) => UnitySceneManager.UnloadSceneAsync( buildIndex );
        public static AsyncOperation UnloadSceneAsync( UnityScene scene, UnloadSceneOptions options ) => UnitySceneManager.UnloadSceneAsync( scene, options );
        public static AsyncOperation UnloadSceneAsync( string sceneName, UnloadSceneOptions options ) => UnitySceneManager.UnloadSceneAsync( sceneName, options );
        public static AsyncOperation UnloadSceneAsync( int buildIndex, UnloadSceneOptions options ) => UnitySceneManager.UnloadSceneAsync( buildIndex, options );
        
        [Obsolete( "Use SceneManager.sceneCount and SceneManager.GetSceneAt(int index) to loop the all scenes instead." )]
        public static UnityScene[] GetAllScenes() => UnitySceneManager.GetAllScenes();
        [Obsolete( "Use SceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details." )]
        public static bool UnloadScene( UnityScene scene ) => UnitySceneManager.UnloadScene( scene );
        [Obsolete( "Use SceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details." )]
        public static bool UnloadScene( int sceneBuildIndex ) => UnitySceneManager.UnloadScene( sceneBuildIndex );
        [Obsolete( "Use SceneManager.UnloadSceneAsync. This function is not safe to use during triggers and under other circumstances. See Scripting reference for more details." )]
        public static bool UnloadScene( string sceneName ) => UnitySceneManager.UnloadScene( sceneName );
        // @formatter:on

    #if UNITY_EDITOR
        public static bool IsSceneEnabledInBuild( string path )
        {
            return EditorBuildSettings.scenes
                .Where( editorBuildSettingsScene => editorBuildSettingsScene.path == path )
                .Any( editorBuildSettingsScene => editorBuildSettingsScene.enabled );
        }
    #endif

        public static List<string> GetScenesInBuild()
        {
            var scenes = new List<string>();
            const StringComparison comparison = StringComparison.Ordinal;

            for ( int i = 0; i < sceneCountInBuildSettings; i++ ) {
                string scenePath = GetScenePathByBuildIndex( i );
                int lastSlash = scenePath.LastIndexOf( "/", comparison );
                var length = scenePath.LastIndexOf( ".", comparison ) - lastSlash - 1;
                var substring = scenePath.Substring( lastSlash + 1, length );
                scenes.Add( substring );
            }

            return scenes;
        }

        public static UnityScene[] GetLoadedScenes()
        {
            int count = sceneCount;
            var loadedScenes = new List<UnityEngine.SceneManagement.Scene>( count );

            for ( int i = 0; i < count; i++ ) {
                var scene = GetSceneAt( i );
                if ( scene.isLoaded ) loadedScenes.Add( scene );
                else Debug.LogWarning( $"{scene.name} NOT LOADED" );
            }

            return loadedScenes.ToArray();
        }
    }
}