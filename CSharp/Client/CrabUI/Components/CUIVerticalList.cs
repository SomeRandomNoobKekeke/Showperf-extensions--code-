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
    public CUIComponent Content;

    public float scroll; public float Scroll
    {
      get => scroll;
      set
      {
        scroll = Math.Min(0, Math.Max(value, Real.Height - Content.Real.Height));
        Content.Absolute.Top = scroll;
      }
    }



    public void ValidateScroll()
    {
      scroll = Math.Min(0, Math.Max(scroll, Real.Height - Content.Real.Height));
      Content.Absolute.Top = scroll;
    }

    public override void UpdateOwnLayout()
    {
      ValidateScroll();
      base.UpdateOwnLayout();
    }

    public override CUIComponent Append(CUIComponent c) => Content.Append(c);
    public override void RemoveChild(CUIComponent c) => Content.RemoveChild(c);
    public override void RemoveAllChildren() => Content.RemoveAllChildren();

    public CUIVerticalList(float x, float y, float w, float h) : base(x, y, w, h)
    {
      HideChildrenOutsideFrame = true;

      Content = new CUIComponent();
      Content.Relative.Width = 1f;
      Content.Absolute.Top = 0;

      Content.Layout = new CUILayoutList(Content, vertical: true);
      append(Content);

      OnScroll += (float s) => Scroll += s;

      Content.BackgroundColor = Color.Transparent;
      BackgroundColor = Color.Transparent;
      //Content.Debug = true;
    }
  }
}