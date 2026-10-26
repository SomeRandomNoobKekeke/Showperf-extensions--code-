using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public class CustomPatchRepo
{
  private Dictionary<string, PatchInfoBase> _Patches = new();
  public ImmutableDictionary<string, PatchInfoBase> Patches => _Patches.ToImmutableDictionary();


  public void Add(PatchInfoBase info) => _Patches[info.ID] = info;
}