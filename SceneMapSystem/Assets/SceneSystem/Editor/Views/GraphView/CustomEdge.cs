#if UNITY_EDITOR

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class CustomEdge : GraphElement
    {
        private SceneNodeView m_FromNode;
        public SceneNodeView FromNode
        {
            get => m_FromNode;
            set
            {
                m_FromNode = value;

                var startNodeRect = value.GetPosition();
                var startPos = startNodeRect.center - new Vector2( 15, 0 );
                From = new Vector2Int((int)startPos.x, (int)startPos.y);

                Repaint();
            }
        }

        private SceneNodeView m_ToNode;
        public SceneNodeView ToNode
        {
            get => m_ToNode;
            set
            {
                m_ToNode = value;

                var endPos = value.GetPosition().center - new Vector2( 15, 0 );
                To = new Vector2Int((int)endPos.x, (int)endPos.y);

                Repaint();
            }
        }

        public Vector2Int From { get; private set; }
        public Vector2Int To { get; private set; }

        public CustomEdge( SceneNodeView fromNode, SceneNodeView toNode )
        {
            name = $"{fromNode.name} --> {toNode.name}";
            style.position = Position.Absolute;
            
            FromNode = fromNode;
            ToNode = toNode;

            generateVisualContent = OnGenerateVisualContent;
        }

        public void Repaint()
        {
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent( MeshGenerationContext context )
        {
            var painter = context.painter2D;

            painter.lineWidth = 2.5f;
            painter.lineCap = LineCap.Round;
            Painter.PaintLine( painter, From, To, Color.grey );
        }
    }
}
#endif