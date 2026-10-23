using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

public partial class CallGraph
{
  public class Node(MethodBase method)
  {
    public MethodBase Method { get; set; } = method;
    public List<MethodBase> CallingMethods { get; set; } = [];
    public List<MethodBase> CalledMethods { get; set; } = [];
  }

}