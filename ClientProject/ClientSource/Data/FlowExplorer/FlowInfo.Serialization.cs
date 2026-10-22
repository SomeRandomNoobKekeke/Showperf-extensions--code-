using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.Text.Encodings.Web;

namespace ShowperfExtensions;

public partial class FlowInfo
{
  public string Serialize()
  {
    return JsonSerializer.Serialize(this.ToDictionary(
      kvp => MethodBaseParser.Serialize(kvp.Key),
      kvp => kvp.Value.Select(MethodBaseParser.Serialize)
    ), new JsonSerializerOptions()
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
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
      Logger.Default.Error(e);
    }

    FlowInfo info = new();
    foreach (var (method, calledMethods) in dict)
    {
      MethodBase mb = MethodBaseParser.Parse(method);
      if (mb is not null)
      {
        info[mb] = calledMethods.Select(MethodBaseParser.Parse).Where(mb => mb != null).ToArray();
      }
    }

    return info;
  }
}