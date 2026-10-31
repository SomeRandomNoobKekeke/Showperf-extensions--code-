using CUILibs;
using HarmonyLib;
// using System.Collections.Immutable; //TODO
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchInstaller
{
  private Harmony Harmony { get; } = new Harmony("showperf.patches");
  public HashSet<PatchInfoBase> InstalledPatches { get; } = new();

  public Dictionary<string, PatchInfoBase> PatchesByID
    => InstalledPatches.ToDictionary(
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
      foreach (var info in InstalledPatches)
      {
        foreach (MethodBase target in info.Targets)
        {
          dict.TryAdd(target, []);
          dict[target].Add(info);
        }
      }
      return dict;
    }
  }


  public bool IsInstalled(PatchInfoBase info) => InstalledPatches.Contains(info);
  public bool TogglePatch(PatchInfoBase info)
    => InstalledPatches.Contains(info) ? RemovePatch(info) : InstallPatch(info);

  public bool InstallPatch(PatchInfoBase info)
  {
    if (InstalledPatches.Contains(info)) return false;

    try
    {
      info.Install(Harmony);
    }
    catch (Exception e)
    {
      Logger.Default.Warning($"Couldn't install {info}: {e.Message}");
      Plugin.FrameLogger.Warning($"Couldn't install {info}: {e.Message}");
      return false;
    }

    InstalledPatches.Add(info);
    PatchInstalled?.Invoke(info);
    return true;
  }

  public bool RemovePatch(PatchInfoBase info)
  {
    if (!InstalledPatches.Contains(info)) return false;

    try
    {
      info.Remove(Harmony);
    }
    catch (Exception e)
    {
      Logger.Default.Warning($"Couldn't uninstall {info}: {e.Message}");
      return false;
    }

    InstalledPatches.Remove(info);
    PatchRemoved?.Invoke(info);
    return true;
  }

  public void RemoveAllPatches()
  {
    foreach (PatchInfoBase info in InstalledPatches)
    {
      info.Remove(Harmony);
    }

    InstalledPatches.Clear();
  }

}