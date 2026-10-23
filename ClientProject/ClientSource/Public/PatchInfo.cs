using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public record PatchInfo(
  string ID,
  MethodBase TargetMethod,
  MethodInfo PatchMethod,
  HarmonyPatchType PatchType,
  string Description = ""
)
{
  public string ID { get; } = ID ?? throw new ArgumentNullException(nameof(ID));
  public string Description { get; init; } = Description ?? throw new ArgumentNullException(nameof(ID));
  public MethodBase TargetMethod { get; } = TargetMethod ?? throw new ArgumentNullException(nameof(PatchInfo.TargetMethod));
  public MethodInfo PatchMethod { get; } = PatchMethod ?? throw new ArgumentNullException(nameof(PatchInfo.PatchMethod));
  public HarmonyPatchType PatchType { get; } = PatchType;


  public virtual bool Equals(PatchInfo? other)
  {
    if (other is null) return false;
    if (TargetMethod != other.TargetMethod) return false;
    if (PatchMethod != other.PatchMethod) return false;
    if (ID != other.ID) return false;

    return true;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(
      ID,
      TargetMethod,
      PatchMethod,
      PatchType
    );
  }


}