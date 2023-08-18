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

        private VisualElement m_Root;
        private VisualElement m_Content;

        private SerializedProperty m_HasExitTimeProp;
        private SerializedProperty m_TransitionSettingsProp;
        private SerializedProperty m_TransitionConditionsProp;
        private SerializedProperty m_ExitTimeProp;
        private SerializedProperty m_FixedDurationProp;
        private SerializedProperty m_TransitionDurationProp;
        private SerializedProperty m_TransitionOffsetProp;

        private PropertyField m_HasExitTimeField;
        private Foldout m_SettingsFoldout;
        private PropertyField m_ExitTimeField;
        private PropertyField m_FixedDurationField;
        private PropertyField m_TransitionDurationField;
        private PropertyField m_TransitionOffsetField;
        private IMGUIContainer m_ConditionsIMGUIContainer;

        private ReorderableList conditionsList;
        public event Action listChanged;

        public SceneTransitionInspectorView()
        {
            var inspectorTree = (VisualTreeAsset) AssetDatabase.LoadAssetAtPath( GUIUtility.TransitionInspectorUxmlPath, typeof( VisualTreeAsset ) );
            m_Root = inspectorTree.Clone();

            m_Content = m_Root.Q<VisualElement>( "content" );
            m_HasExitTimeField = m_Root.Q<PropertyField>( "has-exit-time__field" );
            m_SettingsFoldout = m_Root.Q<Foldout>( "settings__foldout" );
            m_ExitTimeField = m_Root.Q<PropertyField>( "exit-time__field" );
            m_FixedDurationField = m_Root.Q<PropertyField>( "fixed-duration__field" );
            m_TransitionDurationField = m_Root.Q<PropertyField>( "transition-duration__field" );
            m_TransitionOffsetField = m_Root.Q<PropertyField>( "transition-offset__field" );
            m_ConditionsIMGUIContainer = m_Root.Q<IMGUIContainer>( "reorderable-list__imgui-container" );

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

            conditionsList = new ReorderableList( null, typeof( bool ) );
            m_ConditionsIMGUIContainer.onGUIHandler = () =>
            {
                using ( var scope = new EditorGUI.ChangeCheckScope() )
                {
                    conditionsList.DoLayoutList();
                    if ( scope.changed ) Save();
                }
            };

            hierarchy.Add( m_Root );
        }

        public void Initialize( SceneMapEditorWindow window )
        {
            m_Window = window;
        }

        public void Display( SerializedProperty property )
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

        private void Save()
        {
            m_Window.SaveAndRebuild();
        }
    }
}