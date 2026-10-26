using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public partial class CallGraphBrowserPage : CUIPage
{
  public static MethodInfo UpdateRoot => typeof(GameMain).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance)!;
  public static MethodInfo DrawRoot => typeof(GameMain).GetMethod("Draw", BindingFlags.NonPublic | BindingFlags.Instance)!;



  private CallGraphExplorer CallGraphExplorer => Plugin.CallGraphExplorer;
  private MethodBlock CalledFromBlock;
  private MethodBlock CallingToBlock;
  private MethodBlock OverridenInBlock;
  private MethodBlock OverridesBlock;

  private CUIVerticalList Main;
  public MethodBase Current
  {
    get => CallGraphExplorer.Current;
    set
    {
      CallGraphExplorer.Current = value;
      Refresh();
    }
  }


  protected override void HandleOpen() => Refresh();

  public override void Refresh()
  {
    Main.Scroll = 0;

    OverridesBlock.Methods = CallGraphExplorer.IsOverride() ? [CallGraphExplorer.GetBaseDefinition()] : [];
    CalledFromBlock.Methods = CallGraphExplorer.GetCallingMethods();
    CallingToBlock.Methods = CallGraphExplorer.GetCalledMethods();
    OverridenInBlock.Methods = CallGraphExplorer.GetOverridenIn();
  }


  private void SelectMethod(MethodBase mb)
  {
    Commands.SendUp(MethodsPage.SelectMethod_Command, mb);
  }

  public CallGraphBrowserPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

    this["layout"]["controls"] = new CUIDefault.VerticalPanel();

    this["layout"]["controls"]["home buttons"] = new CUIDefault.HorizontalPanel();
    this["layout"]["controls"]["home buttons"]["Update Root"] = new CUIButton("GameMain.Update()")
    {
      OnMouseDown = (e) => SelectMethod(UpdateRoot),
    };
    this["layout"]["controls"]["home buttons"]["Draw Root"] = new CUIButton("GameMain.Draw()")
    {
      OnMouseDown = (e) => SelectMethod(DrawRoot),
    };

    this["layout"]["main"] = new CUIDefault.ListWithScrollbar() { Flex = 1, };

    Main = this["layout"]["main"].Get<CUIVerticalList>("list");
    Main.Padding = new CUISizes(4, 4, 4, 4);

    Main["overrides block"] = OverridesBlock = new MethodBlock("Overrides:");
    Main["overriden in block"] = OverridenInBlock = new MethodBlock("Overriden In:");
    Main["called from block"] = CalledFromBlock = new MethodBlock("Called From:");
    Main["calling to block"] = CallingToBlock = new MethodBlock("Calling To:");

    Current = UpdateRoot;
    this.DeepPalette = CUICore.Palettes.Tertiary;
  }
}