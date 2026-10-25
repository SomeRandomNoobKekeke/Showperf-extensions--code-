using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class MethodDetailsPage : CUIPage
{
  public StopWatchWrapper StopWatchWrapper => Plugin.StopWatchWrapper;

  private MethodBase _Method; public MethodBase Method
  {
    get => _Method;
    set
    {
      _Method = value;
      Refresh();
    }
  }

  private CUIVerticalList Main;

  protected override void HandleOpen() => Refresh();


  public override void Refresh()
  {
    Main.Clear();

    if (Method is null)
    {
      Main["no method"] = new CUIDefault.SolidTextBlock("No Method");
      return;
    }

    Main["Wrapped In Stopwatch"] = new CUIToggleButton("Wrapped In Stopwatch")
    {
      State = StopWatchWrapper.IsPatched(Method),
      OnToggle = (state) => ToggleMethod(),
    };

    Main["lorem"] = new CUITextBlock(Utils.Lorem)
    {
      ResizeStrategy = ResizeStrategy.Wrap,
    };
  }


  private void ToggleMethod()
  {
    if (Method is null) return;
    StopWatchWrapper.TogglePatch(Method);
  }

  public MethodDetailsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["main"] = Main = new CUIVerticalList()
    {
      FitContent = new CUIBool2(false, true),

    };
  }

}