using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class ShowperfFrame : CUIDefault.Frame
{
  public StopwatchWrapper StopwatchWrapper = new();
  public CustomPatchesPage CustomPatchesPage { get; } = new();

  public void OpenPage(CUIPage page)
  {
    Get<CUIPages>("layout.main").Open(page);
  }

  public ShowperfFrame() : base("Showperf", 400, 600)
  {
    Anchor = CUIAnchor.RightCenter;
    OnOpen += DeepRefresh;
    ResizableBoth = true;

    this["layout"]["handle"].Children.Insert(0, new ReloadLuaButton());
    this["layout"]["header"] = new CUIDefault.HorizontalPanel();

    using (new CUIContextStyle<CUIButton>(btn => btn.Flex = 1))
    {
      this["layout"]["header"]["Stopwatch Wrapper"] = new CUIButton("Stopwatch Wrapper")
      {
        OnMouseDown = (e) => OpenPage(StopwatchWrapper),
      };

      this["layout"]["header"]["Custom Patches"] = new CUIButton("Custom Patches")
      {
        OnMouseDown = (e) => OpenPage(CustomPatchesPage),
      };
    }
    this["layout"]["header"].DeepPalette = CUICore.Palettes.Secondary;

    this["layout"]["main"] = new CUIPages()
    {
      Flex = 1,
    };

    OpenPage(StopwatchWrapper);
  }
}