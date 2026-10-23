using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public class CallGraphExplorer
{
  public CallGraph CallGraph { get; set; }

  public MethodBase Current { get; set; }

  public bool Contains(MethodBase mb) => CallGraph.ContainsKey(mb);

  public MethodBase[] GetCalledMethods()
    => CallGraph.TryGetValue(Current, out MethodBase[] called) ? called : [];

}