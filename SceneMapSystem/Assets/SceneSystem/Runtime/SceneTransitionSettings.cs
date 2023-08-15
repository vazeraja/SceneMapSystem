using System;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct SceneTransitionSettings
    {
        // [SerializeField] internal TimeMode m_TimeMode;
        [SerializeField] internal float m_ExitTime;
        [SerializeField] internal bool m_FixedDuration;
        [SerializeField] internal float m_TransitionDuration;
        [SerializeField] internal float m_TransitionOffset;
    }
}