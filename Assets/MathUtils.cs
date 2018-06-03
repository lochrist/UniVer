using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVer
{
    public class MathUtils
    {
        public static float Round(float value, int digits = 4)
        {
            return (float) Math.Round((double) value, digits);
        }

        public static Vector2 Round(Vector2 value, int digits = 4)
        {
            return new Vector2(Round(value.x, digits), Round(value.y, digits));
        }

        public static float Angle(Vector2 v0, Vector2 v1)
        {
            return Mathf.Atan2(
                v0.x * v1.y - v0.y * v1.x,
                v0.x * v1.x + v0.y * v1.y
                );
        }

        public static float Angle2(Vector2 v0, Vector2 left, Vector2 right)
        {
            return Angle(left - v0, right - v0);
        }

        public static float SquareDistance(Vector2 v0, Vector2 v1)
        {
            var dx = v0.x - v1.x;
            var dy = v0.y - v1.y;
            return dx * dx + dy * dy;
        }

        public static Vector2 Normal(Vector2 v0, Vector2 v1)
        {
            // perpendicular
            var nx = v0.y - v1.y;
            var ny = v1.x - v0.x;

            // normalize
            var len = 1.0f / Mathf.Sqrt(nx * nx + ny * ny);
            return new Vector2(nx * len, ny * len);
        }

        public static Vector2 Neg(Vector2 v0)
        {
            return new Vector2(-v0.x, -v0.y);
        }

        public static Vector2 Perp(Vector2 v0)
        {
            return new Vector2(-v0.y, v0.x);
        }

        public static Vector2 Rotate(Vector2 v, Vector2 origin, float angle)
        {
            var d = v - origin;
            return new Vector2(
                d.x * Mathf.Cos(angle) - d.y * Mathf.Sin(angle) + origin.x,
                d.x * Mathf.Sin(angle) + d.y * Mathf.Cos(angle) + origin.y
                );
        }
    }
}