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
    public override string Name => "CUITextBlock";

    public string text = ""; public string Text
    {
      get => text;
      set { text = value; NeedsContentSizeUpdate = true; }
    }
    public string WrappedText = "";
    public Color TextColor { get; set; } = Color.White;
    public GUIFont Font { get; set; } = GUIStyle.MonospacedFont;
    public bool Wrap { get; set; } = true;

    public override Vector2 CalculateContentSize()
    {
      if (Wrap && RelativeWidth != null)
      {
        WrappedText = Font.WrapText(Text, Size.X);
        return Font.MeasureString(WrappedText);
      }
      else
      {
        WrappedText = Text;
        return Font.MeasureString(WrappedText);
      }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      Font.DrawString(spriteBatch, WrappedText, RealPosition, TextColor);
    }

    public CUITextBlock(string text = "")
    {
      Text = text;
      BackgroundColor = Color.Yellow * 0.3f;
    }
  }
}