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
    public Color OnColor = CUIColors.ToggleButtonOn;
    public Color OffColor = CUIColors.ToggleButtonOff;

    public event Action<bool> OnStateChange;
    private bool state; public bool State
    {
      get => state;
      set { state = value; OnStateChange?.Invoke(state); }
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      if (State) BackgroundColor = OnColor;
      else BackgroundColor = OffColor;

      base.Draw(spriteBatch);
    }
    public CUIToggleButton(string text) : base(text)
    {
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;

      BorderColor = CUIColors.ComponentBorder;

      TextAling.Type = CUIAnchorType.CenterCenter;

      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
      OnMouseDown += (CUIMouse m) => State = !State;
    }

    public CUIToggleButton(string text, float? width, float? height) : this(text, null, null, width, height) { }
    public CUIToggleButton(string text, float? x, float? y, float? w, float? h) : this(text)
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}