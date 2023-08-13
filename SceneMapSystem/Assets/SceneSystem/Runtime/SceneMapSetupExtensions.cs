using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using TNS.SceneSystem.Editor;
using UnityEditor;
#endif

namespace TNS.SceneSystem
{
    public static class SceneMapSetupExtensions
    {
        public static SceneCollection AddSceneCollection( this SceneMapAsset asset, string name = "New SceneCollection" )
        {
            name += $" {asset.SceneCollections.Count + 1}";

            if ( asset.FindCollectionByName( name ) != null )
                throw new InvalidOperationException( $"An action map called '{name}' already exists in the asset" );

            var collection = new SceneCollection( name );
            collection.GenerateId();
            asset.AddSceneCollection( collection );

            return collection;
        }

        public static void AddSceneCollection( this SceneMapAsset asset, SceneCollection collection )
        {
            if ( collection == null )
                throw new ArgumentNullException( nameof( collection ) );
            if ( string.IsNullOrEmpty( collection.name ) )
                throw new InvalidOperationException( "Collections added to a scene map asset must be named" );
            if ( collection.asset != null )
                throw new InvalidOperationException(
                    $"Cannot add map '{collection}' to asset '{asset}' as it has already been added to asset '{collection.asset}'" );
            ////REVIEW: some of the rules here seem stupid; just replace?
            if ( asset.FindCollectionByName( collection.name ) != null )
                throw new InvalidOperationException( $"An action map called '{collection.name}' already exists in the asset" );

            ArrayHelpers.Append( ref asset._SceneCollections, collection );
            collection._Asset = asset;
        }

        public static void RemoveSceneCollection( this SceneMapAsset asset, SceneCollection collection )
        {
            // Ignore if not part of this asset.
            if ( collection._Asset != asset )
                return;

            ArrayHelpers.Erase( ref asset._SceneCollections, collection );
            collection._Asset = null;
        }

        public static SceneReference AddSceneReference( this SceneCollection collection, string name = "New SceneReference",
            SceneReferenceType type = default )
        {
            if ( collection == null )
                throw new ArgumentNullException( nameof( collection ) );
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentException( "Profile must have name", nameof( name ) );
            if ( collection.FindScene( name ) != null )
                throw new InvalidOperationException( $"Cannot add action with duplicate name '{name}' to set '{collection.name}'" );

            name += $" {collection.scenes.Count + 1}";

            // Append reference to array.
            var scene = new SceneReference();
            scene._Name = name;
            scene.GenerateId();
            ArrayHelpers.Append( ref collection._Scenes, scene );
            scene._Collection = collection;

            return scene;
        }

        public static void RemoveSceneReference( this SceneCollection collection, string sceneId )
        {
            var scene = collection.FindScene( sceneId );
            if ( scene == null )
                throw new ArgumentException( $"Profile '{sceneId}' does not exist; nowhere to remove from", nameof( scene ) );
            if ( scene.collection == null )
                throw new ArgumentException( $"Profile '{sceneId}' does not belong to an collection; nowhere to remove from", nameof( scene ) );

            var index = collection.scenes.IndexOfReference( scene );
            ArrayHelpers.EraseAt( ref collection._Scenes, index );

            scene._Collection = null;
        }

        public static SceneTransition AddTransition( this SceneCollection collection, SceneReference origin, SceneReference target )
        {
            //Cancel if any transition with the same origin and target already exist
            if ( collection._SceneTransitions.Any( t => t.OriginID == origin.id && t.TargetID == target.id ) ) {
                return null;
            }

            var transition = new SceneTransition( origin, target );
            ArrayHelpers.Append( ref collection._SceneTransitions, transition );

            return null;
        }

        public static bool RemoveTransition( this SceneCollection collection, SceneTransition transition )
        {
            var t = collection.FindTransition( transition.ID );
            
            var index = collection.sceneTransitions.IndexOfReference( t );
            ArrayHelpers.EraseAt( ref collection._SceneTransitions, index );

            return true;
        }

        public static void RenameParameter(this SceneCollection collection, string newName)
        {
            // collection._Parameters.ToList().Find();
            
        }

        public static bool AddParameter( this SceneCollection collection, SceneTransitionParameterType type, out SceneTransitionParameter parameter )
        {
            var param = new SceneTransitionParameter( type );
            
            //TODO: Make sure parameter names are unique

            ArrayHelpers.Append( ref collection._Parameters, param );
            parameter = param;
            return true;
        }

        public static bool RemoveParameter( this SceneCollection collection, string parameterID, out SceneTransitionParameter parameter )
        {
            var param = collection.FindParameter( parameterID );

            if ( param == null ) {
                Debug.LogError( $"Transition '{parameterID}' does not exist; nowhere to remove from");
                parameter = null;
                return false;
            }
            
            parameter = param;
            
            var index = collection.parameters.IndexOfReference( param );
            ArrayHelpers.EraseAt( ref collection._Parameters, index );

            return true;
        }

        public static int GetSceneCountInAsset( this SceneMapAsset asset )
        {
            return asset.FindAll( SceneMapAsset.DataType.Scene ).Count;
        }

        public static void Rebind( this SceneMapAsset asset )
        {
            foreach ( var sceneReference in asset.SceneCollections.SelectMany( collection => collection.scenes ) ) {
                sceneReference.scene.Rebind();
            }
        }

    #if UNITY_EDITOR
        internal static void SaveChangesToAsset( this SceneMapAsset asset )
        {
            // Update JSON.
            var m_ImportedAssetJson = asset.ToJson();

            // Write out, if changed.
            var assetPath = AssetDatabase.GetAssetPath( asset );
            var existingJson = File.ReadAllText( assetPath );
            if ( m_ImportedAssetJson != existingJson ) {
                CheckOut( assetPath );
                File.WriteAllText( assetPath, m_ImportedAssetJson );
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
    #endif
    }
}