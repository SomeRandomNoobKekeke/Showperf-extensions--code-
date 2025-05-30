using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;
namespace CrabUI
{
  public class CUIButton : CUITextBlock
  {
    [CUISerializable]
    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;
    [CUISerializable] public Color DisabledColor { get; set; }
    [CUISerializable] public Color InactiveColor { get; set; }
    [CUISerializable] public Color MouseOverColor { get; set; }
    [CUISerializable] public Color MousePressedColor { get; set; }

    // BackgroundColor is used in base.Draw, but here it's calculated from colors above
    // so it's not a prop anymore, and i don't want to serialize it
    public new Color BackgroundColor { get => backgroundColor; set => SetBackgroundColor(value); }

    public override void Draw(SpriteBatch spriteBatch)
    {
      if (Disabled)
      {
        BackgroundColor = DisabledColor;
      }
      else
      {
        BackgroundColor = InactiveColor;
        if (MouseOver) BackgroundColor = MouseOverColor;
        if (MousePressed) BackgroundColor = MousePressedColor;
      }
      base.Draw(spriteBatch);
    }
    public CUIButton() : base()
    {
      Text = "CUIButton";
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;
      ConsumeSwipe = true;

      InactiveColor = CUIPallete.Default.Secondary.Off;
      MouseOverColor = CUIPallete.Default.Secondary.OffHover;
      MousePressedColor = CUIPallete.Default.Secondary.On;
      BorderColor = CUIPallete.Default.Secondary.Border;
      DisabledColor = CUIPallete.Default.Secondary.Disabled;
      BackgroundColor = InactiveColor;

      TextAlign = new Vector2(0.5f, 0.5f);
      Padding = new Vector2(4, 2);

      OnMouseDown += (e) =>
      {
        if (!Disabled) SoundPlayer.PlayUISound(ClickSound);
      };
    }

    public CUIButton(string text) : this()
    {
      Text = text;
    }


  }
}