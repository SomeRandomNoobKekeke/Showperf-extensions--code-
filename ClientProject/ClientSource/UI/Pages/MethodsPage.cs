using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class MethodsPage : CUIPage
{
  public const string ShowMethodDetails_Command = "show method details";
  public const string OpenInMethodBrowser_Command = "open in method browser";


  public TypeExplorerPage TypeExplorerPage { get; } = new();
  public CallGraphBrowserPage CallGraphBrowserPage { get; } = new();
  public MethodDetailsPage MethodDetailsPage { get; } = new();

  public CUIPages Pages { get; }

  public MethodsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIDefault.HorizontalPanel();
    this["layout"]["main"] = Pages = new CUIPages() { Flex = 1 };


    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "ShowperfFrame pages";
      btn.CullChildren = true;
    }))
    {
      this["layout"]["header"]["CallGraphBrowser"] = new CUIRadioButton("Call Graph")
      {
        OnSelected = () => Pages.OpenPage(CallGraphBrowserPage),
      };

      this["layout"]["header"]["Browse Methods"] = new CUIRadioButton("Type Explorer")
      {
        OnSelected = () => Pages.OpenPage(TypeExplorerPage),
      };

      this["layout"]["header"]["Method Details"] = new CUIRadioButton("Details")
      {
        OnSelected = () => Pages.OpenPage(MethodDetailsPage),
      };
    }


    Commands.ListenFor<MethodBase>(ShowMethodDetails_Command, (mb) =>
    {
      Get<CUIRadioButton>("layout.header.Method Details").Select();
      MethodDetailsPage.Method = mb;
    });

    Commands.ListenFor(OpenInMethodBrowser_Command, (o) =>
    {
      Get<CUIRadioButton>("layout.header.Browse Methods").Select();
      TypeExplorerPage.GoToUnknown(o);
    });


    Get<CUIRadioButton>("layout.header.CallGraphBrowser").Select();
  }

}