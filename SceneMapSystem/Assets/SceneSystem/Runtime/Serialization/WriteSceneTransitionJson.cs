using System;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct WriteSceneTransitionJson
    {
        public string id;
        public string label;
        public string originID;
        public string targetID;

        public static WriteSceneTransitionJson FromSceneTransition( SceneTransition transition )
        {
            try {
                return new WriteSceneTransitionJson
                {
                    id = transition.ID,
                    label = transition.Label,
                    originID = transition.OriginID,
                    targetID = transition.TargetID
                };
            }
            catch ( Exception e ) {
                Debug.Log( e );
                throw;
            }
        }
    }
    
    [Serializable]
    public struct ReadSceneTransitionJson
    {
        public string id;
        public string label;
        public string originID;
        public string targetID;

        public SceneTransition ToSceneTransition( )
        {
            try {
                return new SceneTransition
                {
                    ID = string.IsNullOrEmpty( id ) ? null : id,
                    Label = string.IsNullOrEmpty( label ) ? null : label,
                    OriginID = string.IsNullOrEmpty( originID ) ? null : originID,
                    TargetID = string.IsNullOrEmpty( targetID ) ? null : targetID,
                };
            }
            catch ( Exception e ) {
                Console.WriteLine( e );
                throw;
            }
        }

        public static SceneTransition ToSceneTransition( ReadSceneTransitionJson sceneTransitionJson )
        {
            return new SceneTransition
            {
                ID = string.IsNullOrEmpty( sceneTransitionJson.id ) ? null : sceneTransitionJson.id,
                Label = string.IsNullOrEmpty( sceneTransitionJson.label ) ? null : sceneTransitionJson.label,
                OriginID = string.IsNullOrEmpty( sceneTransitionJson.originID ) ? null : sceneTransitionJson.originID,
                TargetID = string.IsNullOrEmpty( sceneTransitionJson.targetID ) ? null : sceneTransitionJson.targetID,
            };
        }
    }
}