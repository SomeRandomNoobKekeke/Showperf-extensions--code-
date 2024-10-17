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
    public CUIDirection Direction
    {
      get => listLayout.Direction;
      set => listLayout.Direction = value;
    }

    public float Scroll
    {
      get => ChildrenOffset.X;
      set
      {
        if (!Scrollable) return;
        SetChildrenOffset(
          ChildrenOffset with { X = value }
        );

      }
    }

    internal override CUIBoundaries ChildrenBoundaries => new CUIBoundaries(minY: 0, maxY: Real.Height);


    internal override CUIBoundaries ChildOffsetBounds => new CUIBoundaries(
      minY: 0,
      maxY: 0,
      minX: LeftGap,
      maxX: Math.Min(Real.Width - listLayout.TotalWidth - RightGap, 0)
    );
    public CUIHorizontalList() : base()
    {
      HideChildrenOutsideFrame = true;

      listLayout = new CUILayoutHorizontalList();
      Layout = listLayout;

      OnScroll += (m) => Scroll += m.Scroll;

      BackgroundColor = Color.Transparent;
      // BorderColor = Color.Transparent;
    }
  }
}