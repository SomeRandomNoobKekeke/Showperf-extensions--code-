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
  //TODO i could use 2 different anchors for parent and child
  public class CUIAnchor
  {
    public static Vector2 Direction(Vector2 anchor)
    {
      return Vector2.One - anchor * 2;
    }

    public static Vector2 PosIn(CUIComponent host) => PosIn(host.Real, host.Anchor);
    public static Vector2 PosIn(CUIRect rect, Vector2 anchor)
    {
      return new Vector2(
        rect.Left + rect.Width * anchor.X,
        rect.Top + rect.Height * anchor.Y
      );
    }

    public static Vector2 GetChildPos(CUIRect parent, Vector2 anchor, Vector2 offset, Vector2 childSize)
    {
      return PosIn(parent, anchor) + offset - PosIn(new CUIRect(childSize), anchor);
    }
  }
}