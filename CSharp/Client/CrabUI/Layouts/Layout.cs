using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUILayout
  {
    internal CUIComponent Host;

    //NOTE: This looks ugly, but no matter how i try to isolate this logic it gets only uglier
    // i've been stuck here for too long so i'll just do this
    // and each update pattern in fact used only once, so i think no big deal
    private void propagateChangedDown()
    {
      changed = true;
      DecorChanged = true;
      foreach (CUIComponent child in Host.Children)
      {
        child.Layout.propagateChangedDown();
      }
    }
    private bool changed = true; public bool Changed
    {
      get => changed;
      set
      {
        changed = value;
        if (value)
        {
          DecorChanged = true;
          if (Host.Parent != null) Host.Parent.Layout.changed = true;
          foreach (CUIComponent child in Host.Children)
          {
            child.Layout.propagateChangedDown();
          }
        }
      }
    }

    private void propagateAbsoluteChangedUp()
    {
      absoluteChanged = true;
      Host.Parent?.Layout.propagateAbsoluteChangedUp();
    }
    private bool absoluteChanged = true; public bool AbsoluteChanged
    {
      get => absoluteChanged;
      set
      {
        //HACK this looks excessive, but without this dropdown won't work
        absoluteChanged = value;
        if (value) Host.Parent?.Layout.propagateAbsoluteChangedUp();
        // if (value) Host.Parent?.Layout.propagateAbsoluteChangedUp();
        // else absoluteChanged = false;
      }
    }
    public bool decorChanged = true; public bool DecorChanged
    {
      get => decorChanged;
      set
      {
        decorChanged = value;
      }
    }

    internal virtual void Update()
    {
      if (Changed)
      {
        if (Host.HideChildrenOutsideFrame)
        {
          foreach (CUIComponent c in Host.Children)
          {
            c.CulledOut = !c.Real.Intersect(Host.Real);
          }
        }

        // do something
        Changed = false;
      }
    }

    internal virtual void UpdateDecor()
    {
      if (DecorChanged)
      {
        Host.UpdatePseudoChildren();
        DecorChanged = false;
      }
    }

    internal virtual void ResizeToContent()
    {
      if (AbsoluteChanged && (Host.FitContent.X || Host.FitContent.Y))
      {
        // do something
      }

      AbsoluteChanged = false;
    }


    //TODO idk
    protected CUIRect CheckChildBoundaries(float x, float y, float w, float h)
    {
      // x + w > Host.Right
      if (Host.ChildrenBoundaries.Width.HasValue && x + w > Host.Real.Left + Host.ChildrenBoundaries.Width.Value)
      {
        x = Host.Real.Left + Host.ChildrenBoundaries.Width.Value - w;
      }
      // y + h > Host.Bottom
      if (Host.ChildrenBoundaries.Height.HasValue && y + h > Host.Real.Top + Host.ChildrenBoundaries.Height.Value)
      {
        y = Host.Real.Top + Host.ChildrenBoundaries.Height.Value - h;
      }
      // x < Host.Left
      if (Host.ChildrenBoundaries.Left.HasValue && x < Host.Real.Left + Host.ChildrenBoundaries.Left.Value)
      {
        x = Host.Real.Left + Host.ChildrenBoundaries.Left.Value;
      }
      // y < Host.Top
      if (Host.ChildrenBoundaries.Top.HasValue && y < Host.Real.Top + Host.ChildrenBoundaries.Top.Value)
      {
        y = Host.Real.Top + Host.ChildrenBoundaries.Top.Value;
      }


      return new CUIRect(x, y, w, h);
    }

    public CUILayout(CUIComponent host = null)
    {
      Host = host;
    }
  }
}