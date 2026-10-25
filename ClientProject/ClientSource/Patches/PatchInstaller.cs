using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchInstaller
{
  private Harmony Harmony { get; } = new Harmony("showperf.patches");
  private HashSet<PatchInfo> _InstalledPatches { get; } = new();
  public ImmutableHashSet<PatchInfo> InstalledPatches => _InstalledPatches.ToImmutableHashSet();


  public bool IsInstalled(PatchInfo info) => _InstalledPatches.Contains(info);
  public void TogglePatch(PatchInfo info)
  {
    if (_InstalledPatches.Contains(info)) RemovePatch(info);
    else InstallPatch(info);
  }

  public void InstallPatch(PatchInfo info)
  {
    if (_InstalledPatches.Contains(info)) return;

    switch (info.PatchType)
    {
      case HarmonyPatchType.Prefix:
        Harmony.Patch(original: info.TargetMethod, prefix: new HarmonyMethod(info.PatchMethod));
        break;
      case HarmonyPatchType.Postfix:
        Harmony.Patch(original: info.TargetMethod, postfix: new HarmonyMethod(info.PatchMethod));
        break;
      case HarmonyPatchType.Finalizer:
        Harmony.Patch(original: info.TargetMethod, finalizer: new HarmonyMethod(info.PatchMethod));
        break;
      case HarmonyPatchType.Transpiler:
        Harmony.Patch(original: info.TargetMethod, transpiler: new HarmonyMethod(info.PatchMethod));
        break;
      case HarmonyPatchType.ILManipulator:
        Harmony.Patch(original: info.TargetMethod, ilmanipulator: new HarmonyMethod(info.PatchMethod));
        break;
    }

    _InstalledPatches.Add(info);
  }

  public void RemovePatch(PatchInfo info)
  {
    if (!_InstalledPatches.Contains(info)) return;

    Harmony.Unpatch(info.TargetMethod, info.PatchMethod);
    _InstalledPatches.Remove(info);
  }

  public void RemoveAllPatches()
  {
    foreach (PatchInfo info in _InstalledPatches)
    {
      Harmony.Unpatch(info.TargetMethod, info.PatchMethod);
    }

    _InstalledPatches.Clear();
  }

}