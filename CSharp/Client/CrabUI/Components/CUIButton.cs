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
  public partial class CUIButton : CUIComponent
  {
    public string text = ""; public string Text
    {
      get => text;
      set
      {
        text = value;
        WrapText();
      }
    }

    public void WrapText()
    {
      float? width = null;
      if (AbsoluteMax.Width.HasValue) width = AbsoluteMax.Width;
      if (Absolute.Width.HasValue) width = Absolute.Width;

      if (width.HasValue)
      {
        WrappedText = Font.WrapText(Text, width.Value);
      }
      else
      {
        WrappedText = Text;
      }
    }

    public string wrappedText = ""; public string WrappedText
    {
      get => wrappedText;
      set
      {
        wrappedText = value;
        TextRealSize = Font.MeasureString(wrappedText) * TextScale;
        AbsoluteMin.Size = TextRealSize;
      }
    }

    public CUITextAling TextAling = CUITextAling.Center;
    public Color TextColor { get; set; } = Color.White;
    public GUIFont Font { get; set; } = GUIStyle.SmallFont;
    public Vector2 TextRealSize;
    public Vector2 TextDrawPos;

    public float TextScale { get; set; } = 1f;

    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;


    public override void UpdateOwnLayout()
    {
      if (TextAling == CUITextAling.Start) TextDrawPos = Real.Position + Padding;
      if (TextAling == CUITextAling.Center) TextDrawPos = Real.Position + (Real.Size - TextRealSize) / 2.0f;
      if (TextAling == CUITextAling.End) TextDrawPos = Real.Position - Padding + (Real.Size - TextRealSize);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
      BackgroundColor = new Color(0, 0, 32);
      if (MouseOver) BackgroundColor = new Color(0, 0, 64);
      if (MousePressed) BackgroundColor = new Color(0, 32, 127);

      base.Draw(spriteBatch);

      Font.DrawString(spriteBatch, Text, TextDrawPos, TextColor, rotation: 0, origin: new Vector2(0, 0), TextScale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);
    }
    public CUIButton(string text) : base()
    {
      Text = text;
      PassMouseClicks = false;
      PassDragAndDrop = false;
      BackgroundColor = new Color(0, 0, 32);
      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
    }

    public CUIButton(float x, float y, float w, float h) : base(x, y, w, h)
    {
      PassMouseClicks = false;
      PassDragAndDrop = false;
      BackgroundColor = new Color(0, 0, 32);
      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
    }
  }
}