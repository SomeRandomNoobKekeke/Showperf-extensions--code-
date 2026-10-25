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
  public Dictionary<MethodBase, List<MethodBase>> OverridenIn { get; set; } = [];
  public Dictionary<MethodBase, MethodBase> Overrides { get; set; } = [];
  public static ICallGraphSerializer Serializer => Plugin.CallGraphSerializer;

  public void Connect()
  {

    CallingMethods = [];

    foreach (var (method, calledMethods) in CalledMethods)
    {
      foreach (MethodBase mb in calledMethods)
      {
        if (!CallingMethods.ContainsKey(mb)) CallingMethods[mb] = new();
        CallingMethods[mb].Add(method);
      }
    }

    Overrides = [];
    foreach (var (method, overridenIn) in OverridenIn)
    {
      foreach (MethodBase mb in overridenIn)
      {
        Overrides[mb] = method;
      }
    }
  }

  public override string ToString() => Serializer.Serialize(this);
}