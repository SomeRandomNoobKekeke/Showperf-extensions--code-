using HarmonyLib;
// using System.Collections.Immutable; //TODO
using System.Reflection;

namespace ShowperfExtensions;

public partial class CustomPatchRepo
{
  public Dictionary<string, Entry> Patches = new();
  private Dictionary<MethodBase, HashSet<Entry>> PatchesByMethod = new();

  public void Add(Entry entry)
  {
    Patches[entry.ID] = entry;

    foreach (MethodBase method in entry.PatchInfo.Targets)
    {
      PatchesByMethod.TryAdd(method, []);
      PatchesByMethod[method].Add(entry);
    }
  }
}