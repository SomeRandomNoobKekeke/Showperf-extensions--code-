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
    Main["AboveGUIBlock"] = new CUIDefault.BoolField()
    {
      Text = "Draw frame above vanilla GUI",
      State = PluginSettings.FrameAboveGUI,
      OnToggle = (state) => PluginSettings.Reactive.FrameAboveGUI = state,
    };

    Main["edit palettes"] = new CUIDefault.IconButtonField(CUISprite.BrushAndPaletteIcon, "Edit Frame Palettes")
    {
      OnClick = () =>
      {
        CUIFrame frame = CUIPalette.OpenPreview(PluginSettings.Palettes);
        frame["layout"].Children.Insert(1, new CUIButton("Reset Palettes") //HACK
        {
          OnClick = () => PluginSettings.ResetPalettes(),
        });

        frame.OnClose += () => PluginSettings.Save();
      },
    };
  }


  public SettingsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new(0, 0, 1, 1) };
    this["layout"]["main"] = Main = new CUIVerticalList() { Flex = 1 };
  }


}