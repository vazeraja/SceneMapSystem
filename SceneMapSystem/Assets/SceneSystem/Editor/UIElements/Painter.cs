#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TNS.SceneSystem.Editor
{
    public static class Painter
    {
        public static void PaintTriangle( Painter2D painter, IReadOnlyList<Vector3> tri, Color color )
        {
            painter.strokeColor = color;
            painter.BeginPath();

            for ( int i = 0; i < tri.Count; i++ ) {
                if ( i == tri.Count - 1 ) {
                    painter.MoveTo( tri[i] );
                    painter.LineTo( tri[0] );
                    continue;
                }

                painter.MoveTo( tri[i] );
                painter.LineTo( tri[i + 1] );
            }

            painter.Stroke();
        }

        public static void PaintLine( Painter2D painter, Vector2 start, Vector2 end, Color color )
        {
            painter.strokeColor = color;
            painter.BeginPath();
            painter.MoveTo( start );
            painter.LineTo( end );
            painter.Stroke();
        }


        public static void RotateTowards( IList<Vector3> tri, Vector2 start, Vector2 end )
        {
            float angle = Vector2.SignedAngle( Vector2.right, end - start ) + 90.0f;

            for ( int i = 0; i < tri.Count; i++ ) {
                tri[i] = Quaternion.Euler( 0, 0, angle ) * tri[i];

                Vector2 pos = Lerp( start, end, 0.4f ) + new Vector2( 0, 0 );

                tri[i] += (Vector3)( pos );
            }
        }

        public static Vector2 Lerp( Vector2 start, Vector2 end, float t )
        {
            t = Mathf.Clamp01( t );

            return Vector2.Lerp( start, end, t );
        }
    }
}
#endif