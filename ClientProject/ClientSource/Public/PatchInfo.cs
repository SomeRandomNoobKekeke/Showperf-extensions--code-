using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public abstract record PatchInfoBase(string ID, string Description = "")
{
  public abstract void Install(Harmony harmony);
  public abstract void Remove(Harmony harmony);
}

public record PatchInfo(
  string ID,
  MethodBase TargetMethod,
  MethodInfo PatchMethod,
  HarmonyPatchType PatchType,
  string Description = ""
) : PatchInfoBase(ID, Description)
{
  public override void Install(Harmony harmony)
  {
    switch (PatchType)
    {
      case HarmonyPatchType.Prefix:
        harmony.Patch(original: TargetMethod, prefix: new HarmonyMethod(PatchMethod));
        break;
      case HarmonyPatchType.Postfix:
        harmony.Patch(original: TargetMethod, postfix: new HarmonyMethod(PatchMethod));
        break;
      case HarmonyPatchType.Finalizer:
        harmony.Patch(original: TargetMethod, finalizer: new HarmonyMethod(PatchMethod));
        break;
      case HarmonyPatchType.Transpiler:
        harmony.Patch(original: TargetMethod, transpiler: new HarmonyMethod(PatchMethod));
        break;
      case HarmonyPatchType.ILManipulator:
        harmony.Patch(original: TargetMethod, ilmanipulator: new HarmonyMethod(PatchMethod));
        break;
    }
  }
  public override void Remove(Harmony harmony)
  {
    harmony.Unpatch(TargetMethod, PatchMethod);
  }
}

public record MultiPatchInfo(
  string ID,
  IEnumerable<PatchInfo> Patches,
  string Description = ""
) : PatchInfoBase(ID, Description)
{
  public override void Install(Harmony harmony)
  {
    foreach (PatchInfo patch in Patches)
    {
      patch.Install(harmony);
    }
  }

  public override void Remove(Harmony harmony)
  {
    foreach (PatchInfo patch in Patches)
    {
      patch.Remove(harmony);
    }
  }
}