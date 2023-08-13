#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class InspectorPanelView
    {
        private readonly VisualElement m_Root;
        private readonly VisualElement m_ContentContainer;
        private readonly SceneMapEditor m_Window;

        private readonly RibbonFoldout m_SceneSettingsRibbonFoldout;
        private readonly RibbonFoldout m_LoadingSettingsRibbonFoldout;

        private SerializedProperty m_SceneSettingsProperty;

        private SerializedProperty m_TypeProperty;
        private SerializedProperty m_ModeProperty;
        private SerializedProperty m_LoadingSceneProperty;
        private SerializedProperty m_LoadingParametersProperty;

        private SerializedProperty m_UseLoadingScreenProperty;
        private SerializedProperty m_ThreadPriorityProperty;
        private SerializedProperty m_SecureLoadProperty;
        private SerializedProperty m_InterpolateProperty;
        private SerializedProperty m_InterpolationSpeedProperty;

        public InspectorPanelView( SceneMapEditor window, VisualElement root )
        {
            m_Root = root;
            m_Window = window;

            m_ContentContainer = root.Q<VisualElement>( "inspector-content__content-container" );

            m_SceneSettingsRibbonFoldout = new RibbonFoldout();
            m_SceneSettingsRibbonFoldout.SetLabel( "Scene Settings" );
            m_SceneSettingsRibbonFoldout.SetRibbonColor( GUIUtility.RibbonFoldoutColorOrange );

            m_LoadingSettingsRibbonFoldout = new RibbonFoldout();
            m_LoadingSettingsRibbonFoldout.SetLabel( "Loading Settings" );
            m_LoadingSettingsRibbonFoldout.SetRibbonColor( GUIUtility.RibbonFoldoutColorBlue );

            m_ContentContainer.Add( m_SceneSettingsRibbonFoldout );
            m_ContentContainer.Add( m_LoadingSettingsRibbonFoldout );

            GUIUtility.Events.SceneSelected += OnSceneSelected;
            GUIUtility.Events.CollectionSelected += OnCollectionSelected;
            GUIUtility.Events.TransitionSelected += OnTransitionSelected;
        }


        private void OnTransitionSelected( SceneTransition transition )
        {
            ClearContent();

            var collectionsProp = m_Window.SerializedSceneMap.FindProperty( "_SceneCollections" );
            var selectedCollectionProp = collectionsProp.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );
            var transitionsProp = selectedCollectionProp.FindPropertyRelative( "_SceneTransitions" );
            var transitionProp = transitionsProp.GetArrayElementAtIndex( m_Window.SelectedCollection.FindTransitionIndex( transition.ID ) );

            var field = new PropertyField( transitionProp );
            field.bindingPath = transitionProp.propertyPath;
            field.BindProperty( m_Window.SerializedSceneMap );

            m_ContentContainer.Add( field );
        }

        private void OnSceneSelected( int index )
        {
            ClearContent();
            AddFoldouts();
            
            var collectionsProp = m_Window.SerializedSceneMap.FindProperty( "_SceneCollections" );
            var selectedCollectionProp = collectionsProp.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );
            var scenesProp = selectedCollectionProp.FindPropertyRelative( "_Scenes" );
            var sceneProp = scenesProp.GetArrayElementAtIndex( index );

            var sceneSettingsProp = sceneProp.FindPropertyRelative( "_SceneSettings" );

            UpdateGUI( sceneSettingsProp );
        }

        private void OnCollectionSelected( int index )
        {
            
        }

        public void ClearContent()
        {
            m_ContentContainer.Clear();
        }

        public void AddFoldouts()
        {
            m_ContentContainer.Add( m_SceneSettingsRibbonFoldout );
            m_ContentContainer.Add( m_LoadingSettingsRibbonFoldout );
        }

        public void ClearFoldoutGUI()
        {
            m_SceneSettingsRibbonFoldout.CreateGUI( null );
            m_LoadingSettingsRibbonFoldout.CreateGUI( null );
        }

        public void UpdateGUI( SerializedProperty serializedProperty )
        {
            UpdateProperties( serializedProperty );

            m_SceneSettingsRibbonFoldout.CreateGUI( DrawSceneSettingsProperties );
            m_LoadingSettingsRibbonFoldout.CreateGUI( DrawLoadingSettingsProperties );
        }

        private void UpdateProperties( SerializedProperty serializedProperty )
        {
            m_SceneSettingsProperty = serializedProperty;

            m_TypeProperty = m_SceneSettingsProperty.FindPropertyRelative( "_Type" );
            m_ModeProperty = m_SceneSettingsProperty.FindPropertyRelative( "_Mode" );
            m_LoadingSceneProperty = m_SceneSettingsProperty.FindPropertyRelative( "_LoadingScene" );
            m_LoadingParametersProperty = m_SceneSettingsProperty.FindPropertyRelative( "_LoadingParameters" );

            m_UseLoadingScreenProperty = m_LoadingParametersProperty.FindPropertyRelative( "_UseLoadingScreen" );
            m_ThreadPriorityProperty = m_LoadingParametersProperty.FindPropertyRelative( "_ThreadPriority" );
            m_SecureLoadProperty = m_LoadingParametersProperty.FindPropertyRelative( "_SecureLoad" );
            m_InterpolateProperty = m_LoadingParametersProperty.FindPropertyRelative( "_Interpolate" );
            m_InterpolationSpeedProperty = m_LoadingParametersProperty.FindPropertyRelative( "_InterpolationSpeed" );
        }

        private void DrawLoadingSettingsProperties()
        {
            // [Serializable]
            // public struct LoadingParameters
            // {
            //     public bool _UseLoadingScreen;
            //     public ThreadPriority _ThreadPriority;
            //     public bool _SecureLoad;
            //     public bool _Interpolate;
            //     public float _InterpolationSpeed;
            //
            //     public LoadingParameters(LoadSceneMode mode)
            //     {
            //         _UseLoadingScreen = false;
            //         _ThreadPriority = ThreadPriority.High;
            //         _SecureLoad = true;
            //         _Interpolate = false;
            //         _InterpolationSpeed = 1f;
            //     }
            // }

            m_Window.SerializedSceneMap.Update();

            EditorGUILayout.PropertyField( m_UseLoadingScreenProperty );
            EditorGUILayout.PropertyField( m_ThreadPriorityProperty );
            EditorGUILayout.PropertyField( m_SecureLoadProperty );
            EditorGUILayout.PropertyField( m_InterpolateProperty );
            EditorGUILayout.PropertyField( m_InterpolationSpeedProperty );

            if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            {
                m_Window.SaveChangesToAsset();
            }
        }

        private void DrawSceneSettingsProperties()
        {
            // [Serializable]
            // public struct SceneSettings
            // {
            //     [SerializeField] internal SceneReferenceType _Type; // enum
            //     [SerializeField] internal LoadSceneMode _Mode; // enum
            //     [SerializeField] internal Scene _LoadingScene; // scene
            //     [SerializeField] internal LoadingParameters _LoadingParameters; // struct
            // }

            m_Window.SerializedSceneMap.Update();

            EditorGUILayout.PropertyField( m_TypeProperty );
            EditorGUILayout.PropertyField( m_ModeProperty );
            EditorGUILayout.PropertyField( m_LoadingSceneProperty );

            if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            {
                m_Window.SaveChangesToAsset();
            }
        }
    }
}
#endif