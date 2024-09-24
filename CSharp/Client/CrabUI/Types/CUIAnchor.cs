using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public enum CUIAnchorType
  {
    LeftTop, CenterTop, RightTop,
    LeftCenter, CenterCenter, RightCenter,
    LeftBottom, CenterBottom, RightBottom,
  }


  public class CUIAnchor
  {
    public CUIAnchorType Type = CUIAnchorType.LeftTop;
    public Vector2 PosIn(CUIRect rect) => Type switch
    {
      CUIAnchorType.LeftTop => rect.LeftTop,
      CUIAnchorType.LeftCenter => rect.LeftCenter,
      CUIAnchorType.LeftBottom => rect.LeftBottom,
      CUIAnchorType.CenterTop => rect.CenterTop,
      CUIAnchorType.CenterCenter => rect.CenterCenter,
      CUIAnchorType.CenterBottom => rect.CenterBottom,
      CUIAnchorType.RightTop => rect.RightTop,
      CUIAnchorType.RightCenter => rect.RightCenter,
      CUIAnchorType.RightBottom => rect.RightBottom,
    };

    public Vector2 GetChildPos(CUIRect parent, Vector2 offset, Vector2 childSize)
    {
      return PosIn(parent) + offset - PosIn(new CUIRect(childSize));
    }

    public CUIAnchor(CUIAnchorType type = CUIAnchorType.LeftTop)
    {
      Type = type;
    }
  }
}