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
  public void TogglePatch(PatchInfoBase info)
  {
    Logger.Default.LogVars(_InstalledPatches.Contains(info));
    Logger.Default.LogVars(Logger.Wrap.Props(info));
    Logger.Default.LogVars(Logger.Wrap.IEnumerable(
      InstalledPatches.Where(i => i.TargetMethod == info.TargetMethod)
    ));


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