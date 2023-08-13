using System;
using System.Globalization;

namespace TNS.SceneSystem
{
    [Serializable]
    public struct ReadRectJson
    {
        public string x; // float
        public string y; // float
        public string width; // float
        public string height; // float

        public static SerializableRect ToRect( ReadRectJson rectJson )
        {
            float xValue = 0f;
            if ( !string.IsNullOrEmpty( rectJson.x ) ) {
                xValue = float.Parse( rectJson.x );
            }

            float yValue = 0f;
            if ( !string.IsNullOrEmpty( rectJson.y ) ) {
                yValue = float.Parse( rectJson.y );
            }

            float widthValue = 0f;
            if ( !string.IsNullOrEmpty( rectJson.width ) ) {
                widthValue = float.Parse( rectJson.width );
            }

            float heightValue = 0f;
            if ( !string.IsNullOrEmpty( rectJson.height ) ) {
                heightValue = float.Parse( rectJson.height );
            }

            return new SerializableRect( xValue, yValue, widthValue, heightValue );
        }
    }


    [Serializable]
    public struct WriteRectJson
    {
        public string x; // float
        public string y; // float
        public string width; // float
        public string height; // float

        public static WriteRectJson FromRect( SerializableRect rect )
        {
            return new WriteRectJson
            {
                x = rect.x.ToString( CultureInfo.InvariantCulture ),
                y = rect.y.ToString( CultureInfo.InvariantCulture ),
                width = rect.width.ToString( CultureInfo.InvariantCulture ),
                height = rect.height.ToString( CultureInfo.InvariantCulture )
            };
        }
    }
}