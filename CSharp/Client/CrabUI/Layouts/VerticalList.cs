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
      if (Changed.Value)
      {
        Sizes.Clear();
        Resizible.Clear();

        TotalHeight = 0;

        float w = Host.Real.Width;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;
          Vector2 s = new Vector2(w, h);

          if (!c.FillEmptySpace)
          {
            if (c.Relative.Height.HasValue) h = c.Relative.Height.Value * Host.Real.Height;
            if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;

            if (c.RelativeMin.Height.HasValue) h = Math.Max(h, c.RelativeMin.Height.Value * Host.Real.Height);
            if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);

            if (c.RelativeMax.Height.HasValue) h = Math.Min(h, c.RelativeMax.Height.Value * Host.Real.Height);
            if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);

            s = c.AmIOkWithThisSize(new Vector2(w, h));

            TotalHeight += s.Y;
          }

          CUIComponentSize size = new CUIComponentSize(c, s);

          Sizes.Add(size);
          if (c.FillEmptySpace) Resizible.Add(size);
        }

        float dif = Host.Real.Height - TotalHeight;

        Resizible.ForEach(c => c.Size = new Vector2(c.Size.X, dif / Resizible.Count));

        Host.ChildrenSizeCalculated();

        float y = 0;
        foreach (CUIComponentSize c in Sizes)
        {
          c.Component.Real = CheckChildBoundaries(
            Host.Real.Left + Host.ChildrenOffset.X,
            Host.Real.Top + Host.ChildrenOffset.Y + y,
            c.Size.X,
            c.Size.Y
          );

          y += c.Size.Y;
        }

        Changed.Value = false;
      }
    }

    internal override void ResizeToContent()
    {
      if (Host.FitContent.X)
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

        Host.Absolute = Host.Absolute with { Width = tw };
      }

      if (Host.FitContent.Y)
      {
        float th = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;
          if (!c.FillEmptySpace)
          {
            if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;
            if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);
            if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);

            th += h;
          }
        }

        Host.Absolute = Host.Absolute with { Height = th };
      }

      AbsoluteChanged.Value = false;
    }



    public CUILayoutVerticalList(CUIComponent host = null) : base(host)
    {

    }
  }
}