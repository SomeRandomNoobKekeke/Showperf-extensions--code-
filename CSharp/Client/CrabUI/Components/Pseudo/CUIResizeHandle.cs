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
  public class CUIResizeHandle
  {
    public CUIComponent Host;
    public CUIRect Real;

    public CUIAnchor Anchor;
    public CUINullRect Absolute;

    public Color BackgroundColor = Color.White;
    public Color GrabbedColor = Color.Cyan;
    public Vector2 GrabOffset;

    public bool Grabbed;
    public bool Visible = false;

    public bool IsHit(Vector2 cursorPos) => Visible && Real.Contains(cursorPos);

    public void BeginResize(Vector2 cursorPos)
    {
      Grabbed = true;
      GrabOffset = cursorPos - Real.Position;
    }

    public void EndResize()
    {
      Grabbed = false;
    }

    // TODO check bugs when trying to resize beyond absolute min
    public void Resize(Vector2 cursorPos)
    {
      if (Anchor.Type == CUIAnchorType.RightBottom)
      {
        Host.Absolute.Width = Math.Max(Real.Width, cursorPos.X - Host.Real.Left - GrabOffset.X + Real.Width);
        Host.Absolute.Height = Math.Max(Real.Height, cursorPos.Y - Host.Real.Top - GrabOffset.Y + Real.Height);
        return;
      }

      if (Anchor.Type == CUIAnchorType.LeftBottom)
      {
        float w = Math.Max(
          Real.Width,
          Host.Real.Width - (cursorPos.X - Host.Real.Left - GrabOffset.X)
        );

        Host.Absolute.Left = Host.Real.Right - w - Host.Parent.Real.Left;
        Host.Absolute.Width = w;
        Host.Absolute.Height = Math.Max(Real.Height, cursorPos.Y - Host.Real.Top - GrabOffset.Y + Real.Height);
        return;
      }
    }
    public void Update()
    {
      if (!Visible) return;

      float x, y, w, h;
      x = y = w = h = 0;

      if (Absolute.Left.HasValue) x = Absolute.Left.Value;
      if (Absolute.Top.HasValue) y = Absolute.Top.Value;
      if (Absolute.Width.HasValue) w = Absolute.Width.Value;
      if (Absolute.Height.HasValue) h = Absolute.Height.Value;

      Vector2 Pos = Anchor.PosOf(Host.Real) + new Vector2(x, y) - Anchor.PosOf(new CUIRect(0, 0, w, h));

      Real = new CUIRect(Pos, new Vector2(w, h));
    }
    public void Draw(SpriteBatch spriteBatch)
    {
      if (!Visible) return;
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, Grabbed ? GrabbedColor : BackgroundColor, isFilled: true);
    }

    public CUIResizeHandle(CUIComponent host, CUIAnchorType anchor)
    {
      Host = host;

      Anchor = new CUIAnchor(anchor);
      BackgroundColor = Host.BorderColor;

      Absolute = new CUINullRect(0, 0, 15, 15);
    }
  }
}