using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public class CallGraphExplorer
{
  public CallGraph CallGraph { get; set; } = new();

  public MethodBase? Current { get; set; }

  public bool Contains(MethodBase mb) => CallGraph.ContainsKey(mb);

  public List<MethodBase> GetCallingMethods()
    => CallGraph.TryGetValue(Current, out CallGraph.Node node) ? node.CallingMethods : [];
  public List<MethodBase> GetCalledMethods()
    => CallGraph.TryGetValue(Current, out CallGraph.Node node) ? node.CalledMethods : [];

}