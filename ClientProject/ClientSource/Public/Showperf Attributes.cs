using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.Reflection;
using HarmonyLib;

namespace ShowperfExtensions;

public class ShowperfPatchAttribute(string? id = null, string? description = null) : Attribute
{
  public string? ID { get; } = id;
  public string? Desctiption { get; } = description;
}

// Unfortunatelly you can't just pass MethodBase to Attribute, it's not allowed in Attribute params
// also expressions are not allowed in params
// .net docs suggest passing type and method name, but it'll ambiguate overloads, generics etc
// I'll just reuse MethodBaseParser
public class PatchMethodAttribute : Attribute
{
  private static Dictionary<string, bool> AlreadyPrintedWarning = [];


  public MethodBase Target { get; }
  public string TargetGameVersion { get; }
  public HarmonyPatchType PatchType { get; }


  public PatchMethodAttribute(
    string fullMethodName,
    string targetGameVersion,
    HarmonyPatchType patchType = HarmonyPatchType.Prefix
  )
  {
    Target = MethodBaseParser.Parse(fullMethodName);
    TargetGameVersion = targetGameVersion;
    PatchType = patchType;

    //HACK Harmony internally creates these attributes 4 more times, i'll just block repeated warnings
    if (AlreadyPrintedWarning.TryAdd(fullMethodName, true))
    {
      if (
        !string.IsNullOrEmpty(TargetGameVersion) &&
        TargetGameVersion != GameVersion.CurrentVersion.ToString()
      )
      {
        Logger.Default.Warning($"Showperf patch for [{Logger.White(fullMethodName)}] was made for [{Logger.White(TargetGameVersion)}] and is potentially outdated");
      }

      if (Target is null)
      {
        Logger.Default.Error($"Showperf patch for [{Logger.White(fullMethodName)}] couldn't find its target method, you sure it exists?");
      }
    }
  }
}