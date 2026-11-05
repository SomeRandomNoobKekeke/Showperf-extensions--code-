using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using CUILibs;
using System.Diagnostics;
using Barotrauma;

namespace ShowperfExtensions;

public partial class MeasurementsPage : CUIPage
{
  public CaptureCore? Capture => Plugin.Capture;
  public PluginSettings Settings { get; set; }

  public CUIFastTextDump Dump { get; }
  private CUIDefault.ListWithScrollbar ScrollWrapper;
  private CUITextBlock SumTextBlock;




  public MeasurementsPage(PluginSettings settings)
  {
    Settings = settings;

    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

    this["layout"]["controls"] = CreateControlsBlock();

    this["layout"]["total"] = SumTextBlock = new CUITextBlock("...")
    {
      Borders = new CUISizes(bottom: 1),
    };

    this["layout"]["scroll wrapper"] = ScrollWrapper = new CUIDefault.ListWithScrollbar()
    {
      Flex = 1,
      List = {
        BottomGap = ResizeHandle.DefaultSize.Y * 2,
      }
    };
    ScrollWrapper.List.ScrollChanged += (scroll) => CullLines();

    this["layout"]["scroll wrapper"]["list"]["dump"] = Dump = new CUIFastTextDump()
    {
      Offset = new Vector2(10, 10),
      // Solid = true,
    };

    Capture!.Window.Rotated += RefreshData;
  }

}