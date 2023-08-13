using System;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    internal struct ReadSceneReferenceJson
    {
        public string active;
        public string name; // string
        public string id; // string
        public string path; // Scene (struct)
        public ReadSceneSettingsJson sceneSettingsJson;

        public ReadRectJson nodePositionJson;

        public SceneReference ToScene( string name = null )
        {
            try {
                return new SceneReference
                {
                    _Active = !string.IsNullOrEmpty( active ) && bool.Parse( active ),
                    _Name = name ?? this.name,
                    _Id = string.IsNullOrEmpty( id ) ? null : id,
                    _Scene = new Scene( path ),
                    _SceneSettings = ReadSceneSettingsJson.ToSceneSettings(sceneSettingsJson),
                    _NodePosition = ReadRectJson.ToRect(nodePositionJson)
                };
            }
            catch ( Exception e ) {
                Console.WriteLine( e );
                throw;
            }
        }

        public static SceneReference ToScene( ReadSceneReferenceJson referenceJson )
        {
            try {
                return new SceneReference
                {
                    _Active = !string.IsNullOrEmpty( referenceJson.active ) && bool.Parse( referenceJson.active ),
                    _Name = referenceJson.name,
                    _Id = string.IsNullOrEmpty( referenceJson.id ) ? null : referenceJson.id,
                    _Scene = new Scene( referenceJson.path ),
                    _NodePosition = ReadRectJson.ToRect(referenceJson.nodePositionJson)
                };
            }
            catch ( Exception e ) {
                Console.WriteLine( e );
                throw;
            }
        }
    }
    
    [Serializable]
    internal struct WriteSceneReferenceJson
    {
        public string active;
        public string name; // string
        public string id; // string
        public string path; // Scene (struct)
        public WriteSceneSettingsJson sceneSettingsJson;
        
        public WriteRectJson nodePositionJson; 

        public static WriteSceneReferenceJson FromScene( SceneReference reference )
        {
            
                return new WriteSceneReferenceJson
                {
                    active = reference._Active.ToString(),
                    name = reference._Name,
                    id = reference._Id,
                    path = reference._Scene.Path,
                    sceneSettingsJson = WriteSceneSettingsJson.FromSceneSettings(reference.sceneSettings),
                    nodePositionJson = WriteRectJson.FromRect(reference.nodePosition)
                };
        
        }
    }
}