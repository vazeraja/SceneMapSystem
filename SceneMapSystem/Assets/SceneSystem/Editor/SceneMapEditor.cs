#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public class SceneMapEditor : EditorWindow
    {
        private const string kDefaultAssetLayout = "{}";

        [MenuItem( "Tools/Scene System/Scene Map" )]
        public static void CreateSceneMapAsset()
        {
            ProjectWindowUtil.CreateAssetWithContent( "New Controls." + SceneMapAsset.Extension,
                kDefaultAssetLayout, AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.SceneTemplateIconPath ) );
        }

        [MenuItem( "Tools/Scene System/Repair" )]
        public static void Repair()
        {
            ProjectWindowUtil.CreateAssetWithContent( "New Controls." + SceneMapAsset.Extension,
                SceneMapUtility.DefaultLayout, AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.SceneTemplateIconPath ) );
        }

        private static SceneMapEditor FindEditorForAsset( SceneMapAsset asset )
        {
            var windows = Resources.FindObjectsOfTypeAll<SceneMapEditor>();
            return windows.FirstOrDefault( w => w._AssetManager.ImportedAssetObjectEquals( asset ) );
        }

        internal static SceneMapEditor FindEditorForAssetWithGUID( string guid )
        {
            var windows = Resources.FindObjectsOfTypeAll<SceneMapEditor>();
            return windows.FirstOrDefault( w => w._AssetManager.Guid == guid );
        }

        [OnOpenAsset]
        public static bool OnOpenAsset( int instanceId, int line )
        {
            var path = AssetDatabase.GetAssetPath( instanceId );
            if ( !path.EndsWith( k_FileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                return false;

            string collectionToSelect = null;
            string sceneReferenceToSelect = null;

            // Grab SceneMapAsset.
            // NOTE: We defer checking out an asset until we save it. This allows a user to open an
            // .scenemap asset and look at it without forcing a checkout.
            var obj = EditorUtility.InstanceIDToObject( instanceId );
            var asset = obj as SceneMapAsset;
            if ( asset == null )
            {
                // Check if theb user clicked on an action inside the asset.
                var sceneReferenceAsset = obj as SceneReferenceAsset;
                if ( sceneReferenceAsset != null )
                {
                    asset = sceneReferenceAsset.asset;
                    collectionToSelect = sceneReferenceAsset.sceneReference.collection.name;
                    sceneReferenceToSelect = sceneReferenceAsset.sceneReference.name;
                }
                else
                    return false;
            }

            var window = OpenEditor( asset );

            window.SceneCollectionView.TrySelectIndex( 0 );

            // If user clicked on an action inside the asset, focus on that action (if we can find it).
            if ( sceneReferenceToSelect != null && window.SceneCollectionView.TrySelectItem( collectionToSelect ) )
            {
                window.SceneReferenceView.TrySelectItem( sceneReferenceToSelect );
            }

            return true;
        }

        internal static SceneMapEditor OpenEditor( SceneMapAsset asset )
        {
            var window = FindEditorForAsset( asset );
            if ( window == null )
            {
                window = CreateInstance<SceneMapEditor>();

                window._AssetManager = new SceneMapAssetManager( asset );
                window._AssetManager.Initialize();

                window.minSize = new Vector2( 600, 300 );
                window.titleContent = new GUIContent( $"{window._AssetManager.Name} (SceneMap)" );
                
                window.RebuildLists();
            }

            window.Show();
            window.Focus();

            return window;
        }

        private VisualTreeAsset m_VisualTreeAsset;

        private static bool s_RefreshPending;
        private const string k_FileExtension = "." + SceneMapAsset.Extension;

        [SerializeField] internal SceneMapAssetManager _AssetManager;
        public SceneMapAsset SceneMap => _AssetManager._AssetObjectForEditing;
        public SerializedObject SerializedSceneMap => _AssetManager.SerializedObject;
        public SceneCollection SelectedCollection => SceneCollectionView.SelectedItem;
        internal SceneReference SelectedScene => SceneReferenceView.SelectedItem;
        public int SelectedCollectionIndex => SceneCollectionView.SelectedItemIndex;
        public int SelectedSceneIndex => SceneReferenceView.SelectedItemIndex;

        public List<SceneCollection> SceneCollectionItems => SceneMap.SceneCollections.ToList();
        private List<SceneReference> SceneReferenceItems
        {
            get
            {
                if ( SelectedCollection == null || !SelectedCollection.scenes.Any() )
                {
                    return new List<SceneReference>();
                }

                return SelectedCollection.FindAllScenes();
            }
        }


        private SceneCollectionView SceneCollectionView { get; set; }
        private SceneReferenceView SceneReferenceView { get; set; }
        internal ControlsPanelView ControlsView { get; private set; }
        internal InspectorPanelView InspectorView { get; private set; }
        private RibbonTabsView RibbonTabsView { get; set; }
        private SceneCollectionParameterView m_ParameterView => RibbonTabsView.CollectionGraphTab.m_ParameterView;
        

        private void OnEnable()
        {
            var root = rootVisualElement;
            if ( root == null )
                return;

            root.Clear();
            GUIUtility.Events.Clear();

            m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>( GUIUtility.SceneMapEditorWindowUxmlPath );
            m_VisualTreeAsset.CloneTree( root );

            SceneCollectionView = new SceneCollectionView( this, root.Q<ListView>( GUIUtility.SceneCollectionsListView ) );
            SceneReferenceView = new SceneReferenceView( this, root.Q<ListView>( GUIUtility.SceneReferenceListView ) );
            ControlsView = new ControlsPanelView( this, root.Q<VisualElement>( GUIUtility.RightPanel ) );
            InspectorView = new InspectorPanelView( this, root.Q<VisualElement>( GUIUtility.InspectorContainer ) );
            RibbonTabsView = new RibbonTabsView( this, root.Q<VisualElement>( GUIUtility.RibbonContainer ) );
            
            // Reinitialize after assembly reload.
            if ( TryInitializeAssetManager() )
                return;

            SceneCollectionView.TrySelectIndex( 0 );
        }
        
        private void OnDestroy()
        {
            _AssetManager?.Dispose();
        }

        private bool TryInitializeAssetManager()
        {
            if ( _AssetManager == null ) return false;
            if ( _AssetManager.Initialize() )
            {
                RebuildLists();
                return false;
            }

            // The asset we want to edit no longer exists.
            Close();

            return true;
        }

        public void SaveAndRebuild()
        {
            Debug.Log( "SaveAndRebuild()" );
            SaveChangesToAsset();
            RebuildLists();
        }

        public void SaveChangesToAsset()
        {
            if ( !_AssetManager.HasChanged ) return;
            if ( s_RefreshPending ) return;

            _AssetManager.SaveChangesToAsset();
            _AssetManager.ApplyChanges();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void RebuildLists()
        {
            SceneCollectionView?.Rebuild( SceneCollectionItems );
            SceneReferenceView?.Rebuild( SceneReferenceItems );
            m_ParameterView.Rebuild( SelectedCollection?.parameters.ToList() );
        }

        public void SwapListElements<T>( int index1, int index2 )
        {
            if ( typeof( T ) == typeof( SceneCollection ) )
            {
                SceneMap._SceneCollections.SwapElements( index1, index2 );
                SceneCollectionView.TrySelectIndex( index1 );
            }

            if ( typeof( T ) == typeof( SceneReference ) )
            {
                SelectedCollection._Scenes.SwapElements( index1, index2 );
                SceneReferenceView.TrySelectIndex( index1 );
            }

            if ( typeof( T ) == typeof( SceneTransitionParameter ) )
            {
                SelectedCollection._Parameters.SwapElements( index1, index2 );
                m_ParameterView.TrySelectIndex( index1 );
            }

            SaveAndRebuild();
        }

        [Shortcut( "SceneMapEditor/Save", typeof( SceneMapEditor ), KeyCode.S, ShortcutModifiers.Control )]
        private static void SaveShortcut( ShortcutArguments arguments )
        {
            var window = (SceneMapEditor) arguments.context;
            window.SaveChangesToAsset();
        }

        public static void RefreshAllOnAssetReimport()
        {
            if ( s_RefreshPending )
                return;

            // We don't want to refresh right away but rather wait for the next editor update
            // to then do one pass of refreshing action editor windows.
            EditorApplication.delayCall += RefreshAllOnAssetReimportCallback;
            s_RefreshPending = true;
        }

        private static void RefreshAllOnAssetReimportCallback()
        {
            s_RefreshPending = false;

            // When the asset is modified outside of the editor
            // and the importer settings are visible in the inspector
            // the asset references in the importer inspector need to be force rebuild
            // (otherwise we gets lots of exceptions)
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
}
#endif