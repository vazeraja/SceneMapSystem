using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

namespace TNS.InputMiddleware
{
    [Serializable]
    public class InputProcessor : InputConfiguration.IPlayerActions, IInputMiddleware
    {
        private InputState inputState;

        public string Name => "InputProcessor";
        public bool Active => true;
        public int Priority => 0;

        public event Action<Vector2> OnMovementChanged;
        public event Action<Vector2> OnMovementPressed;
        public event Action<Vector2> OnMovementReleased;
        public event Action<Vector2> OnMouseDeltaChanged;
        public event Action OnLeftClickPressed;
        public event Action OnLeftClickReleased;
        public event Action OnRightClickPressed;
        public event Action OnRightClickReleased;
        public event Action OnLeftShiftPressed;
        public event Action OnLeftShiftReleased;
        public event Action OnSpacebarPressed;
        public event Action OnSpacebarReleased;

        public InputState Process( InputState input )
        {
            input.Copy( inputState );

            return input;
        }

        public void OnMove( InputAction.CallbackContext value )
        {
            var movementDirection = value.ReadValue<Vector2>();

            inputState.movementDirection = movementDirection;

            OnMovementChanged?.Invoke( inputState.movementDirection );

            switch ( value.phase )
            {
                case InputActionPhase.Started:
                    OnMovementPressed?.Invoke( inputState.movementDirection );
                    break;
                case InputActionPhase.Canceled:
                    OnMovementReleased?.Invoke( inputState.movementDirection );
                    break;
            }

            // ReSharper disable once InvertIf
            // Determine whether SpriteRenderer should flipX or not
            if ( inputState.movementDirection.x != 0 )
            {
                var sign = (int) Mathf.Sign( inputState.movementDirection.x );
                inputState.shouldFlip = sign < 0;
            }
        }

        public void OnMouse( InputAction.CallbackContext value )
        {
            // (x,y) coordinates for screen size
            var mouseScreenPos = value.ReadValue<Vector2>();

            inputState.mouseScreenPosition = mouseScreenPos;
        }

        public void OnMouseDelta( InputAction.CallbackContext value )
        {
            var delta = value.ReadValue<Vector2>();

            inputState.mouseDeltaX = delta.x * 0.05f;
            inputState.mouseDeltaY = delta.y * 0.05f;

            OnMouseDeltaChanged?.Invoke( new Vector2( inputState.mouseDeltaX, inputState.mouseDeltaY ) );
        }

        public void OnMouseLeftClick( InputAction.CallbackContext value )
        {
            inputState.isLeftClickPressed = value.ReadValue<float>() > 0.5f;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ( value.phase )
            {
                case InputActionPhase.Started:
                    OnLeftClickPressed?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    OnLeftClickReleased?.Invoke();
                    break;
            }
        }


        public void OnMouseRightClick( InputAction.CallbackContext value )
        {
            inputState.isRightClickPressed = value.ReadValue<float>() > 0.5f;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ( value.phase )
            {
                case InputActionPhase.Started:
                    OnRightClickPressed?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    OnRightClickReleased?.Invoke();
                    break;
            }
        }

        public void OnLeftShift( InputAction.CallbackContext value )
        {
            inputState.isLeftShiftPressed = value.ReadValue<float>() > 0.5f;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ( value.phase )
            {
                case InputActionPhase.Started:
                    OnLeftShiftPressed?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    OnLeftShiftReleased?.Invoke();
                    break;
            }
        }

        public void OnSpacebar( InputAction.CallbackContext value )
        {
            inputState.isSpacebarPressed = value.ReadValue<float>() > 0.5f;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ( value.phase )
            {
                case InputActionPhase.Started:
                    OnSpacebarPressed?.Invoke();
                    inputState.lastTimeSpacebarPressed = Time.time;
                    break;
                case InputActionPhase.Canceled:
                    OnSpacebarReleased?.Invoke();
                    break;
            }
        }
    }
}