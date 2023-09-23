using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TNS.SceneSystem
{
    public class LoadingManager
    {
        private SceneReference m_SceneReference;
        private LoadingParameters m_LoadingParameters;

        private float m_LoadProgress;
        private float m_InterpolatedLoadProgress;

        private readonly SceneLoadingAntiSpill m_AntiSpill = new SceneLoadingAntiSpill();
        private UnityEngine.SceneManagement.Scene[] m_InitialScenes;
        private AsyncOperation m_UnloadOriginAsyncOperation;
        private AsyncOperation m_LoadDestinationAsyncOperation;
        private AsyncOperation m_UnloadLoadingAsyncOperation;
        private const float asyncProgressLimit = 0.9f;

        public LoadingManager()
        {
            SceneManagerCallbacks.Instance.onUpdate += UpdateLoadProgress;
        }

        internal void ReleaseSceneManagerOperation()
        {
            SceneManagerCallbacks.Instance.onUpdate -= UpdateLoadProgress;
        }

        public CoroutineHandle BeginLoad( SceneReference sceneReference )
        {
            m_SceneReference = sceneReference;
            m_LoadingParameters = sceneReference.sceneSettings._LoadingParameters;

            return SceneManagerCallbacks.Instance.RunCoroutine( LoadSequence() );
        }

        private IEnumerator LoadSequence()
        {
            yield return InitiateLoad();
            yield return LoadLoadingScreen( m_SceneReference.sceneSettings._LoadingScene.Name );

            // m_AntiSpill.PrepareAntiFill( m_SceneReference.scene.Name, "Test" );

            yield return UnloadOriginScenes();
            yield return LoadDestinationScene( m_SceneReference.scene.Name );
            yield return ActivateDestinationScene();
            yield return UnloadLoadingScreen( m_SceneReference.sceneSettings._LoadingScene.Name );
        }

        private IEnumerator InitiateLoad()
        {
            SceneManager.isLoadingInProgress = true;

            m_InitialScenes = SceneManager.GetLoadedScenes();
            Time.timeScale = 1f;

            Application.backgroundLoadingPriority = m_LoadingParameters._ThreadPriority;

            if ( m_LoadingParameters._SecureLoad ) {
                if ( !SceneManager.IsSceneInBuild( m_SceneReference.scene.Name ) ) {
                    Debug.LogError( "Impossible to load the '" + m_SceneReference.scene.Name + "' scene, " +
                                    "there is no such scene in the project's build settings." );
                    yield break;
                }

                if ( !SceneManager.IsSceneInBuild( m_SceneReference.sceneSettings._LoadingScene.Name ) ) {
                    Debug.LogError( "Impossible to load the '" + m_SceneReference.sceneSettings._LoadingScene.Name + "' scene, " +
                                    "there is no such scene in the project's build settings." );
                    yield break;
                }
            }

            SceneManager.TriggerSceneLoadStarted();
        }

        private IEnumerator LoadLoadingScreen( string loadingSceneName )
        {
            var loadingSceneOperation = SceneManager.LoadSceneAsync( loadingSceneName, LoadSceneMode.Additive );
            while ( !loadingSceneOperation.isDone ) yield return null;

            // SceneManager.SetActiveScene( SceneManager.GetSceneByName( loadingSceneName ) );
        }

        private IEnumerator UnloadOriginScenes()
        {
            foreach ( UnityEngine.SceneManagement.Scene scene in m_InitialScenes ) {
                if ( !scene.IsValid() || !scene.isLoaded ) {
                    Debug.LogWarning( "Invalid scene : " + scene.name );
                    continue;
                }

                // Debug.Log( "UnloadOriginScene Path: " + scene.path);
                SceneManager.TriggerUnloadOriginScene( scene );

                m_UnloadOriginAsyncOperation = SceneManager.UnloadSceneAsync( scene );
                while ( m_UnloadOriginAsyncOperation.progress < asyncProgressLimit ) {
                    yield return null;
                }
            }
        }

        private IEnumerator LoadDestinationScene( string sceneToLoadName )
        {
            SceneManager.TriggerLoadDestinationScene();

            m_LoadDestinationAsyncOperation = SceneManager.LoadSceneAsync( sceneToLoadName, LoadSceneMode.Additive );
            m_LoadDestinationAsyncOperation.completed += _ => { SceneManager.SetActiveScene( SceneManager.GetSceneByName( sceneToLoadName ) ); };

            m_LoadDestinationAsyncOperation.allowSceneActivation = false;

            while ( m_LoadDestinationAsyncOperation.progress < asyncProgressLimit ) {
                m_LoadProgress = m_LoadDestinationAsyncOperation.progress;
                yield return null;
            }

            SceneManager.TriggerLoadProgressComplete();

            // when the load is close to the end (it'll never reach it), we set it to 100%
            m_LoadProgress = 1f;

            // // we wait for the bar to be visually filled to continue
            if ( m_LoadingParameters._Interpolate ) {
                while ( m_InterpolatedLoadProgress < 1f ) {
                    yield return null;
                }
            }

            SceneManager.TriggerInterpolatedLoadProgressComplete();
        }

        private IEnumerator ActivateDestinationScene()
        {
            yield return CoroutineHelpers.WaitForFrames( 1 );

            SceneManager.TriggerDestinationSceneActivation();

            m_LoadDestinationAsyncOperation.allowSceneActivation = true;
            while ( m_LoadDestinationAsyncOperation.progress < 1.0f ) {
                yield return null;
            }
        }

        private IEnumerator UnloadLoadingScreen( string loadingScreenSceneName )
        {
            SceneManager.TriggerUnloadLoadingScreen();

            yield return null; // mandatory yield to avoid an unjustified warning
            m_UnloadLoadingAsyncOperation = SceneManager.UnloadSceneAsync( loadingScreenSceneName );
            while ( m_UnloadLoadingAsyncOperation.progress < asyncProgressLimit ) {
                yield return null;
            }

            SceneManager.isLoadingInProgress = false;
        }

        private void UpdateLoadProgress( float time )
        {
            // Debug.Log( "unscaledDeltaTime" + time );
            if ( !SceneManager.isLoadingInProgress ) return;

            SceneManager.TriggerRealtimeProgressValue( m_LoadProgress );

            if ( m_LoadingParameters._Interpolate ) {
                var amount = time * m_LoadingParameters._InterpolationSpeed;

                m_InterpolatedLoadProgress = MathHelpers.Approach( m_InterpolatedLoadProgress, m_LoadProgress, amount );
                SceneManager.TriggerInterpolatedProgressValue( m_InterpolatedLoadProgress );
            }
            else {
                SceneManager.TriggerInterpolatedProgressValue( m_LoadProgress );
            }
        }
    }
}