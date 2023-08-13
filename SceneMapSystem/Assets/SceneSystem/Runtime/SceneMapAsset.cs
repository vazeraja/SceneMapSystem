using System;
using System.Collections.Generic;
using System.Linq;
using TNS.SceneSystem.Editor;
using UnityEngine;

namespace TNS.SceneSystem
{
    public class SceneMapAsset : ScriptableObject
    {
        public enum DataType
        {
            Collection,
            Scene,
            Parameter,
            Transition
        }
        
        public const string Extension = "scenemap";

        [SerializeField] internal SceneCollection[] _SceneCollections;
        [SerializeField] internal SceneCollection _DefaultSceneCollection;

        public ReadOnlyArray<SceneCollection> SceneCollections => new ReadOnlyArray<SceneCollection>( _SceneCollections );
        public SceneCollection DefaultSceneCollection => _DefaultSceneCollection;
        public Dictionary<string, SceneReference> SceneReferencesByPath => GetSceneReferenceDictionary();
        
        public SceneReference FindScene( string actionNameOrId, bool throwIfNotFound = false )
        {
            if ( actionNameOrId == null )
                throw new ArgumentNullException( nameof( actionNameOrId ) );

            if ( _SceneCollections != null ) {
                // Check if we have a "map/action" path.
                var indexOfSlash = actionNameOrId.IndexOf( '/' );
                if ( indexOfSlash == -1 ) {
                    // No slash so it's just a simple action name. Return either first enabled action or, if
                    // none are enabled, first action with the given name.
                    SceneReference firstActionFound = null;
                    foreach ( var collection in _SceneCollections ) {
                        var action = collection.FindScene( actionNameOrId );
                        if ( action != null ) {
                            if ( action.active || action._Id == actionNameOrId ) // Match by ID is always exact.
                                return action;
                            firstActionFound ??= action;
                        }
                    }

                    if ( firstActionFound != null )
                        return firstActionFound;
                }
                else {
                    // Have a path. First search for the map, then for the action.
                    var mapName = new Substring( actionNameOrId, 0, indexOfSlash );
                    var actionName = new Substring( actionNameOrId, indexOfSlash + 1 );

                    if ( mapName.isEmpty || actionName.isEmpty )
                        throw new ArgumentException( "Malformed action path: " + actionNameOrId, nameof( actionNameOrId ) );

                    foreach ( var map in _SceneCollections ) {
                        if ( Substring.Compare( map.name, mapName, StringComparison.InvariantCultureIgnoreCase ) != 0 )
                            continue;

                        var actions = map._Scenes;
                        foreach ( var action in actions ) {
                            if ( Substring.Compare( action.name, actionName, StringComparison.InvariantCultureIgnoreCase ) == 0 )
                                return action;
                        }

                        break;
                    }
                }
            }

            if ( throwIfNotFound )
                throw new ArgumentException( $"No action '{actionNameOrId}' in '{this}'" );

            return null;
        }

        private Dictionary<string, SceneReference> GetSceneReferenceDictionary()
        {
            var dict = new Dictionary<string, SceneReference>();

            foreach ( var collection in _SceneCollections ) {
                foreach ( var sceneReference in collection.scenes ) {
                    dict.TryAdd( sceneReference.path, sceneReference );
                }
            }

            return dict;
        }

        public void SetDefault( SceneCollection collection )
        {
            if ( collection == null ) return;
            if ( collection.asset == null ) return;

            _DefaultSceneCollection = collection;
        }

        internal SceneCollection FindCollectionByID( string id ) => _SceneCollections?.FirstOrDefault( c => c.id == id );
        public SceneCollection FindCollectionByName( string name ) => _SceneCollections?.FirstOrDefault( c => c.name == name );

        public List<string> FindAll( DataType type )
        {
            return type switch
            {
                DataType.Collection => // Returns all collection IDs in map
                    _SceneCollections.ToList().Select( x => x.id ).ToList(),
                DataType.Scene => // Returns all reference IDs in map
                    _SceneCollections.ToList().Select( c => c.scenes.Select( s => s._Id ) ).SelectMany( c => c ).ToList(),
                _ => throw new ArgumentOutOfRangeException( nameof( type ), type, null )
            };
        }


        public string ToJson()
        {
            var fileJson = new WriteFileJson
            {
                _Name = name,
                _DefaultCollection = SceneCollection.WriteCollectionJson.FromCollection( _DefaultSceneCollection ),
                _Collections = SceneCollection.WriteFileJson.FromCollections( _SceneCollections ).collections,
            };

            return JsonUtility.ToJson( fileJson, true );
        }

        public void LoadFromJson( string json )
        {
            if ( string.IsNullOrEmpty( json ) )
                throw new ArgumentNullException( nameof( json ) );

            ReadFileJson parsedJson = JsonUtility.FromJson<ReadFileJson>( json );
            parsedJson.ToAsset( this );
        }

        public static SceneMapAsset FromJson( string json )
        {
            if ( string.IsNullOrEmpty( json ) )
                throw new ArgumentNullException( nameof( json ) );

            var asset = CreateInstance<SceneMapAsset>();
            asset.LoadFromJson( json );
            return asset;
        }

        [Serializable]
        internal struct WriteFileJson
        {
            public string _Name;
            public SceneCollection.WriteCollectionJson _DefaultCollection;
            public SceneCollection.WriteCollectionJson[] _Collections;
        }

        [Serializable]
        internal struct ReadFileJson
        {
            public string _Name;
            public SceneCollection.ReadCollectionJson _DefaultCollection;
            public SceneCollection.ReadCollectionJson[] _Collections;

            public void ToAsset( SceneMapAsset asset )
            {
                asset.name = _Name;
                asset._DefaultSceneCollection = SceneCollection.ReadCollectionJson.ToCollection( _DefaultCollection );
                asset._SceneCollections = new SceneCollection.ReadFileJson { collections = _Collections }.ToCollections();

                // Link collections to their asset.
                if ( asset._SceneCollections != null ) {
                    foreach ( var collection in asset._SceneCollections )
                        collection._Asset = asset;
                }

                if ( asset._DefaultSceneCollection != null ) {
                    asset._DefaultSceneCollection._Asset = asset;
                }
            }
        }
    }
}