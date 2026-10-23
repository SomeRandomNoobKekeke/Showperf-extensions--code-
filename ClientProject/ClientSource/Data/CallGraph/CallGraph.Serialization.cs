using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.Text.Encodings.Web;

namespace ShowperfExtensions;

public partial class CallGraph
{
  public string Serialize()
  {
    Dictionary<string, IEnumerable<string>> dict = new();
    foreach (var (method, node) in this)
    {
      string key = MethodBaseParser.Serialize(method);

      if (dict.ContainsKey(key))
      {
        // i think it's the only case so whatever
        // https://github.com/FakeFishGames/Barotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Text/TextManager.cs#L491
        Logger.Default.Warning($"CallGraph has duplicate: {method.DeclaringType}.{method}");
      }

      dict[MethodBaseParser.Serialize(method)] = node.CalledMethods.Select(MethodBaseParser.Serialize);
    }

    return JsonSerializer.Serialize(dict, new JsonSerializerOptions()
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
  }
  public static CallGraph Parse(string raw)
  {
    Dictionary<string, IEnumerable<string>> dict = [];

    try
    {
      dict = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<string>>>(raw);
    }
    catch (Exception e)
    {
      Logger.Default.Error(e);
    }

    CallGraph callGraph = new();
    foreach (var (method, calledMethods) in dict)
    {
      MethodBase mb = MethodBaseParser.Parse(method);
      if (mb is null)
      {
        Logger.Default.Warning($"Couldn't deserialize [{method}]");
        continue;
      }
      Node node = callGraph[mb] = new Node(mb);
      node.CalledMethods = calledMethods.Select(MethodBaseParser.Parse).Where(mb => mb != null).ToList();
    }

    callGraph.Connect();

    return callGraph;
  }

  /// <summary>
  /// should be called once
  /// </summary>
  internal void Connect()
  {
    foreach (var (method, node) in this)
    {
      foreach (MethodBase called in node.CalledMethods)
      {
        if (this.TryGetValue(called, out Node calledNode))
        {
          calledNode.CallingMethods.Add(method);
        }
        // else
        // {
        //   Logger.Default.Warning($"Called method is not in CallGraph [{method}]->[{called}]");
        // }
      }
    }
  }
}