using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Helper
{

    public static Rect[] SplitRect(Rect rect, bool vertical, params float[] widths)
    {
        var result = new Rect[widths.Length];
        var sizes = new float[widths.Length];

        var fixedTotal = 0f;
        var expandWeights = 0f;
        for (var i = 0; i < widths.Length; i++)
        {
            var w = widths[i];
            if (w < 0)
            {
                expandWeights -= w;
            }
            else
            {
                sizes[i] = w;
                fixedTotal += w;
            }
        }

        var expandTotal = (vertical ? rect.height : rect.width) - fixedTotal;
        for (var i = 0; i < widths.Length; i++)
        {
            var w = widths[i];
            if (w < 0) sizes[i] = expandTotal * (-w) / expandWeights;
        }

        var pos = vertical ? rect.y : rect.x;
        for (var i = 0; i < sizes.Length; i++)
        {
            var w = sizes[i];
            result[i] = vertical ? new Rect(rect.x, pos, rect.width, w) : new Rect(pos, rect.y, w, rect.height);
            pos += w;
        }

        return result;
    }

    public static string Chomp(string s)
    {
        if (s.EndsWith("\n")) return s.Substring(0, s.Length - 1);
        return s;
    }

    public static string SanitizeFileName(string name)
    {
        var reg = new Regex("[\\/:\\*\\?<>\\|\\\"]");
        return reg.Replace(name, "_");
    }

    public static Vector3[] AddVector3(Vector3[] src0, Vector3[] src1, float scale)
    {
        var result = new Vector3[src0.Length];
        for (int i = 0; i < src0.Length; i++) result[i] = src0[i] + src1[i] * scale;
        return result;
    }

}
