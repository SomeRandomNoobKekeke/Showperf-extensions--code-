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
  public MethodBookmarksList MethodBookmarksList { get; } = Plugin.Get<MethodBookmarksList>();

  private CUIVerticalList BookmarkList;


  public override void Refresh()
  {
    BookmarkList.Clear();

    foreach (MethodBase mb in MethodBookmarksList)
    {
      BookmarkList.Add(new MethodLineComponent(mb));
    }
  }


  public BookmarkedMethodsPage()
  {
    Background.Color = Color.Black;

    this["layout"] = new CUIVerticalList() { Relative = new(0, 0, 1, 1) };
    this["layout"]["bookmarks"] = BookmarkList = new CUIVerticalList() { Flex = 1 };
  }


}