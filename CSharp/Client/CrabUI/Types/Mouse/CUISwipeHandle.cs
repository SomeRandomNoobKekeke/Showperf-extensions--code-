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
    public Vector2 StartPosition;

    public void BeginSwipe(Vector2 cursorPos)
    {
      Grabbed = true;
      StartPosition = Host.Real.Position;
    }

    public void EndSwipe()
    {
      Grabbed = false;
      CUI.Main.OnSwipeEnd(this);
    }

    public void SwipeTo(Vector2 to)
    {
      // Vector2 pos = to - Host.Parent.Real.Position - GrabOffset;
      // Host.Absolute.Position = pos;
      // Host.InvokeOnDrag(pos.X, pos.Y);
    }

    public CUISwipeHandle(CUIComponent host)
    {
      Host = host;
    }
  }
}