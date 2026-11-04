using Barotrauma;
using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace ShowperfExtensions;

#if FAKE

public class CaptureWindow
{
  public void AddTicks(string id, double value) { }
  public void AddTicks(Identifier id, double value) { }
}

public static class Capture
{
  public static CaptureWindow Update { get; } = new CaptureWindow();
}

#endif

public static class Capture
{
  public static CaptureWindow Update => Plugin.Capture?.Window;
}