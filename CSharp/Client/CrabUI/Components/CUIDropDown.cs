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


    protected override void Draw(SpriteBatch spriteBatch)
    {

      base.Draw(spriteBatch);
    }
    public CUIDropDown() : base("CUIDropDown")
    {
      ConsumeMouseClicks = true;
      ConsumeDragAndDrop = true;

      BorderColor = CUIColors.ComponentBorder;
      Wrap = false;

      Padding = new Vector2(2, 2);


      OnMouseDown += (CUIMouse m) => SoundPlayer.PlayUISound(ClickSound);
    }

    public CUIDropDown(float? width, float? height) : this()
    {
      Relative = new CUINullRect(null, null, width, height);
    }
  }
}