using System;
using System.Globalization;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct ReadSceneCollectionParameterJson
    {
        public string id;
        public string name;
        public string type;
        public string defaultFloat;
        public string defaultInt;
        public string defaultBool;
        public string defaultTrigger;
        
        public SceneTransitionParameter ToSceneCollectionParameter( )
        {
            var defaultFloatValue = 1f;
            if ( !string.IsNullOrEmpty( defaultFloat ) )
            {
                defaultFloatValue = float.Parse( defaultFloat );
            }

            var defaultIntValue = 1;
            if ( !string.IsNullOrEmpty( defaultInt ) )
            {
                defaultIntValue = int.Parse( defaultInt );
            }

            return new SceneTransitionParameter
            {
                m_ID = id,
                m_Name = name,
                m_Type = EnumHelpers.ConvertToEnum<SceneTransitionParameterType>( type ),
                m_DefaultFloat = defaultFloatValue,
                m_DefaultInt = defaultIntValue,
                m_DefaultBool = !string.IsNullOrEmpty( defaultBool ) && bool.Parse( defaultBool ),
                m_DefaultTrigger = !string.IsNullOrEmpty( defaultTrigger ) && bool.Parse( defaultTrigger ),
            };
        }

        public static SceneTransitionParameter ToSceneCollectionParameter( ReadSceneCollectionParameterJson parameterJson )
        {
            var defaultFloat = 1f;
            if ( !string.IsNullOrEmpty( parameterJson.defaultFloat ) )
            {
                defaultFloat = float.Parse( parameterJson.defaultFloat );
            }

            var defaultInt = 1;
            if ( !string.IsNullOrEmpty( parameterJson.defaultInt ) )
            {
                defaultInt = int.Parse( parameterJson.defaultInt );
            }

            return new SceneTransitionParameter
            {
                m_ID = parameterJson.id,
                m_Name = parameterJson.name,
                m_Type = EnumHelpers.ConvertToEnum<SceneTransitionParameterType>( parameterJson.type ),
                m_DefaultFloat = defaultFloat,
                m_DefaultInt = defaultInt,
                m_DefaultBool = !string.IsNullOrEmpty( parameterJson.defaultBool ) && bool.Parse( parameterJson.defaultBool ),
                m_DefaultTrigger = !string.IsNullOrEmpty( parameterJson.defaultTrigger ) && bool.Parse( parameterJson.defaultTrigger ),
            };
        }
    }

    [Serializable]
    public struct WriteSceneCollectionParameterJson
    {
        public string id; // string
        public string name; // string
        public string type; // enum
        public string defaultFloat; // float
        public string defaultInt; // int
        public string defaultBool; // bool
        public string defaultTrigger;

        public static WriteSceneCollectionParameterJson FromSceneCollectionParameter( SceneTransitionParameter parameter )
        {
            try
            {
                return new WriteSceneCollectionParameterJson
                {
                    id = parameter.m_ID,
                    name = parameter.m_Name,
                    type = parameter.m_Type.ToString(),
                    defaultFloat = parameter.m_DefaultFloat.ToString( CultureInfo.InvariantCulture ),
                    defaultInt = parameter.m_DefaultInt.ToString(),
                    defaultBool = parameter.m_DefaultBool.ToString(),
                    defaultTrigger = parameter.m_DefaultTrigger.ToString()
                };
            }
            catch ( Exception e )
            {
                Debug.Log( e );
                throw;
            }
        }
    }
}