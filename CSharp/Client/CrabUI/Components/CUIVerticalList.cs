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
        ChildrenOffset = ChildrenOffset with { Y = value };
      }
    }

    internal override CUIBoundaries ChildrenBoundaries => new CUIBoundaries(minX: 0, maxX: Real.Width);

    internal override CUIBoundaries ChildOffsetBounds => new CUIBoundaries(
      minX: 0,
      maxX: 0,
      maxY: TopGap,
      minY: Math.Min(Real.Height - listLayout.TotalHeight - BottomGap, 0)
    );


    public CUIVerticalList() : base()
    {
      HideChildrenOutsideFrame = true;

      listLayout = new CUILayoutVerticalList();
      Layout = listLayout;

      OnScroll += (m) =>
      {
        Scroll += m.Scroll;
      };

      BackgroundColor = Color.Transparent;
      // BorderColor = Color.Transparent;
    }

  }
}