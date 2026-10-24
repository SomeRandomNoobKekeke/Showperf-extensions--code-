using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

public partial class CallGraph
{
  public Dictionary<MethodBase, List<MethodBase>> CalledMethods { get; set; } = [];
  public Dictionary<MethodBase, List<MethodBase>> CallingMethods { get; set; } = [];
  public Dictionary<MethodBase, List<MethodBase>> Overrides { get; set; } = [];

  public static ICallGraphSerializer Serializer => Plugin.CallGraphSerializer;

  public void Connect()
  {

    CallingMethods = new Dictionary<MethodBase, List<MethodBase>>();

    foreach (var (method, calledMethods) in CalledMethods)
    {
      foreach (MethodBase mb in calledMethods)
      {
        if (!CallingMethods.ContainsKey(mb)) CallingMethods[mb] = new();
        CallingMethods[mb].Add(method);
      }
    }
  }

  public override string ToString() => Serializer.Serialize(this);
}