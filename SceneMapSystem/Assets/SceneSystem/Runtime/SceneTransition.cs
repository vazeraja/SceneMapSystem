﻿using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TNS.SceneSystem
{
    public enum TimeMode
    {
        Scaled,
        Unscaled
    }

    [Serializable]
    public class SceneTransition
    {
        [SerializeField] internal string m_Label = string.Empty;
        [SerializeField] internal string m_ID = string.Empty;
        [SerializeField] internal string m_OriginID = string.Empty;
        [SerializeField] internal string m_TargetID = string.Empty;

        [SerializeField] internal bool m_HasExitTime;
        [SerializeField] internal SceneTransitionSettings m_Settings;
        
        private event Action onEnterTransition;
        private event Action onExitTransition;

        public SceneTransition() { }

        public SceneTransition( SceneReference origin, SceneReference target )
        {
            m_ID = Guid.NewGuid().ToString();
            m_Label = origin + " --> " + target;
            m_OriginID = origin.id;
            m_TargetID = target.id;
        }
    }
}