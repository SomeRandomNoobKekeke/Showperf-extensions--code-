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

    public Color InactiveColor = CUIColors.ButtonInactive;
    public Color MouseOverColor = CUIColors.ButtonHover;
    public Color MousePressedColor = CUIColors.ButtonPressed;

    protected override void Draw(SpriteBatch spriteBatch)
    {
      BackgroundColor = InactiveColor;
      if (MouseOver) BackgroundColor = MouseOverColor;
      if (MousePressed) BackgroundColor = MousePressedColor;

      base.Draw(spriteBatch);
    }
    public CUIButton(string text = "") : base(text)
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

    public CUIButton(string text, float? x, float? y, float? w, float? h) : this(text)
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}