using System.Collections;
using UnityEngine;

namespace TNS.SceneSystem
{
    public static class CoroutineHelpers
    {
        public static CoroutineHandle RunCoroutine( MonoBehaviour owner, IEnumerator coroutine )
        {
            return new CoroutineHandle( owner, coroutine );
        }
        
        public static IEnumerator WaitForFrames( int frameCount )
        {
            while ( frameCount > 0 ) {
                frameCount--;
                yield return null;
            }
        }
    }
}