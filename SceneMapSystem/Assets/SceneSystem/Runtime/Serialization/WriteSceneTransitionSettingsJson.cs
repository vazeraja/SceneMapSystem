using System;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct WriteSceneTransitionSettingsJson
    {
        public string TimeMode;
        public string ExitTime;
        public string FixedDuration;
        public string TransitionDuration;
        public string TransitionOffset;

        public static WriteSceneTransitionSettingsJson FromSceneTransitionSettings( SceneTransitionSettings settings )
        {
            try
            {
                return new WriteSceneTransitionSettingsJson
                {
                    // TimeMode = settings.m_TimeMode.ToString(),
                    ExitTime = settings.m_ExitTime.ToString(),
                    FixedDuration = settings.m_FixedDuration.ToString(),
                    TransitionDuration = settings.m_TransitionDuration.ToString(),
                    TransitionOffset = settings.m_TransitionOffset.ToString()
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
    public struct ReadSceneTransitionSettingsJson
    {
        public string TimeMode;
        public string ExitTime;
        public string FixedDuration;
        public string TransitionDuration;
        public string TransitionOffset;

        public static SceneTransitionSettings ToSceneTransition( ReadSceneTransitionSettingsJson settingsJson )
        {
            var exitTime = 1f;
            if ( !string.IsNullOrEmpty( settingsJson.ExitTime ) )
            {
                exitTime = float.Parse( settingsJson.ExitTime );
            }

            var TransitionDuration = 1f;
            if ( !string.IsNullOrEmpty( settingsJson.TransitionDuration ) )
            {
                TransitionDuration = float.Parse( settingsJson.TransitionDuration );
            }

            var TransitionOffset = 1f;
            if ( !string.IsNullOrEmpty( settingsJson.TransitionOffset ) )
            {
                TransitionOffset = float.Parse( settingsJson.TransitionOffset );
            }


            return new SceneTransitionSettings
            {
                // m_TimeMode = EnumHelpers.ConvertToEnum<TimeMode>( settingsJson.TimeMode ),
                m_ExitTime = exitTime,
                m_FixedDuration = !string.IsNullOrEmpty( settingsJson.FixedDuration ) && bool.Parse( settingsJson.FixedDuration ),
                m_TransitionDuration = TransitionDuration,
                m_TransitionOffset = TransitionOffset
            };
        }
    }
}