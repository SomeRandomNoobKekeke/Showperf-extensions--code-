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

        CUIDebug.Capture(Host, CUIDebugEventType.LayoutUpdate, $"{Host.Real}");
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
        CUIDebug.Capture(Host, CUIDebugEventType.ResizeToContent, $"{Host.AbsoluteMin} {Host.Absolute}");
      }

      AbsoluteChanged.Value = false;
    }


    //TODO idk
    protected CUIRect CheckChildBoundaries(float x, float y, float w, float h)
    {
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