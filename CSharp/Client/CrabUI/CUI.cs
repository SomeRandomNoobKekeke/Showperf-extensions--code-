#define SHOWPERF

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public static class CUI
  {

    // [Conditional("SHOWPERF")]
    public static void Capture(string name, int hash, double ticks) => ShowPerfExtensions.Mod.Capture(name, hash, ticks);
    // [Conditional("SHOWPERF")]
    public static void Capture(string name, double ticks) => ShowPerfExtensions.Mod.Capture(name, ticks);

    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Cyan;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }
  }
}