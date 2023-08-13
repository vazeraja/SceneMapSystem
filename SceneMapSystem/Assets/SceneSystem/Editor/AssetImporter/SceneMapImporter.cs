#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;

#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace TNS.SceneSystem.Editor
{
    [ScriptedImporter( kVersion, SceneMapAsset.Extension )]
    public class SceneMapImporter : ScriptedImporter
    {
        private const int kVersion = 13;

        [SerializeField] private bool m_GenerateWrapperCode;
        [SerializeField] private string m_WrapperCodePath;
        [SerializeField] private string m_WrapperClassName;
        [SerializeField] private string m_WrapperCodeNamespace;

        private static InlinedArray<Action> s_OnImportCallbacks;
        public static event Action onImport
        {
            add => s_OnImportCallbacks.Append( value );
            remove => s_OnImportCallbacks.Remove( value );
        }

        public override void OnImportAsset( AssetImportContext ctx )
        {
            if ( ctx == null )
                throw new ArgumentNullException( nameof( ctx ) );

            // Will need to do this if tool will support dragging and dropping old scene maps
            var text = File.ReadAllText( ctx.assetPath );

            // Create asset.
            var asset = ScriptableObject.CreateInstance<SceneMapAsset>();

            // Parse JSON.
            try {
                //// make sure action names are unique
                asset.LoadFromJson( text );
            }
            catch ( Exception exception ) {
                ctx.LogImportError( $"Could not parse input actions in JSON format from '{ctx.assetPath}' ({exception})" );
                DestroyImmediate( asset );
                return;
            }

            // Force name of asset to be that on the file on disk instead of what may be serialized
            // as the 'name' property in JSON.
            asset.name = Path.GetFileNameWithoutExtension( assetPath );

            // Load icons.
            ////REVIEW: the icons won't change if the user changes skin; not sure it makes sense to differentiate here
            var sceneMapIcon = AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.SceneTemplateIconPath );
            var sceneAssetIcon = AssetDatabase.LoadAssetAtPath<Texture2D>( GUIUtility.SceneAssetIconPath );

            // Add asset.
            ctx.AddObjectToAsset( "<root>", asset, sceneMapIcon );
            ctx.SetMainObject( asset );

            // Make sure all the elements in the asset have GUIDs and that they are indeed unique.
            var maps = asset.SceneCollections;
            foreach ( var map in maps ) {
                // Make sure action map has GUID.
                if ( string.IsNullOrEmpty( map.id ) || asset.SceneCollections.Count( x => x.id == map.id ) > 1 )
                    map.GenerateId();

                // Make sure all actions have GUIDs.
                foreach ( var action in map.scenes ) {
                    var actionId = action.id;
                    if ( string.IsNullOrEmpty( actionId ) || asset.SceneCollections.Sum( m => m.scenes.Count( a => a.id == actionId ) ) > 1 )
                        action.GenerateId();
                }
            }

            // Create subasset for each action.
            foreach ( var map in maps ) {
                foreach ( var action in map.scenes ) {
                    var actionReference = ScriptableObject.CreateInstance<SceneReferenceAsset>();
                    actionReference.Set( action );
                    ctx.AddObjectToAsset( action.id, actionReference, sceneAssetIcon );
            
                    // Backwards-compatibility (added for 1.0.0-preview.7).
                    // We used to call AddObjectToAsset using objectName instead of action.m_Id as the object name. This fed
                    // the action name (*and* map name) into the hash generation that was used as the basis for the file ID
                    // object the InputActionReference object. Thus, if the map and/or action name changed, the file ID would
                    // change and existing references to the InputActionReference object would become invalid.
                    //
                    // What we do here is add another *hidden* InputActionReference object with the same content to the
                    // asset. This one will use the old file ID and thus preserve backwards-compatibility. We should be able
                    // to remove this for 2.0.
                    //
                    // Case: https://fogbugz.unity3d.com/f/cases/1229145/
                    var backcompatActionReference = Instantiate( actionReference );
                    backcompatActionReference.name = actionReference.name; // Get rid of the (Clone) suffix.
                    backcompatActionReference.hideFlags = HideFlags.HideInHierarchy;
                    ctx.AddObjectToAsset( actionReference.name, backcompatActionReference, sceneAssetIcon );
                }
            }
            
            // Generate wrapper code, if enabled.
            if ( m_GenerateWrapperCode ) {
                var className = !string.IsNullOrEmpty(m_WrapperClassName) ? m_WrapperClassName : CSharpCodeHelpers.MakeTypeName(asset.name);

                // Debug.Log( $"SceneMapImporter: {className}" );
            }

            SceneMapEditor.RefreshAllOnAssetReimport();
            foreach ( var callback in s_OnImportCallbacks )
                callback();
        }
    }
}
#endif