using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.Text.Encodings.Web;

namespace ShowperfExtensions;

public class MetadataCallGraphSerializer : ICallGraphSerializer
{
  public Module BarotraumaModule = typeof(Barotrauma.GameMain).Assembly.Modules.First();

  public string Serialize(CallGraph callGraph)
  {
    Dictionary<int, IEnumerable<int>> calledMethodsDict = new();
    foreach (var (method, calledMethods) in callGraph.CalledMethods)
    {
      calledMethodsDict[method.MetadataToken] = calledMethods.Select(mb => mb.MetadataToken);
    }

    Dictionary<int, IEnumerable<int>> overridesDict = new();
    foreach (var (baseDef, overrides) in callGraph.Overrides)
    {
      overridesDict[baseDef.MetadataToken] = overrides.Select(mb => mb.MetadataToken);
    }

    return JsonSerializer.Serialize(new Dictionary<string, object>
    {
      ["Called Methods"] = calledMethodsDict,
      ["Overrides"] = overridesDict,
    }, new JsonSerializerOptions()
    {
      // WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
  }


  public CallGraph Parse(string raw)
  {
    Dictionary<string, Dictionary<int, IEnumerable<int>>> dict = [];
    CallGraph callGraph = new();

    try
    {
      dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, IEnumerable<int>>>>(raw);
    }
    catch (Exception e)
    {
      Logger.Default.Error(e);
      return callGraph;
    }

    Dictionary<int, IEnumerable<int>> calledMethodsDict = dict.GetValueOrDefault("Called Methods") ?? [];
    foreach (var (method, calledMethods) in calledMethodsDict)
    {
      MethodBase mb = BarotraumaModule.ResolveMethod(method);
      if (mb is null)
      {
        Logger.Default.Warning($"Couldn't deserialize [{method}]");
        continue;
      }
      callGraph.CalledMethods[mb] = calledMethods.Select(m => BarotraumaModule.ResolveMethod(m)).ToList();
    }

    Dictionary<int, IEnumerable<int>> overridesDict = dict.GetValueOrDefault("Overrides") ?? [];
    foreach (var (baseDef, overrides) in overridesDict)
    {
      MethodBase mb = BarotraumaModule.ResolveMethod(baseDef);
      if (mb is null)
      {
        Logger.Default.Warning($"Couldn't deserialize [{baseDef}]");
        continue;
      }
      callGraph.Overrides[mb] = overrides.Select(m => BarotraumaModule.ResolveMethod(m)).ToList();
    }

    callGraph.Connect();

    return callGraph;
  }



}