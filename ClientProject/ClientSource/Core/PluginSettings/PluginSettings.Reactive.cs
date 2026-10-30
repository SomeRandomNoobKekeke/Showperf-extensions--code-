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

public partial class PluginSettings
{
  public class Reactive_Part
  {
    public PluginSettings Self { get; set; }

    public bool FrameAboveGUI
    {
      get => Self.FrameAboveGUI;
      set
      {
        Self.FrameAboveGUI = value;
        Self.Save();

        ShowperfFrame frame = Plugin.Get<ShowperfFrame>();

        frame.TargetMainComponent = FrameAboveGUI ? CUI.TopMain : CUI.Main;
        if (frame.IsOpen)
        {
          frame.Close();
          frame.Open();
        }
      }
    }
  }

  public Reactive_Part Reactive { get; } = new();
}