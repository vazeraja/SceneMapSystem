using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TNS.InputMiddleware
{
    public class InputManager : MonoBehaviour //, IGlobalSystem
    {
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        private static void Bootstrap()
        {
            var go = Instantiate( Resources.Load( nameof( InputManager ) ) );
            if ( go == null ) throw new ApplicationException();

            go.name = nameof( InputManager );
            DontDestroyOnLoad( go );
        }

        public string SystemName => nameof( InputManager );

        private InputConfiguration InputConfiguration { get; set; }
        private InputProcessor InputProcessor { get; set; }

        private void Awake()
        {
            InputConfiguration = new InputConfiguration();
            InputProcessor = new InputProcessor();

            InputConfiguration.Player.SetCallbacks( InputProcessor );
        }

        private void OnEnable()
        {
            EnableInput();
            InputMiddlewareService.RegisterMiddleware( InputProcessor );
        }

        private void OnDisable()
        {
            DisableInput();
            InputMiddlewareService.UnregisterMiddleware( InputProcessor );
        }

        private void Update()
        {
            InputMiddlewareService.InputProvider.Process();
        }

        public void EnableInput()
        {
            InputConfiguration.Enable();
        }

        public void DisableInput()
        {
            InputConfiguration.Disable();
        }

        public static void ChangeCursor( Texture2D cursorType, bool useHotspot = false )
        {
            var w = cursorType.width / 2;
            var h = cursorType.height / 2;
            var hotspot = new Vector2( w, h );

            Cursor.SetCursor( cursorType, useHotspot ? Vector2.zero : hotspot, CursorMode.Auto );
        }
    }
}