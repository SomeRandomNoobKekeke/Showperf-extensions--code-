using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;

namespace ShowperfExtensions;

public class FlowInfo
{
  private static ILogger Logger => CUILibs.Logger.Default;


  public Dictionary<MethodBase, MethodBase[]> Data { get; set; } = [];




  public string Serialize()
  {
    return JsonSerializer.Serialize(Data.ToDictionary(
      kvp => MethodBaseParser.Serialize(kvp.Key),
      kvp => kvp.Value.Select(MethodBaseParser.Serialize)
    ), new JsonSerializerOptions()
    {
      WriteIndented = true,
    });
  }
  public static FlowInfo Parse(string raw)
  {
    Dictionary<string, IEnumerable<string>> dict = [];

    try
    {
      dict = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<string>>>(raw);
    }
    catch (Exception e)
    {
      Logger.Error(e);
    }

    FlowInfo info = new();
    foreach (var (method, calledMethods) in dict)
    {
      MethodBase mb = MethodBaseParser.Parse(method);
      if (mb is not null)
      {
        info.Data[mb] = calledMethods.Select(MethodBaseParser.Parse).Where(mb => mb != null).ToArray();
      }
    }

    return info;
  }


  public FlowInfo() { }
  public FlowInfo(Dictionary<MethodBase, MethodBase[]> data) => Data = data;
}