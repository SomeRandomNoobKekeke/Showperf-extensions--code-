using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIVerticalList : CUIComponent
  {
    public override string Name => "CUIVerticalList";

    public override void ApplyParentSizeRestrictions(CUIComponent c)
    {
      c.RelativeWidth = 1f;
      c.RelativeLeft = 0f;
    }

    public override void UpdateLayout()
    {
      float y = 0;
      Children.ForEach(c =>
      {
        c.Top = y;
        y += c.Height;
      });

      Children.ForEach(c => c.ApplyRelPosition(applyX: true, applyY: false));
    }



    public CUIVerticalList(float x, float y, float w, float h) : base(x, y, w, h)
    {
      HideChildrenOutsideFrame = true;
    }
    public CUIVerticalList(Vector2 position, Vector2 size) : base(position, size)
    {
      HideChildrenOutsideFrame = true;
    }
  }
}