using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace TNS.SceneSystem
{
    public static class PlayerLoopHelpers
    {
        public static PlayerLoopSystem FindSubSystem<T>( PlayerLoopSystem def )
        {
            if ( def.type == typeof( T ) ) {
                return def;
            }

            if ( def.subSystemList != null ) {
                foreach ( var s in def.subSystemList ) {
                    var system = FindSubSystem<T>( s );
                    if ( system.type == typeof( T ) ) {
                        return system;
                    }
                }
            }

            return default( PlayerLoopSystem );
        }
        
        public static bool AddSubSystem<T>(ref PlayerLoopSystem system, PlayerLoopSystem addition)
        {
            if (system.type == typeof(T)) {
                var copyList = system.subSystemList.ToList();
                copyList.Add(addition);
                system.subSystemList = copyList.ToArray();
                return true;
            }
            
            if (system.subSystemList != null)
            {
                for (var i = 0; i < system.subSystemList.Length; i++)
                {
                    if (AddSubSystem<T>(ref system.subSystemList[i], addition))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        
        public static bool ReplaceSystem<T>(ref PlayerLoopSystem system, PlayerLoopSystem replacement)
        {
            if (system.type == typeof(T))
            {
                system = replacement;
                return true;
            }
            if (system.subSystemList != null)
            {
                for (var i = 0; i < system.subSystemList.Length; i++)
                {
                    if (ReplaceSystem<T>(ref system.subSystemList[i], replacement))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static void DumpPlayerLoop(PlayerLoopSystem playerLoop)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine( $"PlayerLoop List" );
            foreach ( var header in playerLoop.subSystemList ) {
                sb.AppendFormat( "------{0}------", header.type.Name );
                sb.AppendLine();

                if ( header.subSystemList is null ) {
                    sb.AppendFormat( "{0} has no subsystems!", header.ToString() );
                    sb.AppendLine();
                    continue;
                }

                foreach ( var subSystem in header.subSystemList ) {
                    sb.AppendFormat( "{0}", subSystem.type.Name );
                    sb.AppendLine();

                    if ( subSystem.subSystemList != null ) {
                        UnityEngine.Debug.LogWarning( "More Subsystem:" + subSystem.subSystemList.Length );
                    }
                }
            }

            Debug.Log( sb.ToString() );
        }
    }
}