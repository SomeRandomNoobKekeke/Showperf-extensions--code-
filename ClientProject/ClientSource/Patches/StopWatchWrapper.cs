using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopWatchWrapper
{
  public PatchInstaller PatchInstaller => Plugin.PatchInstaller;

  public void Patch(MethodBase target)
  {
    StopWatchWrapperPatch info = new StopWatchWrapperPatch(
      $"Wrapper around: {MethodBaseParser.Serialize(target)}",
      target
    );

    PatchInstaller.InstallPatch(info);
  }

  public void UnPatch(MethodBase target)
  {
    StopWatchWrapperPatch info = new StopWatchWrapperPatch(
      $"Wrapper around: {MethodBaseParser.Serialize(target)}",
      target
    );

    PatchInstaller.RemovePatch(info);
  }

  public void TogglePatch(MethodBase target)
  {
    StopWatchWrapperPatch info = new StopWatchWrapperPatch(
      $"Wrapper around: {MethodBaseParser.Serialize(target)}",
      target
    );

    PatchInstaller.TogglePatch(info);
  }

  public bool IsPatched(MethodBase target)
  {
    return PatchInstaller.InstalledPatches.Any(
      info => info is StopWatchWrapperPatch &&
      info.TargetMethod == target
    );
  }
}