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
    public static string ParseString(string s)
    {
      return s;
    }

    public static Dictionary<Type, MethodInfo> Parse;

    static CUIExtensions()
    {
      Parse = new Dictionary<Type, MethodInfo>();

      Parse[typeof(string)] = typeof(CUIExtensions).GetMethod("ParseString");
    }

  }
}