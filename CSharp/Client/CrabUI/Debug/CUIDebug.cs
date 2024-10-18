#define CUIDEBUG
// #define SHOWPERF

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
  public static class CUIDebug
  {
#if !CUIDEBUG
    [Conditional("DONT")]
#endif
    public static void Info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      var fi = new FileInfo(source);

      CUI.log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Cyan * 0.5f);
      CUI.log(msg, Color.Cyan);
    }

#if !CUIDEBUG
    [Conditional("DONT")]
#endif
    public static void Err(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      var fi = new FileInfo(source);

      CUI.log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Orange * 0.5f);
      CUI.log(msg, Color.Orange);
    }

#if !CUIDEBUG
    [Conditional("DONT")]
#endif
    public static void Capture(CUIComponent host, CUIComponent target, string method, string sprop, string tprop, string value)
    {
      if (target == null || target.IgnoreDebug || !target.Debug || CUIDebugWindow.Main == null) return;

      CUIDebugWindow.Main.Capture(new CUIDebugEvent(host, target, method, sprop, tprop, value));
    }

#if !CUIDEBUG
    [Conditional("DONT")]
#endif
    public static void Flush() => CUIDebugWindow.Main?.Flush();


    public static int CUIShowperfCategory = 1000;
#if (!SHOWPERF || !CUIDEBUG)
    [Conditional("DONT")]
#endif
    public static void CaptureTicks(double ticks, string name, int hash) => ShowPerfExtensions.Mod.CaptureTicks(ticks, CUIShowperfCategory, name, hash);

#if (!SHOWPERF || !CUIDEBUG)
    [Conditional("DONT")]
#endif
    public static void CaptureTicks(double ticks, string name) => ShowPerfExtensions.Mod.CaptureTicks(ticks, CUIShowperfCategory, name);

#if (!SHOWPERF || !CUIDEBUG)
    [Conditional("DONT")]
#endif
    public static void EnsureCategory() => ShowPerfExtensions.Mod.EnsureCategory(CUIShowperfCategory);
  }
}