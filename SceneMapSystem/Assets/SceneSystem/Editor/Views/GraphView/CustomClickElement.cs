#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class CustomClickElement : GraphElement
    {
        public Action<CustomEdge> onRightClick;
        public Action<CustomEdge> onLeftClick;

        public CustomClickElement(CustomEdge edge)
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( GUIUtility.CircleTemplateUxmlPath ).CloneTree( this );
            name = "CustomClickElement";
            style.minWidth = 21f;
            style.minHeight = 21f;
            style.position = Position.Absolute;

            var cEvent = new Clickable( evt =>
            {
                if ( evt == null ) throw new ArgumentNullException( nameof( evt ) );
                var a = ( evt.currentTarget as VisualElement );

                onRightClick?.Invoke( edge );
            } );
            cEvent.activators.Clear();
            cEvent.activators.Add( new ManipulatorActivationFilter { button = MouseButton.RightMouse } );
            this.AddManipulator( cEvent );

            var cEvent2 = new Clickable( evt =>
            {
                if ( evt == null ) throw new ArgumentNullException( nameof( evt ) );
                var a = ( evt.currentTarget as VisualElement );

                onLeftClick?.Invoke( edge );
            } );
            this.AddManipulator( cEvent2 );
        }
    }
}

#endif