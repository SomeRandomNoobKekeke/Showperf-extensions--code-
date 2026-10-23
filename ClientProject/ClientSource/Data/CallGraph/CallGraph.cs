using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

public partial class CallGraph : Dictionary<MethodBase, CallGraph.Node>
{
  public override string ToString() => Serialize();
}