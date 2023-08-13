﻿#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using Aarthificial.Typewriter.Editor.Lists;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneCollectionView : UnityStyleListView<SceneCollection>
    {
        private readonly SceneMapEditor m_Window;
        private readonly Button m_AddCollectionButton;

        public SceneCollectionView( SceneMapEditor window, ListView view ) : base( window, view )
        {
            m_Window = window;
            m_AddCollectionButton = window.rootVisualElement.Q<Button>( "add-collection" );

            selectionChanged += OnSelectionChanged;
            itemLeftClicked += OnItemLeftClicked;
            itemRightClicked += OnItemRightClicked;
            itemLabelChanged += OnItemLabelChanged;

            m_AddCollectionButton.clicked += AddCollection;

            SetItemColor( GUIUtility.ListItemColorCollection );
            SetIndicatorColor( Color.white );
            SetIndicatorIcon( AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.HomeIconPath ) );
            SetIndicatorSize( new Vector2( 0.9f, 0.9f ) );
        }

        protected override void BindListViewItem( VisualElement element, int i )
        {
            base.BindListViewItem( element, i );

            DisableFields( element );
            UpdateIndicator( element );
        }

        private void OnSelectionChanged( IEnumerable<object> selectedObjs )
        {
            GUIUtility.Events.TriggerCollectionSelected( SelectedItemIndex );
        }

        private void OnItemLeftClicked( VisualElement element )
        {
            var itemWrapper = (ItemWrapper<SceneCollection>) element.userData;
            if ( ListView.selectedIndex == itemWrapper.Index )
            {
                GUIUtility.Events.TriggerCollectionSelected( SelectedItemIndex );
            }
            else
            {
                ListView.SetSelection( itemWrapper.Index );
            }
        }

        private void OnItemLabelChanged( VisualElement element, string text )
        {
            var itemWrapper = (ItemWrapper<SceneCollection>) element.userData;
            itemWrapper.Data.SetName( text );
            
            GUIUtility.Events.TriggerLabelChanged(SceneMapAsset.DataType.Collection, text);
        }


        private void OnItemRightClicked( VisualElement ele )
        {
            var itemInfo = (ItemWrapper<SceneCollection>) ele.userData;
            var menu = new GenericMenu();

            menu.AddItem( new GUIContent( "Remove" ), false, () => RemoveCollection( itemInfo.Data ) );
            menu.AddItem( new GUIContent( "Set Default" ), false, () => SetDefault( itemInfo.Data ) );
            menu.ShowAsContext();
        }

        private void UpdateIndicator( VisualElement element )
        {
            var itemWrapper = (ItemWrapper<SceneCollection>) element.userData;
            var indicator = element.Q( "", "list-item__indicator-icon" );

            var map = itemWrapper.Data._Asset;

            if ( map.DefaultSceneCollection.id == itemWrapper.Data.id )
            {
                indicator.SetBackgroundImageColor( Color.white );
            }
            else
            {
                indicator.SetBackgroundImageColor( GUIUtility.ZeroAlphaColor );
            }
        }

        private void SetDefault( SceneCollection collection )
        {
            m_Window.SceneMap.SetDefault( collection );
            m_Window.SaveAndRebuild();
        }

        private void AddCollection()
        {
            // Create new collection in the sceneMap asset
            var collection = m_Window.SceneMap.AddSceneCollection();

            GUIUtility.Events.TriggerCollectionCreated( collection );

            // Select the newly created item
            var itemsCount = ListView.viewController.itemsSource.Count;
            ListView.SetSelection( itemsCount - 1 );
        }

        private void RemoveCollection( SceneCollection collection )
        {
            // Remove the collection from the sceneMap asset
            m_Window.SceneMap.RemoveSceneCollection( collection );

            GUIUtility.Events.TriggerCollectionRemoved( collection );

            var itemsCount = ListView.viewController.itemsSource.Count;

            // // Clear the GUI since the removed item may be being displayed
            // m_Window.ControlsView.ClearFoldoutGUI();
            // m_Window.InspectorView.ClearGUI();

            // Select the last item in the list
            ListView.SetSelection( itemsCount - 1 );
        }
    }
}
#endif