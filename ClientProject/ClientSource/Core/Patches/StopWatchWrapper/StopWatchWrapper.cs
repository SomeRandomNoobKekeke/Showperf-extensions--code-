using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopwatchWrapper
{
  public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();

  private StopwatchWrapperPatch CreatePatchInfo(MethodBase target)
    => new StopwatchWrapperPatch(target);



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
      info => info is StopwatchWrapperPatch swp &&
      swp.TargetMethod == target
    );
  }



  public StopwatchWrapper()
  {
    ConnectToPatchInstaller();
    ConnectToCaptureWindow();
  }
}