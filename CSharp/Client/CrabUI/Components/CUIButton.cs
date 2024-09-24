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
      BackgroundColor = CUIColors.ButtonInactive;
      if (MouseOver) BackgroundColor = CUIColors.ButtonHover;
      if (MousePressed) BackgroundColor = CUIColors.ButtonPressed;

      base.Draw(spriteBatch);
    }
    public CUIButton(string text) : base(text)
    {
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;
      //ConsumeSwipe = true;
      BorderColor = CUIColors.ComponentBorder;

      TextAling.Type = CUIAnchorType.CenterCenter;
      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
    }

    public CUIButton(string text, float? width, float? height) : this(text)
    {
      Relative = new CUINullRect(null, null, width, height);
    }
  }
}