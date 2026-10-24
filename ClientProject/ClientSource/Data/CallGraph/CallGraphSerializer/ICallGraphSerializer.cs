using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.Text.Encodings.Web;

namespace ShowperfExtensions;

public interface ICallGraphSerializer
{
  public string Serialize(CallGraph callGraph);
  public CallGraph Parse(string raw);
}