using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  // TODO think, mb make this generic and also act like a normal list
  // Because otherwise you'll have to store list of children somewhere outside which is cringe

  //TODO add scrollbar
  public class CUIVerticalList : CUIComponent
  {
    public static CUIVerticalList Default = new CUIVerticalList();
    public bool Scrollable { get; set; }

    public float TopGap = 0;
    public float BottomGap = 10f;

    // TODO why i have to have 2 vars of same thing? this smells like a bad solution
    protected CUILayoutVerticalList listLayout;
    public CUIDirection Direction
    {
      get => listLayout.Direction;
      set => listLayout.Direction = value;
    }

    public float Scroll
    {
      get => ChildrenOffset.Y;
      set
      {
        if (!Scrollable) return;
        ChildrenOffset = new Vector2(ChildrenOffset.X, value);
      }
    }

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(0, null, Real.Width, null);
    // HACK w,h here means right and bottom, this is sneaky, rethink
    internal override CUINullRect ChildOffsetBounds => new CUINullRect(
      0,
      TopGap,
      0,
      Math.Min(Real.Height - listLayout.TotalHeight - BottomGap, 0)
    );

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

    public CUIVerticalList() : base()
    {
      HideChildrenOutsideFrame = true;

      listLayout = new CUILayoutVerticalList();
      Layout = listLayout;

      OnScroll += (float s) => Scroll += s;

      BackgroundColor = Color.Transparent;
      // BorderColor = Color.Transparent;
    }

  }
}