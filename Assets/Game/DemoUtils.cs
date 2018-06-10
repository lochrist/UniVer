using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoUtils {
    public static Color backgroundColor = ToColor(100, 100, 100);
    public static Color constraintColor = ToColor("#d8dde2");
    public static Color pinColor = ToColor(0, 153, 255);
    public static Color angleColor = ToColor(255, 255, 0);
    public static Color worldBoundsColor = ToColor(45, 45, 45);
    public static Color vertexColor = ToColor("#2dad8f");
    public static Color selectedVertexColor = ToColor(0, 0, 255);
    public static Color rectangleColor = ToColor(70, 70, 70);
    public static Color triangleColor = ToColor(225, 10, 10);
    public static Color dragConstraintColor = ToColor(0, 0, 220);
    public const float margin = 30f;
    public static Vector2 offset = new Vector2(DemoUtils.margin, DemoUtils.margin);
    public static float pinSize = 6f;
    public static float vertexSize = 3f;

    public static Color ToColor(int r, int g, int b, float a = 1.0f)
    {
        return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, a);
    }

    public static Color ToColor(string color)
    {
        if ((color.StartsWith("#")) && (color.Length == 7))
        {
            var r = Int32.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = Int32.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = Int32.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            return ToColor(r, g, b);
        }

        return Color.black;
    }
}
