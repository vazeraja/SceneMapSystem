using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TNS.InputMiddleware
{
    [CreateAssetMenu( fileName = "InputProvider", menuName = "InputProvider", order = 0 )]
    public class InputProvider : ScriptableObject
    {
        private InputState inputState;

        public event Action<Vector2> OnMovementChanged;
        public event Action<Vector2> OnMovementPressed;
        public event Action<Vector2> OnMovementReleased;
        public event Action<Vector2> OnMouseDeltaChanged;
        public event Action OnSpacebarPressed;
        public event Action OnSpacebarReleased;
        public event Action OnLeftClickPressed;
        public event Action OnLeftClickReleased;
        public event Action OnRightClickPressed;
        public event Action OnRightClickReleased;
        public event Action OnLeftShiftPressed;
        public event Action OnLeftShiftReleased;

        public static implicit operator InputState( InputProvider provider ) => provider.GetState();

        public void Awake()
        {
            Clear();
        }

        public void Clear()
        {
            OnMovementChanged = null;
            OnMovementPressed = null;
            OnMovementReleased = null;
            OnMouseDeltaChanged = null;
            OnSpacebarPressed = null;
            OnSpacebarReleased = null;
            OnLeftClickPressed = null;
            OnLeftClickReleased = null;
            OnRightClickPressed = null;
            OnRightClickReleased = null;
            OnLeftShiftPressed = null;
            OnLeftShiftReleased = null;
        }

        // ---------- Used in the InputMiddlewareSet inspector UnityEvent ----------
        public void RegisterProviderEvents( IInputMiddleware middleware )
        {
            // Subscribe Movement Events
            
            middleware.OnMovementChanged += BroadcastMovementChanged;
            middleware.OnMovementPressed += BroadcastMovementPressed;
            middleware.OnMovementReleased += BroadcastMovementReleased;
            middleware.OnMouseDeltaChanged += BroadcastMouseDeltaChanged;

            // Subscribe Spacebar Events
            middleware.OnSpacebarPressed += BroadcastSpacebarPressed;
            middleware.OnSpacebarReleased += BroadcastSpacebarReleased;

            // Subscribe LeftClick Events
            middleware.OnLeftClickPressed += BroadcastLeftClickPressed;
            middleware.OnLeftClickReleased += BroadcastLeftClickReleased;

            // Subscribe RightClick Events
            middleware.OnRightClickPressed += BroadcastRightClickPressed;
            middleware.OnRightClickReleased += BroadcastRightClickReleased;

            // Subscribe LeftShift Events
            middleware.OnLeftShiftPressed += BroadcastLeftShiftPressed;
            middleware.OnLeftShiftReleased += BroadcastLeftShiftReleased;
        }

        public void UnregisterProviderEvents( IInputMiddleware middleware )
        {
            middleware.OnMovementChanged += BroadcastMovementChanged;
            middleware.OnMovementPressed -= BroadcastMovementPressed;
            middleware.OnMovementReleased -= BroadcastMovementReleased;
            middleware.OnMouseDeltaChanged += BroadcastMouseDeltaChanged;
            
            middleware.OnSpacebarPressed -= BroadcastSpacebarPressed;
            middleware.OnSpacebarReleased -= BroadcastSpacebarReleased;

            middleware.OnLeftClickPressed -= BroadcastLeftClickPressed;
            middleware.OnLeftClickReleased -= BroadcastLeftClickReleased;

            middleware.OnRightClickPressed -= BroadcastRightClickPressed;
            middleware.OnRightClickReleased -= BroadcastRightClickReleased;

            middleware.OnLeftShiftPressed -= BroadcastLeftShiftPressed;
            middleware.OnLeftShiftReleased -= BroadcastLeftShiftReleased;
        }

        private void BroadcastSpacebarPressed()
        {
            if ( !inputState.disableSpacebar )
            {
                OnSpacebarPressed?.Invoke();
            }
            else
            {
                Debug.Log( "Spacebar disabled" );
            }
        }

        private void BroadcastSpacebarReleased()
        {
            if ( !inputState.disableSpacebar )
            {
                OnSpacebarReleased?.Invoke();
            }
            else
            {
                Debug.Log( "Spacebar disabled" );
            }
        }

        // TODO: Change these functions so events invoke only if allowed similar to spacebar
        private void BroadcastMovementChanged( Vector2 value ) => OnMovementChanged?.Invoke( value );
        private void BroadcastMovementPressed( Vector2 value ) => OnMovementPressed?.Invoke( value );
        private void BroadcastMovementReleased( Vector2 value ) => OnMovementReleased?.Invoke( value );
        private void BroadcastMouseDeltaChanged( Vector2 value ) => OnMouseDeltaChanged?.Invoke( value );
        private void BroadcastLeftClickPressed() => OnLeftClickPressed?.Invoke();
        private void BroadcastLeftClickReleased() => OnLeftClickReleased?.Invoke();
        private void BroadcastRightClickPressed() => OnRightClickPressed?.Invoke();
        private void BroadcastRightClickReleased() => OnRightClickReleased?.Invoke();
        private void BroadcastLeftShiftPressed() => OnLeftShiftPressed?.Invoke();
        private void BroadcastLeftShiftReleased() => OnLeftShiftReleased?.Invoke();

        public InputState GetState() => inputState;
        public void SetState( InputState state ) => inputState = state;

        internal void Process()
        {
            inputState.Reset();

            foreach ( var middleware in InputMiddlewareService.InputMiddlewareSet.GetSortedSet() )
            {
                inputState = middleware.Process( inputState );
            }
        }
    }
}