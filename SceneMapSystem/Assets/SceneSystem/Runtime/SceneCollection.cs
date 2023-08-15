using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public sealed class SceneCollection : ICloneable, ISerializationCallbackReceiver, IDisposable
    {
        [SerializeField] internal SceneMapAsset _Asset;
        [SerializeField] internal string _Id;
        [SerializeField] internal string _Name;
        [SerializeField] internal SceneReference _DefaultScene;
        [SerializeField] internal SceneReference[] _Scenes;
        [SerializeField] internal SceneTransition[] _SceneTransitions;
        [SerializeField] internal SceneTransitionParameter[] _Parameters;
        
        public string name => _Name;
        public string id => _Id;
        public SceneMapAsset asset => _Asset;
        public ReadOnlyArray<SceneReference> scenes => new ReadOnlyArray<SceneReference>( _Scenes );
        public ReadOnlyArray<SceneTransition> sceneTransitions => new ReadOnlyArray<SceneTransition>( _SceneTransitions );
        public ReadOnlyArray<SceneTransitionParameter> parameters => new ReadOnlyArray<SceneTransitionParameter>( _Parameters );
        public SceneReference defaultScene => _DefaultScene;
        
        public SceneCollection() { }

        public SceneCollection( string name )
        {
            _Name = name;
        }

        public int FindDefaultSceneIndex()
        {
            for ( var index = 0; index < scenes.Count; index++ ) {
                var profile = scenes[index];
                if ( profile.id == _DefaultScene.id ) {
                    return index;
                }
            }

            return default;
        }
        
        private int FindParameterIndex( string id )
        {
            if ( _Parameters == null )
                return -1;
            var parametersLength = _Parameters.Length;
            for ( var i = 0; i < parametersLength; ++i )
                if ( _Parameters[i].m_ID == id )
                    return i;

            return -1;
        }
        
        public SceneTransitionParameter FindParameter(string nameOrId, bool throwIfNotFound = false)
        {
            if ( nameOrId == null ) throw new ArgumentNullException( nameof( nameOrId ) );

            var index = FindParameterIndex( nameOrId );
            if ( index != -1 ) return _Parameters[index];

            if ( throwIfNotFound ) throw new ArgumentException( $"No parameter '{nameOrId}' in '{this}'", nameof( nameOrId ) );

            return null;
        } 
        
        internal int FindTransitionIndex( string id )
        {
            if ( _SceneTransitions == null )
                return -1;
            var parametersLength = _SceneTransitions.Length;
            for ( var i = 0; i < parametersLength; ++i )
                if ( _SceneTransitions[i].m_ID == id )
                    return i;

            return -1;
        }
        
        public SceneTransition FindTransition(string nameOrId, bool throwIfNotFound = false)
        {
            if ( nameOrId == null ) throw new ArgumentNullException( nameof( nameOrId ) );

            var index = FindTransitionIndex( nameOrId );
            if ( index != -1 ) return _SceneTransitions[index];

            if ( throwIfNotFound ) throw new ArgumentException( $"No transition '{nameOrId}' in '{this}'", nameof( nameOrId ) );

            return null;
        } 

        private int FindSceneIndex( string id )
        {
            if ( _Scenes == null )
                return -1;
            var actionCount = _Scenes.Length;
            for ( var i = 0; i < actionCount; ++i )
                if ( _Scenes[i]._Id == id )
                    return i;

            return -1;
        }

        public SceneReference FindScene( string nameOrId, bool throwIfNotFound = false )
        {
            if ( nameOrId == null ) throw new ArgumentNullException( nameof( nameOrId ) );

            var index = FindSceneIndex( nameOrId );
            if ( index != -1 ) return _Scenes[index];

            if ( throwIfNotFound ) throw new ArgumentException( $"No action '{nameOrId}' in '{this}'", nameof( nameOrId ) );

            return null;
        }


        public List<SceneReference> FindAllScenes()
        {
            if ( _Scenes.LengthSafe() == 0 )
                return new List<SceneReference>();

            return _Scenes.ToList();
        }

        internal void SetName( string text )
        {
            if ( _Name == text ) return;

            var newName = new InternedString( text );
            var collection = _Asset.FindCollectionByName( newName );

            if ( collection != null && collection != this )
                throw new InvalidOperationException( "Name cannot be the same as another collection!" );

            _Name = newName;
        }
        
        public void SetDefault( SceneReference scene )
        {
            _DefaultScene = scene;
        }

        public override string ToString()
        {
            return _Name;
        }

        public void GenerateId() => _Id = Guid.NewGuid().ToString();

        #region Serialization

        [Serializable]
        internal struct ReadCollectionJson
        {
            public string name;
            public string id;
            public ReadSceneReferenceJson defaultScene;
            public ReadSceneReferenceJson[] scenes;
            public ReadSceneTransitionJson[] sceneTransitions;
            public ReadSceneCollectionParameterJson[] parameters;

            public static SceneCollection ToCollection( ReadCollectionJson collectionJson )
            {
                var sceneList = new List<SceneReference>();
                var sceneTransitionList = new List<SceneTransition>();
                var parametersList = new List<SceneTransitionParameter>();

                var mapName = collectionJson.name;

                // This check is very important. When creating a new .scenemap asset, an import is started using an empty asset
                // which will fail and throw fatal errors since the collectionJson will also have empty values
                if ( string.IsNullOrEmpty( mapName ) )
                    return new SceneCollection();

                SceneCollection collection = new SceneCollection( mapName )
                {
                    _Id = string.IsNullOrEmpty( collectionJson.id ) ? null : collectionJson.id,
                    _DefaultScene = ReadSceneReferenceJson.ToScene( collectionJson.defaultScene )
                };

                // Process profiles in collection.
                var sceneCountInMap = collectionJson.scenes?.Length ?? 0;
                for ( var n = 0; n < sceneCountInMap; ++n ) {
                    var jsonMapScene = collectionJson.scenes![n];

                    if ( string.IsNullOrEmpty( jsonMapScene.name ) )
                        throw new InvalidOperationException( $"Scene in collection '{mapName}' has no name" );

                    // Create action.
                    var scene = jsonMapScene.ToScene();
                    sceneList.Add( scene );
                }

                var sceneTransitionCountInMap = collectionJson.sceneTransitions?.Length ?? 0;
                for ( var n = 0; n < sceneTransitionCountInMap; ++n ) {
                    var jsonSceneTransition = collectionJson.sceneTransitions![n];

                    var transition = jsonSceneTransition.ToSceneTransition();
                    sceneTransitionList.Add( transition );
                }
                
                var parameterCountInMap = collectionJson.parameters?.Length ?? 0;
                for ( var n = 0; n < sceneTransitionCountInMap; ++n ) {
                    var jsonCollectionParameter = collectionJson.parameters![n];

                    var parameter = jsonCollectionParameter.ToSceneCollectionParameter();
                    parametersList.Add( parameter );
                }

                // Finalize arrays.
                collection._Scenes = sceneList.ToArray();
                collection._SceneTransitions = sceneTransitionList.ToArray();
                collection._Parameters = parametersList.ToArray();

                foreach ( var scene in sceneList ) {
                    if ( scene == collection.defaultScene ) {
                        collection._DefaultScene = scene;
                    }

                    scene._Collection = collection;
                }

                return collection;
            }
        }

        [Serializable]
        internal struct WriteCollectionJson
        {
            public string name;
            public string id;
            public WriteSceneReferenceJson defaultScene;
            public WriteSceneReferenceJson[] scenes;
            public WriteSceneTransitionJson[] sceneTransitions;
            public WriteSceneCollectionParameterJson[] parameters;

            public static WriteCollectionJson FromCollection( SceneCollection collection )
            {
                WriteSceneReferenceJson[] jsonScenes = null;
                WriteSceneTransitionJson[] jsonTransitions = null;
                WriteSceneCollectionParameterJson[] jsonParameters = null;
                
                WriteSceneReferenceJson defaultSceneReference = new WriteSceneReferenceJson();
                if ( collection._DefaultScene != null ) {
                    defaultSceneReference = WriteSceneReferenceJson.FromScene( collection._DefaultScene );
                }

                var scenes = collection._Scenes;
                if ( scenes != null ) {
                    var actionCount = scenes.Length;
                    jsonScenes = new WriteSceneReferenceJson[actionCount];

                    for ( var i = 0; i < actionCount; ++i )
                        jsonScenes[i] = WriteSceneReferenceJson.FromScene( scenes[i] );
                }

                var sceneTransitions = collection._SceneTransitions;
                if ( sceneTransitions != null ) {
                    var transitionCount = sceneTransitions.Length;
                    jsonTransitions = new WriteSceneTransitionJson[transitionCount];

                    for ( var i = 0; i < transitionCount; ++i )
                        jsonTransitions[i] = WriteSceneTransitionJson.FromSceneTransition( sceneTransitions[i] );
                }
                
                var parameters = collection._Parameters;
                if ( parameters != null ) {
                    var parameterCount = parameters.Length;
                    jsonParameters = new WriteSceneCollectionParameterJson[parameterCount];

                    for ( var i = 0; i < parameterCount; ++i )
                        jsonParameters[i] = WriteSceneCollectionParameterJson.FromSceneCollectionParameter( parameters[i] );
                }


                return new WriteCollectionJson
                {
                    name = collection.name,
                    id = collection.id,
                    defaultScene = defaultSceneReference,
                    scenes = jsonScenes,
                    sceneTransitions = jsonTransitions,
                    parameters = jsonParameters
                };
            }
        }

        [Serializable]
        internal struct WriteFileJson
        {
            public WriteCollectionJson[] collections;

            public static WriteFileJson FromCollections( IEnumerable<SceneCollection> collections )
            {
                var sceneCollections = collections as SceneCollection[] ?? collections.ToArray();

                var mapCount = sceneCollections.Length;
                if ( mapCount == 0 )
                    return new WriteFileJson();

                var mapsJson = new WriteCollectionJson[mapCount];
                var index = 0;
                foreach ( var map in sceneCollections )
                    mapsJson[index++] = WriteCollectionJson.FromCollection( map );

                return new WriteFileJson { collections = mapsJson };
            }
        }

        [Serializable]
        internal struct ReadFileJson
        {
            public ReadSceneReferenceJson[] profiles;
            public ReadCollectionJson[] collections;

            public SceneCollection[] ToCollections()
            {
                var collectionList = new List<SceneCollection>();
                var sceneList = new List<List<SceneReference>>();
                var sceneTransitionList = new List<List<SceneTransition>>();
                var parametersList = new List<List<SceneTransitionParameter>>();

                // Process profiles listed at toplevel.
                var sceneCount = profiles?.Length ?? 0;
                for ( var i = 0; i < sceneCount; ++i ) {
                    var jsonProfile = profiles![i];

                    if ( string.IsNullOrEmpty( jsonProfile.name ) )
                        throw new InvalidOperationException( $"Profile number {i + 1} has no name" );

                    ////REVIEW: make sure all action names are unique?

                    // Determine name of action collection.
                    string mapName = null;
                    var actionName = jsonProfile.name;
                    var indexOfFirstSlash = actionName.IndexOf( '/' );
                    if ( indexOfFirstSlash != -1 ) {
                        mapName = actionName.Substring( 0, indexOfFirstSlash );
                        actionName = actionName.Substring( indexOfFirstSlash + 1 );

                        if ( string.IsNullOrEmpty( actionName ) )
                            throw new InvalidOperationException(
                                $"Invalid action name '{jsonProfile.name}' (missing action name after '/')" );
                    }

                    // Try to find existing collection.
                    SceneCollection collection = null;
                    var collectionIndex = 0;
                    for ( ; collectionIndex < collectionList.Count; ++collectionIndex ) {
                        if ( string.Compare( collectionList[collectionIndex].name, mapName, StringComparison.InvariantCultureIgnoreCase ) == 0 ) {
                            collection = collectionList[collectionIndex];
                            break;
                        }
                    }

                    // Create new collection if it's the first action in the collection.
                    if ( collection == null ) {
                        // NOTE: No collection IDs supported on this path.
                        collection = new SceneCollection( mapName );
                        collectionIndex = collectionList.Count;
                        collectionList.Add( collection );
                        sceneList.Add( new List<SceneReference>() );
                        sceneTransitionList.Add( new List<SceneTransition>() );
                        parametersList.Add( new List<SceneTransitionParameter>() );
                    }

                    // Create action.
                    var scene = jsonProfile.ToScene( actionName );
                    sceneList[collectionIndex].Add( scene );
                }

                // Process collections.
                var collectionCount = collections?.Length ?? 0;
                for ( var i = 0; i < collectionCount; ++i ) {
                    var jsonMap = collections![i];

                    var mapName = jsonMap.name;
                    if ( string.IsNullOrEmpty( mapName ) )
                        throw new InvalidOperationException( $"Map number {i + 1} has no name" );

                    // Try to find existing collection.
                    SceneCollection collection = null;
                    var collectionIndex = 0;
                    for ( ; collectionIndex < collectionList.Count; ++collectionIndex ) {
                        if ( string.Compare( collectionList[collectionIndex].name, mapName, StringComparison.InvariantCultureIgnoreCase ) == 0 ) {
                            collection = collectionList[collectionIndex];
                            break;
                        }
                    }

                    // Create new collection if we haven't seen it before.
                    if ( collection == null ) {
                        collection = new SceneCollection( mapName )
                        {
                            _Id = string.IsNullOrEmpty( jsonMap.id ) ? null : jsonMap.id,
                            _DefaultScene = ReadSceneReferenceJson.ToScene( jsonMap.defaultScene )
                        };
                        collectionIndex = collectionList.Count;
                        collectionList.Add( collection );
                        sceneList.Add( new List<SceneReference>() );
                        sceneTransitionList.Add( new List<SceneTransition>() );
                        parametersList.Add( new List<SceneTransitionParameter>() );
                    }

                    // Process profiles in collection.
                    var profileCountInMap = jsonMap.scenes?.Length ?? 0;
                    for ( var n = 0; n < profileCountInMap; ++n ) {
                        var jsonMapScene = jsonMap.scenes![n];

                        if ( string.IsNullOrEmpty( jsonMapScene.name ) )
                            throw new InvalidOperationException( $"Action number {i + 1} in collection '{mapName}' has no name" );

                        // Create action.
                        var scene = jsonMapScene.ToScene();
                        sceneList[collectionIndex].Add( scene );
                    }

                    var transitionCountInMap = jsonMap.sceneTransitions?.Length ?? 0;
                    for ( var n = 0; n < transitionCountInMap; ++n ) {
                        var jsonMapSceneTransition = jsonMap.sceneTransitions![n];

                        // Create transition.
                        var sceneTransition = jsonMapSceneTransition.ToSceneTransition();
                        sceneTransitionList[collectionIndex].Add( sceneTransition );
                    }
                    
                    var parameterCountInMap = jsonMap.parameters?.Length ?? 0;
                    for ( var n = 0; n < parameterCountInMap; ++n ) {
                        var jsonMapParameter = jsonMap.parameters![n];

                        // Create parameter.
                        var sceneParameter = jsonMapParameter.ToSceneCollectionParameter();
                        parametersList[collectionIndex].Add( sceneParameter );
                    }
                }

                // Finalize arrays.
                for ( var i = 0; i < collectionList.Count; ++i ) {
                    var collection = collectionList[i];

                    var scenes = sceneList[i].ToArray();
                    collection._Scenes = scenes;

                    var sceneTransitions = sceneTransitionList[i].ToArray();
                    collection._SceneTransitions = sceneTransitions;
                    
                    var parameters = parametersList[i].ToArray();
                    collection._Parameters = parameters;

                    foreach ( var scene in scenes ) {
                        if ( scene == collection.defaultScene ) {
                            collection._DefaultScene = scene;
                        }

                        scene._Collection = collection;
                    }
                }

                return collectionList.ToArray();
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // Restore references.
            if ( _Scenes == null ) return;

            foreach ( var scene in _Scenes )
                scene._Collection = this;
        }

        #endregion

        public void Dispose() { }

        public SceneCollection Clone()
        {
            var clone = new SceneCollection
            {
                _Name = _Name
            };

            // Clone actions.
            // ReSharper disable once InvertIf
            if ( _Scenes != null ) {
                var sceneCount = _Scenes.Length;
                var sceneReferences = new SceneReference[sceneCount];
                for ( var i = 0; i < sceneCount; ++i ) {
                    var original = _Scenes[i];
                    sceneReferences[i] = new SceneReference()
                    {
                        _Name = original._Name,
                        _Collection = clone,
                        // _Type = original._Type,
                        _Scene = original._Scene,
                    };
                }

                clone._Scenes = sceneReferences;
            }

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}