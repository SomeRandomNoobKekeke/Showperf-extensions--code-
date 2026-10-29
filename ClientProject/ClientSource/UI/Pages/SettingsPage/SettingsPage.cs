using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using CUILibs;

namespace ShowperfExtensions;

public class SettingsPage : CUIPage
{

  private PluginSettings PluginSettings { get; } = Plugin.Get<PluginSettings>();

  private CUIVerticalList Main;

  protected override void HandleOpen() => Refresh();
  public override void Refresh()
  {
    Main.Clear();
    Main["AboveGUIBlock"] = new CUIDefault.HorizontalPanel();
    Main["AboveGUIBlock"]["tickbox"] = new CUICheckBox()
    {
      State = PluginSettings.FrameAboveGUI,
      OnToggle = (state) =>
      {
        PluginSettings.FrameAboveGUI = state;
        ShowperfFrame frame = Plugin.Get<ShowperfFrame>();

        frame.TargetMainComponent = PluginSettings.FrameAboveGUI ? CUI.TopMain : CUI.Main;
        if (frame.IsOpen) frame.Open();
      },
    };
    Main["AboveGUIBlock"]["label"] = new CUITextBlock("Draw frame above vanilla GUI");
  }


  public SettingsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new(0, 0, 1, 1) };
    this["layout"]["main"] = Main = new CUIVerticalList() { Flex = 1 };
    Background.Color = Color.Green;
  }


}