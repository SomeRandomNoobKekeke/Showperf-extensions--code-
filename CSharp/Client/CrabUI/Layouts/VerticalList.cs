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
  public class CUILayoutVerticalList : CUILayout
  {
    internal float TotalHeight;
    public CUIDirection Direction;

    private class CUIComponentSize
    {
      public CUIComponent Component;
      public Vector2 Size;
      public CUIComponentSize(CUIComponent component, Vector2 size)
      {
        Component = component;
        Size = size;
      }
    }
    private List<CUIComponentSize> Sizes = new List<CUIComponentSize>();
    private List<CUIComponentSize> Resizible = new List<CUIComponentSize>();

    internal override void Update()
    {
      if (Changed)
      {
        Sizes.Clear();
        Resizible.Clear();

        TotalHeight = 0;



        float w = Host.Real.Width;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;
          Vector2 s = new Vector2(w, h);

          if (!c.FillEmptySpace.Y)
          {
            if (c.Relative.Height.HasValue)
            {
              h = c.Relative.Height.Value * Host.Real.Height;
              CUIDebug.Capture(Host, c, "Layout.Update", "Relative.Height", "h", h.ToString());
            }
            if (c.Absolute.Height.HasValue)
            {
              h = c.Absolute.Height.Value;
              CUIDebug.Capture(Host, c, "Layout.Update", "Absolute.Height", "h", h.ToString());
            }

            if (c.RelativeMin.Height.HasValue)
            {
              h = Math.Max(h, c.RelativeMin.Height.Value * Host.Real.Height);
              CUIDebug.Capture(Host, c, "Layout.Update", "RelativeMin.Height", "h", h.ToString());
            }
            if (c.AbsoluteMin.Height.HasValue)
            {
              h = Math.Max(h, c.AbsoluteMin.Height.Value);
              CUIDebug.Capture(Host, c, "Layout.Update", "AbsoluteMin.Height", "h", h.ToString());
            }

            if (c.RelativeMax.Height.HasValue)
            {
              h = Math.Min(h, c.RelativeMax.Height.Value * Host.Real.Height);
              CUIDebug.Capture(Host, c, "Layout.Update", "RelativeMax.Height", "h", h.ToString());
            }
            if (c.AbsoluteMax.Height.HasValue)
            {
              h = Math.Min(h, c.AbsoluteMax.Height.Value);
              CUIDebug.Capture(Host, c, "Layout.Update", "AbsoluteMax.Height", "h", h.ToString());
            }

            s = new Vector2(w, h);
            Vector2 okSize = c.AmIOkWithThisSize(s);
            CUIDebug.Capture(Host, c, "Layout.Update", "AmIOkWithThisSize", "s", okSize.ToString());

            s = okSize;

            if (!c.Fixed) s /= c.Scale;

            TotalHeight += s.Y;

            Sizes.Add(new CUIComponentSize(c, s));
          }
          else
          {
            Resizible.Add(new CUIComponentSize(c, s));
          }
        }

        float dif = Math.Max(0, Host.Real.Height - TotalHeight);


        Resizible.ForEach(c =>
        {
          c.Size = new Vector2(c.Size.X, dif / Resizible.Count);
          //c.Component.AmIOkWithThisSize(new Vector2(c.Size.X, dif / Resizible.Count));
          CUIDebug.Capture(Host, c.Component, "Layout.Update", "Resizible.ForEach", "c.Size", c.Size.ToString());
        });

        //Host.ChildrenSizeCalculated();

        CUI3DOffset offset = Host.ChildOffsetBounds.Check(Host.ChildrenOffset);


        if (Direction == CUIDirection.Straight)
        {
          float y = 0;
          foreach (CUIComponentSize c in Sizes)
          {
            CUIRect real = Host.ChildrenBoundaries.Check(0, y, c.Size.X, c.Size.Y);
            real = offset.Transform(real);
            real = real.Shift(Host.Real.Position);

            c.Component.SetReal(real);

            y += c.Size.Y;
          }
        }

        if (Direction == CUIDirection.Reverse)
        {
          float y = Host.Real.Height;
          foreach (CUIComponentSize c in Sizes)
          {
            y -= c.Size.Y;

            CUIRect real = Host.ChildrenBoundaries.Check(0, y, c.Size.X, c.Size.Y);
            real = offset.Transform(real);
            real = real.Shift(Host.Real.Position);

            c.Component.SetReal(real);
          }
        }
      }

      base.Update();
    }

    internal override void ResizeToContent()
    {
      if (AbsoluteChanged && Host.FitContent.X)
      {
        float tw = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float w = 0;
          if (c.Absolute.Width.HasValue) w = c.Absolute.Width.Value;
          if (c.AbsoluteMin.Width.HasValue) w = Math.Max(w, c.AbsoluteMin.Width.Value);
          if (c.AbsoluteMax.Width.HasValue) w = Math.Min(w, c.AbsoluteMax.Width.Value);

          tw = Math.Max(tw, w);
        }

        CUIDebug.Capture(null, Host, "ResizeToContent", "tw", "AbsoluteMin.Width", tw.ToString());
        Host.SetAbsoluteMin(Host.AbsoluteMin with { Width = tw });
      }

      if (AbsoluteChanged && Host.FitContent.Y)
      {
        float th = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;
          if (!c.FillEmptySpace.Y)
          {
            if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;
            if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);
            if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);
            th += h;
          }
        }

        CUIDebug.Capture(null, Host, "ResizeToContent", "th", "Absolute.Height", th.ToString());
        Host.SetAbsolute(Host.Absolute with { Height = th });
      }

      base.ResizeToContent();
    }



    public CUILayoutVerticalList(CUIDirection d = CUIDirection.Straight, CUIComponent host = null) : base(host)
    {
      Direction = d;
    }
  }
}