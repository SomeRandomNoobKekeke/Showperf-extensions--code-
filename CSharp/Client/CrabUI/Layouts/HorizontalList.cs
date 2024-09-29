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
  public class CUILayoutHorizontalList : CUILayout
  {
    internal float TotalWidth;

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

        TotalWidth = 0;

        float h = Host.Real.Height;
        foreach (CUIComponent c in Host.Children)
        {
          float w = 0;
          Vector2 s = new Vector2(w, h);

          if (!c.FillEmptySpace.X)
          {
            if (c.Relative.Width.HasValue) w = c.Relative.Width.Value * Host.Real.Width;
            if (c.Absolute.Width.HasValue) w = c.Absolute.Width.Value;

            if (c.RelativeMin.Width.HasValue) w = Math.Max(w, c.RelativeMin.Width.Value * Host.Real.Width);
            if (c.AbsoluteMin.Width.HasValue) w = Math.Max(w, c.AbsoluteMin.Width.Value);

            if (c.RelativeMax.Width.HasValue) w = Math.Min(w, c.RelativeMax.Width.Value * Host.Real.Width);
            if (c.AbsoluteMax.Width.HasValue) w = Math.Min(w, c.AbsoluteMax.Width.Value);

            s = c.AmIOkWithThisSize(new Vector2(w, h));

            TotalWidth += s.X;
          }

          CUIComponentSize size = new CUIComponentSize(c, s);

          Sizes.Add(size);
          if (c.FillEmptySpace.X) Resizible.Add(size);
        }

        float dif = Host.Real.Width - TotalWidth;

        Resizible.ForEach(c => c.Size = c.Component.AmIOkWithThisSize(new Vector2(dif / Resizible.Count, c.Size.Y)));

        Host.ChildrenSizeCalculated();

        float x = 0;
        foreach (CUIComponentSize c in Sizes)
        {
          c.Component.Real = CheckChildBoundaries(
            Host.Real.Left + Host.ChildrenOffset.X + x,
            Host.Real.Top + Host.ChildrenOffset.Y,
            c.Size.X,
            c.Size.Y
          );

          x += c.Size.X;
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
          if (!c.FillEmptySpace.X)
          {
            if (c.Absolute.Width.HasValue) w = c.Absolute.Width.Value;
            if (c.AbsoluteMin.Width.HasValue) w = Math.Max(w, c.AbsoluteMin.Width.Value);
            if (c.AbsoluteMax.Width.HasValue) w = Math.Min(w, c.AbsoluteMax.Width.Value);
            tw += w;
          }
        }

        Host.AbsoluteMin = Host.AbsoluteMin with { Width = tw };
      }

      if (Host.FitContent.Y)
      {
        float th = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;
          if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;
          if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);
          if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);
          th = Math.Max(th, h);
        }

        Host.Absolute = Host.Absolute with { Height = th };
      }

      AbsoluteChanged.Value = false;
    }

    public CUILayoutHorizontalList(CUIComponent host = null) : base(host)
    {

    }
  }
}