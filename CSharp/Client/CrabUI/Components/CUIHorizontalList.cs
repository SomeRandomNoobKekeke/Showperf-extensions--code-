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

    public float Scroll
    {
      get => ChildrenOffset.X;
      set
      {
        if (!Scrollable) return;
        ChildrenOffset = new Vector2(value, ChildrenOffset.Y);
      }
    }

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(null, 0, null, Real.Height);

    internal override CUINullRect ChildOffsetBounds => new CUINullRect(0, 0, 0, 0);

    //TODO test, i just copypasted code from vlist and didn't test at all, lol
    internal override void ChildrenSizeCalculated()
    {
      CUINullRect bounds = ChildOffsetBounds;
      float x = ChildrenOffset.X;
      float y = ChildrenOffset.Y;

      if (bounds.Left.HasValue) x = Math.Min(bounds.Left.Value, x);
      if (bounds.Width.HasValue) x = Math.Max(bounds.Width.Value, x);

      if (bounds.Top.HasValue) y = Math.Min(bounds.Top.Value, y);
      if (bounds.Height.HasValue) y = Math.Max(bounds.Height.Value, y);

      ChildrenOffset = new Vector2(x, y);
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