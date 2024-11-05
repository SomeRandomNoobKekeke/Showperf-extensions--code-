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
    [CUISerializable]
    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;

    [CUISerializable] public Color DisabledColor { get; set; }
    [CUISerializable] public Color OnColor { get; set; }
    [CUISerializable] public Color OnHoverColor { get; set; }
    [CUISerializable] public Color OffColor { get; set; }
    [CUISerializable] public Color OffHoverColor { get; set; }

    // BackgroundColor is used in base.Draw, but here it's calculated from OnColor/OffColor
    // so it's not a prop anymore, and i don't want to serialize it
    public new Color BackgroundColor { get => backgroundColor; set => SetBackgroundColor(value); }


    private string onText;
    private string offText;
    [CUISerializable]
    public string OnText
    {
      get => onText;
      set { onText = value; if (State && onText != null) Text = onText; }
    }

    [CUISerializable]
    public string OffText
    {
      get => offText;
      set { offText = value; if (!State && offText != null) Text = offText; }
    }

    public event Action<bool> OnStateChange;
    public Action<bool> AddOnStateChange { set { OnStateChange += value; } }


    protected bool state;
    [CUISerializable]
    public bool State
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
      if (state && OnText != null) Text = OnText;
      if (!state && OffText != null) Text = OffText;
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
      BackgroundColor = OffColor;

      TextAlign = new Vector2(0.5f, 0.5f);
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
      Text = text;
    }

  }
}