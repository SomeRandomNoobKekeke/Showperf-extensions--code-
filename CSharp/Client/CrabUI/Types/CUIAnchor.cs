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

  // TODO ackshually i think this could be just vector
  public class CUIAnchor
  {
    public CUIAnchorType Type;

    public Vector2 Direction => Type switch
    {
      CUIAnchorType.LeftTop => new Vector2(1, 1),
      CUIAnchorType.LeftCenter => new Vector2(1, 0),
      CUIAnchorType.LeftBottom => new Vector2(1, -1),
      CUIAnchorType.CenterTop => new Vector2(0, 1),
      CUIAnchorType.CenterCenter => new Vector2(0, 0),
      CUIAnchorType.CenterBottom => new Vector2(0, -1),
      CUIAnchorType.RightTop => new Vector2(-1, 1),
      CUIAnchorType.RightCenter => new Vector2(-1, 0),
      CUIAnchorType.RightBottom => new Vector2(-1, -1),
    };


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

    public static Vector2 PosIn(CUIRect rect, CUIAnchorType Type) => Type switch
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

    public static Vector2 GetChildPos(CUIRect parent, CUIAnchorType Type, Vector2 offset, Vector2 childSize)
    {
      return PosIn(parent, Type) + offset - PosIn(new CUIRect(childSize), Type);
    }

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