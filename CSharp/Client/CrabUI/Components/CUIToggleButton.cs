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
  public class CUIToggleButton : CUITextBlock
  {
    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;
    public Color OnColor;
    public Color OnHoverColor;
    public Color OffColor;
    public Color OffHoverColor;
    private string onText; public string OnText
    {
      get => onText;
      set { onText = value; Text = state ? OnText : OffText; }
    }
    private string offText; public string OffText
    {
      get => offText;
      set { offText = value; Text = state ? OnText : OffText; }
    }

    public event Action<bool> OnStateChange;
    public Action<bool> AddOnStateChange { set { OnStateChange += value; } }
    private bool state; public bool State
    {
      get => state;
      set
      {
        state = value;
        Text = state ? OnText : OffText;
        OnStateChange?.Invoke(state);
      }
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      if (State)
      {
        if (MouseOver) BackgroundColor = OnHoverColor;
        else BackgroundColor = OnColor;
      }
      else
      {
        if (MouseOver) BackgroundColor = OffHoverColor;
        else BackgroundColor = OffColor;
      }

      base.Draw(spriteBatch);
    }
    public CUIToggleButton(string text = "") : base(text)
    {
      OnText = text;
      OffText = text;

      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;

      OnColor = CUIPallete.Default.Secondary.On;
      OnHoverColor = CUIPallete.Default.Secondary.OnHover;
      OffColor = CUIPallete.Default.Secondary.Off;
      OffHoverColor = CUIPallete.Default.Secondary.OffHover;
      BorderColor = CUIPallete.Default.Secondary.Border;

      TextAling.Type = CUIAnchorType.CenterCenter;
      Padding = new Vector2(4, 2);

      OnMouseDown += (e) => SoundPlayer.PlayUISound(ClickSound);
      OnMouseDown += (e) => State = !State;
    }

    public CUIToggleButton(string text, float? width, float? height) : this(text)
    {
      Relative = new CUINullRect(null, null, width, height);
    }
    public CUIToggleButton(string text, float? x = null, float? y = null, float? w = null, float? h = null) : this(text)
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}