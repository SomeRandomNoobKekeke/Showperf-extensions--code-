using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;
namespace CrabUI
{
  public class CUITextBlock : CUIComponent
  {
    public event Action OnTextChanged;
    public Action AddOnTextChanged { set { OnTextChanged += value; } }

    [CUISerializable] public bool Wrap;
    [CUISerializable] public Color TextColor;
    [CUISerializable] public GUIFont Font = GUIStyle.Font;
    [CUISerializable] public bool Ghost;
    [CUISerializable] public Vector2 TextAlign;


    [CUISerializable]
    public string Text { get => text; set => SetText(value); }
    [CUISerializable]
    public float TextScale { get => textScale; set => SetTextScale(value); }



    //TODO Uncringe
    #region Cringe
    private Vector2 RealTextSize;
    private Vector2 TextDrawPos;
    protected string WrappedText = "";
    protected Vector2? WrappedForThisSize;
    protected Vector2 WrappedSize;
    protected bool NeedReWrapping;
    #endregion

    protected string text = ""; internal void SetText(string value)
    {
      text = value ?? "";
      OnTextChanged?.Invoke();

      if (!Ghost)
      {
        NeedReWrapping = true;
        OnPropChanged();
        OnAbsolutePropChanged();
      }
      else
      {
        WrappedText = text;
        // OnDecorPropChanged();
      }
    }

    protected float textScale = 0.9f; internal void SetTextScale(float value)
    {
      textScale = value; OnDecorPropChanged();
    }


    //FIXME find a solution that doesn't overwrite absolute min 
    protected void DoWrapFor(Vector2 size)
    {
      if ((!WrappedForThisSize.HasValue || size == WrappedForThisSize.Value) && !NeedReWrapping) return;

      if (Wrap) WrappedText = Font.WrapText(Text, size.X / TextScale - Padding.X * 2);
      else WrappedText = Text;

      RealTextSize = Font.MeasureString(WrappedText) * TextScale;

      bool FirstTimeHuh = AbsoluteMin.Width == null || AbsoluteMin.Height == null;

      Vector2 minSize = RealTextSize + Padding * 2;
      SetAbsoluteMin(AbsoluteMin with { Size = minSize });

      WrappedForThisSize = size;
      WrappedSize = new Vector2(Math.Max(size.X, minSize.X), Math.Max(size.Y, minSize.Y));

      // WHY??? Coz Font.MeasureString adds an extra line for wrapped text with w=0
      if (FirstTimeHuh)
      {
        if (Wrap) WrappedText = Font.WrapText(Text, WrappedSize.X / TextScale - Padding.X * 2);
        else WrappedText = Text;

        RealTextSize = Font.MeasureString(WrappedText) * TextScale;

        minSize = RealTextSize + Padding * 2;
        SetAbsoluteMin(AbsoluteMin with { Size = minSize });
      }

      WrappedForThisSize = size;
      WrappedSize = new Vector2(Math.Max(size.X, minSize.X), Math.Max(size.Y, minSize.Y));
      NeedReWrapping = false;
    }

    internal override Vector2 AmIOkWithThisSize(Vector2 size)
    {
      DoWrapFor(size);
      return WrappedSize;
    }

    internal override void UpdatePseudoChildren()
    {

      TextDrawPos = CUIAnchor.GetChildPos(Real, TextAlign, Vector2.Zero, RealTextSize / Scale)
      + Padding * CUIAnchor.Direction(TextAlign) / Scale;

      CUIDebug.Capture(null, this, "UpdatePseudoChildren", "", "TextDrawPos", $"{TextDrawPos - Real.Position}");
    }


    public override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      // Font.DrawString(spriteBatch, WrappedText, TextDrawPos, TextColor, rotation: 0, origin: Vector2.Zero, TextScale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);

      Font.Value.DrawString(spriteBatch, WrappedText, TextDrawPos, TextColor, rotation: 0, origin: Vector2.Zero, TextScale / Scale, se: SpriteEffects.None, layerDepth: 0.1f);
    }

    public CUITextBlock()
    {
      Padding = new Vector2(4, 0);

      BackgroundColor = Color.Transparent;
      BorderColor = Color.Transparent;
      TextColor = CUIPallete.Default.Secondary.Text;
    }

    public CUITextBlock(string text) : this()
    {
      Text = text;
    }
  }
}