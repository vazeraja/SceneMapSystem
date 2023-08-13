#if UNITY_EDITOR

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public sealed class SceneNodeView : Node
    {
        internal readonly SceneReference m_SceneReference;

        private SceneCollectionGraphView m_GraphView;
        private SceneCollectionGraphView GraphView => m_GraphView ??= GetFirstAncestorOfType<SceneCollectionGraphView>();

        private Port m_InputPort;
        private Port m_OutputPort;

        public SceneNodeView(SceneReference sceneReference ) : base( GUIUtility.NodeViewUxmlPath )
        {
            m_SceneReference = sceneReference;
            title = sceneReference.name;
            name = sceneReference.name;
            viewDataKey = sceneReference.id;
            style.left = sceneReference._NodePosition.x;
            style.top = sceneReference._NodePosition.y;
            
            AddToClassList( "simpleAnimation" );
            
            // var inputPort = InstantiatePort( Orientation.Horizontal, Direction.Input, Port.Capacity.Single, null );
            // var outputPort = InstantiatePort( Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null );
            // inputPort.visible = false;
            // outputPort.visible = false;
            // inputContainer.Add(inputPort);
            // outputContainer.Add(outputPort);
        }

        public override void BuildContextualMenu( ContextualMenuPopulateEvent evt )
        {
            // base.BuildContextualMenu( evt );
            var scenesInCollection = m_SceneReference.collection.scenes;
            foreach ( var scene in scenesInCollection ) {
                var startScene = m_SceneReference;
                var targetScene = scene;
                
                if ( targetScene.id == m_SceneReference.id ) continue;

                evt.menu.AppendAction( $"Add Transition To/{targetScene.name}", action =>
                {
                    GraphView.GraphTab.AddTransition(startScene, targetScene);
                } );
            }
        }

        public override void SetPosition( Rect newPos )
        {
            base.SetPosition( newPos );
            GraphView.UpdateEdgePositions(this);
            m_SceneReference._NodePosition = new Rect( new Vector2( newPos.xMin, newPos.yMin ), new Vector2( 0, 0 ) );
        }
    }
}

#endif