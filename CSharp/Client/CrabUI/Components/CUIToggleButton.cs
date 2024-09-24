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
    public event Action<bool> OnStateChange;
    private bool state; public bool State
    {
      get => state;
      set { state = value; OnStateChange?.Invoke(state); }
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      BackgroundColor = new Color(0, 0, 32);
      if (State) BackgroundColor = new Color(0, 200, 64);

      base.Draw(spriteBatch);
    }
    public CUIToggleButton(string text) : base(text)
    {
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;

      BorderColor = Color.White;
      Wrap = false;

      Padding = new Vector2(2, 2);
      TextAling.Type = CUIAnchorType.CenterCenter;

      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
      OnMouseDown += (CUIMouse m) => State = !State;
    }
  }
}