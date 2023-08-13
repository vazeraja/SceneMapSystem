#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class UnityStyleTreeView<T>
    {
        private readonly TreeView m_TreeView;
        private IList<TreeViewItemData<T>> m_Items;

        private VisualElement m_SelectedTreeElement;
        private SelectionType m_SelectionType;

        internal Action<VisualElement> m_ItemRightClicked;
        internal Action<VisualElement> m_ItemLeftClicked;
        internal Action<VisualElement> m_ItemFoldoutClicked;
        internal Action m_PanelLeftClicked;

        private readonly Color m_ZeroAlphaColor = new Color32( 0, 0, 0, 0 );
        private readonly Color m_SelectedItemColor = new Color32( 44, 93, 135, 255 );
        private readonly Color m_ListItemBorderColor = new Color32( 36, 36, 36, 255 );
        private static Texture2D FoldoutIconClosed => (Texture2D)EditorGUIUtility.IconContent( "d_IN_foldout" ).image;
        private static Texture2D FoldoutIconOpen => (Texture2D)EditorGUIUtility.IconContent( "d_IN_foldout_on" ).image;

        public int selectedItemIndex { get; private set; }
        public T selectedItem { get; private set; }

        public SelectionType selectionType
        {
            get => m_SelectionType;
            set
            {
                m_SelectionType = value;
                Rebuild( m_Items );
            }
        }

        protected UnityStyleTreeView( TreeView treeView )
        {
            m_TreeView = treeView;
            m_SelectionType = SelectionType.Single;

            m_TreeView.makeItem = MakeSceneProfileTreeViewItem;
            m_TreeView.bindItem = BindSceneProfileTreeViewItem;
            m_TreeView.selectionType = m_SelectionType;

            var sceneProfilePanelContentContainer = treeView.Q( "unity-content-and-vertical-scroll-container" );
            sceneProfilePanelContentContainer.AddManipulator( new Clickable( evt => m_PanelLeftClicked?.Invoke() ) );

            m_ItemLeftClicked += OnItemLeftClicked;
            m_ItemFoldoutClicked += OnItemFoldoutClicked;
            m_PanelLeftClicked += PanelLeftClicked;
        }


        public void Rebuild( IList<TreeViewItemData<T>> items )
        {
            m_Items = items;
            m_TreeView.SetRootItems( items );
            m_TreeView.Rebuild();
        }

        private void PanelLeftClicked()
        {
            if ( m_TreeView.selectionType != SelectionType.None ) m_TreeView.ClearSelection();
        }

        private void OnItemLeftClicked( VisualElement ele )
        {
            var itemWrapper = (ItemWrapper<T>)ele.userData;

            SetSelection( ele );
            SetSelectedItem( itemWrapper.Data );
            selectedItemIndex = itemWrapper.Index;
        }

        private void OnItemFoldoutClicked( VisualElement ele )
        {
            var itemWrapper = (ItemWrapper<string>)ele.userData;
            ToggleExpandedState( itemWrapper.ID, ele.Q( "dropDownImage" ) );
        }

        private void SetSelectedItem( T data )
        {
            selectedItem = data;
        }

        private void SetSelection( VisualElement ele )
        {
            m_SelectedTreeElement?.SetBackgroundColor( m_ZeroAlphaColor );
            m_SelectedTreeElement = ele;
            m_SelectedTreeElement.SetBackgroundColor( m_SelectedItemColor );
        }

        private void ToggleExpandedState( int id, VisualElement icon )
        {
            if ( !m_TreeView.IsExpanded( id ) ) {
                m_TreeView.ExpandItem( id );
                icon.SetBackgroundImage( FoldoutIconOpen );
            }
            else {
                m_TreeView.CollapseItem( id );
                icon.SetBackgroundImage( FoldoutIconClosed );
            }
        }

        private void BindSceneProfileTreeViewItem( VisualElement container, int i )
        {
            var label = container.Q<Label>( "element-label" );

            var id = m_TreeView.GetIdForIndex( i );
            var data = m_TreeView.GetItemDataForIndex<T>( i );

            var itemWrapper = new ItemWrapper<T>();
            itemWrapper.Data = data;
            itemWrapper.ID = id;
            itemWrapper.Index = i;

            container.userData = itemWrapper;
            label.text = data.ToString();
        }

        private VisualElement MakeSceneProfileTreeViewItem()
        {
            var container = new VisualElement();
            container.name = "treeViewItemContainer";
            container.style.flexDirection = new StyleEnum<FlexDirection>( FlexDirection.Row );
            container.style.height = 25f;
            container.style.marginLeft = -18f;
            container.style.borderBottomColor = m_ListItemBorderColor;
            container.style.borderBottomWidth = 1f;

            var rcEvent = new Clickable( evt =>
            {
                if ( evt == null ) throw new ArgumentNullException( nameof( evt ) );
                var a = ( evt.currentTarget as VisualElement );
                var b = a.GetFirstAncestorWhere( element => element.name == "treeViewItemContainer" );

                m_ItemRightClicked?.Invoke( b );
            } );
            rcEvent.activators.Clear();
            rcEvent.activators.Add( new ManipulatorActivationFilter { button = MouseButton.RightMouse } );

            var lcEvent1 = new Clickable( evt =>
            {
                if ( evt == null ) throw new ArgumentNullException( nameof( evt ) );
                var a = ( evt.currentTarget as VisualElement );
                var b = a.GetFirstAncestorWhere( element => element.name == "treeViewItemContainer" );

                m_ItemLeftClicked?.Invoke( b );
            } );
            var lcEvent2 = new Clickable( evt =>
            {
                if ( evt == null ) throw new ArgumentNullException( nameof( evt ) );
                var a = ( evt.currentTarget as VisualElement );
                var b = a.GetFirstAncestorWhere( element => element.name == "treeViewItemContainer" );
                m_ItemFoldoutClicked?.Invoke( b );
            } );

            var leftColorElement = new VisualElement();
            leftColorElement.name = "Left-Color";
            leftColorElement.style.width = 13f;
            leftColorElement.style.backgroundColor = GUIUtility.ListItemColorScene;

            var dropDownImage = new VisualElement();
            dropDownImage.name = "dropDownImage";
            dropDownImage.style.backgroundImage = new StyleBackground( FoldoutIconClosed );
            dropDownImage.style.backgroundSize = new StyleBackgroundSize( new BackgroundSize( BackgroundSizeType.Contain ) );
            dropDownImage.style.width = 15f;

            var itemLabel = new Label();
            itemLabel.name = "element-label";
            itemLabel.style.unityTextAlign = new StyleEnum<TextAnchor>( TextAnchor.MiddleLeft );
            itemLabel.style.flexGrow = 1f;

            itemLabel.AddManipulator( rcEvent );
            itemLabel.AddManipulator( lcEvent1 );
            dropDownImage.AddManipulator( lcEvent2 );

            container.Add( leftColorElement );
            container.Add( dropDownImage );
            container.Add( itemLabel );

            return container;
        }
    }
}
#endif