namespace TNS.SceneSystem
{
    public struct SceneTransitionCondition
    {
        private SceneTransitionConditionMode m_ConditionMode;
        private string m_ConditionEventName;
        private string m_ConditionEventID;
        private float m_EventThreshold;

        /// <summary>
        ///   <para>The mode of the condition.</para>
        /// </summary>
        public SceneTransitionConditionMode mode
        {
            get => this.m_ConditionMode;
            set => this.m_ConditionMode = value;
        }

        /// <summary>
        ///   <para>The name of the parameter used in the condition.</para>
        /// </summary>
        public string ParameterName
        {
            get => m_ConditionEventName;
            set => m_ConditionEventName = value;
        }
        
        /// <summary>
        ///   <para>The name of the parameter used in the condition.</para>
        /// </summary>
        public string ParameterID
        {
            get => m_ConditionEventID;
            set => m_ConditionEventID = value;
        }

        /// <summary>
        ///   <para>The AnimatorParameter's threshold value for the condition to be true.</para>
        /// </summary>
        public float Threshold
        {
            get => this.m_EventThreshold;
            set => this.m_EventThreshold = value;
        }
    }
}