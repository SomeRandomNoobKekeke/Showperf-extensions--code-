using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class ShowperfFrame : CUIDefault.Frame
{
  public const string ShowMethodDetails_Command = "show method details";
  public const string OpenInMethodBrowser_Command = "open in method browser";


  public CUIPages Pages { get; }
  public MethodBrowserPage MethodBrowserPage = new();
  public CustomPatchesPage CustomPatchesPage { get; } = new();
  public MethodDetailsPage MethodDetailsPage { get; } = new();

  public ShowperfFrame() : base("Showperf", 400, 600)
  {
    Anchor = CUIAnchor.Center;
    OnOpen += DeepRefresh;
    ResizableBoth = true;



    this["layout"]["handle"].Children.Insert(0, new ReloadLuaButton());
    this["layout"]["header"] = new CUIDefault.HorizontalPanel();
    this["layout"]["main"] = Pages = new CUIPages() { Flex = 1 };

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "ShowperfFrame pages";
    }))
    {
      this["layout"]["header"]["Browse Methods"] = new CUIRadioButton("Browse Methods")
      {
        OnSelected = () => Pages.OpenPage(MethodBrowserPage),
        IsSelected = true,
      };

      this["layout"]["header"]["Custom Patches"] = new CUIRadioButton("Custom Patches")
      {
        OnSelected = () => Pages.OpenPage(CustomPatchesPage),
      };

      this["layout"]["header"]["Method Details"] = new CUIRadioButton("Method Details")
      {
        OnSelected = () => Pages.OpenPage(MethodDetailsPage),
      };
    }
    this["layout"]["header"].DeepPalette = CUICore.Palettes.Secondary;

    Commands.ListenFor<MethodBase>(ShowMethodDetails_Command, (mb) =>
    {
      Get<CUIRadioButton>("layout.header.Method Details").Select();
      MethodDetailsPage.Method = mb;
    });

    Commands.ListenFor(OpenInMethodBrowser_Command, (o) =>
    {
      Get<CUIRadioButton>("layout.header.Browse Methods").Select();
      MethodBrowserPage.GoToUnknown(o);
    });

    Pages.OpenPage(MethodBrowserPage);
  }
}