using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;

namespace CrabUI
{
  // Idk how to access extension methods via reflection :(
  public static class CUIExtensions
  {
    public static string ParseString(string s) => s; // BaroDev (wide)
    public static GUISoundType ParseGUISoundType(string s) => Enum.Parse<GUISoundType>(s);
    public static Color ParseColor(string s) => XMLExtensions.ParseColor(s);
    public static Vector2 ParseVector2(string s)
    {
      int ix = s.IndexOf("X:") + 2;
      int iy = s.IndexOf("Y:") + 2;
      int ib = s.IndexOf("}");

      string sx = s.Substring(ix, iy - ix - 2);
      string sy = s.Substring(iy, ib - iy);

      float x = 0;
      float y = 0;

      float.TryParse(sx, out x);
      float.TryParse(sy, out y);

      return new Vector2(x, y);
    }

    public static Dictionary<Type, MethodInfo> Parse;
    public static Dictionary<Type, MethodInfo> CustomToString;

    static CUIExtensions()
    {
      Parse = new Dictionary<Type, MethodInfo>();
      CustomToString = new Dictionary<Type, MethodInfo>();

      Parse[typeof(string)] = typeof(CUIExtensions).GetMethod("ParseString");
      Parse[typeof(GUISoundType)] = typeof(CUIExtensions).GetMethod("ParseGUISoundType");
      Parse[typeof(Color)] = typeof(CUIExtensions).GetMethod("ParseColor");
      Parse[typeof(Vector2)] = typeof(CUIExtensions).GetMethod("ParseVector2");
    }

  }
}