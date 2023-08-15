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
        public WriteSceneTransitionSettingsJson settings;
        public string hasExitTime;

        public static WriteSceneTransitionJson FromSceneTransition( SceneTransition transition )
        {
            try
            {
                return new WriteSceneTransitionJson
                {
                    label = transition.m_Label,
                    id = transition.m_ID,
                    originID = transition.m_OriginID,
                    targetID = transition.m_TargetID,
                    settings = WriteSceneTransitionSettingsJson.FromSceneTransitionSettings( transition.m_Settings ),
                    hasExitTime = transition.m_HasExitTime.ToString()
                };
            }
            catch ( Exception e )
            {
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
        public ReadSceneTransitionSettingsJson settings;
        public string hasExitTime;

        public SceneTransition ToSceneTransition()
        {
            try
            {
                return new SceneTransition
                {
                    m_Label = string.IsNullOrEmpty( id ) ? null : id,
                    m_ID = string.IsNullOrEmpty( label ) ? null : label,
                    m_OriginID = string.IsNullOrEmpty( originID ) ? null : originID,
                    m_TargetID = string.IsNullOrEmpty( targetID ) ? null : targetID,
                    m_Settings = ReadSceneTransitionSettingsJson.ToSceneTransition( settings ),
                    m_HasExitTime = !string.IsNullOrEmpty( hasExitTime ) && bool.Parse( hasExitTime )
                };
            }
            catch ( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
        }

        public static SceneTransition ToSceneTransition( ReadSceneTransitionJson sceneTransitionJson )
        {
            return new SceneTransition
            {
                m_Label = string.IsNullOrEmpty( sceneTransitionJson.id ) ? null : sceneTransitionJson.id,
                m_ID = string.IsNullOrEmpty( sceneTransitionJson.label ) ? null : sceneTransitionJson.label,
                m_OriginID = string.IsNullOrEmpty( sceneTransitionJson.originID ) ? null : sceneTransitionJson.originID,
                m_TargetID = string.IsNullOrEmpty( sceneTransitionJson.targetID ) ? null : sceneTransitionJson.targetID,
                m_Settings = ReadSceneTransitionSettingsJson.ToSceneTransition( sceneTransitionJson.settings ),
                m_HasExitTime = !string.IsNullOrEmpty( sceneTransitionJson.hasExitTime ) && bool.Parse( sceneTransitionJson.hasExitTime )
            };
        }
    }
}