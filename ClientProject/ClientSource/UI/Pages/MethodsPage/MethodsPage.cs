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
  public const string SelectMethod_Command = "SelectMethod_Command";


  public TypeExplorerPage TypeExplorerPage { get; } = Plugin.Get<TypeExplorerPage>();
  public CallGraphBrowserPage CallGraphBrowserPage { get; } = Plugin.Get<CallGraphBrowserPage>();
  public BookmarkedMethodsPage BookmarkedMethodsPage { get; } = Plugin.Get<BookmarkedMethodsPage>();

  public CUIHorizontalList PageButtons { get; }
  public CUIPages Main { get; }

  public MethodLineComponent CurrentMethodLine { get; private set; }

  private MethodBase _Current; public MethodBase Current
  {
    get => _Current;
    set
    {
      _Current = value;

      CurrentMethodLine.Method = value;

      TypeExplorerPage.GoToMethod(Current);
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
    this["layout"]["main"] = Main = new CUIPages()
    {
      Flex = 1,
      OnPageOpened = (page) => Plugin.FrameLogger?.Log(""),
    };

    this["layout"]["header"]["page buttons"] = PageButtons = new CUIDefault.HorizontalPanel();

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "method page subpages";
      btn.CullChildren = true;
    }))
    {
      this["layout"]["header"]["page buttons"]["Call Graph"] = new CUIRadioButton("Call Graph")
      {
        OnSelected = () => Main.OpenPage(CallGraphBrowserPage),
      };

      this["layout"]["header"]["page buttons"]["Type Explorer"] = new CUIRadioButton("Type Explorer")
      {
        OnSelected = () => Main.OpenPage(TypeExplorerPage),
      };

      this["layout"]["header"]["page buttons"]["Bookmarks"] = new CUIRadioButton("Bookmarks")
      {
        OnSelected = () => Main.OpenPage(BookmarkedMethodsPage),
      };
    }
    this["layout"]["header"]["page buttons"].DeepPalette = CUICore.Palettes.Secondary;
    this["layout"]["header"]["current method block"] = CreateCurrentMethodBlock();

    Commands.ListenFor<MethodBase>(SelectMethod_Command, (mb) => Current = mb);


    PageButtons.Get<CUIRadioButton>("Type Explorer").Select();
  }

}