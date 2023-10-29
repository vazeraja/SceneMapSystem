using System;
using UnityEngine;

namespace TNS.InputMiddleware
{
    public interface IInputMiddleware
    {
        string Name { get; }
        bool Active { get; }
        int Priority { get; }

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

        InputState Process(InputState input);
    }
}