using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using CUILibs;

namespace ShowperfExtensions;

public class MethodsPage : CUIPage
{
  public const string ShowMethodDetails_Command = "ShowMethodDetails_Command";
  public const string OpenInMethodBrowser_Command = "OpenInMethodBrowser_Command";
  public const string SelectMethod_Command = "SelectMethod_Command";
  public const string MethodPatched_Notification = "MethodPatched_Notification";


  public StopwatchWrapper StopwatchWrapper => Plugin.StopwatchWrapper;

  public TypeExplorerPage TypeExplorerPage { get; private set; } = new();
  public CallGraphBrowserPage CallGraphBrowserPage { get; private set; } = new();
  public MethodDetailsPage MethodDetailsPage { get; private set; } = new();

  public CUIHorizontalList Subpages { get; private set; }
  public CUIPages Main { get; private set; }

  public MethodLineComponent CurrentMethodLine { get; private set; }

  private MethodBase _Current; public MethodBase Current
  {
    get => _Current;
    set
    {
      _Current = value;

      CurrentMethodLine.Method = value;

      TypeExplorerPage.GoToMethod(Current);
      MethodDetailsPage.Method = Current;
      CallGraphBrowserPage.Current = Current;
    }
  }


  private CUIComponent CreateCurrentMethodBlock()
  {
    CUIComponent block = new CUIDefault.HorizontalPanel();

    block["label"] = new CUITextBlock("Selected Method: ");
    block["method line"] = CurrentMethodLine = new MethodLineComponent() { Flex = 1 };
    block.DeepPalette = CUICore.Palettes.Secondary2;

    return block;
  }

  public MethodsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIDefault.VerticalPanel();
    this["layout"]["main"] = Main = new CUIPages() { Flex = 1 };

    this["layout"]["header"]["subpages"] = Subpages = new CUIDefault.HorizontalPanel();

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "method page subpages";
      btn.CullChildren = true;
    }))
    {
      this["layout"]["header"]["subpages"]["CallGraphBrowser"] = new CUIRadioButton("Call Graph")
      {
        OnSelected = () => Main.OpenPage(CallGraphBrowserPage),
      };

      this["layout"]["header"]["subpages"]["Type Explorer"] = new CUIRadioButton("Type Explorer")
      {
        OnSelected = () => Main.OpenPage(TypeExplorerPage),
      };

      this["layout"]["header"]["subpages"]["Method Details"] = new CUIRadioButton("Details")
      {
        OnSelected = () => Main.OpenPage(MethodDetailsPage),
      };
    }
    this["layout"]["header"]["subpages"].DeepPalette = CUICore.Palettes.Secondary;


    this["layout"]["header"]["current method block"] = CreateCurrentMethodBlock();


    Commands.ListenFor<MethodBase>(ShowMethodDetails_Command, (mb) =>
    {
      Subpages.Get<CUIRadioButton>("Method Details").Select();
      MethodDetailsPage.Method = mb;
    });

    Commands.ListenFor(OpenInMethodBrowser_Command, (o) =>
    {
      Subpages.Get<CUIRadioButton>("Type Explorer").Select();
      TypeExplorerPage.GoToUnknown(o);
    });

    Commands.ListenFor<MethodBase>(SelectMethod_Command, (mb) =>
    {
      Current = mb;
    });


    Subpages.Get<CUIRadioButton>("Type Explorer").Select();
  }

}