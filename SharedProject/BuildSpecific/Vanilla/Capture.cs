using Barotrauma;
using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using ShowperfPatches;

namespace ShowperfExtensions;


public static class Capture
{
  public static void Ticks(string id, double value) { }
  public static void Ticks(Identifier id, double value) { }
  public static void Ticks(string id, Stopwatch sw) { }
  public static void Ticks(Identifier id, Stopwatch sw) { }
  public static void Ticks(string id, DisposableStopwatch sw) { }
  public static void Ticks(Identifier id, DisposableStopwatch sw) { }
}


