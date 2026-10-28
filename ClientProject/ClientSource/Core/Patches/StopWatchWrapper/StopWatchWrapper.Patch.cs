using Barotrauma;
using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopwatchWrapper
{
  public class StopwatchWrapperPatch(
    string ID,
    MethodBase TargetMethod,
    string Description = ""
  ) : MultiPatchInfo(
    ID,
    TargetMethod,
    [
      new PatchInfo($"{ID} prefix", TargetMethod,
        typeof(StopwatchWrapperPatch).GetMethod("Prefix")!,
        HarmonyPatchType.Prefix
      ),
      new PatchInfo($"{ID} postfix",TargetMethod,
        typeof(StopwatchWrapperPatch).GetMethod("Postfix")!,
        HarmonyPatchType.Postfix
      ),
    ],
    Description
  )
  {
    public static void ReportElapsedTicks(MethodBase method, long ticks)
    {
      Plugin.Capture?.Update.AddTicks(MethodBaseParser.Serialize(method).ToIdentifier(), ticks);
    }

    public static void Prefix(out Stopwatch __state)
    {
      __state = new Stopwatch();
      __state.Start();
    }

    public static void Postfix(MethodBase __originalMethod, Stopwatch __state)
    {
      __state.Stop();
      ReportElapsedTicks(__originalMethod, __state.ElapsedTicks);
    }
  }
}