using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public class CallGraphExplorer
{
  public CallGraph CallGraph { get; set; } = new();

  public MethodBase? Current { get; set; }

  public List<MethodBase> GetCallingMethods()
  {
    if (Current is null) return [];
    return CallGraph.CallingMethods.TryGetValue(Current, out List<MethodBase>? methods) ? methods : [];
  }

  public List<MethodBase> GetCalledMethods()
  {
    if (Current is null) return [];
    return CallGraph.CalledMethods.TryGetValue(Current, out List<MethodBase>? methods) ? methods : [];
  }

  public List<MethodBase> GetOverridenIn()
  {
    if (Current is null) return [];
    return CallGraph.OverridenIn.TryGetValue(Current, out List<MethodBase>? methods) ? methods : [];
  }

  public bool IsOverride()
  {
    if (Current is not MethodInfo mi) return false;
    return mi.GetBaseDefinition() != mi;
  }

  public MethodInfo GetBaseDefinition()
  {
    return (Current as MethodInfo).GetBaseDefinition();
  }
}