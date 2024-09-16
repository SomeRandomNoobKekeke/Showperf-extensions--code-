using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUITextBlock : CUIComponent
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

    //TODO rethink
    public bool TextWrapped = false;

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
        TextRealSize = Font.MeasureString(wrappedText) * TextScale + Padding * 2;
        AbsoluteMin.Size = TextRealSize;
      }
    }

    public CUITextAling TextAling = CUITextAling.Start;
    public Color TextColor { get; set; } = Color.White;
    public GUIFont Font { get; set; } = GUIStyle.SmallFont;
    public Vector2 TextRealSize;
    public Vector2 TextDrawPos;
    public float TextScale { get; set; } = 1f;

    public override void UpdateOwnLayout()
    {
      // if (!TextWrapped) WrapText();

      if (TextAling == CUITextAling.Start) TextDrawPos = Real.Position + Padding;
      if (TextAling == CUITextAling.Center) TextDrawPos = Real.Position + (Real.Size - TextRealSize) / 2.0f;
      if (TextAling == CUITextAling.End) TextDrawPos = Real.Position - Padding + (Real.Size - TextRealSize);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      Font.DrawString(spriteBatch, Text, TextDrawPos, TextColor, rotation: 0, origin: new Vector2(0, 0), TextScale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);
    }

    public CUITextBlock(string text = "")
    {
      Padding = new Vector2(2, -2);
      Text = text;
      BackgroundColor = Color.Transparent;
      BorderColor = Color.Transparent;
    }
  }
}