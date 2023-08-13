using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TNS.SceneSystem
{
    [Serializable]
    internal struct ReadSceneSettingsJson
    {
        public string _Type; // enum
        public string _Mode; // enum
        public string _LoadingScenePath; // Scene (struct) - stored using its path
        public ReadLoadingParametersJson _LoadingParameters; // LoadingParameters (struct)

        public static SceneSettings ToSceneSettings( ReadSceneSettingsJson settingsJson )
        {
            return new SceneSettings
            {
                _Type = EnumHelpers.ConvertToEnum<SceneReferenceType>( settingsJson._Type ),
                _Mode = EnumHelpers.ConvertToEnum<LoadSceneMode>( settingsJson._Mode ),
                _LoadingScene = new Scene( settingsJson._LoadingScenePath ),
                _LoadingParameters = ReadLoadingParametersJson.ToLoadingParameters( settingsJson._LoadingParameters ),
            };
        }
    }
    
    [Serializable]
    internal struct WriteSceneSettingsJson
    {
        public string _Type; // enum
        public string _Mode; // enum
        public string _LoadingScenePath; // Scene (struct) - stored using its path
        public WriteLoadingParametersJson _LoadingParameters; // LoadingParameters (struct)
        
        public static WriteSceneSettingsJson FromSceneSettings( SceneSettings sceneSettings )
        {
            try {
                return new WriteSceneSettingsJson
                {
                    _Type = sceneSettings._Type.ToString(),
                    _Mode = sceneSettings._Mode.ToString(),
                    _LoadingScenePath = sceneSettings._LoadingScene.Path,
                    _LoadingParameters = WriteLoadingParametersJson.FromLoadingParameters(sceneSettings._LoadingParameters)
                };
            }
            catch ( Exception e ) {
                Debug.Log( e );
                throw;
            }
        }
    }
}