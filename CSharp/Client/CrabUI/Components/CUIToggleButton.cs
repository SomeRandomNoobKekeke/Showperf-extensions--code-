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

    public Color DisabledColor;
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
    protected bool state; public bool State
    {
      get => state;
      set
      {
        SetState(value);
        OnStateChange?.Invoke(state);
      }
    }
    // To let you set state silently
    public void SetState(bool value)
    {
      state = value;
      Text = state ? OnText : OffText;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
      if (Disabled)
      {
        BackgroundColor = DisabledColor;
      }
      else
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
      }

      base.Draw(spriteBatch);
    }

    public CUIToggleButton() : base()
    {
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;
      ConsumeSwipe = true;

      OnColor = CUIPallete.Default.Secondary.On;
      OnHoverColor = CUIPallete.Default.Secondary.OnHover;
      OffColor = CUIPallete.Default.Secondary.Off;
      OffHoverColor = CUIPallete.Default.Secondary.OffHover;
      BorderColor = CUIPallete.Default.Secondary.Border;
      DisabledColor = CUIPallete.Default.Secondary.Disabled;

      TextAling.Type = CUIAnchorType.CenterCenter;
      Padding = new Vector2(4, 2);

      OnMouseDown += (e) =>
      {
        if (!Disabled)
        {
          State = !State;
          SoundPlayer.PlayUISound(ClickSound);
        }
      };
    }

    public CUIToggleButton(string text) : this()
    {
      OnText = text;
      OffText = text;
      Text = text;
    }

  }
}