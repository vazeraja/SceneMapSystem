using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneCollectionParameterView : UnityStyleListView<SceneTransitionParameter>
    {
        private readonly SceneMapEditor m_Window;
        private readonly ToolbarMenu m_AddButton;

        private SceneCollection SelectedCollection => m_Window.SelectedCollection;

        public SceneCollectionParameterView( SceneMapEditor window, ListView listView ) : base( window, listView )
        {
            m_Window = window;
            m_AddButton = window.rootVisualElement.Q<ToolbarMenu>( GUIUtility.AddParameterButton );

            itemLeftClicked += OnItemLeftClicked;
            itemRightClicked += OnItemRightClicked;
            itemLabelChanged += OnItemLabelChanged;

            m_AddButton.menu.AppendAction( "Float", AddParameter, DropdownMenuAction.AlwaysEnabled, SceneTransitionParameterType.Float );
            m_AddButton.menu.AppendAction( "Int", AddParameter, DropdownMenuAction.AlwaysEnabled, SceneTransitionParameterType.Int );
            m_AddButton.menu.AppendAction( "Bool", AddParameter, DropdownMenuAction.AlwaysEnabled, SceneTransitionParameterType.Bool );
            m_AddButton.menu.AppendAction( "Trigger", AddParameter, DropdownMenuAction.AlwaysEnabled, SceneTransitionParameterType.Trigger );

            HideIndicator();
            SetItemIcon( AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.AlignTopIconPath ) );
            SetItemIconMargins( 4, -4, -2, 2 );
            SetItemColor( GUIUtility.ZeroAlphaColor );
        }

        protected override void BindListViewItem( VisualElement element, int i )
        {
            base.BindListViewItem( element, i );

            var parameter = ( (ItemWrapper<SceneTransitionParameter>) element.userData ).Data;
            var type = parameter.m_Type;

            var indicatorContainer = element.Q( "list-item__indicator" );
            var floatField = (FloatField) element.Q( "floatField" );
            var intField = (IntegerField) element.Q( "integerField" );
            var boolField = (Toggle) element.Q( "boolField" );
            var triggerField = (RadioButton) element.Q( "triggerField" );

            switch ( type )
            {
                case SceneTransitionParameterType.Float:
                    floatField.style.display = DisplayStyle.Flex;
                    floatField.style.width = 50;
                    floatField.value = parameter.defaultFloat;
                    floatField.RegisterValueChangedCallback( evt =>
                    {
                        if ( Math.Abs( evt.newValue - evt.previousValue ) > 0.001f )
                            parameter.defaultFloat = evt.newValue;
                    } );

                    GUIUtility.HideElements( intField, boolField, triggerField );
                    break;
                case SceneTransitionParameterType.Int:
                    intField.style.display = DisplayStyle.Flex;
                    intField.style.width = 50;
                    intField.value = parameter.defaultInt;
                    intField.RegisterValueChangedCallback( evt =>
                    {
                        if ( evt.newValue != evt.previousValue )
                            parameter.defaultInt = evt.newValue;
                    } );

                    GUIUtility.HideElements( floatField, boolField, triggerField );
                    break;
                case SceneTransitionParameterType.Bool:
                    boolField.style.display = DisplayStyle.Flex;
                    boolField.style.marginRight = 39;
                    boolField.value = parameter.defaultBool;
                    boolField.RegisterValueChangedCallback( evt =>
                    {
                        if ( evt.newValue != evt.previousValue )
                            parameter.defaultBool = evt.newValue;
                    } );


                    GUIUtility.HideElements( floatField, intField, triggerField );
                    break;
                case SceneTransitionParameterType.Trigger:
                    triggerField.style.display = DisplayStyle.Flex;
                    triggerField.style.marginRight = 39;
                    triggerField.value = parameter.DefaultTrigger;
                    triggerField.RegisterCallback<ChangeEvent<bool>>( OnTriggerChangeEvent );

                    if ( triggerField.value )
                    {
                        indicatorContainer.Insert( 0, CreateResetButton() );
                    }

                    Button CreateResetButton()
                    {
                        var resetButton = new Button();
                        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>( GUIUtility.ResetButtonStyleSheet );
                        var backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.RewindIconPath );
                        resetButton.AddToClassList( "resetButton" );
                        resetButton.styleSheets.Add( styleSheet );
                        resetButton.style.backgroundImage = backgroundImage;
                        resetButton.clickable.clicked += () =>
                        {
                            switch ( triggerField.value )
                            {
                                case true:
                                    resetButton.RemoveFromHierarchy();
                                    triggerField.value = false;
                                    parameter.DefaultTrigger = false;
                                    break;
                            }
                        };

                        return resetButton;
                    }

                    void OnTriggerChangeEvent( ChangeEvent<bool> evt )
                    {
                        switch ( evt.newValue )
                        {
                            case true:
                                indicatorContainer.Insert( 0, CreateResetButton() );
                                parameter.DefaultTrigger = true;
                                break;
                        }
                    }

                    GUIUtility.HideElements( floatField, intField, boolField );

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnItemLeftClicked( VisualElement element )
        {
            var itemWrapper = (ItemWrapper<SceneTransitionParameter>) element.userData;
            if ( ListView.selectedIndex == itemWrapper.Index ) { }
            else
            {
                ListView.SetSelection( itemWrapper.Index );
            }
        }

        private void OnItemRightClicked( VisualElement element )
        {
            var menu = new GenericMenu();
            var itemInfo = (ItemWrapper<SceneTransitionParameter>) element.userData;

            menu.AddItem( new GUIContent( "Remove" ), false, () => RemoveParameter( itemInfo.Data.m_ID ) );
            menu.ShowAsContext();
        }

        private static void OnItemLabelChanged( VisualElement element, string text )
        {
            var itemInfo = (ItemWrapper<SceneTransitionParameter>) element.userData;
            itemInfo.Data.m_Name = text;

            GUIUtility.Events.TriggerLabelChanged(SceneMapAsset.DataType.Parameter, text);
        }

        public void AddParameter( DropdownMenuAction action )
        {
            var type = (SceneTransitionParameterType) action.userData;
            if ( !SelectedCollection.AddParameter( type, out var param ) )
                return;

            GUIUtility.Events.TriggerParameterCreated( SelectedCollection, param );
        }

        private void RemoveParameter( string parameterID )
        {
            if ( !SelectedCollection.RemoveParameter( parameterID, out var parameter ) )
                return;

            GUIUtility.Events.TriggerParameterRemoved( SelectedCollection, parameter );
        }
    }
}