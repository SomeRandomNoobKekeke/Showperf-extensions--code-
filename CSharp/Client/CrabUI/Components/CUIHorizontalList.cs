using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIHorizontalList : CUIComponent
  {
    public bool Scrollable { get; set; }

    public float LeftGap = 0f;
    public float RightGap = 0f;

    private CUILayoutHorizontalList listLayout;

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

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(null, 0, null, Real.Height);

    private void ValidateScroll()
    {
      scroll = Math.Min(LeftGap, Math.Max(scroll, Real.Width - listLayout.TotalWidth - RightGap));
      ChildrenOffset = new Vector2(scroll, 0);
    }

    internal override void ChildrenSizeCalculated()
    {
      ValidateScroll();
    }

    public CUIHorizontalList() : base()
    {
      HideChildrenOutsideFrame = true;

      listLayout = new CUILayoutHorizontalList();
      Layout = listLayout;

      OnScroll += (float s) => Scroll += s;

      BackgroundColor = Color.Transparent;
      // BorderColor = Color.Transparent;
    }

    public CUIHorizontalList(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}