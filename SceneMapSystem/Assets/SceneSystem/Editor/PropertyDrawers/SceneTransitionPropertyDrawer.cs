using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor.PropertyDrawers
{
    [CustomPropertyDrawer( typeof( SceneTransition ) )]
    public class SceneTransitionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI( SerializedProperty property )
        {
            var content = new VisualElement();
            content.SetMargins( new Margins( 5, 5, 2, 2 ) );

            var hasExitTimeProp = property.FindPropertyRelative( nameof( SceneTransition.m_HasExitTime ) );
            var showSettingsProp = property.FindPropertyRelative( nameof( SceneTransition.m_ShowSettings ) );
            var transitionSettingsProp = property.FindPropertyRelative( nameof( SceneTransition.m_Settings ) );
            var transitionConditionsProp = property.FindPropertyRelative( nameof( SceneTransition.m_Conditions ) );

            var exitTimeProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_ExitTime ) );
            var fixedDurationProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_FixedDuration ) );
            var transitionDurationProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_TransitionDuration ) );
            var transitionOffsetProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_TransitionOffset ) );

            var hasExitTimeField = SerializedPropertyHelpers.CreatePropertyField( hasExitTimeProp );
            var exitTimeField = SerializedPropertyHelpers.CreatePropertyField( exitTimeProp );
            var fixedDurationField = SerializedPropertyHelpers.CreatePropertyField( fixedDurationProp );
            var transitionDurationField = SerializedPropertyHelpers.CreatePropertyField( transitionDurationProp );
            var transitionOffsetField = SerializedPropertyHelpers.CreatePropertyField( transitionOffsetProp );

            hasExitTimeField.RegisterValueChangeCallback( evt => exitTimeField.SetEnabled( evt.changedProperty.boolValue ) );

            var settingsFoldoutField = new Foldout {text = "Settings"};
            settingsFoldoutField.Add( exitTimeField );
            settingsFoldoutField.Add( fixedDurationField );
            settingsFoldoutField.Add( transitionDurationField );
            settingsFoldoutField.Add( transitionOffsetField );

            content.Add( hasExitTimeField );
            content.Add( settingsFoldoutField );

            return content;
        }
    }
}