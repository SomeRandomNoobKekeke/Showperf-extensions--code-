using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public class FlowExplorer
{
  public FlowInfo FlowInfo { get; set; }

  public MethodBase Current { get; set; }

  public bool Contains(MethodBase mb) => FlowInfo.ContainsKey(mb);

  public MethodBase[] GetCalledMethods()
    => FlowInfo.TryGetValue(Current, out MethodBase[] called) ? called : [];

}