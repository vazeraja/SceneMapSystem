using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct SceneSettings
    {
        [SerializeField] internal SceneReferenceType _Type; // enum
        [SerializeField] internal LoadSceneMode _Mode; // enum
        [SerializeField] internal Scene _LoadingScene; // scene
        [SerializeField] internal LoadingParameters _LoadingParameters; // struct
    }
}