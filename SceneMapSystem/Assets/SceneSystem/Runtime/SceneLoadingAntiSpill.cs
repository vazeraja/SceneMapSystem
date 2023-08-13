using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TNS.SceneSystem
{
    /// <summary>
    /// This helper class, meant to be used by the MMAdditiveSceneLoadingManager, creates a temporary scene to store objects that might get instantiated, and empties it in the destination scene once loading is complete
    /// </summary>
    public class SceneLoadingAntiSpill
    {
        public UnityEngine.SceneManagement.Scene _antiSpillScene;
        protected UnityEngine.SceneManagement.Scene _destinationScene;
        protected UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.Scene> _onActiveSceneChangedCallback;
        protected string _sceneToLoadName;
        protected string _antiSpillSceneName;
        protected List<UnityEngine.GameObject> _spillSceneRoots = new List<UnityEngine.GameObject>( 50 );
        protected static List<string> _scenesInBuild;

        /// <summary>
        /// Creates the temporary scene
        /// </summary>
        /// <param name="sceneToLoadName"></param>
        public virtual void PrepareAntiFill( string sceneToLoadName, string antiSpillSceneName = "" )
        {
            _destinationScene = default;
            _sceneToLoadName = sceneToLoadName;

            if ( antiSpillSceneName == "" ) {
                _antiSpillScene = SceneManager.CreateScene( $"AntiSpill_{sceneToLoadName}" );

                PrepareAntiFillSetSceneActive();
            }
            else {
                _scenesInBuild = SceneManager.GetScenesInBuild();
                if ( !_scenesInBuild.Contains( antiSpillSceneName ) ) {
                    Debug.LogError( "SceneLoadingAntiSpill : impossible to load the '" + antiSpillSceneName + "' scene, " +
                                    "there is no such scene in the project's build settings." );
                    return;
                }

                SceneManager.LoadScene( antiSpillSceneName, LoadSceneMode.Additive );
                _antiSpillScene = SceneManager.GetSceneByName( antiSpillSceneName );
                _antiSpillSceneName = _antiSpillScene.name;
                SceneManager.sceneLoaded += PrepareAntiFillOnSceneLoaded;
            }
        }

        /// <summary>
        /// When not creating an anti fill scene, acts once the scene has been actually created and is ready to be set active
        /// This is bypassed when creating the scene
        /// </summary>
        /// <param name="newScene"></param>
        /// <param name="mode"></param>
        protected virtual void PrepareAntiFillOnSceneLoaded( UnityEngine.SceneManagement.Scene newScene, LoadSceneMode mode )
        {
            if ( newScene.name != _antiSpillSceneName ) {
                return;
            }

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= PrepareAntiFillOnSceneLoaded;
            PrepareAntiFillSetSceneActive();
        }

        /// <summary>
        /// Sets the anti spill scene active
        /// </summary>
        protected virtual void PrepareAntiFillSetSceneActive()
        {
            if ( _onActiveSceneChangedCallback != null ) {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback;
            }

            _onActiveSceneChangedCallback = OnActiveSceneChanged;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += _onActiveSceneChangedCallback;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene( _antiSpillScene );
        }

        /// <summary>
        /// Once the destination scene has been loaded, we catch that event and prepare to empty
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        protected virtual void OnActiveSceneChanged( UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to )
        {
            if ( from == _antiSpillScene ) {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback;
                _onActiveSceneChangedCallback = null;

                EmptyAntiSpillScene();
            }
        }

        /// <summary>
        /// Empties the contents of the anti spill scene into the destination scene
        /// </summary>
        protected virtual void EmptyAntiSpillScene()
        {
            if ( _antiSpillScene.IsValid() && _antiSpillScene.isLoaded ) {
                _spillSceneRoots.Clear();
                _antiSpillScene.GetRootGameObjects( _spillSceneRoots );

                _destinationScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( _sceneToLoadName );

                if ( _spillSceneRoots.Count > 0 ) {
                    if ( _destinationScene.IsValid() && _destinationScene.isLoaded ) {
                        foreach ( var root in _spillSceneRoots ) {
                            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene( root, _destinationScene );
                        }
                    }
                }

                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync( _antiSpillScene );
            }
        }
    }
}