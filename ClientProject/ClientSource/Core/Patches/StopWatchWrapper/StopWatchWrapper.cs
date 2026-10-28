using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopwatchWrapper
{
  public FunnyDictOfWeakSets<MethodBase, ITrackStopwatchPatches> StopwatchPatcherListeners { get; } = new();

  public PatchInstaller PatchInstaller { get; }

  private StopwatchWrapperPatch CreatePatchInfo(MethodBase target)
    => new StopwatchWrapperPatch(
      $"Stopwatch around: {MethodBaseParser.Serialize(target)}",
      target
    );

  private void NotifyStopwatchPatcherListeners(MethodBase mb, bool state)
  {
    foreach (var (method, listeners) in StopwatchPatcherListeners)
    {
      foreach (ITrackStopwatchPatches listener in listeners)
      {
        listener.NotifyStopwatchToggled(mb, state);
      }
    }
  }

  public void Patch(MethodBase target)
  {
    StopwatchWrapperPatch info = CreatePatchInfo(target);
    PatchInstaller.InstallPatch(info);
  }

  public void UnPatch(MethodBase target)
  {
    StopwatchWrapperPatch info = CreatePatchInfo(target);
    PatchInstaller.RemovePatch(info);
  }

  public void TogglePatch(MethodBase target)
  {
    StopwatchWrapperPatch info = CreatePatchInfo(target);
    PatchInstaller.TogglePatch(info);
  }

  public bool IsPatched(MethodBase target)
  {
    return PatchInstaller.InstalledPatches.Any(
      info => info is StopwatchWrapperPatch &&
      info.TargetMethod == target
    );
  }



  public StopwatchWrapper(PatchInstaller patchInstaller)
  {
    PatchInstaller = patchInstaller;

    PatchInstaller.PatchInstalled += (info) =>
    {
      if (info is StopwatchWrapperPatch swp)
      {
        NotifyStopwatchPatcherListeners(swp.TargetMethod, true);
      }
    };

    PatchInstaller.PatchRemoved += (info) =>
    {
      if (info is StopwatchWrapperPatch swp)
      {
        NotifyStopwatchPatcherListeners(swp.TargetMethod, false);
      }
    };
  }
}