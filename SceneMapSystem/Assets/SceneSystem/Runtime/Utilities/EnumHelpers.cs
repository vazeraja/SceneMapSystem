using System;

namespace TNS.SceneSystem
{
    public class EnumHelpers
    {
        public static T ConvertToEnum<T>( string json )
        {
            T enumType;
            if ( !string.IsNullOrEmpty( json ) )
                enumType = (T)Enum.Parse( typeof( T ), json, true );
            else {
                enumType = default;
            }

            return enumType;
        }
    }
}