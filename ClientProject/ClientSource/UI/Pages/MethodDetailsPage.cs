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

  public override void Refresh()
  {
    this["layout"].Get<CUIToggleButton>("wrap in stopwatch").State = StopWatchWrapper.IsPatched(Method);
  }


  public MethodDetailsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

    this["layout"]["wrap in stopwatch"] = new CUIToggleButton("Wrap In Stopwatch")
    {
      OnToggle = (state) => StopWatchWrapper.TogglePatch(Method),
    };

    this["layout"]["lorem"] = new CUITextBlock(Utils.Lorem)
    {
      ResizeStrategy = ResizeStrategy.Wrap,
    };
  }

}