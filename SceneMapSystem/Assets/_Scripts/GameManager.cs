using System;
using System.Collections;
using System.Collections.Generic;
using TNS.SceneSystem;
using UnityEngine;

namespace MyNamespace
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SceneMapAsset m_SceneMap;

        private void Awake()
        {
            var demo1 = m_SceneMap.FindScene( "Demo1" );
            Debug.Log( demo1.name );
        }
    }
}