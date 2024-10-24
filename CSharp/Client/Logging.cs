// #define DEBUG

using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using System.IO;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Cyan;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }

#if !DEBUG
    [Conditional("DONT")]
#endif
    public static void info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      if (mod.debug)
      {
        var fi = new FileInfo(source);

        log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Cyan * 0.5f);
        log(msg, Color.Cyan);
      }
    }

#if !DEBUG
    [Conditional("DONT")]
#endif
    public static void err(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      if (mod.debug)
      {
        var fi = new FileInfo(source);

        log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Orange * 0.5f);
        log(msg, Color.Orange);
      }
    }
  }
}
