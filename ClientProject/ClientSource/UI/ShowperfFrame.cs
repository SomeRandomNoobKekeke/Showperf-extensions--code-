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
  public MeasurementsPage MeasurementsPage { get; } = new();

  private CUIComponent PageButtons;

  public ShowperfFrame() : base("Showperf", 600, 600)
  {
    Anchor = CUIAnchor.Center;
    OnOpen += DeepRefresh;
    ResizableBoth = true;

    this["layout"]["handle"].Children.Insert(0, new ReloadLuaButton());
    this["layout"]["page buttons"] = PageButtons = new CUIDefault.HorizontalPanel();
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
      this["layout"]["page buttons"]["Measurements"] = new CUIRadioButton("Measurements")
      {
        OnSelected = () => Pages.OpenPage(MeasurementsPage),
      };
      this["layout"]["page buttons"]["Custom Patches"] = new CUIRadioButton("Patches")
      {
        OnSelected = () => Pages.OpenPage(CustomPatchesPage),
      };
      this["layout"]["page buttons"]["Methods"] = new CUIRadioButton("Methods")
      {
        OnSelected = () => Pages.OpenPage(MethodsPage),
      };
    }

    PageButtons.Get<CUIRadioButton>("Methods").Select();
    // MethodsPage.Subpages.Get<CUIRadioButton>("Type Explorer").Select();
  }
}