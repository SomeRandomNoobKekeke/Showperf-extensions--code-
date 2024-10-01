#define SHOWPERF

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



namespace CrabUI
{
  public static class CUI
  {
    public static CUIMainComponent Main => CUIMainComponent.Main;

    public static int CUIShowperfCategory = 1000;

    public static bool Debug = true;

#if !SHOWPERF
    [Conditional("DONT")]
#endif
    public static void Capture(double ticks, string name, int hash) => ShowPerfExtensions.Mod.Capture(ticks, CUIShowperfCategory, name, hash);

#if !SHOWPERF
    [Conditional("DONT")]
#endif
    public static void Capture(double ticks, string name) => ShowPerfExtensions.Mod.Capture(ticks, CUIShowperfCategory, name);

#if !SHOWPERF
    [Conditional("DONT")]
#endif
    public static void EnsureCategory() => ShowPerfExtensions.Mod.EnsureCategory(CUIShowperfCategory);

    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Yellow;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }

    public static void info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      if (Debug)
      {
        var fi = new FileInfo(source);

        log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Yellow * 0.5f);
        log(msg, Color.Yellow);
      }
    }


  }
}