using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopWatchWrapper
{
  public class StopWatchWrapperPatch(
    string ID,
    MethodBase TargetMethod,
    string Description = ""
  ) : MultiPatchInfo(
    ID,
    TargetMethod,
    [
      new PatchInfo($"{ID} prefix", TargetMethod,
        typeof(StopWatchWrapperPatch).GetMethod("Prefix")!,
        HarmonyPatchType.Prefix
      ),
      new PatchInfo($"{ID} postfix",TargetMethod,
        typeof(StopWatchWrapperPatch).GetMethod("Postfix")!,
        HarmonyPatchType.Postfix
      ),
    ],
    Description
  )
  {
    public static void Prefix(out Stopwatch __state)
    {
      __state = new Stopwatch();
      __state.Start();
    }

    public static void Postfix(MethodBase __originalMethod, Stopwatch __state)
    {
      __state.Stop();
      Logger.Default.Log($"{__originalMethod} took [{__state.ElapsedTicks}] ticks");
    }
  }
}