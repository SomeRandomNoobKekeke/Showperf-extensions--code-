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
    foreach (var (method, calledMethods) in this)
    {
      string key = MethodBaseParser.Serialize(method);

      if (dict.ContainsKey(key))
      {
        // i think it's the only case so whatever
        // https://github.com/FakeFishGames/Barotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Text/TextManager.cs#L491
        Logger.Default.Warning($"CallGraph has duplicate: {method.DeclaringType}.{method}");
      }

      dict[MethodBaseParser.Serialize(method)] = calledMethods.Select(MethodBaseParser.Serialize);
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

    CallGraph info = new();
    foreach (var (method, calledMethods) in dict)
    {
      MethodBase mb = MethodBaseParser.Parse(method);
      if (mb is null)
      {
        Logger.Default.Warning($"Couldn't deserialize [{method}]");
        continue;
      }

      info[mb] = calledMethods.Select(MethodBaseParser.Parse).Where(mb => mb != null).ToArray();
    }

    return info;
  }
}