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

  public CallGraphExplorer CallGraphExplorer { get; } = Plugin.Get<CallGraphExplorer>();

  private MethodBlockComponent ThisMethodBlock;
  private MethodBlockComponent CalledFromBlock;
  private MethodBlockComponent CallingToBlock;
  private MethodBlockComponent OverridenInBlock;
  private MethodBlockComponent OverridesBlock;

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

    ThisMethodBlock.Methods = [Current];
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

    this["layout"]["main"] = new CUIDefault.ListWithScrollbar()
    {
      Flex = 1,
      List = {
        BottomGap = ResizeHandle.DefaultSize.Y * 2 ,
        Padding= new CUISizes(4, 4, 4, 4),
      },
    };
    Main = this["layout"]["main"].Get<CUIVerticalList>("list");

    Main["this method block"] = ThisMethodBlock = new MethodBlockComponent("This Method:")
    {
      DeepPaletteImmunity = true,
      DeepPalette = CUIPalette.FromColor(Color.Lime),
    };
    Main["overrides block"] = OverridesBlock = new MethodBlockComponent("Overrides:");
    Main["overriden in block"] = OverridenInBlock = new MethodBlockComponent("Overriden In:");
    Main["called from block"] = CalledFromBlock = new MethodBlockComponent("Called From:");
    Main["calling to block"] = CallingToBlock = new MethodBlockComponent("Calling To:");

    Current = UpdateRoot;
    DeepPalette = CUICore.Palettes.Tertiary;
  }
}