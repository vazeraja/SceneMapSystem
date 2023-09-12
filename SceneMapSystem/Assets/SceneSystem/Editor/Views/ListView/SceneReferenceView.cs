#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneReferenceView : UnityStyleListView<SceneReference>
    {
        private readonly SceneMapEditorWindow m_Window;
        private readonly Button m_AddButton;

        public SceneReferenceView( SceneMapEditorWindow window, ListView view ) : base( window, view )
        {
            m_Window = window;
            m_AddButton = window.rootVisualElement.Q<Button>( "add-scene" );

            selectionChanged += OnSelectionChanged;
            itemRightClicked += ShowMenu;
            itemLeftClicked += OnLeftClick;
            itemLabelChanged += OnItemLabelChanged;
            m_AddButton.clicked += AddScene;

            SetItemColor( GUIUtility.ListItemColorScene );
            GUIUtility.Events.SceneReferenceCreated += reference => { };
        }

        protected override void BindListViewItem( VisualElement element, int i )
        {
            base.BindListViewItem( element, i );

            DisableFields( element );
            UpdateItemIndicator( element );
        }

        private void OnSelectionChanged( IEnumerable<object> objs )
        {
            GUIUtility.Events.TriggerSceneSelected( SelectedItemIndex );
        }

        private void OnLeftClick( VisualElement element )
        {
            var itemWrapper = (ItemWrapper<SceneReference>) element.userData;
            if ( ListView.selectedIndex == itemWrapper.Index )
            {
                GUIUtility.Events.TriggerSceneSelected( itemWrapper.Index );
            }
            else
            {
                ListView.SetSelection( itemWrapper.Index );
            }
        }

        private static void UpdateItemIndicator( VisualElement element )
        {
            var itemWrapper = (ItemWrapper<SceneReference>) element.userData;
            var indicator = element.Q( "", "list-item__indicator-icon" );

            var isSceneInBuild = SceneManager.IsSceneInBuild( itemWrapper.Data.scene.Name );
            var isSceneEnabledInBuild = SceneManager.IsSceneEnabledInBuild( itemWrapper.Data.scene.Path );

            switch ( isSceneInBuild )
            {
                case true:
                    indicator.SetBackgroundImageColor( GUIUtility.InBuildEnabledColor );

                    if ( !isSceneEnabledInBuild )
                    {
                        indicator.SetBackgroundImageColor( GUIUtility.InBuildDisabledColor );
                    }

                    break;
                case false:
                    indicator.SetBackgroundImageColor( GUIUtility.NotInBuildColor );
                    break;
            }
        }

        private void OnItemLabelChanged( VisualElement element, string text )
        {
            var itemWrapper = (ItemWrapper<SceneReference>) element.userData;

            itemWrapper.Data.SetName( text );

            GUIUtility.Events.TriggerLabelChanged( SceneMapAsset.DataType.Scene, text );
        }

        private void ShowMenu( VisualElement element )
        {
            var itemInfo = (ItemWrapper<SceneReference>) element.userData;
            var menu = new GenericMenu();

            menu.AddItem( new GUIContent( "Load/Additive" ), false,
                _ => EditorSceneManager.OpenScene( SelectedItem.path, OpenSceneMode.Additive ), null );
            menu.AddItem( new GUIContent( "Load/AdditiveWithoutLoading" ), false,
                _ => EditorSceneManager.OpenScene( SelectedItem.path, OpenSceneMode.AdditiveWithoutLoading ), null );
            menu.AddItem( new GUIContent( "Load/Single" ), false,
                _ => EditorSceneManager.OpenScene( SelectedItem.path, OpenSceneMode.Single ), null );
            menu.AddItem( new GUIContent( "Remove" ), false, () => RemoveScene( itemInfo.Data ) );
            menu.ShowAsContext();
        }

        private void AddScene() 
        {
            if ( m_Window.SelectedCollection == null )
            {
                Debug.Log( "Select a collection first in order to add a scene" );
                return;
            }

            var scene = m_Window.SelectedCollection.AddSceneReference();
            GUIUtility.Events.TriggerSceneReferenceCreated( scene );

            var index = m_Window.SelectedCollection.FindSceneIndex( scene.id );
            
            EditorApplication.delayCall += () =>
            {
                ListView.SetSelection( index );
            };
        }

        private void RemoveScene( SceneReference scene )
        {
            m_Window.ControlsView.ClearFoldoutGUI();
            
            m_Window.SelectedCollection.RemoveSceneReference( scene.id );

            GUIUtility.Events.TriggerSceneReferenceRemoved( scene );

            var index = m_Window.SelectedCollection.FindSceneIndex( scene.id );
            if ( index >= 1 )
            {
                EditorApplication.delayCall += () =>
                {
                    ListView.SetSelection( index - 1 );
                };
            }    
 
            // var itemsCount = ListView.viewController.itemsSource.Count;
        } 
    }
}
#endif