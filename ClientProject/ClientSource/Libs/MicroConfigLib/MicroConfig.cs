using System.Reflection;
using System.IO;
using System.Text.Json;
using CUILibs;
using CursedUI;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace MicroConfigLib;

public class MicroConfig(string savePath)
{
  public string SaveDirPath => Path.GetDirectoryName(SavePath);
  public string SavePath { get; set; } = savePath;

  public ILogger Logger => CUILibs.Logger.Default;

  public IEnumerable<PropertyInfo> SerializableProps
    => GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(pi => pi.CanRead && pi.CanWrite)
      .Where(pi => pi.DeclaringType == GetType())
      .Where(pi => pi.GetCustomAttribute<DontSerialize>() == null);

  public void Load()
  {
    if (!File.Exists(SavePath)) return;

    Dictionary<string, string> dict = [];
    try
    {
      dict = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SavePath)) ?? [];
    }
    catch (Exception e)
    {
      Logger.Warning($"{this} couldn't load [{SavePath}]:\n{e.Message}");
      return;
    }

    foreach (PropertyInfo pi in SerializableProps)
    {
      if (dict.TryGetValue(pi.Name, out string raw))
      {
        pi.SetValue(this, CUICore.Parser.Parse(raw, pi.PropertyType));
      }
    }
  }

  public void Save()
  {
    if (!Directory.Exists(SaveDirPath)) Directory.CreateDirectory(SaveDirPath);

    Dictionary<string, string> values = SerializableProps.ToDictionary(
      pi => pi.Name,
      pi => CUICore.Parser.Serialize(pi.GetValue(this))
    );

    File.WriteAllText(
      SavePath,
      JsonSerializer.Serialize(
        values,
        new JsonSerializerOptions()
        {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        }
      )
    );
  }
}