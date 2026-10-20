using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class ShowperfFrame : CUIDefault.Frame
{
  public MethodBrowserPage MethodBrowser = new();
  public CustomPatchesPage CustomPatchesPage { get; } = new();

  public void OpenPage(CUIPage page)
  {
    Get<CUIPages>("layout.main").OpenPage(page);
  }

  public ShowperfFrame() : base("Showperf", 400, 600)
  {
    Anchor = CUIAnchor.Center;
    OnOpen += DeepRefresh;
    ResizableBoth = true;

    this["layout"]["handle"].Children.Insert(0, new ReloadLuaButton());
    this["layout"]["header"] = new CUIDefault.HorizontalPanel();

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "ShowperfFrame pages";
    }))
    {
      this["layout"]["header"]["Browse Methods"] = new CUIRadioButton("Browse Methods")
      {
        OnSelected = () => OpenPage(MethodBrowser),
        IsSelected = true,
      };

      this["layout"]["header"]["Custom Patches"] = new CUIRadioButton("Custom Patches")
      {
        OnMouseDown = (e) => OpenPage(CustomPatchesPage),
      };
    }
    this["layout"]["header"].DeepPalette = CUICore.Palettes.Secondary;

    this["layout"]["main"] = new CUIPages()
    {
      Flex = 1,
    };

    OpenPage(MethodBrowser);
  }
}