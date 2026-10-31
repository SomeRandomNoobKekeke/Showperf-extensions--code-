using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CustomPatchRepo
{
  private Dictionary<string, Entry> _Patches = new();
  public ImmutableDictionary<string, Entry> Patches => _Patches.ToImmutableDictionary();

  private Dictionary<MethodBase, HashSet<Entry>> _PatchesByMethod = new();

  public void Add(Entry entry)
  {
    _Patches[entry.ID] = entry;

    foreach (MethodBase method in entry.PatchInfo.Targets)
    {
      _PatchesByMethod.TryAdd(method, []);
      _PatchesByMethod[method].Add(entry);
    }
  }
}