using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using CUILibs;

namespace ShowperfExtensions;

public class BookmarkedMethodsPage : CUIPage
{
  public static BindingFlags Pls => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
  public static MethodInfo GameMainUpdate => typeof(GameMain).GetMethod("Update", Pls)!;
  public static MethodInfo GameMainDraw => typeof(GameMain).GetMethod("Draw", Pls)!;
  public static MethodInfo GameScreenUpdate => typeof(GameScreen).GetMethod("Update")!;
  public static MethodInfo GameScreenDraw => typeof(GameScreen).GetMethod("Draw")!;
  public static MethodInfo GameSessionStartRound => typeof(GameSession).GetMethod("StartRound", [
    typeof(LevelData),typeof(bool)
  ])!;


  public MethodBookmarksList MethodBookmarksList { get; } = Plugin.Get<MethodBookmarksList>();

  private MethodBlockComponent RootMethodsBlock;
  private MethodBlockComponent BookmarksBlock;

  protected override void HandleOpen() => Refresh();
  public override void Refresh()
  {
    BookmarksBlock.Methods = MethodBookmarksList.ToList();
  }


  public BookmarkedMethodsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new(0, 0, 1, 1) };
    this["layout"]["main"] = new CUIDefault.ListWithScrollbar()
    {
      Flex = 1,
      List = {
        BottomGap = ResizeHandle.DefaultSize.Y * 2 ,
        Padding= new CUISizes(4, 4, 4, 4),
      },
    };

    this["layout"]["main"]["list"]["roots"] = RootMethodsBlock = new MethodBlockComponent("Roots")
    {
      Methods = [GameMainUpdate, GameMainDraw, GameScreenUpdate, GameScreenDraw, GameSessionStartRound],
    };

    this["layout"]["main"]["list"]["bookmarks"] = BookmarksBlock = new MethodBlockComponent("Bookmarks");

    MethodBookmarksList.Changed += Refresh;
    DeepPalette = Plugin.Palettes.Tertiary;
  }


}