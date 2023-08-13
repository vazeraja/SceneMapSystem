using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class DragAndDrop : MouseManipulator
    {
        private struct DragData
        {
            public VisualElement DraggedObject;
            public VisualElement HoveredObject;
        }

        private static string s_DragDataType = "VisualElement";
        private SceneMapEditor m_Window;
        private bool m_GotMouseDown;

        public DragAndDrop( SceneMapEditor window )
        {
            m_Window = window;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>( OnMouseEnterEvent );
            target.RegisterCallback<MouseEnterEvent>( OnMouseLeaveEvent );

            target.RegisterCallback<MouseDownEvent>( OnMouseDownEvent );
            target.RegisterCallback<MouseMoveEvent>( OnMouseMoveEvent );
            target.RegisterCallback<MouseUpEvent>( OnMouseUpEvent );

            target.RegisterCallback<DragEnterEvent>( OnDragEnterEvent );
            target.RegisterCallback<DragLeaveEvent>( OnDragLeaveEvent );
            target.RegisterCallback<DragUpdatedEvent>( OnDragUpdatedEvent );
            target.RegisterCallback<DragPerformEvent>( OnDragPerformEvent );
            target.RegisterCallback<DragExitedEvent>( OnDragExitedEvent );
        }


        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>( OnMouseEnterEvent );
            target.UnregisterCallback<MouseEnterEvent>( OnMouseLeaveEvent );

            target.UnregisterCallback<MouseDownEvent>( OnMouseDownEvent );
            target.UnregisterCallback<MouseMoveEvent>( OnMouseMoveEvent );
            target.UnregisterCallback<MouseUpEvent>( OnMouseUpEvent );

            target.UnregisterCallback<DragEnterEvent>( OnDragEnterEvent );
            target.UnregisterCallback<DragLeaveEvent>( OnDragLeaveEvent );
            target.UnregisterCallback<DragUpdatedEvent>( OnDragUpdatedEvent );
            target.UnregisterCallback<DragPerformEvent>( OnDragPerformEvent );
            target.UnregisterCallback<DragExitedEvent>( OnDragExitedEvent );
        }


        private void OnDragEnterEvent( DragEnterEvent e )
        {
            var dragData = new DragData();

            var draggedElement = ( (DragData) UnityEditor.DragAndDrop.GetGenericData( s_DragDataType ) ).DraggedObject;
            var hoveredElement = (VisualElement) e.target;

            var draggedElementData = (ItemWrapperBase) draggedElement.userData;
            var hoveredElementData = (ItemWrapperBase) hoveredElement.userData;

            if ( draggedElementData.ItemType == hoveredElementData.ItemType && draggedElementData.ItemType == typeof( SceneReference ) )
            {
                var draggedData = ( (ItemWrapper<SceneReference>) draggedElementData ).Data;
                var hoveredData = ( (ItemWrapper<SceneReference>) hoveredElementData ).Data;

                if ( hoveredData.id != draggedData.id )
                {
                    // Debug.Log( "Hovered Element: " + hoveredData.name );

                    hoveredElement.style.borderBottomColor = GUIUtility.ListItemBorderHoverColor;

                    dragData.DraggedObject = draggedElement;
                    dragData.HoveredObject = hoveredElement;
                    UnityEditor.DragAndDrop.SetGenericData( s_DragDataType, dragData );
                }
            }

            if ( draggedElementData.ItemType == hoveredElementData.ItemType && draggedElementData.ItemType == typeof( SceneCollection ) )
            {
                var draggedData = ( (ItemWrapper<SceneCollection>) draggedElementData ).Data;
                var hoveredData = ( (ItemWrapper<SceneCollection>) hoveredElementData ).Data;

                if ( hoveredData.id != draggedData.id )
                {
                    // Debug.Log( "Hovered Element: " + hoveredData.name );

                    hoveredElement.style.borderBottomColor = GUIUtility.ListItemBorderHoverColor;

                    dragData.DraggedObject = draggedElement;
                    dragData.HoveredObject = hoveredElement;
                    UnityEditor.DragAndDrop.SetGenericData( s_DragDataType, dragData );
                }
            }
            
            if ( draggedElementData.ItemType == hoveredElementData.ItemType && draggedElementData.ItemType == typeof( SceneTransitionParameter ) )
            {
                var draggedData = ( (ItemWrapper<SceneTransitionParameter>) draggedElementData ).Data;
                var hoveredData = ( (ItemWrapper<SceneTransitionParameter>) hoveredElementData ).Data;

                if ( hoveredData.m_ID != draggedData.m_ID )
                {
                    // Debug.Log( "Hovered Element: " + hoveredData.name );

                    hoveredElement.style.borderBottomColor = GUIUtility.ListItemBorderHoverColor;

                    dragData.DraggedObject = draggedElement;
                    dragData.HoveredObject = hoveredElement;
                    UnityEditor.DragAndDrop.SetGenericData( s_DragDataType, dragData );
                }
            }
        }

        private void OnDragLeaveEvent( DragLeaveEvent evt )
        {
            var newData = new DragData();

            var dragData = (DragData) UnityEditor.DragAndDrop.GetGenericData( s_DragDataType );
            newData.DraggedObject = dragData.DraggedObject;

            if ( dragData.HoveredObject != null )
            {
                dragData.HoveredObject.style.borderBottomColor = GUIUtility.ListItemBorderColor;

                newData.HoveredObject = null;
                UnityEditor.DragAndDrop.SetGenericData( s_DragDataType, newData );
            }
        }

        private void OnDragUpdatedEvent( DragUpdatedEvent evt )
        {
            var data = UnityEditor.DragAndDrop.GetGenericData( s_DragDataType );
            if ( data != null )
            {
                UnityEditor.DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
        }

        private void OnDragPerformEvent( DragPerformEvent evt )
        {
            var dragData = (DragData) UnityEditor.DragAndDrop.GetGenericData( s_DragDataType );
            if ( dragData.HoveredObject != null && dragData.DraggedObject != null )
            {
                dragData.HoveredObject.style.borderBottomColor = GUIUtility.ListItemBorderColor;

                var hoveredElementUserData = (ItemWrapperBase) dragData.HoveredObject.userData;
                var draggedElementUserData = (ItemWrapperBase) dragData.DraggedObject.userData;

                if ( hoveredElementUserData.ItemType == typeof( SceneReference ) )
                {
                    var hoveredDataIndex = ( (ItemWrapper<SceneReference>) hoveredElementUserData ).Index;
                    var draggedDataIndex = ( (ItemWrapper<SceneReference>) draggedElementUserData ).Index;

                    m_Window.SwapListElements<SceneReference>(hoveredDataIndex, draggedDataIndex);
                }

                if ( hoveredElementUserData.ItemType == typeof( SceneCollection ) )
                {
                    var hoveredDataIndex = ( (ItemWrapper<SceneCollection>) hoveredElementUserData ).Index;
                    var draggedDataIndex = ( (ItemWrapper<SceneCollection>) draggedElementUserData ).Index;

                    m_Window.SwapListElements<SceneCollection>(hoveredDataIndex, draggedDataIndex);
                }
                
                if ( hoveredElementUserData.ItemType == typeof( SceneTransitionParameter ) )
                {
                    var hoveredDataIndex = ( (ItemWrapper<SceneTransitionParameter>) hoveredElementUserData ).Index;
                    var draggedDataIndex = ( (ItemWrapper<SceneTransitionParameter>) draggedElementUserData ).Index;

                    m_Window.SwapListElements<SceneTransitionParameter>(hoveredDataIndex, draggedDataIndex);
                }
            }
        }

        private void OnDragExitedEvent( DragExitedEvent evt )
        {
            var dragData = new DragData();
            UnityEditor.DragAndDrop.SetGenericData( s_DragDataType, dragData );
        }

        private void OnMouseEnterEvent( MouseEnterEvent evt ) { }

        private void OnMouseLeaveEvent( MouseEnterEvent evt ) { }

        void OnMouseDownEvent( MouseDownEvent e )
        {
            var targetElement = (VisualElement) e.target;
            if ( targetElement.ClassListContains("drag-handle__element"))
            {
                if ( !m_GotMouseDown && e.button == 0 && e.pressedButtons == 1 )
                {
                    m_GotMouseDown = true;
                }
            }
        }

        void OnMouseMoveEvent( MouseMoveEvent e )
        {
            if ( m_GotMouseDown && e.button == 0 && e.pressedButtons == 1 )
            {
                var dragData = new DragData();
                dragData.DraggedObject = (VisualElement) e.target;

                UnityEditor.DragAndDrop.PrepareStartDrag();
                UnityEditor.DragAndDrop.SetGenericData( s_DragDataType, dragData );
                UnityEditor.DragAndDrop.StartDrag( "StartDrag" );
                m_GotMouseDown = false;
            }
        }

        void OnMouseUpEvent( MouseUpEvent e )
        {
            if ( m_GotMouseDown && e.clickCount == 0 && e.button == 0 )
            {
                m_GotMouseDown = false;
                UnityEditor.DragAndDrop.AcceptDrag();
            }
        }
    }
}