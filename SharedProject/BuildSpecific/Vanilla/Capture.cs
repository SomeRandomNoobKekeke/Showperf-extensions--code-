using Barotrauma;
using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace ShowperfExtensions;


public class CaptureWindow
{
  public void AddTicks(string id, double value) { }
  public void AddTicks(Identifier id, double value) { }
}

public static class Capture
{
  public static CaptureWindow Update { get; } = new CaptureWindow();
}


