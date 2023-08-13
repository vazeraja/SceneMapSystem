using System;
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
        [SerializeField] private string m_Label = string.Empty;
        [SerializeField] private string m_ID = string.Empty;
        [SerializeField] private string m_OriginID = string.Empty;
        [SerializeField] private string m_TargetID = string.Empty;

        [SerializeField] private float m_Duration;
        [SerializeField] private TimeMode m_TimeMode;
        private event Action onEnterTransition;
        private event Action onExitTransition;

        public SceneTransition() { }

        public SceneTransition( SceneReference origin, SceneReference target )
        {
            m_ID = Guid.NewGuid().ToString();
            m_Label = origin + " -->" + target;
            m_OriginID = origin.id;
            m_TargetID = target.id;
        }

        /// <summary>
        /// Gets or sets the time mode of the transition duration (scaled vs unscaled)
        /// </summary>
        public TimeMode TimeMode
        {
            get => m_TimeMode;
            set => m_TimeMode = value;
        }

        /// <summary>
        /// Gets or sets the duration in seconds the transition takes to finish
        /// before the transition is triggered
        /// </summary>
        public float Duration
        {
            get => m_Duration;
            set => m_Duration = Mathf.Max( 0, value );
        }

        /// <summary>
        /// The unique id of the transition
        /// </summary>
        public string ID
        {
            get => m_ID;
            set => m_ID = value;
        }

        /// <summary>
        /// The label of the transition. This does not need to be unique.
        /// </summary>
        public string Label
        {
            get => m_Label;
            set => m_Label = value;
        }

        /// <summary>
        /// Gets the event which is invoked when the transition is entered
        /// </summary>
        public Action OnEnterTransition => onEnterTransition;

        /// <summary>
        /// Gets the event which is invoked when the transition is exited
        /// </summary>
        public Action OnExitTransition => onExitTransition;

        /// <summary>
        /// The name of the origin state of the transition
        /// </summary>
        public string OriginID
        {
            get => m_OriginID;
            set => m_OriginID = value;
        }

        /// <summary>
        /// The name of the target state of the transition
        /// </summary>
        public string TargetID
        {
            get => m_TargetID;
            set => m_TargetID = value;
        }
    }
}