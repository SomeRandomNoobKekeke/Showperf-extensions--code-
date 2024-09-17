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
    protected CUIComponent Host;

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

    public CUILayout(CUIComponent host)
    {
      Host = host;
    }
  }
}