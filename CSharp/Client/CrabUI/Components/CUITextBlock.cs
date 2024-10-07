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
    public bool Wrap;
    private string WrappedText = "";
    private Vector2? WrappedForThisSize;


    internal override Vector2 AmIOkWithThisSize(Vector2 size)
    {
      if (!WrappedForThisSize.HasValue || size != WrappedForThisSize.Value)
      {

        if (Wrap) WrappedText = Font.WrapText(Text, size.X / TextScale - Padding.X * 2);
        else WrappedText = Text;

        RealTextSize = Font.MeasureString(WrappedText) * TextScale;

        Vector2 minSize = RealTextSize + Padding * 2;
        SetAbsoluteMin(AbsoluteMin with { Size = minSize });

        WrappedForThisSize = size;

        return new Vector2(Math.Max(size.X, minSize.X), Math.Max(size.Y, minSize.Y));
      }
      else
      {
        return size;
      }
    }

    public CUIAnchor TextAling = new CUIAnchor(CUIAnchorType.LeftTop);
    public Color TextColor;
    public GUIFont Font = GUIStyle.Font;
    private float textScale = 0.9f; public float TextScale
    {
      get => textScale;
      set { textScale = value; OnDecorPropChanged(); }
    }
    private Vector2 RealTextSize;
    private Vector2 TextDrawPos;


    internal override void UpdatePseudoChildren()
    {
      TextDrawPos = TextAling.GetChildPos(Real, Vector2.Zero, RealTextSize) + Padding * TextAling.Direction;
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      // Font.DrawString(spriteBatch, WrappedText, TextDrawPos, TextColor, rotation: 0, origin: Vector2.Zero, TextScale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);

      Font.Value.DrawString(spriteBatch, WrappedText, TextDrawPos, TextColor, rotation: 0, origin: Vector2.Zero, TextScale, se: SpriteEffects.None, layerDepth: 0.1f);
    }

    public CUITextBlock(string text = "")
    {
      Padding = new Vector2(4, 0);
      Text = text;

      BackgroundColor = Color.Transparent;
      BorderColor = Color.Transparent;
      TextColor = CUIPallete.Default.Secondary.Text;
    }

    public CUITextBlock(string text, float? width, float? height) : this(text)
    {
      Relative = new CUINullRect(null, null, width, height);
    }
    public CUITextBlock(string text, float? x, float? y, float? w, float? h) : this(text)
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}