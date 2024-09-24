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
  public class CUIDropDown : CUITextBlock
  {
    public GUISoundType ClickSound { get; set; } = GUISoundType.Select;


    public CUIComponent Options;


    internal override void UpdatePseudoChildren()
    {
      base.UpdatePseudoChildren();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);
    }
    public CUIDropDown() : base("CUIDropDown")
    {
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;

      BorderColor = CUIColors.ComponentBorder;
      BackgroundColor = CUIColors.ButtonInactive;

      Options = new CUIComponent(0, 0, 1, 10);
      Options.BackgroundColor = Color.Yellow * 0.25f;
      Append(Options);


      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
    }

    public CUIDropDown(float? width, float? height) : this()
    {
      Relative = new CUINullRect(null, null, width, height);
    }
  }
}