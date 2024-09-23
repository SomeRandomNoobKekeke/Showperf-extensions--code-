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
  public class CUIButton : CUITextBlock
  {
    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;

    protected override void Draw(SpriteBatch spriteBatch)
    {
      BackgroundColor = new Color(0, 0, 32);
      if (MouseOver) BackgroundColor = new Color(0, 0, 64);
      if (MousePressed) BackgroundColor = new Color(0, 32, 127);

      base.Draw(spriteBatch);
    }
    public CUIButton(string text) : base(text)
    {
      PassMouseClicks = false;
      PassDragAndDrop = false;
      BackgroundColor = new Color(0, 0, 32);
      BorderColor = Color.White;
      Wrap = false;

      Padding = new Vector2(2, 2);
      TextAling = CUITextAling.Center;
      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
    }
  }
}