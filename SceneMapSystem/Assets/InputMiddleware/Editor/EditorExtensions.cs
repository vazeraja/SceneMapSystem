using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TNS.InputMiddleware.Editor
{
    [InitializeOnLoad]
    public static class EditorExtensions
    {
        static EditorExtensions()
        {
            EditorApplication.playModeStateChanged -= SetupEvents;
            EditorApplication.playModeStateChanged += SetupEvents;
        }


        public static event Action EnteredEditMode;
        public static event Action ExitingEditMode;
        public static event Action EnteredPlayMode;
        public static event Action ExitingPlayMode;

        private static void SetupEvents( PlayModeStateChange state )
        {
            switch ( state ) {
                case PlayModeStateChange.ExitingEditMode:
                    ExitingEditMode?.Invoke();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EnteredPlayMode?.Invoke();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    ExitingPlayMode?.Invoke();
                    break;
                case PlayModeStateChange.EnteredEditMode: 
                    EnteredEditMode?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( state ), state, null );
            }
        }
    }
}