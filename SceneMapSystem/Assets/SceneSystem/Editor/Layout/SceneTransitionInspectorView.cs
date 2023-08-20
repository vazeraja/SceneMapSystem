using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneTransitionInspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SceneTransitionInspectorView, UxmlTraits> { }

        private SceneMapEditorWindow m_Window;
        public SceneTransition m_SceneTransition;

        private VisualElement m_Root;
        private VisualElement m_Content;

        private VisualElement m_HelpBox;
        private Label m_HelpBoxText;

        private SerializedProperty m_HasExitTimeProp;
        private SerializedProperty m_TransitionSettingsProp;
        private SerializedProperty m_TransitionConditionsProp;
        private SerializedProperty m_ExitTimeProp;
        private SerializedProperty m_FixedDurationProp;
        private SerializedProperty m_TransitionDurationProp;
        private SerializedProperty m_TransitionOffsetProp;

        private readonly PropertyField m_HasExitTimeField;
        private readonly Foldout m_SettingsFoldout;
        private readonly PropertyField m_ExitTimeField;
        private readonly PropertyField m_FixedDurationField;
        private readonly PropertyField m_TransitionDurationField;
        private readonly PropertyField m_TransitionOffsetField;
        private readonly IMGUIContainer m_ConditionsIMGUIContainer;

        private readonly ReorderableList m_ConditionsList;

        public SceneTransitionInspectorView()
        {
            var inspectorTree = (VisualTreeAsset) AssetDatabase.LoadAssetAtPath( GUIUtility.TransitionInspectorUxmlPath, typeof( VisualTreeAsset ) );
            m_Root = inspectorTree.Clone();

            m_Content = m_Root.Q<VisualElement>( "content" );
            m_HelpBox = m_Root.Q<VisualElement>( "help-box" );
            m_HelpBoxText = m_Root.Q<Label>( "text" );
            m_HasExitTimeField = m_Root.Q<PropertyField>( "has-exit-time__field" );
            m_SettingsFoldout = m_Root.Q<Foldout>( "settings__foldout" );
            m_ExitTimeField = m_Root.Q<PropertyField>( "exit-time__field" );
            m_FixedDurationField = m_Root.Q<PropertyField>( "fixed-duration__field" );
            m_TransitionDurationField = m_Root.Q<PropertyField>( "transition-duration__field" );
            m_TransitionOffsetField = m_Root.Q<PropertyField>( "transition-offset__field" );
            m_ConditionsIMGUIContainer = m_Root.Q<IMGUIContainer>( "reorderable-list__imgui-container" );

            SetHelpText( "Edit the settings displayed below to control how this scene transition is executed" );
            var ribbonFoldout = new RibbonFoldout();
            ribbonFoldout.SetLabel( "Transition Settings" );
            m_SettingsFoldout.text = "Settings";

            m_HasExitTimeField.RegisterValueChangeCallback( evt =>
            {
                Save();
                m_ExitTimeField.SetEnabled( evt.changedProperty.boolValue );
            } );
            m_ExitTimeField.RegisterValueChangeCallback( evt => Save() );
            m_FixedDurationField.RegisterValueChangeCallback( evt => Save() );
            m_TransitionDurationField.RegisterValueChangeCallback( evt => Save() );
            m_TransitionOffsetField.RegisterValueChangeCallback( evt => Save() );

            m_ConditionsList = new ReorderableList( null, typeof( bool ) );
            m_ConditionsIMGUIContainer.onGUIHandler = () =>
            {
                using ( var scope = new EditorGUI.ChangeCheckScope() )
                {
                    m_ConditionsList.DoLayoutList();
                    if ( scope.changed ) Save();
                }
            };

            m_Content.RemoveFromHierarchy();

            ribbonFoldout.m_IMGUIContainer.Add( m_Content );
            m_Root.hierarchy.Add( ribbonFoldout );

            hierarchy.Add( m_Root );
        }

        public void Initialize( SceneMapEditorWindow window, SceneTransition transition )
        {
            m_Window = window;
            m_SceneTransition = transition;
        }

        public void Bind( SerializedProperty property )
        {
            m_HasExitTimeProp = property.FindPropertyRelative( nameof( SceneTransition.m_HasExitTime ) );
            m_TransitionSettingsProp = property.FindPropertyRelative( nameof( SceneTransition.m_Settings ) );
            m_TransitionConditionsProp = property.FindPropertyRelative( nameof( SceneTransition.m_Conditions ) );

            m_ExitTimeProp = m_TransitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_ExitTime ) );
            m_FixedDurationProp = m_TransitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_FixedDuration ) );
            m_TransitionDurationProp = m_TransitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_TransitionDuration ) );
            m_TransitionOffsetProp = m_TransitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_TransitionOffset ) );

            m_HasExitTimeField.BindProperty( m_HasExitTimeProp );
            m_ExitTimeField.BindProperty( m_ExitTimeProp );
            m_FixedDurationField.BindProperty( m_FixedDurationProp );
            m_TransitionDurationField.BindProperty( m_TransitionDurationProp );
            m_TransitionOffsetField.BindProperty( m_TransitionOffsetProp );
        }

        public void SetHelpText( string text )
        {
            m_HelpBoxText.text = text;
        }

        private void Save()
        {
            m_Window.SaveAndRebuild();
        }
    }
}