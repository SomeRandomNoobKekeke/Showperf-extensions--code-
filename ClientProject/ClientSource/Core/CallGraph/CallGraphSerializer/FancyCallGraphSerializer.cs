using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.Text.Encodings.Web;

namespace ShowperfExtensions;

public class FancyCallGraphSerializer : ICallGraphSerializer
{
  public string Serialize(CallGraph callGraph)
  {
    Dictionary<string, Dictionary<string, IEnumerable<string>>> finalDict = [];


    Dictionary<string, IEnumerable<string>> calledMethodsDict = [];

    foreach (var (method, calledMethods) in callGraph.CalledMethods)
    {
      string key = MethodBaseParser.Serialize(method);

      if (calledMethodsDict.ContainsKey(key))
      {
        // i think it's the only case so whatever
        // https://github.com/FakeFishGames/Barotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Text/TextManager.cs#L491
        Logger.Default.Warning($"CallGraph has duplicate: {method.DeclaringType}.{method}");
      }

      calledMethodsDict[key] = calledMethods.Select(MethodBaseParser.Serialize);
    }

    finalDict["Called Methods"] = calledMethodsDict;



    Dictionary<string, IEnumerable<string>> overridesDict = [];
    foreach (var (baseDef, overridenIn) in callGraph.OverridenIn)
    {
      overridesDict[MethodBaseParser.Serialize(baseDef)] = overridenIn.Select(MethodBaseParser.Serialize);
    }
    finalDict["Overrides"] = overridesDict;

    return JsonSerializer.Serialize(finalDict, new JsonSerializerOptions()
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
  }


  public CallGraph Parse(string raw)
  {
    Dictionary<string, Dictionary<string, IEnumerable<string>>> dict = [];
    CallGraph callGraph = new();

    try
    {
      dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, IEnumerable<string>>>>(raw);
    }
    catch (Exception e)
    {
      Logger.Default.Error(e);
      return callGraph;
    }


    Dictionary<string, IEnumerable<string>> calledMethodsDict = dict.GetValueOrDefault("Called Methods") ?? [];
    foreach (var (method, calledMethods) in calledMethodsDict)
    {
      MethodBase mb = MethodBaseParser.Parse(method);
      if (mb is null)
      {
        Logger.Default.Warning($"Couldn't deserialize [{method}]");
        continue;
      }
      callGraph.CalledMethods[mb] = calledMethods.Select(MethodBaseParser.Parse).Where(mb => mb != null).ToList();
    }

    Dictionary<string, IEnumerable<string>> overridesDict = dict.GetValueOrDefault("Overrides") ?? [];
    foreach (var (baseDef, overridenIn) in overridesDict)
    {
      MethodBase mb = MethodBaseParser.Parse(baseDef);
      if (mb is null)
      {
        Logger.Default.Warning($"Couldn't deserialize [{baseDef}]");
        continue;
      }
      callGraph.OverridenIn[mb] = overridenIn.Select(MethodBaseParser.Parse).Where(mb => mb != null).ToList();
    }

    callGraph.Connect();

    return callGraph;
  }



}