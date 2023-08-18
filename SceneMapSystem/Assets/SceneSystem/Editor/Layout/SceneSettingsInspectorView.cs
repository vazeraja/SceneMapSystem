using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneSettingsInspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SceneSettingsInspectorView, UxmlTraits> { }

        private SceneMapEditorWindow m_Window;
        private bool m_IsLoading;

        private VisualElement m_Root;
        private VisualElement m_Content;
        private RibbonFoldout m_SceneSettingsFoldout;
        private RibbonFoldout m_LoadingSettingsFoldout;

        private SerializedProperty m_TypeProp;
        private SerializedProperty m_ModeProp;
        private SerializedProperty m_LoadingSceneProp;
        private SerializedProperty m_LoadingParametersProp;

        private SerializedProperty m_UseLoadingScreenProp;
        private SerializedProperty m_ThreadPriorityProp;
        private SerializedProperty m_SecureLoadProp;
        private SerializedProperty m_InterpolateProp;
        private SerializedProperty m_InterpolationSpeedProp;

        private PropertyField m_TypePropField;
        private PropertyField m_ModePropField;
        private PropertyField m_LoadingScenePropField;

        private PropertyField m_UseLoadingScreenField;
        private PropertyField m_ThreadPriorityField;
        private PropertyField m_SecureLoadField;
        private PropertyField m_InterpolateField;
        private PropertyField m_InterpolationSpeedField;

        public SceneSettingsInspectorView()
        {
            var inspectorTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( GUIUtility.SceneSettingsInspectorUxmlPath );
            m_Root = inspectorTree.Clone();
            
            m_IsLoading = true;

            m_Content = m_Root.Q<VisualElement>( "content" );
            m_SceneSettingsFoldout = m_Root.Q<RibbonFoldout>( "SceneSettings__Foldout" );
            m_LoadingSettingsFoldout = m_Root.Q<RibbonFoldout>( "LoadingSettings__Foldout" );

            m_SceneSettingsFoldout.SetLabel( "Scene Settings" );
            m_LoadingSettingsFoldout.SetLabel( "Loading Settings" );
            
            m_SceneSettingsFoldout.SetRibbonColor( GUIUtility.RibbonFoldoutColorOrange );
            m_LoadingSettingsFoldout.SetRibbonColor( GUIUtility.RibbonFoldoutColorBlue );

            m_SceneSettingsFoldout.m_IMGUIContainer.Add( m_TypePropField = new PropertyField() );
            m_SceneSettingsFoldout.m_IMGUIContainer.Add( m_ModePropField = new PropertyField() );
            m_SceneSettingsFoldout.m_IMGUIContainer.Add( m_LoadingScenePropField = new PropertyField() );

            m_LoadingSettingsFoldout.m_IMGUIContainer.Add( m_UseLoadingScreenField = new PropertyField() );
            m_LoadingSettingsFoldout.m_IMGUIContainer.Add( m_ThreadPriorityField = new PropertyField() );
            m_LoadingSettingsFoldout.m_IMGUIContainer.Add( m_SecureLoadField = new PropertyField() );
            m_LoadingSettingsFoldout.m_IMGUIContainer.Add( m_InterpolateField = new PropertyField() );
            m_LoadingSettingsFoldout.m_IMGUIContainer.Add( m_InterpolationSpeedField = new PropertyField() );

            m_TypePropField.RegisterValueChangeCallback( evt => Save() );
            m_ModePropField.RegisterValueChangeCallback( evt => Save() );
            m_LoadingScenePropField.RegisterValueChangeCallback( evt => Save() );
            
            m_UseLoadingScreenField.RegisterValueChangeCallback( evt => Save() );
            m_ThreadPriorityField.RegisterValueChangeCallback( evt => Save() );
            m_SecureLoadField.RegisterValueChangeCallback( evt => Save() );
            m_InterpolateField.RegisterValueChangeCallback( evt => Save() );
            m_InterpolationSpeedField.RegisterValueChangeCallback( evt => Save() );

            hierarchy.Add( m_Root );
        }

        public void Initialize( SceneMapEditorWindow window )
        {
            m_Window = window;
        }

        public void Display( SerializedProperty property )
        {
            m_TypeProp = property.FindPropertyRelative( nameof( SceneSettings._Type ) );
            m_ModeProp = property.FindPropertyRelative( nameof( SceneSettings._Mode ) );
            m_LoadingSceneProp = property.FindPropertyRelative( nameof( SceneSettings._LoadingScene ) );
            m_LoadingParametersProp = property.FindPropertyRelative( nameof( SceneSettings._LoadingParameters ) );

            m_UseLoadingScreenProp = m_LoadingParametersProp.FindPropertyRelative( nameof( LoadingParameters._UseLoadingScreen ) );
            m_ThreadPriorityProp = m_LoadingParametersProp.FindPropertyRelative( nameof( LoadingParameters._ThreadPriority ) );
            m_SecureLoadProp = m_LoadingParametersProp.FindPropertyRelative( nameof( LoadingParameters._SecureLoad ) );
            m_InterpolateProp = m_LoadingParametersProp.FindPropertyRelative( nameof( LoadingParameters._Interpolate ) );
            m_InterpolationSpeedProp = m_LoadingParametersProp.FindPropertyRelative( nameof( LoadingParameters._InterpolationSpeed ) );

            m_TypePropField.BindProperty( m_TypeProp );
            m_ModePropField.BindProperty( m_ModeProp );
            m_LoadingScenePropField.BindProperty( m_LoadingSceneProp );

            m_UseLoadingScreenField.BindProperty( m_UseLoadingScreenProp );
            m_ThreadPriorityField.BindProperty( m_ThreadPriorityProp );
            m_SecureLoadField.BindProperty( m_SecureLoadProp );
            m_InterpolateField.BindProperty( m_InterpolateProp );
            m_InterpolationSpeedField.BindProperty( m_InterpolationSpeedProp );
        }

        private void Save()
        {
            m_Window.SaveAndRebuild();
        }
    }
}