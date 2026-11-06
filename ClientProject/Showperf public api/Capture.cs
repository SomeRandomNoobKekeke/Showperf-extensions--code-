using Barotrauma;
using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using ShowperfPatches;

namespace ShowperfExtensions;

/// <summary>
/// Methods in this class send ticks to showperf window  
/// Showperf window stores values by Identifier, so methods with Identifier are faster
/// </summary>
public static class Capture
{

  /// <summary>
  /// Adds [value] ticks to showperf window
  /// </summary>
  /// <param name="id">id</param>
  /// <param name="value">ElapsedTicks</param>
  public static void Ticks(string id, double value) { }

  /// <summary>
  /// Adds [value] ticks to showperf window
  /// slightly faster than string version
  /// </summary>
  /// <param name="id">id</param>
  /// <param name="value">ElapsedTicks</param>
  public static void Ticks(Identifier id, double value) { }

  /// <summary>
  /// Adds [sw.ElapsedTicks] ticks to showperf window
  /// </summary>
  public static void Ticks(string id, Stopwatch sw) { }

  /// <summary>
  /// Adds [sw.ElapsedTicks] ticks to showperf window
  /// slightly faster than string version
  /// </summary>
  public static void Ticks(Identifier id, Stopwatch sw) { }

  /// <summary>
  /// Adds [sw.ElapsedTicks] ticks to showperf window
  /// </summary>
  public static void Ticks(string id, DisposableStopwatch sw) { }

  /// <summary>
  /// Adds [sw.ElapsedTicks] ticks to showperf window
  /// slightly faster than string version
  /// </summary>
  public static void Ticks(Identifier id, DisposableStopwatch sw) { }

}


