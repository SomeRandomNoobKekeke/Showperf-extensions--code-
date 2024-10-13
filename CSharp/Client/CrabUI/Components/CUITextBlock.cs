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
    private string text = ""; public string Text
    {
      get => text;
      set
      {
        text = value;
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
    }
    //TODO Uncringe
    #region Cringe
    public bool Wrap;
    #region MegaCringe
    public bool Ghost;
    #endregion
    protected string WrappedText = "";
    protected Vector2? WrappedForThisSize;
    protected Vector2 WrappedSize;
    protected bool NeedReWrapping;

    //FIXME find a solution that doesn't overwrite absolute min 
    protected void DoWrapFor(Vector2 size)
    {
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

    #endregion
    internal override Vector2 AmIOkWithThisSize(Vector2 size)
    {
      if (!WrappedForThisSize.HasValue || size != WrappedForThisSize.Value || NeedReWrapping)
      {
        DoWrapFor(size);
      }
      return WrappedSize;
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
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "UpdatePseudoChildren", "", "TextDrawPos", TextDrawPos.ToString());
      }
    }

    public override void Draw(SpriteBatch spriteBatch)
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
    public CUITextBlock(string text, float? x = null, float? y = null, float? w = null, float? h = null) : this(text)
    {
      Relative = new CUINullRect(x, y, w, h);
    }

    public override void ToXML(XElement e)
    {
      e.Add(new XAttribute("Text", Text));
    }
  }
}