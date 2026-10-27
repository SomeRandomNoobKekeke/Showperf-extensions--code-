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

  public TypeExplorerPage TypeExplorerPage { get; } = new();
  public CallGraphBrowserPage CallGraphBrowserPage { get; } = new();
  public MethodDetailsPage MethodDetailsPage { get; } = new();

  public CUIHorizontalList Subpages { get; }
  public CUIPages Main { get; }

  public MethodLineComponent CurrentMethodLine { get; }

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

  private void HandleMethodPatched(MethodBase mb)
  {
    CurrentMethodLine.NotifyMethodPatched(mb);
    Main.OpenedPage.Commands.Execute(MethodPatched_Notification, mb);
  }

  public MethodsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIDefault.VerticalPanel();
    this["layout"]["main"] = Main = new CUIPages() { Flex = 1 };

    StopwatchWrapper.MethodPatched += HandleMethodPatched;


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


    CUIComponent currentMethodBlock = new CUIDefault.HorizontalPanel();
    this["layout"]["header"]["current method block"] = currentMethodBlock;
    currentMethodBlock["label"] = new CUITextBlock("Selected Method: ");
    currentMethodBlock["method line"] = CurrentMethodLine = new MethodLineComponent()
    {
      Flex = 1,
    };
    currentMethodBlock.DeepPalette = CUICore.Palettes.Secondary2;


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