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

    protected CUILayoutVerticalList listLayout;

    private float scroll; public float Scroll
    {
      get => ChildrenOffset.Y;
      set
      {
        if (!Scrollable) return;
        ChildrenOffset = new Vector2(ChildrenOffset.X, value);
      }
    }

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(0, null, Real.Width, null);
    // TODO w,h here means right and bottom, this is sneaky, rethink
    internal override CUINullRect ChildOffsetBounds => new CUINullRect(0, TopGap, 0, Real.Height - listLayout.TotalHeight - BottomGap);

    internal override void ChildrenSizeCalculated()
    {
      CUINullRect bounds = ChildOffsetBounds;
      float x = 0;
      float y = 0;

      if (bounds.Top.HasValue) y = Math.Max(ChildrenOffset.Y, bounds.Top.Value);
      if (bounds.Height.HasValue) y = Math.Min(bounds.Height.Value, ChildrenOffset.Y);

      ChildrenOffset = new Vector2(x, y);
      Info($"{ChildOffsetBounds} {ChildrenOffset}");
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