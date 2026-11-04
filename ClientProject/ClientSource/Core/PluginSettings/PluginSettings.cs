using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using CUILibs;
using System.Text.Json;
using System.IO;
using MicroConfigLib;
using CursedUI;
namespace ShowperfExtensions;

public partial class PluginSettings : MicroConfig
{
  public bool FrameAboveGUI { get; set; } = true;

  public static CUIPalettes DefaultPalettes => new CUIPalettes()
  {
    Primary = CUIPalette.FromColor(new Color(92, 92, 92, 255)),
    Secondary = CUIPalette.FromColor(new Color(0, 191, 221, 255)),
    Tertiary = CUIPalette.FromColor(new Color(0, 113, 183, 255)),
    Quaternary = CUIPalette.FromColor(new Color(74, 45, 153, 255)),
  };

  public CUIPalettes Palettes { get; set; } = DefaultPalettes;

  public void ResetPalettes()
  {
    Palettes.SoftReplaceBy(DefaultPalettes);
    Save();
  }

  public Color[] HeatPalette = [
    Color.MediumSpringGreen,
    Color.Yellow,
    Color.Orange,
    Color.Red,
    Color.Magenta,
    Color.Magenta
  ];
  public Color Gradient(float l) => ToolBox.GradientLerp(l, HeatPalette);


  public PluginSettings(string savePath) : base(savePath)
  {
    Reactive.Self = this;
    Load();
  }
}