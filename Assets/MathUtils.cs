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
    }
}