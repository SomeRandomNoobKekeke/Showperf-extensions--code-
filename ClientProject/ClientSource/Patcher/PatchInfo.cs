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
  public string Name { get; set; } = "???";
  public MethodBase TargetMethod { get; set; } = targetMethod ?? throw new ArgumentNullException(nameof(TargetMethod));
  public MethodInfo PatchMethod { get; set; } = patchMethod ?? throw new ArgumentNullException(nameof(PatchMethod));
  public HarmonyPatchType PatchType { get; set; } = patchType;
}