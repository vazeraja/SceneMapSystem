using UnityEngine;

namespace TNS.InputMiddleware
{
    public static class InputMiddlewareService
    {
        private static InputMiddlewareSet _inputMiddlewareSet;
        public static InputMiddlewareSet InputMiddlewareSet
        {
            get
            {
                if ( _inputMiddlewareSet == null )
                    _inputMiddlewareSet = Resources.Load<InputMiddlewareSet>( "InputMiddlewareSet" );

                return _inputMiddlewareSet;
            }
        }

        private static InputProvider _inputProvider;
        public static InputProvider InputProvider
        {
            get
            {
                if ( _inputProvider == null )
                    _inputProvider = Resources.Load<InputProvider>( "InputProvider" );

                return _inputProvider;
            }
        }

        public static void RegisterMiddleware( IInputMiddleware middleware )
        {
            InputMiddlewareSet.AddMiddleware( middleware );
        }

        public static void UnregisterMiddleware( IInputMiddleware middleware )
        {
            InputMiddlewareSet.RemoveMiddleware( middleware );
        }
    }
}