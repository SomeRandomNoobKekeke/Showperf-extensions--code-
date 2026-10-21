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
      MethodName.Text = MethodBaseParser.Serialize(value);
    }
  }

  public CUITextBlock MethodName { get; }

  public MethodDetailsPage()
  {
    Background.Color = Color.Brown;

    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

    this["layout"]["location panel"] = new CUIDefault.HorizontalPanel();
    this["layout"]["location panel"]["go up"] = new CUIIconButton()
    {
      Icon = CUISprite.ArrowUpIcon,
      OnMouseDown = (e) => Commands.SendUp(ShowperfFrame.OpenInMethodBrowser_Command, Method),
    };
    this["layout"]["location panel"]["method name"] = MethodName = new CUITextBlock("No Method")
    {
      Flex = 1,
      TextAnchor = CUIAnchor.LeftCenter,
      Padding = new CUISizes(4, 4, 4, 4),
    };

    this["layout"]["lorem"] = new CUITextBlock(Utils.Lorem)
    {
      ResizeStrategy = ResizeStrategy.Wrap,
    };
  }

}