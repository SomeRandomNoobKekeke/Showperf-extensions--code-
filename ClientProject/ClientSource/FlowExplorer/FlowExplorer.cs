using System.Reflection;
using HarmonyLib;
using System.Text;

namespace ShowperfExtensions;

public class FlowExplorer
{
  public FlowInfo FlowInfo { get; set; }

  public MethodBase Current { get; set; }

  public MethodBase[] GetCalledMethods()
    => FlowInfo.Data.TryGetValue(Current, out MethodBase[] called) ? called : [];

}