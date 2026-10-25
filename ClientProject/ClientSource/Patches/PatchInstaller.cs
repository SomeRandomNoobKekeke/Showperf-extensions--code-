using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchInstaller
{
  private Harmony Harmony { get; } = new Harmony("showperf.patches");
  private HashSet<PatchInfoBase> _InstalledPatches { get; } = new();
  public ImmutableHashSet<PatchInfoBase> InstalledPatches => _InstalledPatches.ToImmutableHashSet();


  public bool IsInstalled(PatchInfoBase info) => _InstalledPatches.Contains(info);
  public void TogglePatch(PatchInfoBase info)
  {
    if (_InstalledPatches.Contains(info)) RemovePatch(info);
    else InstallPatch(info);
  }

  public void InstallPatch(PatchInfoBase info)
  {
    if (_InstalledPatches.Contains(info)) return;

    info.Install(Harmony);

    _InstalledPatches.Add(info);
  }

  public void RemovePatch(PatchInfoBase info)
  {
    if (!_InstalledPatches.Contains(info)) return;

    info.Remove(Harmony);
    _InstalledPatches.Remove(info);
  }

  public void RemoveAllPatches()
  {
    foreach (PatchInfoBase info in _InstalledPatches)
    {
      info.Remove(Harmony);
    }

    _InstalledPatches.Clear();
  }

}