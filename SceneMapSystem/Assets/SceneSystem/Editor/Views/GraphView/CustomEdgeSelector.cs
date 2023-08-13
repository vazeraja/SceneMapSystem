#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class CustomEdgeSelector : GraphElement
    {
        private readonly CustomEdge m_Edge;

        private readonly CustomClickElement m_TriElement;
        private Vector2 m_TriPosition;
        
        public event Action<CustomEdge> onTriRightClicked
        {
            add => m_TriElement.onRightClick += value;
            remove => m_TriElement.onRightClick -= value;
        }
        public event Action<CustomEdge> onLeftClicked
        {
            add => m_TriElement.onLeftClick += value;
            remove => m_TriElement.onLeftClick -= value;
        }
        
        public CustomEdgeSelector(CustomEdge edge)
        {
            m_Edge = edge;
            style.position = new StyleEnum<Position>( Position.Absolute );

            Add(m_TriElement = new CustomClickElement( edge ));
            
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Repaint()
        {
            MarkDirtyRepaint();
            UpdateClickPosition();
        }
        
        public void UpdateClickPosition()
        {
            m_TriElement?.SetPosition( new Rect( m_TriPosition - new Vector2( 10, 10 ), new Vector2( 0, 0 ) ) );
            m_TriElement?.MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent( MeshGenerationContext context )
        {
            var painter = context.painter2D;

            painter.lineWidth = 2.5f;
            painter.lineCap = LineCap.Round;

            // Draw multiple triangles inside each other because there is no way to fill shapes from multiple lines
            Vector3[] triangle = { new Vector2( -1, 0.5f ) * ( 8 ), new Vector2( 1, 0.5f ) * ( 8 ), new Vector2( 0, -0.5f ) * ( 28 ) };
            Vector3[] triangle2 = { new Vector2( -1, 0.5f ) * ( 6 ), new Vector2( 1, 0.5f ) * ( 6 ), new Vector2( 0, -0.5f ) * ( 21f ) };
            Vector3[] triangle3 = { new Vector2( -1, 0.5f ) * ( 4 ), new Vector2( 1, 0.5f ) * ( 4 ), new Vector2( 0, -0.5f ) * ( 14f ) };
            Vector3[] triangle4 = { new Vector2( -1, 0.5f ) * ( 2 ), new Vector2( 1, 0.5f ) * ( 2 ), new Vector2( 0, -0.5f ) * ( 7f ) };
            
            Painter.RotateTowards( triangle2, m_Edge.From, m_Edge.To );
            Painter.RotateTowards( triangle3, m_Edge.From, m_Edge.To );
            Painter.RotateTowards( triangle4, m_Edge.From, m_Edge.To );
            
            Painter.PaintTriangle( painter, triangle2, Color.white);
            Painter.PaintTriangle( painter, triangle3, Color.white);
            Painter.PaintTriangle( painter, triangle4, Color.white);

            m_TriPosition = triangle4[2];
        }
    }
}

#endif