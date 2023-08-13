using System;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    internal struct ReadLoadingParametersJson
    {
        public string useLoadingScreen; // bool
        public string threadPriority; // enum
        public string secureLoad; // bool
        public string interpolate; // bool
        public string interpolationSpeed; // float

        public static LoadingParameters ToLoadingParameters( ReadLoadingParametersJson parametersJson )
        {
            float interpolationSpeed = 1f;
            if ( !string.IsNullOrEmpty( parametersJson.interpolationSpeed ) ) {
                interpolationSpeed = float.Parse( parametersJson.interpolationSpeed );
            }

            return new LoadingParameters
            {
                _UseLoadingScreen = !string.IsNullOrEmpty( parametersJson.useLoadingScreen ) && bool.Parse( parametersJson.useLoadingScreen ),
                _ThreadPriority = EnumHelpers.ConvertToEnum<ThreadPriority>( parametersJson.threadPriority ),
                _SecureLoad = !string.IsNullOrEmpty( parametersJson.secureLoad ) && bool.Parse( parametersJson.secureLoad ),
                _Interpolate = !string.IsNullOrEmpty( parametersJson.interpolate ) && bool.Parse( parametersJson.interpolate ),
                _InterpolationSpeed = interpolationSpeed
            };
        }
    }

    [Serializable]
    internal struct WriteLoadingParametersJson
    {
        public string useLoadingScreen; // bool
        public string threadPriority; // enum
        public string secureLoad; // bool
        public string interpolate; // bool
        public string interpolationSpeed; // float

        public static WriteLoadingParametersJson FromLoadingParameters( LoadingParameters parameters )
        {
            try {
                return new WriteLoadingParametersJson
                {
                    useLoadingScreen = parameters._UseLoadingScreen.ToString(),
                    threadPriority = parameters._ThreadPriority.ToString(),
                    secureLoad = parameters._SecureLoad.ToString(),
                    interpolate = parameters._Interpolate.ToString(),
                    interpolationSpeed = parameters._InterpolationSpeed.ToString(),
                };
            }
            catch ( Exception e ) {
                Debug.Log( e );
                throw;
            }
        }
    }
}