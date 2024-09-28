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
    #region Cringe
    public class State
    {
      public CUILayout Layout;
      public CUIComponent Host => Layout.Host;
      protected bool _value; public virtual bool Value
      {
        get => _value;
        set
        {
          _value = value;
          PropagateUp();
          PropagateDown();
        }
      }

      public virtual void PropagateUp() { }
      public virtual void PropagateDown() { }

      public State(CUILayout layout) { Layout = layout; }
    }

    public class ChangedState : State
    {
      public override bool Value
      {
        get => _value;
        set
        {
          _value = value;
          if (value && Host != null)
          {
            Host.Layout.DecorChanged.Value = true;
            if (Host.Parent != null) Host.Parent.Layout.Changed.Value = true;
            foreach (CUIComponent child in Host.Children) child.Layout.Changed.PropagateDown();
          }
        }
      }
      public override void PropagateDown()
      {
        _value = true;
        if (Host != null)
        {
          Host.Layout.DecorChanged.Value = true;
          foreach (CUIComponent child in Host.Children)
          {
            child.Layout.Changed.PropagateDown();
          }
        }
      }

      public ChangedState(CUILayout layout) : base(layout) { _value = true; }
    }

    public class DecorChangedState : State
    {
      public override bool Value
      {
        get => _value;
        set => _value = value;
      }
      public DecorChangedState(CUILayout layout) : base(layout) { _value = true; }
    }

    public class AbsoluteChangedState : State
    {
      public override bool Value
      {
        get => _value;
        set
        {
          _value = value;
          if (value && Host != null && Host.Parent != null)
          {
            Host.Parent.Layout.AbsoluteChanged.Value = true;
          }
        }
      }
      public AbsoluteChangedState(CUILayout layout) : base(layout) { _value = true; }
    }
    #endregion



    internal CUIComponent Host;

    public ChangedState Changed;
    public DecorChangedState DecorChanged;
    public AbsoluteChangedState AbsoluteChanged;

    internal virtual void Update()
    {
      if (Changed.Value)
      {
        // do something
        Changed.Value = false;
      }
    }

    internal virtual void UpdateDecor()
    {
      if (DecorChanged.Value)
      {
        Host.UpdatePseudoChildren();
        DecorChanged.Value = false;
      }
    }

    internal virtual void ResizeToContent()
    {
      if (Host.FitContent.X || Host.FitContent.Y)
      {
        // do something
      }

      AbsoluteChanged.Value = false;
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
      Changed = new ChangedState(this);
      DecorChanged = new DecorChangedState(this);
      AbsoluteChanged = new AbsoluteChangedState(this);
    }
  }
}