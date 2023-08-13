using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TNS.SceneSystem
{
    public static class GameObjectHelpers
    {
        public static void TryInstantiate( Object obj, out Object newObj )
        {
            newObj = Object.Instantiate( obj );
            if ( newObj == null ) {
                throw new ApplicationException();
            }
        }

        public static void DebugLogTime( object message, string color = "", int timePrecision = 3, bool displayFrameCount = true )
        {
            string callerObjectName = "";
            if ( displayFrameCount ) {
                callerObjectName = new StackTrace().GetFrame( 1 ).GetMethod().ReflectedType?.Name;
            }

            color = ( color == "" ) ? "#00FFFF" : color;

            // colors
            string colorPrefix = "";
            string colorSuffix = "";
            if ( !string.IsNullOrEmpty( color ) ) {
                colorPrefix = "<color=" + color + ">";
                colorSuffix = "</color>";
            }

            // build output
            string output = "";
            if ( displayFrameCount ) {
                output += "<color=#82d3f9>[f" + Time.frameCount + "]</color> ";
            }

            output += "<color=#f9a682>[" + FloatToTimeString( Time.time, false, true, true, true ) + "]</color> ";
            output += callerObjectName + " : ";
            output += colorPrefix + message + colorSuffix;

            // we output to the console
            Debug.Log( output );
        }

        public static string FloatToTimeString( float t, bool displayHours = false, bool displayMinutes = true, bool displaySeconds = true,
            bool displayMilliseconds = false )
        {
            int intTime = (int)t;
            int hours = intTime / 3600;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            int milliseconds = Mathf.FloorToInt( ( t * 1000 ) % 1000 );

            if ( displayHours && displayMinutes && displaySeconds && displayMilliseconds ) {
                return string.Format( "{0:00}:{1:00}:{2:00}.{3:D3}", hours, minutes, seconds, milliseconds );
            }

            if ( !displayHours && displayMinutes && displaySeconds && displayMilliseconds ) {
                return string.Format( "{0:00}:{1:00}.{2:D3}", minutes, seconds, milliseconds );
            }

            if ( !displayHours && !displayMinutes && displaySeconds && displayMilliseconds ) {
                return string.Format( "{0:D2}.{1:D3}", seconds, milliseconds );
            }

            if ( !displayHours && !displayMinutes && displaySeconds && !displayMilliseconds ) {
                return string.Format( "{0:00}", seconds );
            }

            if ( displayHours && displayMinutes && displaySeconds && !displayMilliseconds ) {
                return string.Format( "{0:00}:{1:00}:{2:00}", hours, minutes, seconds );
            }

            if ( !displayHours && displayMinutes && displaySeconds && !displayMilliseconds ) {
                return string.Format( "{0:00}:{1:00}", minutes, seconds );
            }

            return null;
        }
    }
}