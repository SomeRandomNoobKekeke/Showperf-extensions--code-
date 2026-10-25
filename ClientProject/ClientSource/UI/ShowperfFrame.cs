using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class ShowperfFrame : CUIDefault.Frame
{
  // public static ICUIStyle DefaultStyle => new CUIDefaultStyle<ShowperfFrame>((c) =>
  // {
  //   c.Background.Color = c.Palette["back"].To(Color.Transparent, 0.2f);
  // });

  public CUIPages Pages { get; }

  public CustomPatchesPage CustomPatchesPage { get; } = new();
  public MethodsPage MethodsPage { get; } = new();

  public ShowperfFrame() : base("Showperf", 600, 600)
  {
    Anchor = CUIAnchor.Center;
    OnOpen += DeepRefresh;
    ResizableBoth = true;

    this["layout"]["handle"].Children.Insert(0, new ReloadLuaButton());
    this["layout"]["header"] = new CUIDefault.HorizontalPanel();
    this["layout"]["main"] = Pages = new CUIPages()
    {
      Flex = 1,
      ConsumeMouseEvents = true,
    };

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "ShowperfFrame pages";
      btn.CullChildren = true;
    }))
    {
      this["layout"]["header"]["Custom Patches"] = new CUIRadioButton("Patches")
      {
        OnSelected = () => Pages.OpenPage(CustomPatchesPage),
      };
      this["layout"]["header"]["Methods"] = new CUIRadioButton("Methods")
      {
        OnSelected = () => Pages.OpenPage(MethodsPage),
      };
    }

    Get<CUIRadioButton>("layout.header.Custom Patches").Select();
  }
}