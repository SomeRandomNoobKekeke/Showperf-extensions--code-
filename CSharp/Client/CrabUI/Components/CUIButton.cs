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

    public Color InactiveColor;
    public Color MouseOverColor;
    public Color MousePressedColor;

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

      InactiveColor = CUIPallete.Default.Secondary.Off;
      MouseOverColor = CUIPallete.Default.Secondary.OffHover;
      MousePressedColor = CUIPallete.Default.Secondary.On;
      BorderColor = CUIPallete.Default.Secondary.Border;

      TextAling.Type = CUIAnchorType.CenterCenter;
      Padding = new Vector2(4, 2);

      OnMouseDown += (e) => SoundPlayer.PlayUISound(ClickSound);
    }

    public CUIButton(string text, float? width, float? height) : this(text)
    {
      Relative = new CUINullRect(null, null, width, height);
    }

    public CUIButton(string text, float? x = null, float? y = null, float? w = null, float? h = null) : this(text)
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}