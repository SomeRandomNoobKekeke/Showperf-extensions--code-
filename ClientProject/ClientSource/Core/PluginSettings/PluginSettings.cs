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