using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIFrame : CUIComponent
  {

    public override void Draw(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);
    }

    public override void DrawFront(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BorderColor);

      if (Resizible)
      {
        GUI.DrawRectangle(spriteBatch, ResizeHandle.Position, ResizeHandle.Size, BorderColor, isFilled: true);
      }
    }

    public CUIFrame(float x, float y, float w, float h) : base(x, y, w, h)
    {
      HideChildrenOutsideFrame = true;
      Resizible = true;
      Dragable = true;
      DrawOnTop = true;
    }
  }
}