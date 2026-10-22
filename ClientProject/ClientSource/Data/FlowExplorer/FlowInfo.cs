using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

public partial class FlowInfo : Dictionary<MethodBase, MethodBase[]>
{
  public void Merge(FlowInfo other)
  {
    foreach (var (key, value) in other)
    {
      this[key] = value;
    }
  }

  public override string ToString() => Serialize();

}