using Barotrauma;
using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace ShowperfExtensions;

#if FAKE

public static class Capture
{
  public static void Ticks(string id, double value) { }
  public static void Ticks(Identifier id, double value) { }
  public static void Ticks(string id, Stopwatch sw) { }
  public static void Ticks(Identifier id, Stopwatch sw) { }
  public static void Ticks(string id, DisposableStopwatch sw) { }
  public static void Ticks(Identifier id, DisposableStopwatch sw) { }
}

#endif

public static class Capture
{
  public static void Ticks(string id, double value)
    => Plugin.Capture?.Window.AddTicks(id, value);

  public static void Ticks(Identifier id, double value)
    => Plugin.Capture?.Window.AddTicks(id, value);

  public static void Ticks(string id, Stopwatch sw)
    => Plugin.Capture?.Window.AddTicks(id, sw.ElapsedTicks);

  public static void Ticks(Identifier id, Stopwatch sw)
    => Plugin.Capture?.Window.AddTicks(id, sw.ElapsedTicks);

  public static void Ticks(string id, DisposableStopwatch sw)
    => Plugin.Capture?.Window.AddTicks(id, sw.ElapsedTicks);

  public static void Ticks(Identifier id, DisposableStopwatch sw)
    => Plugin.Capture?.Window.AddTicks(id, sw.ElapsedTicks);
}