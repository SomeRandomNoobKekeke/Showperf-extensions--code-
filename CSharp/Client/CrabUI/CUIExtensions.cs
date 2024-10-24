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


    public static Dictionary<Type, MethodInfo> Parse;
    public static Dictionary<Type, MethodInfo> CustomToString;

    static CUIExtensions()
    {
      Parse = new Dictionary<Type, MethodInfo>();
      CustomToString = new Dictionary<Type, MethodInfo>();

      Parse[typeof(string)] = typeof(CUIExtensions).GetMethod("ParseString");
      Parse[typeof(GUISoundType)] = typeof(CUIExtensions).GetMethod("ParseGUISoundType");
    }

  }
}