using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchInstaller
{
  private Harmony Harmony { get; } = new Harmony("showperf.patches");
  private HashSet<PatchInfoBase> _InstalledPatches { get; } = new();
  public ImmutableHashSet<PatchInfoBase> InstalledPatches => _InstalledPatches.ToImmutableHashSet();

  public Dictionary<string, PatchInfoBase> PatchesByID
    => _InstalledPatches.ToDictionary(
        info => info.ID,
        info => info
      );


  public event Action<PatchInfoBase>? PatchInstalled;
  public event Action<PatchInfoBase>? PatchRemoved;

  public Dictionary<MethodBase, List<PatchInfoBase>> PatchesByMethod
  {
    get
    {
      Dictionary<MethodBase, List<PatchInfoBase>> dict = [];
      foreach (var info in _InstalledPatches)
      {
        dict.TryAdd(info.TargetMethod, []);
        dict[info.TargetMethod].Add(info);
      }
      return dict;
    }
  }


  public bool IsInstalled(PatchInfoBase info) => _InstalledPatches.Contains(info);
  public bool TogglePatch(PatchInfoBase info)
    => _InstalledPatches.Contains(info) ? RemovePatch(info) : InstallPatch(info);

  public bool InstallPatch(PatchInfoBase info)
  {
    if (_InstalledPatches.Contains(info)) return false;

    info.Install(Harmony);

    _InstalledPatches.Add(info);
    PatchInstalled?.Invoke(info);
    return true;
  }

  public bool RemovePatch(PatchInfoBase info)
  {
    if (!_InstalledPatches.Contains(info)) return false;

    info.Remove(Harmony);
    _InstalledPatches.Remove(info);
    PatchRemoved?.Invoke(info);
    return true;
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