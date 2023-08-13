using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct LoadingParameters
    {
        public bool _UseLoadingScreen;
        public ThreadPriority _ThreadPriority;
        public bool _SecureLoad;
        public bool _Interpolate;
        public float _InterpolationSpeed;

        public LoadingParameters(LoadSceneMode mode)
        {
            _UseLoadingScreen = false;
            _ThreadPriority = ThreadPriority.High;
            _SecureLoad = true;
            _Interpolate = false;
            _InterpolationSpeed = 1f;
        }
    }
}