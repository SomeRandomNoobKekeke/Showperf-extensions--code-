using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CustomPatchRepo
{
  private Dictionary<string, Entry> _Patches = new();
  public ImmutableDictionary<string, Entry> Patches => _Patches.ToImmutableDictionary();


  public void Add(Entry entry) => _Patches[entry.PatchInfo.ID] = entry;
}