using System;
using UnityEngine;

namespace TNS.InputMiddleware
{
    [Serializable]
    public struct InputState
    {
        public float mouseDeltaX;
        public float mouseDeltaY;
        
        public Vector2 movementDirection;
        public Vector2 mouseScreenPosition;

        public bool isLeftClickPressed;
        public bool isRightClickPressed;
        public bool isLeftShiftPressed;
        public bool isSpacebarPressed;

        public float lastTimeSpacebarPressed;

        public bool shouldFlip;

        // Do not copy these
        [HideInInspector] public bool disableSpacebar;
        [HideInInspector] public bool disableLeftClick;
        [HideInInspector] public bool disableRightClick;
        [HideInInspector] public bool disableLeftShift;

        public void Copy(InputState copy)
        {
            mouseDeltaX = copy.mouseDeltaX;
            mouseDeltaY = copy.mouseDeltaY;
            
            movementDirection = copy.movementDirection;
            mouseScreenPosition = copy.mouseScreenPosition;
            
            isLeftClickPressed = copy.isLeftClickPressed;
            isRightClickPressed = copy.isRightClickPressed;
            isLeftShiftPressed = copy.isLeftShiftPressed;
            isSpacebarPressed = copy.isSpacebarPressed;
            
            lastTimeSpacebarPressed = copy.lastTimeSpacebarPressed;
            
            shouldFlip = copy.shouldFlip;
            
            disableSpacebar = copy.disableSpacebar;
            disableLeftClick = copy.disableLeftClick;
            disableRightClick = copy.disableRightClick;
            disableLeftShift = copy.disableLeftShift;
        }

        public void Reset()
        {
            movementDirection = Vector2.zero;
            mouseScreenPosition = Vector2.zero;

            isLeftClickPressed = false;
            isRightClickPressed = false;
            isLeftShiftPressed = false;
            isSpacebarPressed = false;
            
            lastTimeSpacebarPressed = 0;
            
            shouldFlip = false;

            disableSpacebar = true;
            disableLeftClick = true;
            disableRightClick = true;
            disableLeftShift = true;
        }
    }
}