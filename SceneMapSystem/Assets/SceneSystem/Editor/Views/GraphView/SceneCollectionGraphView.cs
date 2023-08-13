#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneCollectionGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<SceneCollectionGraphView, UxmlTraits> { }

        public CollectionGraphTabView GraphTab { get; private set; }

        public SceneCollectionGraphView()
        {
            this.AddManipulator( new ContentDragger() );
            this.AddManipulator( new SelectionDragger() );
            this.AddManipulator( new RectangleSelector() );
            this.AddManipulator( new ClickSelector() );
            SetupZoom( ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale );

            Insert( 0, new GridBackground() );

            graphViewChanged = OnGraphViewChanged;
        }

        public void Initialize( CollectionGraphTabView graphTab )
        {
            GraphTab = graphTab;
        }

        public override void AddToSelection( ISelectable selectable )
        {
            base.AddToSelection( selectable );
            // if ( selectable is CustomEdgeControl edgeView ) {
            //     edgeView.Repaint();
            // }
        }

        public IEnumerator LoadGraphSequence( SceneCollection sceneCollection )
        {
            yield return ClearGraphElements();
            yield return AddNodes( sceneCollection );
            yield return AddEdges( sceneCollection );
            yield return AddEdgeSelectors();
            yield return SetupEvents();
            yield return FrameAllElements();
        }

        private IEnumerator FrameAllElements()
        {
            yield return null;

            FrameAll();
        }

        private IEnumerator SetupEvents()
        {
            foreach ( var selector in graphElements.ToList().OfType<CustomEdgeSelector>() )
            {
                selector.onLeftClicked += edge =>
                {
                    var fromScene = edge.FromNode.m_SceneReference;
                    var toScene = edge.ToNode.m_SceneReference;

                    GraphTab.OnTransitionLeftClicked( fromScene, toScene );
                };

                selector.onTriRightClicked += edge =>
                {
                    var fromScene = edge.FromNode.m_SceneReference;
                    var toScene = edge.ToNode.m_SceneReference;

                    GraphTab.OnTransitionRightClicked( fromScene, toScene );
                };
            }

            yield return null;
        }

        private IEnumerator AddNodes( SceneCollection sceneCollection )
        {
            foreach ( var scene in sceneCollection.scenes )
            {
                var node = new SceneNodeView( scene );
                if ( scene._NodePosition.IsEmpty() )
                {
                    scene._NodePosition = new Rect( new Vector2(), new Vector2( 100, 100 ) );
                }

                AddElement( node );
            }

            yield return null;
        }

        private IEnumerator AddEdges( SceneCollection sceneCollection )
        {
            foreach ( var sceneTransition in sceneCollection.sceneTransitions )
            {
                var edge = CreateEdge( sceneTransition.OriginID, sceneTransition.TargetID );
                AddElement( edge );

                GetNodeByGuid( sceneTransition.OriginID ).OnSelected();
                GetNodeByGuid( sceneTransition.TargetID ).OnSelected();
            }

            yield return null;
        }

        private CustomEdge CreateEdge( string startID, string endID )
        {
            var fromNode = (SceneNodeView) GetNodeByGuid( startID );
            var toNode = (SceneNodeView) GetNodeByGuid( endID );
            var customEdge = new CustomEdge( fromNode, toNode );

            return customEdge;
        }

        private IEnumerator AddEdgeSelectors()
        {
            foreach ( var customEdge in graphElements.ToList().OfType<CustomEdge>() )
            {
                customEdge.Repaint();
                AddElement( new CustomEdgeSelector( customEdge ) );
            }

            yield return null;

            foreach ( var selector in graphElements.ToList().OfType<CustomEdgeSelector>() )
            {
                selector.Repaint();
            }
        }

        private IEnumerator ClearGraphElements()
        {
            DeleteElements( graphElements.ToList() );
            yield return null;
        }

        private GraphViewChange OnGraphViewChanged( GraphViewChange gfc )
        {
            gfc.edgesToCreate?.ForEach( edge => { } );
            gfc.movedElements?.ForEach( element => { GraphTab.m_Window.SaveChangesToAsset(); } );

            return gfc;
        }

        public void UpdateEdgePositions( SceneNodeView nodeView )
        {
            foreach ( var customEdge in graphElements.ToList().OfType<CustomEdge>() )
            {
                if ( customEdge.FromNode.viewDataKey == nodeView.viewDataKey )
                {
                    customEdge.FromNode = nodeView;
                }

                if ( customEdge.ToNode.viewDataKey == nodeView.viewDataKey )
                {
                    customEdge.ToNode = nodeView;
                }
            }

            foreach ( var edgeSelector in graphElements.ToList().OfType<CustomEdgeSelector>() )
            {
                edgeSelector.Repaint();
            }
        }

        public override List<Port> GetCompatiblePorts( Port startPort, NodeAdapter nodeAdapter )
        {
            return ports.Where( port =>
                port.node != startPort.node &&
                port.direction != startPort.direction &&
                port.orientation == startPort.orientation
            ).ToList();
        }
    }
}

#endif