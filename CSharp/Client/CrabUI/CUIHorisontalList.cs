using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIHorisontalList : CUIComponent
  {
    public override string Name => "CUIHorisontalList";

    public override void ApplyParentSizeRestrictions(CUIComponent c)
    {
      c.RelativeHeight = 1f;
      c.RelativeTop = 0f;
    }

    public override void UpdateLayout()
    {
      float x = 0;
      Children.ForEach(c =>
      {
        c.Left = x;
        x += c.Width;
      });

      Children.ForEach(c => c.ApplyRelPosition(applyX: false, applyY: true));
    }



    public CUIHorisontalList(float x, float y, float w, float h) : base(x, y, w, h)
    {
      HideChildrenOutsideFrame = true;
      BackgroundColor = Color.Red * 0.5f;
    }
    public CUIHorisontalList(Vector2 position, Vector2 size) : base(position, size)
    {
      HideChildrenOutsideFrame = true;
      BackgroundColor = Color.Red * 0.5f;
    }
  }
}