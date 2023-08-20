#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace TNS.SceneSystem.Editor
{
    public class ControlsPanelView
    {
        private readonly SceneMapEditorWindow m_Window;

        private readonly VisualElement m_Root;
        private readonly VisualElement m_PanelHeader;

        private readonly Button m_OpenSceneButton;
        private readonly Button m_CloseSceneButton;
        private readonly Button m_PlaySceneButton;
        private readonly Button m_BuildSettingsButton;

        // public ScenePropertiesFoldoutView Foldout { get; private set; }
        private readonly Foldout m_Foldout;
        private readonly IMGUIContainer m_IMGUIContainer;

        public ControlsPanelView( SceneMapEditorWindow window, VisualElement root )
        {
            m_Window = window;

            m_Root = root.Q<VisualElement>( GUIUtility.RightPanel );
            m_PanelHeader = root.Q<VisualElement>( GUIUtility.RightPanelHeader );

            // --------------  Scene Control Buttons  -------------- 
            var sceneControlButtons = root.Q<VisualElement>( GUIUtility.SceneControlButtons );
            m_OpenSceneButton = sceneControlButtons.Q<Button>( GUIUtility.OpenSceneButton );
            m_CloseSceneButton = sceneControlButtons.Q<Button>( GUIUtility.CloseSceneButton );
            m_PlaySceneButton = sceneControlButtons.Q<Button>( GUIUtility.PlaySceneButton );
            m_BuildSettingsButton = sceneControlButtons.Q<Button>( GUIUtility.BuildSettingsButton );

            // --------------  Open Scene Button  -------------- 
            var openManipulator = new ContextualMenuManipulator( evt => OpenSceneDropdownMenu( evt.menu ) );
            openManipulator.SwitchToLeftClick();
            m_OpenSceneButton.AddManipulator( openManipulator );
            // -------------- Build Settings Button  -------------- 
            var buildSettingsManipulator = new ContextualMenuManipulator( evt => BuildSettingsDropdownMenu( evt.menu ) );
            buildSettingsManipulator.SwitchToLeftClick();
            m_BuildSettingsButton.AddManipulator( buildSettingsManipulator );
            // --------------  Close Scene Button  -------------- 
            m_CloseSceneButton.clickable.clicked += CloseScene;
            // --------------  Play Scene Button  -------------- 
            m_PlaySceneButton.clickable.clicked += PlayScene;
            UpdateButtonState();

            // --------------  Foldout Elements  -------------- 
            var foldoutContainer = root.Q<VisualElement>( GUIUtility.FoldoutContainer );
            m_Foldout = foldoutContainer.Q<Foldout>( GUIUtility.ScenePropertiesFoldout );
            m_IMGUIContainer = foldoutContainer.Q<IMGUIContainer>( GUIUtility.ScenePropertiesIMGUI );

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            GUIUtility.Events.CollectionSelected += OnCollectionSelected;
            GUIUtility.Events.SceneSelected += OnSceneSelected;
            GUIUtility.Events.ItemLabelChanged += OnListItemLabelChanged;
        }

        ~ControlsPanelView()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            GUIUtility.Events.CollectionSelected -= OnCollectionSelected;
            GUIUtility.Events.SceneSelected -= OnSceneSelected;
            GUIUtility.Events.ItemLabelChanged -= OnListItemLabelChanged;
        }

        private void OnPlayModeStateChanged( PlayModeStateChange _ )
        {
            UpdateButtonState();
        }

        private void OnListItemLabelChanged( SceneMapAsset.DataType dataType, string text )
        {
            UpdateFoldoutHeader( text );
        }

        private void OnSceneSelected( SceneReference scene )
        {
            var collectionsProp = m_Window.SerializedSceneMap.FindProperty( "_SceneCollections" );
            var selectedCollectionProp = collectionsProp.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );
            var scenesProp = selectedCollectionProp.FindPropertyRelative( "_Scenes" );

            var index = m_Window.SelectedCollection.FindSceneIndex( scene.id );
            var sceneProp = scenesProp.GetArrayElementAtIndex( index );
            var sceneName = sceneProp.FindPropertyRelative( "_Name" ).stringValue;

            UpdateControlsHeader( SceneMapAsset.DataType.Scene );

            UpdateFoldoutHeader( sceneName );
            DrawFoldoutGUI( () => SceneReferenceProperties( sceneProp ) );
        }

        private void OnCollectionSelected( int index )
        {
            var sceneCollectionsProp = m_Window.SerializedSceneMap.FindProperty( "_SceneCollections" );
            var collectionProp = sceneCollectionsProp.GetArrayElementAtIndex( index );
            var collectionName = collectionProp.FindPropertyRelative( "_Name" ).stringValue;

            UpdateControlsHeader( SceneMapAsset.DataType.Collection );

            UpdateFoldoutHeader( collectionName );
            DrawFoldoutGUI( () => SceneCollectionProperties( collectionProp ) );
        }

        public void UpdateControlsHeader( SceneMapAsset.DataType dataType )
        {
            m_PanelHeader.Q<Label>().text = dataType switch
            {
                SceneMapAsset.DataType.Collection => "Collection Controls",
                SceneMapAsset.DataType.Scene => "Scene Controls",
                _ => throw new ArgumentOutOfRangeException( nameof( dataType ), dataType, null )
            };
        }

        public void UpdateFoldoutHeader( string newName )
        {
            m_Foldout.text = newName;
        }

        public void ClearFoldoutGUI()
        {
            DrawFoldoutGUI( null );
        }

        private void DrawFoldoutGUI( Action action )
        {
            m_IMGUIContainer.onGUIHandler = action;
        }

        private void SceneReferenceProperties( SerializedProperty serializedProperty )
        {
            m_Window.SerializedSceneMap.Update();

            var activeProp = serializedProperty.FindPropertyRelative( "_Active" );
            var sceneProp = serializedProperty.FindPropertyRelative( "_Scene" );

            EditorGUILayout.PropertyField( activeProp );
            EditorGUILayout.PropertyField( sceneProp );

            if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            {
                m_Window.SaveAndRebuild();
            }
        }

        private void SceneCollectionProperties( SerializedProperty serializedProperty )
        {
            if ( m_Window.SelectedCollection == null ) return;

            m_Window.SerializedSceneMap.Update();

            var scenes = m_Window.SelectedCollection.FindAllScenes();
            var displayedOptions = scenes.Select( p => p.name ).ToArray();
            var defaultSceneIndex = m_Window.SelectedCollection!.FindDefaultSceneIndex();

            // EditorGUILayout.PropertyField( serializedProperty.FindPropertyRelative( "_SceneTransitions" ) );
            // EditorGUILayout.PropertyField( serializedProperty.FindPropertyRelative( "_Parameters" ) );

            using ( var scope = new EditorGUI.ChangeCheckScope() )
            {
                var selectedCollectionPopupIndex = EditorGUILayout.Popup( "Default Scene", defaultSceneIndex, displayedOptions );
                if ( scope.changed )
                {
                    m_Window.SelectedCollection.SetDefault( scenes[selectedCollectionPopupIndex] );
                    m_Window.SaveChangesToAsset();
                }
            }

            if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            {
                m_Window.SaveAndRebuild();
            }
        }


        public void UpdateButtonState()
        {
            if ( Application.isPlaying )
            {
                m_PlaySceneButton.SwitchClasses( classToAdd: "panelButtonPlaymode", classToRemove: "panelButton" );

                m_OpenSceneButton.SwitchClasses( classToAdd: "panelButtonInactive", classToRemove: "panelButton" );
                m_CloseSceneButton.SwitchClasses( classToAdd: "panelButtonInactive", classToRemove: "panelButton" );
                m_BuildSettingsButton.SwitchClasses( classToAdd: "panelButtonInactive", classToRemove: "panelButton" );
            }
            else
            {
                m_PlaySceneButton.SwitchClasses( classToAdd: "panelButton", classToRemove: "panelButtonPlaymode" );

                m_OpenSceneButton.SwitchClasses( classToAdd: "panelButton", classToRemove: "panelButtonInactive" );
                m_CloseSceneButton.SwitchClasses( classToAdd: "panelButton", classToRemove: "panelButtonInactive" );
                m_BuildSettingsButton.SwitchClasses( classToAdd: "panelButton", classToRemove: "panelButtonInactive" );
            }
        }

        private void OpenSceneDropdownMenu( DropdownMenu menu )
        {
            // @formatter:off
            menu.AppendAction( "Single", _ => EditorSceneManager.OpenScene( m_Window.SelectedScene.path, OpenSceneMode.Single ), DropdownMenuAction.AlwaysEnabled );
            menu.AppendAction( "Additive", _ => EditorSceneManager.OpenScene( m_Window.SelectedScene.path, OpenSceneMode.Additive ), DropdownMenuAction.AlwaysEnabled );
            menu.AppendAction( "AdditiveWithoutLoading", _ => EditorSceneManager.OpenScene( m_Window.SelectedScene.path, OpenSceneMode.AdditiveWithoutLoading ), DropdownMenuAction.AlwaysEnabled );
            // @formatter:on        
        }

        private void CloseScene()
        {
            if ( m_Window.SelectedScene == null ) return;

            EditorSceneManager.CloseScene( SceneManager.GetSceneByPath( m_Window.SelectedScene.path ), true );
        }

        private void PlayScene()
        {
            if ( EditorApplication.isPlaying )
            {
                EditorApplication.ExitPlaymode();
                UpdateButtonState();
                return;
            }

            if ( SceneManager.GetActiveScene().path != m_Window.SelectedScene.path )
            {
                EditorSceneManager.OpenScene( m_Window.SelectedScene.path, OpenSceneMode.Single );
            }

            EditorApplication.EnterPlaymode();
            UpdateButtonState();
        }

        private void BuildSettingsDropdownMenu( DropdownMenu menu )
        {
            menu.AppendAction
            (
                "Add",
                _ => SceneMapUtility.AddSceneToBuildSettings( m_Window.SelectedScene.path ),
                DropdownMenuAction.AlwaysEnabled
            );
            menu.AppendAction
            (
                "Remove",
                _ => SceneMapUtility.RemoveSceneFromBuildSettings( m_Window.SelectedScene.path ),
                DropdownMenuAction.AlwaysEnabled
            );
            menu.AppendAction
            (
                "Enable",
                _ => SceneMapUtility.ToggleEnabledInBuildSettings( m_Window.SelectedScene.path, true ),
                DropdownMenuAction.AlwaysEnabled
            );
            menu.AppendAction
            (
                "Disable",
                _ => SceneMapUtility.ToggleEnabledInBuildSettings( m_Window.SelectedScene.path, false ),
                DropdownMenuAction.AlwaysEnabled
            );
        }
    }
}

#endif