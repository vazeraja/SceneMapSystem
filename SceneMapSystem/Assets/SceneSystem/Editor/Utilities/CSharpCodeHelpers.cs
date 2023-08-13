﻿#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TNS.SceneSystem.Editor
{
    internal static class CSharpCodeHelpers
    {
        public static bool IsProperIdentifier( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
                return false;

            if ( char.IsDigit( name[0] ) )
                return false;

            for ( var i = 0; i < name.Length; ++i ) {
                var ch = name[i];
                if ( !char.IsLetterOrDigit( ch ) && ch != '_' )
                    return false;
            }

            return true;
        }

        public static bool IsEmptyOrProperIdentifier( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
                return true;

            return IsProperIdentifier( name );
        }

        public static bool IsEmptyOrProperNamespaceName( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
                return true;

            return name.Split( '.' ).All( IsProperIdentifier );
        }

        ////TODO: this one should add the @escape automatically so no other code has to worry
        public static string MakeIdentifier( string name, string suffix = "" )
        {
            if ( string.IsNullOrEmpty( name ) )
                throw new ArgumentNullException( nameof( name ) );

            if ( char.IsDigit( name[0] ) )
                name = "_" + name;

            // See if we have invalid characters in the name.
            var nameHasInvalidCharacters = false;
            for ( var i = 0; i < name.Length; ++i ) {
                var ch = name[i];
                if ( !char.IsLetterOrDigit( ch ) && ch != '_' ) {
                    nameHasInvalidCharacters = true;
                    break;
                }
            }

            // If so, create a new string where we remove them.
            if ( nameHasInvalidCharacters ) {
                var buffer = new StringBuilder();
                for ( var i = 0; i < name.Length; ++i ) {
                    var ch = name[i];
                    if ( char.IsLetterOrDigit( ch ) || ch == '_' )
                        buffer.Append( ch );
                }

                name = buffer.ToString();
            }

            return name + suffix;
        }

        public static string MakeTypeName( string name, string suffix = "" )
        {
            var symbolName = MakeIdentifier( name, suffix );
            if ( char.IsLower( symbolName[0] ) )
                symbolName = char.ToUpper( symbolName[0] ) + symbolName.Substring( 1 );
            return symbolName;
        }

    #if UNITY_EDITOR
        public static string MakeAutoGeneratedCodeHeader( string toolName, string toolVersion, string sourceFileName = null )
        {
            return
                "//------------------------------------------------------------------------------\n"
                + "// <auto-generated>\n"
                + $"//     This code was auto-generated by {toolName}\n"
                + $"//     version {toolVersion}\n"
                + ( string.IsNullOrEmpty( sourceFileName ) ? "" : $"//     from {sourceFileName}\n" )
                + "//\n"
                + "//     Changes to this file may cause incorrect behavior and will be lost if\n"
                + "//     the code is regenerated.\n"
                + "// </auto-generated>\n"
                + "//------------------------------------------------------------------------------\n";
        }

        public static string ToLiteral( this object value )
        {
            if ( value == null )
                return "null";

            var type = value.GetType();

            if ( type == typeof( bool ) ) {
                if ( (bool)value )
                    return "true";
                return "false";
            }

            if ( type == typeof( char ) )
                return $"'\\u{(int)(char)value:X2}'";

            if ( type == typeof( float ) )
                return value + "f";

            if ( type == typeof( uint ) || type == typeof( ulong ) )
                return value + "u";

            if ( type == typeof( long ) )
                return value + "l";

            if ( type.IsEnum ) {
                var enumValue = type.GetEnumName( value );
                if ( !string.IsNullOrEmpty( enumValue ) )
                    return $"{type.FullName.Replace( "+", "." )}.{enumValue}";
            }

            return value.ToString();
        }

        public static string GetInitializersForPublicPrimitiveTypeFields( this object instance )
        {
            var type = instance.GetType();
            var defaults = Activator.CreateInstance( type );
            var fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public );
            var fieldInits = string.Join( ", ",
                fields.Where( f => ( f.FieldType.IsPrimitive || f.FieldType.IsEnum ) && !f.GetValue( instance ).Equals( f.GetValue( defaults ) ) )
                    .Select( f => $"{f.Name} = {f.GetValue( instance ).ToLiteral()}" ) );

            if ( string.IsNullOrEmpty( fieldInits ) )
                return "()";

            return " { " + fieldInits + " }";
        }

    #endif
    }
}

#endif