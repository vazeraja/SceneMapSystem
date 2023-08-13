#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TNS.SceneSystem.Editor
{
    [Serializable]
    public class SceneMapAssetManager
    {
        [SerializeField] internal SceneMapAsset _AssetObjectForEditing;
        [SerializeField] internal SceneMapAsset _ImportedAssetObject;
        [SerializeField] private string _AssetGuid;
        [SerializeField] private string _ImportedAssetJson;

        public SerializedObject SerializedObject { get; private set; }

        public string Guid => _AssetGuid;

        public string Path
        {
            get
            {
                Debug.Assert( !string.IsNullOrEmpty( _AssetGuid ), "Asset GUID is empty" );
                return AssetDatabase.GUIDToAssetPath( _AssetGuid );
            }
        }

        public string Name
        {
            get
            {
                if ( _ImportedAssetObject != null ) return _ImportedAssetObject.name;
                return !string.IsNullOrEmpty( Path ) ? System.IO.Path.GetFileNameWithoutExtension( Path ) : string.Empty;
            }
        }

        private SceneMapAsset ImportedAsset
        {
            get
            {
                if ( _ImportedAssetObject == null )
                    LoadImportedObjectFromGuid();

                return _ImportedAssetObject;
            }
        }

        public bool HasChanged => _AssetObjectForEditing.ToJson() != ImportedAsset.ToJson();

        public SceneMapAssetManager( SceneMapAsset sceneMapAsset )
        {
            _ImportedAssetObject = sceneMapAsset;
            Debug.Assert( AssetDatabase.TryGetGUIDAndLocalFileIdentifier( ImportedAsset, out _AssetGuid, out long _ ),
                $"Failed to get asset {sceneMapAsset.name} GUID" );
        }


        public bool Initialize()
        {
            if ( _AssetObjectForEditing == null ) {
                if ( ImportedAsset == null )
                    // The asset we want to edit no longer exists.
                    return false;

                CreateWorkingCopyAsset();
            }
            else {
                SerializedObject = new SerializedObject( _AssetObjectForEditing );
            }

            return true;
        }

        public void Dispose()
        {
            SerializedObject?.Dispose();
        }

        public bool ReInitializeIfAssetHasChanged()
        {
            var asset = ImportedAsset;
            var json = asset.ToJson();
            if ( _ImportedAssetJson == json )
                return false;

            CreateWorkingCopyAsset();
            return true;
        }

        private void CreateWorkingCopyAsset()
        {
            if ( _AssetObjectForEditing != null )
                Cleanup();

            var asset = ImportedAsset;
            _AssetObjectForEditing = Object.Instantiate( asset );
            _AssetObjectForEditing.hideFlags = HideFlags.DontSave;
            _AssetObjectForEditing.name = ImportedAsset.name;
            _ImportedAssetJson = asset.ToJson();
            SerializedObject = new SerializedObject( _AssetObjectForEditing );
        }

        public void Cleanup()
        {
            if ( _AssetObjectForEditing == null )
                return;

            Object.DestroyImmediate( _AssetObjectForEditing );
            _AssetObjectForEditing = null;
        }

        public void LoadImportedObjectFromGuid()
        {
            // https://fogbugz.unity3d.com/f/cases/1313185/
            // InputActionEditorWindow being an EditorWindow, it will be saved as part of the editor's
            // window layout. When a project is opened that has no Library/ folder, the layout from the
            // most recently opened project is used. Which means that when opening an .inputactions
            // asset in project A, then closing it, and then opening project B, restoring the window layout
            // also tries to restore the InputActionEditorWindow having that very same asset open -- which
            // will lead nowhere except there happens to be an InputActionAsset with the very same GUID in
            // the project.
            var assetPath = Path;
            if ( !string.IsNullOrEmpty( assetPath ) )
                _ImportedAssetObject = AssetDatabase.LoadAssetAtPath<SceneMapAsset>( assetPath );
        }

        public void ApplyChanges()
        {
            SerializedObject.ApplyModifiedProperties();
            SerializedObject.Update();
        }

        internal void SaveChangesToAsset()
        {
            Debug.Assert( ImportedAsset != null );

            // Update JSON.
            var asset = _AssetObjectForEditing;
            _ImportedAssetJson = asset.ToJson();

            // Write out, if changed.
            var assetPath = Path;
            var existingJson = File.ReadAllText( assetPath );
            if ( _ImportedAssetJson != existingJson ) {
                CheckOut( assetPath );
                File.WriteAllText( assetPath, _ImportedAssetJson );
                AssetDatabase.ImportAsset( assetPath );
            }
        }


        public static void CheckOut( string path )
        {
            if ( string.IsNullOrEmpty( path ) )
                throw new ArgumentNullException( nameof( path ) );

            // Make path relative to project folder.
            var projectPath = Application.dataPath;
            if ( path.StartsWith( projectPath ) && path.Length > projectPath.Length &&
                 ( path[projectPath.Length] == '/' || path[projectPath.Length] == '\\' ) )
                path = path.Substring( 0, projectPath.Length + 1 );

            AssetDatabase.MakeEditable( path );
        }


        public bool ImportedAssetObjectEquals( SceneMapAsset asset )
        {
            return ImportedAsset != null && ImportedAsset.Equals( asset );
        }
    }
}
#endif