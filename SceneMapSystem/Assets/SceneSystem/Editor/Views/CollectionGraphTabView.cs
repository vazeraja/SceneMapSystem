#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class CollectionGraphTabView
    {
        private readonly VisualElement m_Content;
        private readonly SceneCollectionGraphView m_GraphView;
        private readonly Label m_GraphViewLabel;

        private readonly VisualElement m_TabsContainer;
        private readonly ToolbarToggle m_GraphDetailsTabButton;
        private readonly ToolbarToggle m_GraphParametersTabButton;
        private readonly ToolbarToggle [] m_TabButtons;
        private const int initialOption = 1;
        private readonly Action<int> m_OnTabButtonChanged;
        private readonly VisualElement [] m_Tabs;
        private ToolbarToggle m_CurrentTab;
        private int m_CurrentTabIndex;

        internal SceneCollectionParameterView m_ParameterView { get; private set; }

        public readonly SceneMapEditorWindow m_Window;
        public SceneCollection SelectedCollection => m_Window.SelectedCollection;

        public Rect ContentRect => m_Content.contentRect;


        public CollectionGraphTabView( SceneMapEditorWindow window, VisualElement root )
        {
            m_Window = window;
            m_GraphView = root.Q<SceneCollectionGraphView>();
            m_Content = root.Q<VisualElement>( "content" );
            m_TabsContainer = root.Q<VisualElement>( GUIUtility.TabsContainer );
            m_GraphDetailsTabButton = root.Q<ToolbarToggle>( GUIUtility.GraphDetailsToggle );
            m_GraphParametersTabButton = root.Q<ToolbarToggle>( GUIUtility.GraphParametersToggle );
            m_GraphViewLabel = root.Q<Label>( GUIUtility.GraphViewLabel );
            m_GraphView.Initialize( this );

            m_TabButtons = new ToolbarToggle[2];
            m_TabButtons[0] = m_GraphDetailsTabButton;
            m_TabButtons[1] = m_GraphParametersTabButton;
            m_TabButtons[initialOption].SetValue( true );
            m_CurrentTab = m_TabButtons[initialOption];
            m_CurrentTabIndex = initialOption;
            m_Tabs = new VisualElement[2];
            m_Tabs[0] = root.Q<VisualElement>( GUIUtility.GraphDetailsTab );
            m_Tabs[1] = root.Q<VisualElement>( GUIUtility.GraphParametersTab );
            m_OnTabButtonChanged += ChangeTab;

            // Hide other tabs
            for ( var i = 0; i < m_Tabs.Length; i++ )
                if ( i != initialOption )
                    m_Tabs[i].RemoveFromHierarchy();

            // Register button callbacks
            foreach ( var tab in m_TabButtons )
                tab.RegisterCallback<MouseUpEvent>( OnTabPressed );

            GUIUtility.Events.CollectionSelected += OnCollectionSelected;
            GUIUtility.Events.SceneReferenceCreated += _ => LoadGraph( SelectedCollection );
            GUIUtility.Events.SceneReferenceRemoved += _ => LoadGraph( SelectedCollection );

            m_ParameterView = new SceneCollectionParameterView( window, root.Q<ListView>( GUIUtility.ParametersListView ) );
        }


        private void OnCollectionSelected( int index )
        {
            var sceneCollectionsProp = m_Window.SerializedSceneMap.FindProperty( "_SceneCollections" );
            var collectionProp = sceneCollectionsProp.GetArrayElementAtIndex( index );

            var collectionName = collectionProp.FindPropertyRelative( "_Name" ).stringValue;

            SetLabel( collectionName );
            LoadGraph( SelectedCollection );
        }

        private void SetLabel( string text )
        {
            m_GraphViewLabel.text = text;
        }

        private void LoadGraph( SceneCollection sceneCollection )
        {
            m_Window.StartCoroutine( m_GraphView.LoadGraphSequence( sceneCollection ) );
            m_ParameterView.Rebuild( sceneCollection.parameters.ToList() );
        }

        public void OnTransitionLeftClicked( SceneReference origin, SceneReference target )
        {
            var transition = SelectedCollection.sceneTransitions.First( t => t.m_OriginID == origin.id && t.m_TargetID == target.id );
            var index = SelectedCollection.sceneTransitions.IndexOfReference( transition );

            GUIUtility.Events.TriggerTransitionSelected( transition );
        }

        public void OnTransitionRightClicked( SceneReference origin, SceneReference target )
        {
            var menu = new GenericMenu();

            var transition = SelectedCollection.sceneTransitions.First( t => t.m_OriginID == origin.id && t.m_TargetID == target.id );

            menu.AddItem( new GUIContent( "Remove" ), false, () => RemoveTransition( transition ) );
            menu.ShowAsContext(); 
        }

        public void AddTransition( SceneReference startScene, SceneReference targetScene )
        {
            SelectedCollection.AddTransition( startScene, targetScene, out var transition );

            if ( transition != null )
                GUIUtility.Events.TriggerTransitionCreated( transition );

            // Execution timing issue
            EditorApplication.delayCall += () => { LoadGraph( SelectedCollection ); };
        }

        public void RemoveTransition( SceneTransition transition )
        {
            if ( !SelectedCollection.RemoveTransition( transition ) )
            {
                return;
            }

            GUIUtility.Events.TriggerTransitionRemoved( transition );

            // Execution timing issue
            EditorApplication.delayCall += () => { LoadGraph( SelectedCollection ); };
        }

        public void ShowTransitionGUI() { }

        private void ChangeTab( int index )
        {
            if ( index == m_CurrentTabIndex )
                return;

            m_Tabs[m_CurrentTabIndex].RemoveFromHierarchy();
            m_TabsContainer.Add( m_Tabs[index] );
            m_TabsContainer.MarkDirtyRepaint();
            m_CurrentTabIndex = index;
        }

        private void OnTabPressed( MouseUpEvent evt )
        {
            var clickedTab = (ToolbarToggle) evt.target;

            if ( m_CurrentTab == clickedTab )
            {
                m_CurrentTab.value = true;
                return;
            }

            m_CurrentTab = clickedTab;
            m_CurrentTab.value = true;
            m_OnTabButtonChanged?.Invoke( m_TabButtons.ToList().IndexOf( clickedTab ) );

            foreach ( var tab in m_TabButtons.Where( tab => m_CurrentTab != tab ) )
            {
                tab.value = false;
            }
        }
    }
}

#endif