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
    public CUIComponent Host;

    // TODO rethink
    public void PropagateUp()
    {
      changed = true;
      if (Host.Parent != null) Host.Parent.Layout.PropagateUp();
    }

    public void PropagateDown()
    {
      changed = true;
      foreach (CUIComponent child in Host.Children)
      {
        child.Layout.PropagateDown();
      }
    }
    public bool changed = true; public bool Changed
    {
      get => changed;
      set
      {
        changed = value;
        if (changed)
        {
          OwnLayoutChanged = true;
          if (Host.Parent != null) Host.Parent.Layout.PropagateUp();
          foreach (CUIComponent child in Host.Children) child.Layout.PropagateDown();
        }
      }
    }

    public bool OwnLayoutChanged { get; set; }

    public virtual void Update()
    {
      if (!Changed) return;
      Changed = false;
      if (OwnLayoutChanged)
      {
        Host.UpdateOwnLayout();
        OwnLayoutChanged = false;
      }
    }

    public CUILayout(CUIComponent host)
    {
      Host = host;
    }
  }
}