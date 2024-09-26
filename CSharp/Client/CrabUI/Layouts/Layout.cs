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
  public class CUILayout
  {
    internal CUIComponent Host;

    // TODO rethink
    // public void PropagateUp()
    // {
    //   changed = true;
    //   if (Host.Parent != null) Host.Parent.Layout.PropagateUp();
    // }

    private void PropagateDown()
    {
      changed = true;
      Host.DecorChanged = true;
      foreach (CUIComponent child in Host.Children)
      {
        child.Layout.PropagateDown();
      }
    }
    private bool changed = true; public bool Changed
    {
      get => changed;
      set
      {
        changed = value;
        if (changed)
        {
          Host.DecorChanged = true;
          if (Host.Parent != null) Host.Parent.Layout.changed = true;
          foreach (CUIComponent child in Host.Children) child.Layout.PropagateDown();
        }
      }
    }

    internal virtual void Update()
    {
      if (Host.DecorChanged) Host.UpdatePseudoChildren();
      if (!Changed) return;
      Changed = false;
    }


    //TODO this is wrong, i should decrease w,h here
    protected CUIRect CheckChildBoundaries(float x, float y, float w, float h)
    {
      if (Host.ChildrenBoundaries.Width.HasValue && x - Host.Real.Left + w > Host.ChildrenBoundaries.Width.Value)
      {
        x = Host.ChildrenBoundaries.Width.Value - w + Host.Real.Left;
      }

      if (Host.ChildrenBoundaries.Height.HasValue && y - Host.Real.Top + h > Host.ChildrenBoundaries.Height.Value)
      {
        y = Host.ChildrenBoundaries.Height.Value - h + Host.Real.Top;
      }

      if (Host.ChildrenBoundaries.Left.HasValue && x - Host.Real.Left < Host.ChildrenBoundaries.Left.Value)
      {
        x = Host.ChildrenBoundaries.Left.Value + Host.Real.Left;
      }

      if (Host.ChildrenBoundaries.Top.HasValue && y - Host.Real.Top < Host.ChildrenBoundaries.Top.Value)
      {
        y = Host.ChildrenBoundaries.Top.Value + Host.Real.Top;
      }

      return new CUIRect(x, y, w, h);
    }

    public CUILayout(CUIComponent host = null)
    {
      Host = host;
    }
  }
}