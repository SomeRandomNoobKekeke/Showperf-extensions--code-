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
  public class CUILayoutSimple : CUILayout
  {
    internal override void Update()
    {
      if (Host.DecorChanged) Host.UpdatePseudoChildren();

      if (!Changed) return;

      foreach (CUIComponent c in Host.Children)
      {
        float x, y, w, h;

        x = Host.Real.Left;
        if (c.Relative.Left.HasValue) x = Host.Real.Left + c.Relative.Left.Value * Host.Real.Width;
        if (c.Absolute.Left.HasValue) x = Host.Real.Left + c.Absolute.Left.Value;

        if (c.RelativeMin.Left.HasValue) x = Math.Max(x, Host.Real.Left + c.RelativeMin.Left.Value * Host.Real.Width);
        if (c.AbsoluteMin.Left.HasValue) x = Math.Max(x, Host.Real.Left + c.AbsoluteMin.Left.Value);

        if (c.RelativeMax.Left.HasValue) x = Math.Min(x, Host.Real.Left + c.RelativeMax.Left.Value * Host.Real.Width);
        if (c.AbsoluteMax.Left.HasValue) x = Math.Min(x, Host.Real.Left + c.AbsoluteMax.Left.Value);


        y = Host.Real.Top;
        if (c.Relative.Top.HasValue) y = Host.Real.Top + c.Relative.Top.Value * Host.Real.Height;
        if (c.Absolute.Top.HasValue) y = Host.Real.Top + c.Absolute.Top.Value;

        if (c.RelativeMin.Top.HasValue) y = Math.Max(y, Host.Real.Top + c.RelativeMin.Top.Value * Host.Real.Height);
        if (c.AbsoluteMin.Top.HasValue) y = Math.Max(y, Host.Real.Top + c.AbsoluteMin.Top.Value);

        if (c.RelativeMax.Top.HasValue) y = Math.Min(y, Host.Real.Top + c.RelativeMax.Top.Value * Host.Real.Height);
        if (c.AbsoluteMax.Top.HasValue) y = Math.Min(y, Host.Real.Top + c.AbsoluteMax.Top.Value);


        w = 0;
        if (c.Relative.Width.HasValue) w = c.Relative.Width.Value * Host.Real.Width;
        if (c.Absolute.Width.HasValue) w = c.Absolute.Width.Value;

        if (c.RelativeMin.Width.HasValue) w = Math.Max(w, c.RelativeMin.Width.Value * Host.Real.Width);
        if (c.AbsoluteMin.Width.HasValue) w = Math.Max(w, c.AbsoluteMin.Width.Value);

        if (c.RelativeMax.Width.HasValue) w = Math.Min(w, c.RelativeMax.Width.Value * Host.Real.Width);
        if (c.AbsoluteMax.Width.HasValue) w = Math.Min(w, c.AbsoluteMax.Width.Value);


        h = 0;
        if (c.Relative.Height.HasValue) h = c.Relative.Height.Value * Host.Real.Height;
        if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;

        if (c.RelativeMin.Height.HasValue) h = Math.Max(h, c.RelativeMin.Height.Value * Host.Real.Height);
        if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);

        if (c.RelativeMax.Height.HasValue) h = Math.Min(h, c.RelativeMax.Height.Value * Host.Real.Height);
        if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);

        c.Real = new CUIRect(x, y, w, h);
      }

      Changed = false;
    }

    public CUILayoutSimple(CUIComponent host) : base(host)
    {

    }
  }
}