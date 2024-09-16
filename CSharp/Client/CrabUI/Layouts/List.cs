using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUILayoutList : CUILayout
  {
    public bool Vertical { get; set; }

    public bool ResizeToContent { get; set; } = true;

    public override void Update()
    {
      if (!Changed) return;

      Host.UpdateOwnLayout();


      if (Vertical)
      {
        float y = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;
          if (c.Relative.Height.HasValue) h = c.Relative.Height.Value * Host.Real.Height;
          if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;

          if (c.RelativeMin.Height.HasValue) h = Math.Max(h, c.RelativeMin.Height.Value * Host.Real.Height);
          if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);

          if (c.RelativeMax.Height.HasValue) h = Math.Min(h, c.RelativeMax.Height.Value * Host.Real.Height);
          if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);

          c.Real = new CUIRect(Host.Real.Left, Host.Real.Top + y, Host.Real.Width, h);
          y += h;
        }

        if (ResizeToContent) Host.Real.Height = y;
      }
      else
      {
        float x = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float w = 0;
          if (c.Relative.Width.HasValue) w = c.Relative.Width.Value * Host.Real.Width;
          if (c.Absolute.Width.HasValue) w = c.Absolute.Width.Value;

          if (c.RelativeMin.Width.HasValue) w = Math.Max(w, c.RelativeMin.Width.Value * Host.Real.Width);
          if (c.AbsoluteMin.Width.HasValue) w = Math.Max(w, c.AbsoluteMin.Width.Value);

          if (c.RelativeMax.Width.HasValue) w = Math.Min(w, c.RelativeMax.Width.Value * Host.Real.Width);
          if (c.AbsoluteMax.Width.HasValue) w = Math.Min(w, c.AbsoluteMax.Width.Value);

          c.Real = new CUIRect(Host.Real.Left + x, Host.Real.Top, w, Host.Real.Height);
          x += w;
        }

        if (ResizeToContent) Host.Real.Width = x;
      }

      Changed = false;
    }

    public CUILayoutList(CUIComponent host, bool vertical = true) : base(host)
    {
      Vertical = vertical;
    }
  }
}