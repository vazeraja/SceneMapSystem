#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
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

        private SerializedProperty m_SceneCollectionProp;
        private SerializedProperty m_SceneReferencesProp;
        private SerializedProperty m_SceneTransitionsProp;

        private SerializedProperty SceneCollectionProp
        {
            get
            {
                if ( m_SceneCollectionProp != null )
                    return m_SceneCollectionProp;

                var prop = m_Window.SerializedSceneMap.FindProperty( nameof( SceneMapAsset._SceneCollections ) );
                m_SceneCollectionProp = prop.GetArrayElementAtIndex( m_Window.SelectedCollectionIndex );

                return m_SceneCollectionProp;
            }
        }

        private SerializedProperty SceneReferencesProp
        {
            get
            {
                if ( m_SceneReferencesProp != null ) return m_SceneReferencesProp;

                m_SceneReferencesProp = SceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._Scenes ) );
                return m_SceneReferencesProp;
            }
        }

        private SerializedProperty SceneTransitionsProp
        {
            get
            {
                if ( m_SceneTransitionsProp != null ) return m_SceneTransitionsProp;

                m_SceneTransitionsProp = SceneCollectionProp.FindPropertyRelative( nameof( SceneCollection._SceneTransitions ) );
                return m_SceneTransitionsProp;
            }
        }

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

        private ReorderableList conditionsList;

        private void OnTransitionSelected( int index )
        {
            ClearContent();

            var transitionProp = SceneTransitionsProp.GetArrayElementAtIndex( index );

            var hasExitTimeProp = transitionProp.FindPropertyRelative( nameof( SceneTransition.m_HasExitTime ) );
            var showSettingsProp = transitionProp.FindPropertyRelative( nameof( SceneTransition.m_ShowSettings ) );
            var transitionSettingsProp = transitionProp.FindPropertyRelative( nameof( SceneTransition.m_Settings ) );
            var transitionConditionsProp = transitionProp.FindPropertyRelative( nameof( SceneTransition.m_Conditions ) );

            var exitTimeProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_ExitTime ) );
            var fixedDurationProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_FixedDuration ) );
            var transitionDurationProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_TransitionDuration ) );
            var transitionOffsetProp = transitionSettingsProp.FindPropertyRelative( nameof( SceneTransitionSettings.m_TransitionOffset ) );

            conditionsList = new ReorderableList( m_Window.SerializedSceneMap, transitionConditionsProp );

            var transitionPropField = SerializedPropertyHelpers.CreatePropertyField( m_Window, transitionProp );
            m_ContentContainer.Add(transitionPropField);

            return;
            
            using var gui = new IMGUIContainer();
            gui.SetMargins( new Margins( 5, 5, 2, 2 ) );
            gui.onGUIHandler = () =>
            {
                m_Window.SerializedSceneMap.Update();

                EditorGUILayout.PropertyField( hasExitTimeProp );

                showSettingsProp.boolValue = EditorGUILayout.Foldout
                (
                    showSettingsProp.boolValue,
                    new GUIContent( "Settings", null, "Settings which control how this transition will be processed" )
                );

                if ( showSettingsProp.boolValue )
                {
                    using ( new EditorGUI.IndentLevelScope( 1 ) )
                    {
                        using ( new EditorGUI.DisabledScope( !hasExitTimeProp.boolValue ) )
                        {
                            EditorGUILayout.PropertyField( exitTimeProp );
                        }

                        EditorGUILayout.PropertyField( fixedDurationProp );
                        EditorGUILayout.PropertyField( transitionDurationProp );
                        EditorGUILayout.PropertyField( transitionOffsetProp );
                    }
                }

                EditorGUILayout.Separator();
                conditionsList.DoLayoutList();

                if ( m_Window.SerializedSceneMap.ApplyModifiedProperties() )
                {
                    m_Window.SaveChangesToAsset();
                }
            };

            m_ContentContainer.Add( gui );
        }

        private void OnSceneSelected( int index )
        {
            ClearContent();
            AddFoldouts();

            var sceneProp = SceneReferencesProp.GetArrayElementAtIndex( index );
            var sceneSettingsProp = sceneProp.FindPropertyRelative( "_SceneSettings" );

            var sceneSettingsChildrenProp = sceneSettingsProp.GetChildren();
            foreach ( var prop in sceneSettingsChildrenProp )
            {
                if ( prop.type == "LoadingParameters" )
                {
                    foreach ( var loadingParam in prop.GetChildren() )
                    {
                        var loadingParamField = SerializedPropertyHelpers.CreatePropertyField( m_Window, loadingParam );
                        m_LoadingSettingsRibbonFoldout.m_IMGUIContainer.Add( loadingParamField );
                    }

                    continue;
                }

                var field = SerializedPropertyHelpers.CreatePropertyField( m_Window, prop );
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
    }
}
#endif