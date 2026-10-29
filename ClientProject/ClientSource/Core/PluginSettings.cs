using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using CUILibs;
using System.Text.Json;
using System.IO;

namespace ShowperfExtensions;

public class PluginSettings
{
  public string DirPath => Path.Combine("Data", "Mods", ModInfo.Name);
  public string FilePath => Path.Combine(DirPath, "Settings.json");

  public bool FrameAboveGUI { get; set; } = true;

  public Color[] HeatPalette { get; set; } = [
    Color.MediumSpringGreen,
    Color.Yellow,
    Color.Orange,
    Color.Red,
    Color.Magenta,
    Color.Magenta
  ];
  public Color Gradient(float l) => ToolBox.GradientLerp(l, HeatPalette);




  public void Load()
  {
    if (!File.Exists(FilePath)) return;

    Dictionary<string, string> raw = [];
    try
    {
      raw = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(FilePath)) ?? [];
    }
    catch (Exception e)
    {
      Logger.Default.Warning($"couldn't load settings file: {e.Message}");
      return;
    }

    if (raw.TryGetValue("Frame Above GUI", out string? s))
    {
      if (bool.TryParse(s, out bool result))
      {
        FrameAboveGUI = result;
      }
    }


  }

  public void Save()
  {
    if (!Directory.Exists(DirPath)) Directory.CreateDirectory(DirPath);

    Dictionary<string, object> values = new()
    {
      ["Frame Above GUI"] = FrameAboveGUI,
    };

    File.WriteAllText(
      FilePath,
      JsonSerializer.Serialize(
        values,
        new JsonSerializerOptions() { WriteIndented = true, }
      )
    );
  }

  public PluginSettings() => Load();
}