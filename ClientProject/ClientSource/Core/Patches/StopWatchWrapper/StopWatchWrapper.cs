using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopwatchWrapper
{
  public PatchInstaller PatchInstaller => Plugin.PatchInstaller;

  public event Action<MethodBase>? MethodPatched;

  private StopwatchWrapperPatch CreatePatchInfo(MethodBase target)
    => new StopwatchWrapperPatch(
      $"Stopwatch around: {MethodBaseParser.Serialize(target)}",
      target
    );

  public void Patch(MethodBase target)
  {
    StopwatchWrapperPatch info = CreatePatchInfo(target);

    if (PatchInstaller.InstallPatch(info))
    {
      MethodPatched?.Invoke(target);
    }
  }

  public void UnPatch(MethodBase target)
  {
    StopwatchWrapperPatch info = CreatePatchInfo(target);

    if (PatchInstaller.RemovePatch(info))
    {
      MethodPatched?.Invoke(target);
    }
  }

  public void TogglePatch(MethodBase target)
  {
    StopwatchWrapperPatch info = CreatePatchInfo(target);

    if (PatchInstaller.TogglePatch(info))
    {
      MethodPatched?.Invoke(target);
    }
  }

  public bool IsPatched(MethodBase target)
  {
    return PatchInstaller.InstalledPatches.Any(
      info => info is StopwatchWrapperPatch &&
      info.TargetMethod == target
    );
  }
}