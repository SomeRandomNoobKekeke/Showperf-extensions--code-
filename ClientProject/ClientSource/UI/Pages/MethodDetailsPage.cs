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

  }


  public MethodDetailsPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

    this["layout"]["lorem"] = new CUITextBlock(Utils.Lorem)
    {
      ResizeStrategy = ResizeStrategy.Wrap,
    };
  }

}