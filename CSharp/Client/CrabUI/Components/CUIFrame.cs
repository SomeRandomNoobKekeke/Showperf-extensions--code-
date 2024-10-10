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
      if (BackgroundVisible) GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);
    }

    protected override void DrawFront(SpriteBatch spriteBatch)
    {
      if (BorderVisible) GUI.DrawRectangle(spriteBatch, BorderBox.Position, BorderBox.Size, BorderColor, thickness: BorderThickness);

      LeftResizeHandle.Draw(spriteBatch);
      RightResizeHandle.Draw(spriteBatch);

      base.DrawFront(spriteBatch);
    }

    public CUIFrame Open()
    {
      if (CUI.Main == null) return null;
      CUI.Main.Append(this);
      return this;
    }

    public CUIFrame Close()
    {
      RemoveSelf();
      Revealed = false;
      return this;
    }

    public CUIFrame() : base()
    {
      HideChildrenOutsideFrame = true;
      Resizible = true;
      Draggable = true;
    }

    public CUIFrame(float? x = null, float? y = null, float? w = null, float? h = null) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}