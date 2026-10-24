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

  public List<MethodBase> GetOverrides()
  {
    if (Current is null) return [];
    return CallGraph.Overrides.TryGetValue(Current, out List<MethodBase>? methods) ? methods : [];
  }
}