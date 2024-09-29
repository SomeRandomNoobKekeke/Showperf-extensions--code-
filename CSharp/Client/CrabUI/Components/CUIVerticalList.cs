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
    public bool Scrollable { get; set; }

    public float TopGap = 0f;
    public float BottomGap = 0f;

    private CUILayoutVerticalList listLayout;

    private float scroll; public float Scroll
    {
      get => scroll;
      set
      {
        if (!Scrollable) return;
        scroll = value;
        OnChildrenPropChanged();
      }
    }

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(0, null, Real.Width, null);

    private void ValidateScroll()
    {
      scroll = Math.Min(TopGap, Math.Max(scroll, Real.Height - listLayout.TotalHeight - BottomGap));
      ChildrenOffset = new Vector2(0, scroll);
    }

    internal override void ChildrenSizeCalculated()
    {
      ValidateScroll();
    }

    public CUIVerticalList() : base()
    {
      //HideChildrenOutsideFrame = true;

      listLayout = new CUILayoutVerticalList();
      Layout = listLayout;

      OnScroll += (float s) => Scroll += s;

      BackgroundColor = Color.Transparent;
      // BorderColor = Color.Transparent;
    }

    public CUIVerticalList(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}