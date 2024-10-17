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
    public CUIComponent Host;


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
    protected bool changed = true; public bool Changed
    {
      get => changed;
      set
      {
        changed = value;
        if (value)
        {
          //TODO Test performance of this cheap solution
          if (Host.Parent != null) Host.Parent.Layout.propagateChangedDown();
          else propagateChangedDown();

          // DecorChanged = true;
          // if (Host.Parent != null) Host.Parent.Layout.changed = true;
          // foreach (CUIComponent child in Host.Children)
          // {
          //   child.Layout.propagateChangedDown();
          // }
        }
      }
    }

    private void propagateAbsoluteChangedUp()
    {
      absoluteChanged = true;
      Host.Parent?.Layout.propagateAbsoluteChangedUp();
    }
    protected bool absoluteChanged = true; public bool AbsoluteChanged
    {
      get => absoluteChanged;
      set
      {
        //TODO is this enough?
        if (!value) absoluteChanged = false;
        if (value && Host.Parent != null) Host.Parent.Layout.absoluteChanged = true;
      }
    }
    protected bool decorChanged = true; public bool DecorChanged
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
            c.CulledOut = !c.UnCullable && !c.Real.Intersect(Host.Real);
          }
        }

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

    public CUILayout(CUIComponent host = null)
    {
      Host = host;
    }
  }
}