using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;

public class Settings
{

  public Color[] HeatPalette { get; set; } = [
    Color.MediumSpringGreen,
    Color.Yellow,
    Color.Orange,
    Color.Red,
    Color.Magenta,
    Color.Magenta
  ];
  public Color Gradient(float l) => ToolBox.GradientLerp(l, HeatPalette);
}