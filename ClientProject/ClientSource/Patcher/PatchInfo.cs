using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public record PatchInfo(
  MethodBase targetMethod,
  MethodInfo patchMethod,
  HarmonyPatchType patchType = HarmonyPatchType.Prefix
)
{
  public string ID
  {
    get
    {
      StringBuilder sb = new();

      if (Owner is not null) sb.Append($"{Owner}|");
      if (Name is not null)
      {
        sb.Append(Name);
      }
      else
      {
        sb.Append($"{TargetMethod.DeclaringType.Name}.{TargetMethod.Name} {PatchType}");
      }

      return sb.ToString();
    }
  }

  public string? Owner { get; set; }
  public string? Name { get; set; }
  public MethodBase TargetMethod { get; set; } = targetMethod ?? throw new ArgumentNullException(nameof(TargetMethod));
  public MethodInfo PatchMethod { get; set; } = patchMethod ?? throw new ArgumentNullException(nameof(PatchMethod));
  public HarmonyPatchType PatchType { get; set; } = patchType;
}