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
    private string text = ""; public string Text
    {
      get => text;
      set { text = value; OnPropChanged(); }
    }
    public bool Wrap { get; set; } = true;

    private string WrappedText { get; set; } = "";

    internal override Vector2 AmIOkWithThisSize(Vector2 size)
    {
      if (Wrap) WrappedText = Font.WrapText(Text, size.X / TextScale - Padding.X * 2);
      else WrappedText = Text;

      RealTextSize = Font.MeasureString(WrappedText) * TextScale + Padding * 2;

      AbsoluteMin.Size = RealTextSize;

      return new Vector2(size.X, Math.Max(size.Y, RealTextSize.Y));
    }

    public CUITextAling TextAling = CUITextAling.Start;
    public Color TextColor { get; set; } = Color.White;
    public GUIFont Font { get; set; } = GUIStyle.SmallFont;
    private float textScale = 1f; public float TextScale
    {
      get => textScale;
      set { textScale = value; DecorChanged = true; }
    }
    private Vector2 RealTextSize;
    private Vector2 TextDrawPos;


    internal override void UpdatePseudoChildren()
    {
      if (TextAling == CUITextAling.Start) TextDrawPos = Real.Position + Padding;
      if (TextAling == CUITextAling.Center) TextDrawPos = Real.Position + (Real.Size - RealTextSize) / 2.0f;
      if (TextAling == CUITextAling.End) TextDrawPos = Real.Position - Padding + (Real.Size - RealTextSize);
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      Font.DrawString(spriteBatch, WrappedText, TextDrawPos, TextColor, rotation: 0, origin: new Vector2(0, 0), TextScale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);
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