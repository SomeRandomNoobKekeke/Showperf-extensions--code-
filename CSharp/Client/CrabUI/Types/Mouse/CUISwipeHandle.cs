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
  public class CUISwipeHandle
  {
    public CUIComponent Host;
    public bool Grabbed;
    public bool Swipeable;
    public Vector2 PrevPosition;
    public CUIMouseEvent Trigger = CUIMouseEvent.Down;
    public bool ShouldStart(CUIInput input)
    {
      return Swipeable && (
        (Trigger == CUIMouseEvent.Down && input.MouseDown) ||
        (Trigger == CUIMouseEvent.DClick && input.DoubleClick)
      );
    }

    public void BeginSwipe(Vector2 cursorPos)
    {
      Grabbed = true;
      PrevPosition = cursorPos;
    }

    public void EndSwipe()
    {
      Grabbed = false;
      CUI.Main.OnSwipeEnd(this);
    }

    public void Swipe(CUIInput input)
    {
      Host.ChildrenOffset = Host.ChildrenOffset.Shift(
        input.MousePositionDif.X,
        input.MousePositionDif.Y
      );
      Host.InvokeOnSwipe(input.MousePositionDif.X, input.MousePositionDif.Y);
    }

    public CUISwipeHandle(CUIComponent host)
    {
      Host = host;
    }
  }
}