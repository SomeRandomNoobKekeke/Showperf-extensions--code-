using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public abstract class PatchInfoBase(string ID, string Description = "")
{
  public string ID { get; } = ID;
  public string Description { get; } = Description;

  public abstract MethodBase[] Targets { get; }


  public abstract void Install(Harmony harmony);
  public abstract void Remove(Harmony harmony);


  public static bool operator ==(PatchInfoBase a, PatchInfoBase b) => a.Equals(b);
  public static bool operator !=(PatchInfoBase a, PatchInfoBase b) => !a.Equals(b);

  public override bool Equals(object? obj)
  {
    if (obj is not PatchInfoBase other) return false;
    return ID == other.ID;
  }

  public override int GetHashCode() => ID.GetHashCode();
}


public class PatchInfo(
  string ID,
  MethodBase TargetMethod,
  MethodInfo PatchMethod,
  HarmonyPatchType PatchType,
  string Description = ""
) : PatchInfoBase(ID, Description)
{
  public MethodBase TargetMethod { get; } = TargetMethod ?? throw new ArgumentNullException(nameof(TargetMethod));
  public MethodInfo PatchMethod { get; } = PatchMethod ?? throw new ArgumentNullException(nameof(PatchMethod));
  public HarmonyPatchType PatchType { get; } = PatchType;

  public override MethodBase[] Targets { get; } = [TargetMethod];

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


public class MultiPatchInfo(
  string ID,
  IEnumerable<PatchInfo> Patches,
  string Description = ""
) : PatchInfoBase(ID, Description)
{
  public override MethodBase[] Targets { get; } = [.. Patches.Select(patch => patch.TargetMethod)];

  public IEnumerable<PatchInfo> Patches { get; } = Patches ?? throw new ArgumentNullException(nameof(Patches));

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