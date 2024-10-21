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

    public Vector2 Anchor;
    public CUINullRect Absolute;

    //TODO unhardcode these colors
    public Color BackgroundColor = Color.White * 0.25f;
    public Color GrabbedColor = Color.Cyan * 0.5f;
    public Vector2 GrabOffset;

    public bool Grabbed;
    public bool Visible = false;

    public CUIMouseEvent Trigger = CUIMouseEvent.Down;

    public bool ShouldStart(CUIInput input)
    {
      return Visible && Real.Contains(input.MousePosition) && (
        (Trigger == CUIMouseEvent.Down && input.MouseDown) ||
        (Trigger == CUIMouseEvent.DClick && input.DoubleClick)
      );
    }

    public void BeginResize(Vector2 cursorPos)
    {
      Grabbed = true;
      GrabOffset = cursorPos - Real.Position;
    }

    public void EndResize()
    {
      Grabbed = false;
      CUI.Main.OnResizeEnd(this);
    }

    //FIXME bugs when resizing beyond bounds
    public void Resize(Vector2 cursorPos)
    {
      // NOTE: i tried to use GrabOffset and it's just more annoying
      // you can accidentally resize something beyond screen bounds

      // TODO remove == new Vector(1, 1)
      if (Anchor == new Vector2(1, 1))
      {
        Host.SetAbsolute(
          Host.Absolute with
          {
            Width = Math.Max(Real.Width, cursorPos.X - Host.Real.Left),
            Height = Math.Max(Real.Height, cursorPos.Y - Host.Real.Top),
          }
        );
        return;
      }

      if (Anchor == new Vector2(0, 1))
      {
        float w = Math.Max(Real.Width, Host.Real.Width - (cursorPos.X - Host.Real.Left));

        Host.SetAbsolute(
          Host.Absolute with
          {
            Left = Host.Real.Right - w - Host.Parent.Real.Left,
            Width = w,
            Height = Math.Max(Real.Height, cursorPos.Y - Host.Real.Top),
          }
        );

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

      Vector2 Pos = CUIAnchor.GetChildPos(Host.Real, Anchor, new Vector2(x, y), new Vector2(w, h));

      Real = new CUIRect(Pos, new Vector2(w, h));
    }
    public void Draw(SpriteBatch spriteBatch)
    {
      if (!Visible) return;
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, Grabbed ? GrabbedColor : BackgroundColor, isFilled: true);
    }

    public CUIResizeHandle(CUIComponent host, Vector2 anchor)
    {
      Host = host;

      Anchor = anchor;
      //BackgroundColor = Host.BorderColor;

      Absolute = new CUINullRect(0, 0, 15, 10);
      //Host.AbsoluteMin = Host.AbsoluteMin with { Size = Absolute.Size };
    }
  }
}