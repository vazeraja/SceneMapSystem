using System;
using UnityEditor.Animations;
using UnityEngine;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct SerializableRect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public SerializableRect( Rect MyRect )
        {
            x = MyRect.x;
            y = MyRect.y;
            width = MyRect.width;
            height = MyRect.height;
        }

        public SerializableRect( float x, float y, float width, float height )
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool IsEmpty()
        {
            return x == 0 &&
                   y == 0 &&
                   width == 0 &&
                   height == 0;
        }

        public override string ToString()
        {
            return String.Format( "[{0}, {1}, {2}, {3}]", x, y, width, height );
        }

        /// Automatic conversion from SerializableRect to Rect
        public static implicit operator Rect( SerializableRect vRect )
        {
            return new Rect( vRect.x, vRect.y, vRect.width, vRect.height );
        }


        /// Automatic conversion from Rect to SerializableRect
        public static implicit operator SerializableRect( Rect vRect )
        {
            return new SerializableRect( vRect );
        }
    }

    [Serializable]
    public sealed class SceneReference : ICloneable, IDisposable
    {
        [SerializeField] internal bool _Active = true;
        [SerializeField] internal string _Name;
        [SerializeField] internal string _Id;
        [SerializeField] internal Scene _Scene;
        [SerializeField] internal SceneSettings _SceneSettings;

        [NonSerialized] internal SceneCollection _Collection;

        public string name => _Name;
        public string id => _Id;
        public bool active => _Active;
        public string sceneName => _Scene.Name;
        public string path => _Scene.Path;
        public Scene scene => _Scene;
        public SceneSettings sceneSettings => _SceneSettings;
        public SerializableRect nodePosition => _NodePosition;

        public SceneCollection collection => _Collection;

        [SerializeField] internal SerializableRect _NodePosition;
        
        public void LoadScene()
        {
            if ( !IsAssigned() ) return;
            if ( !active ) return;

            SceneManager.LoadScene( this );
        }

        public void LoadSceneAsync()
        {
            if ( !IsAssigned() ) return;
            if ( !active ) return;

            SceneManager.LoadSceneAsync( this );
        }

        internal void SetName( string text )
        {
            var currentName = new InternedString( _Name );
            var newName = new InternedString( text );

            if ( _Name == text ) return;
            // if ( currentName == newName ) return;
            var sceneReference = _Collection.FindScene( newName );
            if ( sceneReference != null && sceneReference != this )
                throw new InvalidOperationException( "Name cannot be the same as another!" );

            _Name = newName;
        }

        public bool IsAssigned() => scene.Path != null;

        public override string ToString()
        {
            return _Name;
        }

        public void GenerateId() => _Id = Guid.NewGuid().ToString();

        public void Dispose() { }

        private SceneReference Clone()
        {
            var clone = new SceneReference
            {
                _Name = _Name,
                _Scene = _Scene,
            };

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}