using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIButton : CUIComponent
  {
    public override string Name => "CUIButton";

    public override void Draw(SpriteBatch spriteBatch)
    {
      BackgroundColor = MouseOver ? MousePressed ? Color.Cyan : Color.Blue : new Color(0, 0, 32);

      base.Draw(spriteBatch);
    }


    public CUIButton(float x, float y, float w, float h) : base(x, y, w, h)
    {

    }
    public CUIButton(Vector2 position, Vector2 size) : base(position, size)
    {

    }
  }
}