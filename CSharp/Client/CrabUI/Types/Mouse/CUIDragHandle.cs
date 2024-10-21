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
  public class CUIDragHandle
  {
    public CUIComponent Host;
    public Vector2 GrabOffset;
    public bool Grabbed;
    public bool Draggable;
    public Vector2 StartPosition;
    public CUIMouseEvent Trigger = CUIMouseEvent.Down;

    public bool ShouldStart(CUIInput input)
    {
      return Draggable && (
        (Trigger == CUIMouseEvent.Down && input.MouseDown) ||
        (Trigger == CUIMouseEvent.DClick && input.DoubleClick)
      );
    }
    public void BeginDrag(Vector2 cursorPos)
    {
      Grabbed = true;
      GrabOffset = cursorPos - CUIAnchor.PosIn(Host);
      StartPosition = Host.Real.Position;
    }

    public void EndDrag()
    {
      Grabbed = false;
      CUI.Main.OnDragEnd(this);
    }

    public void DragTo(Vector2 to)
    {
      Vector2 pos = to - GrabOffset - Host.Parent.Real.Position;
      Host.SetAbsolute(Host.Absolute with { Position = pos });
      Host.InvokeOnDrag(pos.X, pos.Y);
    }

    public CUIDragHandle(CUIComponent host)
    {
      Host = host;
    }
  }
}