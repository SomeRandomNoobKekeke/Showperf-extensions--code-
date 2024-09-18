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

    protected override void Draw(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);
    }

    protected override void DrawFront(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, BorderBox.Position, BorderBox.Size, BorderColor, thickness: BorderThickness);

      if (Resizible)
      {
        GUI.DrawRectangle(spriteBatch, ResizeHandle.Position, ResizeHandle.Size, BorderColor, isFilled: true);
      }
    }

    public CUIFrame() : base()
    {
      HideChildrenOutsideFrame = true;
      Resizible = true;
      Dragable = true;
    }

    public CUIFrame(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }
  }
}