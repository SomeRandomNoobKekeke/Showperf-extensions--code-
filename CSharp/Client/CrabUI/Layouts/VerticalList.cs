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
    public bool ResizeToContent { get; set; } = true;
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
      if (Changed)
      {
        Sizes.Clear();
        Resizible.Clear();

        TotalHeight = 0;
        foreach (CUIComponent c in Host.Children)
        {
          float h = 0;

          if (!c.FillEmptySpace)
          {
            if (c.Relative.Height.HasValue) h = c.Relative.Height.Value * Host.Real.Height;
            if (c.Absolute.Height.HasValue) h = c.Absolute.Height.Value;

            if (c.RelativeMin.Height.HasValue) h = Math.Max(h, c.RelativeMin.Height.Value * Host.Real.Height);
            if (c.AbsoluteMin.Height.HasValue) h = Math.Max(h, c.AbsoluteMin.Height.Value);

            if (c.RelativeMax.Height.HasValue) h = Math.Min(h, c.RelativeMax.Height.Value * Host.Real.Height);
            if (c.AbsoluteMax.Height.HasValue) h = Math.Min(h, c.AbsoluteMax.Height.Value);
            TotalHeight += h;
          }

          CUIComponentSize size = new CUIComponentSize(c, new Vector2(Host.Real.Width, h));

          Sizes.Add(size);
          if (c.FillEmptySpace) Resizible.Add(size);
        }

        float dif = Host.Real.Height - TotalHeight;

        Resizible.ForEach(c => c.Size = new Vector2(c.Size.X, dif / Resizible.Count));

        Host.ChildrenSizeCalculated();

        foreach (CUIComponentSize c in Sizes)
        {
          c.Size = c.Component.AmIOkWithThisSize(c.Size);
        }

        float y = 0;
        foreach (CUIComponentSize c in Sizes)
        {
          c.Component.Real = new CUIRect(
            Host.Real.Left + Host.ChildrenOffset.X,
            Host.Real.Top + Host.ChildrenOffset.Y + y,
            c.Size.X,
            c.Size.Y
          );
          y += c.Size.Y;
        }

        Changed = false;
      }



      if (Host.DecorChanged)
      {
        Host.UpdatePseudoChildren();
        Host.DecorChanged = false;
      }

    }

    public CUILayoutVerticalList(CUIComponent host) : base(host)
    {

    }
  }
}