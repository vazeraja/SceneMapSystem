using System;
using System.Collections;
using System.Collections.Generic;
using TNS.SceneSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyNamespace
{
    enum AorB
    {
        A, B
    }
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_InputAction;
        [SerializeField] private SceneMapAsset m_SceneMap;
        [SerializeField] private AorB m_ID;

        private SceneReference demo1;
        private SceneReference demo2;
        
        private void Awake()
        {
            demo1 = m_SceneMap.FindScene( "Demo1" );
            demo2 = m_SceneMap.FindScene( "Demo2" );
            
            m_InputAction.action.Enable();
            m_InputAction.action.performed += ChangeScene;
        }

        private void ChangeScene( InputAction.CallbackContext context )
        {
            switch ( m_ID )
            {
                case AorB.A:
                    Debug.Log( "A" );
                    demo1.LoadSceneAsync();
                    break;
                case AorB.B:
                    Debug.Log( "B" );
                    demo2.LoadSceneAsync();
                    break;
            }
        }

        private void OnDisable()
        {
            m_InputAction.action.performed -= ChangeScene;
            m_InputAction.action.Disable();
        }
    }
}