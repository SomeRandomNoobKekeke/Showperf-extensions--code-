#define CUIDEBUG

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
    public static void Capture(CUIComponent host, CUIComponent target, string method, string sprop, string tprop, string value)
    {
      if (target == null || target.IgnoreDebug || !target.Debug || CUIDebugWindow.Main == null) return;

      CUIDebugWindow.Main.Capture(new CUIDebugEvent(host, target, method, sprop, tprop, value));
    }

#if !CUIDEBUG
    [Conditional("DONT")]
#endif
    public static void Flush() => CUIDebugWindow.Main?.Flush();
  }
}