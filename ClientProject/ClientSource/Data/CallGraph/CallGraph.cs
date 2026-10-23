using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

public partial class CallGraph : Dictionary<MethodBase, MethodBase[]>
{
  public void Merge(CallGraph other)
  {
    foreach (var (key, value) in other)
    {
      this[key] = value;
    }
  }

  public override string ToString() => Serialize();

}