using System;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public class SceneTransitionParameter
    {
        [SerializeField] internal string m_Name = "";
        [SerializeField] internal string m_ID = "";
        [SerializeField] internal SceneTransitionParameterType m_Type;
        [SerializeField] internal float m_DefaultFloat;
        [SerializeField] internal int m_DefaultInt;
        [SerializeField] internal bool m_DefaultBool;
        [SerializeField] internal bool m_DefaultTrigger;

        public SceneTransitionParameter() { }

        public SceneTransitionParameter( SceneTransitionParameterType type )
        {
            m_ID = Guid.NewGuid().ToString();
            m_Name = type.ToString();
            m_Type = type;
        }

        /// <summary>  
        ///   <para>The type of the parameter.</para>
        /// </summary>
        public SceneTransitionParameterType type
        {
            get => m_Type;
            set => m_Type = value;
        }

        /// <summary>
        ///   <para>The default float value for the parameter.</para>
        /// </summary>
        public float defaultFloat
        {
            get => m_DefaultFloat;
            set => m_DefaultFloat = value;
        }

        /// <summary>
        ///   <para>The default int value for the parameter.</para>
        /// </summary>
        public int defaultInt
        {
            get => m_DefaultInt;
            set => m_DefaultInt = value;
        }

        /// <summary>
        ///   <para>The default bool value for the parameter.</para>
        /// </summary>
        public bool defaultBool
        {
            get => m_DefaultBool;
            set => m_DefaultBool = value;
        }
        
        /// <summary>
        ///   <para>The default bool value for the parameter.</para>
        /// </summary>
        public bool DefaultTrigger
        {
            get => m_DefaultTrigger;
            set => m_DefaultTrigger = value;
        }
        
        /// <summary>
        ///   <para>The name of the parameter.</para>
        /// </summary>
        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }

        /// <summary>
        ///   <para>Returns the hash of the parameter based on its name.</para>
        /// </summary>
        public int nameHash => Animator.StringToHash( m_Name );

        public override bool Equals( object o ) => o is SceneTransitionParameter controllerParameter && m_Name == controllerParameter.m_Name &&
                                                   m_Type == controllerParameter.m_Type &&
                                                   m_DefaultFloat == (double) controllerParameter.m_DefaultFloat &&
                                                   m_DefaultInt == controllerParameter.m_DefaultInt &&
                                                   m_DefaultBool == controllerParameter.m_DefaultBool;

        public override int GetHashCode() => name.GetHashCode();

        public override string ToString()
        {
            return m_Name;
        }
    }
}