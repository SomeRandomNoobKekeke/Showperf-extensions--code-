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
    public bool Scrollable { get; set; } = true;

    private CUILayoutVerticalList listLayout;

    private float scroll; public float Scroll
    {
      get => scroll;
      set
      {
        scroll = value;
        ValidateScroll();
      }
    }

    private void ValidateScroll()
    {
      scroll = Math.Min(0, Math.Max(scroll, Real.Height - listLayout.TotalHeight));
      ChildrenOffset = new Vector2(0, scroll);
    }

    internal override void ChildrenSizeCalculated()
    {
      ValidateScroll();
    }

    public CUIVerticalList(float x, float y, float w, float h) : base(x, y, w, h)
    {
      HideChildrenOutsideFrame = true;

      listLayout = new CUILayoutVerticalList(this);
      Layout = listLayout;

      OnScroll += (float s) => Scroll += s;

      BackgroundColor = Color.Transparent;
    }
  }
}