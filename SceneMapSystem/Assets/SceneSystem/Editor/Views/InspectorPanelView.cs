#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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

        private static PropertyField CreatePropertyField( SceneMapEditor editor, SerializedProperty property )
        {
            var field = new PropertyField( property );
            field.bindingPath = property.propertyPath;
            field.Bind( editor.SerializedSceneMap );
            field.RegisterValueChangeCallback( ( evt ) => { editor.SaveChangesToAsset(); } );
            return field;
        }


        private void OnTransitionSelected( SceneTransition transition )
        {
            ClearContent();

            var collectionsProp = m_Window.SerializedSceneMap.FindProperty( "_SceneCollections" );
            var selectedCollectionProp = collectionsProp.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );
            var transitionsProp = selectedCollectionProp.FindPropertyRelative( "_SceneTransitions" );
            var transitionProp = transitionsProp.GetArrayElementAtIndex( m_Window.SelectedCollection.FindTransitionIndex( transition.m_ID ) );

            var exitTimeProp = transitionProp.FindPropertyRelative( "m_HasExitTime" );
            var transitionSettingsProp = transitionProp.FindPropertyRelative( nameof( SceneTransition.m_Settings ) );
            
            var exitTimeField = CreatePropertyField( m_Window, exitTimeProp );
            m_ContentContainer.Add( exitTimeField );


            var foldout = new Foldout();
            foldout.text = "Settings";
            foreach ( var prop in transitionSettingsProp.GetChildren() )
            {
                if ( prop.type == "bool" )
                {
                    var field = new Toggle( prop.displayName );
                    field.bindingPath = prop.propertyPath;
                    field.Bind( m_Window.SerializedSceneMap );
                    field.RegisterValueChangedCallback( ( evt ) => { m_Window.SaveChangesToAsset(); } );
                    foldout.Add( field );
                    continue;
                }

                var settingsField = CreatePropertyField( m_Window, prop );
                foldout.Add( settingsField );
            }

            m_ContentContainer.Add( foldout );
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

            var sceneSettingsChildrenProp = sceneSettingsProp.GetChildren();
            foreach ( var prop in sceneSettingsChildrenProp )
            {
                if ( prop.type == "LoadingParameters" )
                {
                    foreach ( var loadingParam in prop.GetChildren() )
                    {
                        var loadingParamField = CreatePropertyField( m_Window, loadingParam );
                        m_LoadingSettingsRibbonFoldout.m_IMGUIContainer.Add( loadingParamField );
                    }

                    continue;
                }

                var field = CreatePropertyField( m_Window, prop );
                m_SceneSettingsRibbonFoldout.m_IMGUIContainer.Add( field );
            }
        }

        private void OnCollectionSelected( int index ) { }

        public void ClearContent()
        {
            m_ContentContainer.Clear();
            m_SceneSettingsRibbonFoldout.m_IMGUIContainer.Clear();
            m_LoadingSettingsRibbonFoldout.m_IMGUIContainer.Clear();
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

        private void DrawLoadingSettingsProperties( SerializedProperty serializedProperty )
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

            var loadingParametersProperty = serializedProperty.FindPropertyRelative( "_LoadingParameters" );
            var useLoadingScreenProperty = loadingParametersProperty.FindPropertyRelative( "_UseLoadingScreen" );
            var threadPriorityProperty = loadingParametersProperty.FindPropertyRelative( "_ThreadPriority" );
            var secureLoadProperty = loadingParametersProperty.FindPropertyRelative( "_SecureLoad" );
            var interpolateProperty = loadingParametersProperty.FindPropertyRelative( "_Interpolate" );
            var interpolationSpeedProperty = loadingParametersProperty.FindPropertyRelative( "_InterpolationSpeed" );

            EditorGUILayout.PropertyField( useLoadingScreenProperty );
            EditorGUILayout.PropertyField( threadPriorityProperty );
            EditorGUILayout.PropertyField( secureLoadProperty );
            EditorGUILayout.PropertyField( interpolateProperty );
            EditorGUILayout.PropertyField( interpolationSpeedProperty );

            if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            {
                m_Window.SaveChangesToAsset();
            }
        }

        private void DrawSceneSettingsProperties( SerializedProperty serializedProperty )
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

            var typeProperty = serializedProperty.FindPropertyRelative( "_Type" );
            var modeProperty = serializedProperty.FindPropertyRelative( "_Mode" );
            var loadingSceneProperty = serializedProperty.FindPropertyRelative( "_LoadingScene" );

            EditorGUILayout.PropertyField( typeProperty );
            EditorGUILayout.PropertyField( modeProperty );
            EditorGUILayout.PropertyField( loadingSceneProperty );

            if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
            {
                m_Window.SaveChangesToAsset();
            }
        }
    }
}
#endif