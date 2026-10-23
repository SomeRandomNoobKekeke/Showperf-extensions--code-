using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public class CustomPatchRepo
{
  private Dictionary<string, PatchInfo> _Patches = new();
  public ImmutableDictionary<string, PatchInfo> Patches => _Patches.ToImmutableDictionary();


  public void Add(PatchInfo info) => _Patches[info.ID] = info;
}