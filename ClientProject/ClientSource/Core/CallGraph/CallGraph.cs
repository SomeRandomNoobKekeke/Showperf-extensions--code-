using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

/// <summary>
/// This thing just stores the data
/// </summary>
public partial class CallGraph
{
  public Dictionary<MethodBase, List<MethodBase>> CalledMethods { get; set; } = [];
  public Dictionary<MethodBase, List<MethodBase>> CallingMethods { get; set; } = [];
  public Dictionary<MethodBase, List<MethodBase>> OverridenIn { get; set; } = [];
  public Dictionary<MethodBase, MethodBase> Overrides { get; set; } = [];


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
}