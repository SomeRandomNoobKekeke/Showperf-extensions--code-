using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopwatchWrapper
{
  public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();

  public HashSet<MethodBase> Installed { get; } = [];

  public bool IsInstalled(MethodBase target) => Installed.Contains(target);


  private StopwatchWrapperPatch CreatePatchInfo(MethodBase target) => new StopwatchWrapperPatch(target);

  public void Install(MethodBase target)
  {
    if (IsInstalled(target)) return;

    StopwatchWrapperPatch info = CreatePatchInfo(target);

    bool ok = PatchInstaller.InstallPatch(info);
    if (ok) Installed.Add(target);
  }

  public void Remove(MethodBase target)
  {
    if (!IsInstalled(target)) return;

    StopwatchWrapperPatch info = CreatePatchInfo(target);

    bool ok = PatchInstaller.RemovePatch(info);
    if (ok) Installed.Remove(target);
  }

  public bool TogglePatch(MethodBase target)
  {
    if (IsInstalled(target)) Remove(target); else Install(target);

    return IsInstalled(target);
  }


  public StopwatchWrapper()
  {
    ConnectToPatchInstaller();
    ConnectToCaptureWindow();
  }
}