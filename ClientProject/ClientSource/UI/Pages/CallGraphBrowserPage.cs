using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class CallGraphBrowserPage : CUIPage
{
  public static MethodInfo UpdateRoot => typeof(GameMain).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance)!;
  public static MethodInfo DrawRoot => typeof(GameMain).GetMethod("Draw", BindingFlags.NonPublic | BindingFlags.Instance)!;

  public CallGraphExplorer CallGraphExplorer => Plugin.CallGraphExplorer;
  public CUIVerticalList CalledFromList { get; }
  public CUIVerticalList CallingToList { get; }
  public CUIVerticalList OverridesList { get; }
  public CUIVerticalList Main { get; }
  public CUITextBlock LocationText { get; }


  public MethodBase Current
  {
    get => CallGraphExplorer.Current;
    set
    {
      CallGraphExplorer.Current = value;
      Refresh();
    }
  }

  public CUIComponent CreateMethodButton(MethodBase mb)
  {
    return new CUIButton(MethodBaseParser.Serialize(mb))
    {
      TextAnchor = CUIAnchor.LeftCenter,
      OnMouseDown = (e) => Current = mb,
    };
  }

  public override void Refresh()
  {
    LocationText.Text = MethodBaseParser.Serialize(Current);
    Main.Scroll = 0;

    CalledFromList.Clear();
    foreach (MethodBase mb in CallGraphExplorer.GetCallingMethods())
    {
      CalledFromList.Add(CreateMethodButton(mb));
    }

    CallingToList.Clear();
    foreach (MethodBase mb in CallGraphExplorer.GetCalledMethods())
    {
      CallingToList.Add(CreateMethodButton(mb));
    }

    OverridesList.Clear();
    foreach (MethodBase mb in CallGraphExplorer.GetOverrides())
    {
      OverridesList.Add(CreateMethodButton(mb));
    }
  }


  public CallGraphBrowserPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

    this["layout"]["controls"] = new CUIDefault.VerticalPanel();

    this["layout"]["controls"]["home buttons"] = new CUIDefault.HorizontalPanel();
    this["layout"]["controls"]["home buttons"]["Update Root"] = new CUIButton("GameMain.Update()")
    {
      OnMouseDown = (e) => Current = UpdateRoot,
    };
    this["layout"]["controls"]["home buttons"]["Draw Root"] = new CUIButton("GameMain.Draw()")
    {
      OnMouseDown = (e) => Current = DrawRoot,
    };

    this["layout"]["controls"]["location"] = new CUIDefault.HorizontalPanel();
    this["layout"]["controls"]["location"]["method name"] = LocationText = new CUITextBlock();


    this["layout"]["main"] = new CUIDefault.ListWithScrollbar() { Flex = 1, };

    Main = this.Get<CUIVerticalList>("layout.main.list");

    Main["called from block"] = new CUIDefault.VerticalBackPanel();
    Main["called from block"]["caption"] = new CUITextBlock("Called from:")
    {
      TextAnchor = CUIAnchor.LeftCenter,
    };
    Main["called from block"]["methods"] = CalledFromList = new CUIDefault.VerticalPanel();



    Main["calling to block"] = new CUIDefault.VerticalBackPanel();
    Main["calling to block"]["caption"] = new CUITextBlock("Calling to:")
    {
      TextAnchor = CUIAnchor.LeftCenter,
    }; ;
    Main["calling to block"]["methods"] = CallingToList = new CUIDefault.VerticalPanel();

    Main["overrides block"] = new CUIDefault.VerticalBackPanel();
    Main["overrides block"]["caption"] = new CUITextBlock("Overrides:")
    {
      TextAnchor = CUIAnchor.LeftCenter,
    }; ;
    Main["overrides block"]["methods"] = OverridesList = new CUIDefault.VerticalPanel();

    Current = UpdateRoot;
  }
}