#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class RibbonTabsView
    {
        private readonly VisualElement m_Root;
        private readonly SceneMapEditorWindow m_Window;
        
        private readonly VisualElement m_RibbonContainer;
        private readonly Ribbon m_TabsRibbon;
        private readonly VisualElement[] m_Tabs;
        private int m_CurrentTab;

        private readonly VisualElement m_OptionsAndInfos;
        private readonly Button m_Menu;
        private readonly Button m_Help;
        public event Action HelpClicked = delegate { };
        public event Action MenuClicked = delegate { };
        
        public MapSettingsTabView MapSettingsTab { get; private set; }
        public CollectionGraphTabView CollectionGraphTab { get; private set; }
        
        public RibbonTabsView(SceneMapEditorWindow window, VisualElement root )
        {
            m_Root = root;
            m_Window = window;
            
            m_RibbonContainer = root.Q<VisualElement>( GUIUtility.RibbonContainer );
            m_TabsRibbon = root.Q<Ribbon>( GUIUtility.RibbonTabs );
            m_CurrentTab = m_TabsRibbon.InitialOption;
            m_Tabs = new VisualElement[3];
            m_Tabs[0] = root.Q<VisualElement>( GUIUtility.SettingsTab );
            m_Tabs[1] = root.Q<VisualElement>( GUIUtility.GraphTab );
            m_Tabs[2] = root.Q<VisualElement>( GUIUtility.ExtraSettingsTab );
            m_TabsRibbon.Clicked += ChangeTab;

            MapSettingsTab = new MapSettingsTabView( window, m_Tabs[0] );
            CollectionGraphTab  = new CollectionGraphTabView( window, m_Tabs[1] );

            // Hide other tabs
            for ( int i = 0; i < m_Tabs.Length; i++ ) {
                if ( i != m_TabsRibbon.InitialOption )
                    m_Tabs[i].RemoveFromHierarchy();
            }
            
            m_OptionsAndInfos = root.Q( "options-and-info" );
            m_Menu = m_OptionsAndInfos.Q<Button>( "", GUIUtility.MenuIconButtonClass );
            m_Help = m_OptionsAndInfos.Q<Button>( "", GUIUtility.HelpIconButtonClass );
            m_Help.tooltip = GUIUtility.OpenManualTooltip;
            
            GUIUtility.SetVisibility( m_Help, true );
            m_Help.clickable.clicked += HelpClicked;
            GUIUtility.SetVisibility( m_Menu, true );
            m_Menu.clickable.clicked += MenuClicked;
        }
        
        private void ChangeTab( int index )
        {
            if ( index == m_CurrentTab )
                return;
            
            m_Tabs[m_CurrentTab].RemoveFromHierarchy();
            m_RibbonContainer.Add( m_Tabs[index] );
            m_RibbonContainer.MarkDirtyRepaint();
            m_CurrentTab = index;
        }
    }
}

#endif