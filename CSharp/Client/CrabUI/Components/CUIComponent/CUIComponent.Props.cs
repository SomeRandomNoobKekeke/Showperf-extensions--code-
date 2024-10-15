using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using System.Xml;
using System.Xml.Linq;

namespace CrabUI
{
  public partial class CUIComponent
  {
    //TODO This is potentially cursed
    public object Data;
    public bool HideChildrenOutsideFrame;


    //TODO rethink
    #region Stupid Props
    public bool UnCullable { get; set; } // >:(
    public bool IgnoreParentVisibility { get; set; } // >:(
    public bool IgnoreParentEventIgnorance { get; set; } // >:(
    public bool IgnoreParentZIndex { get; set; } // >:(
    public bool Fixed { get; set; } // >:(
    #endregion
    public CUIAnchor Anchor = new CUIAnchor(CUIAnchorType.LeftTop);

    private int? zIndex; public int? ZIndex
    {
      get => zIndex;
      set
      {
        zIndex = value;
        OnPropChanged();
        foreach (var child in Children)
        {
          //TODO think, should i propagate null?
          if (zIndex.HasValue && !child.IgnoreParentZIndex)
          {
            child.ZIndex = zIndex.Value + 1;
          }
        }
      }
    }
    private bool ignoreEvents; public bool IgnoreEvents
    {
      get => ignoreEvents;
      set
      {
        ignoreEvents = value;
        foreach (var child in Children)
        {
          if (!child.IgnoreParentEventIgnorance) child.IgnoreEvents = value;
        }
      }
    }

    private bool visible = true; public bool Visible
    {
      get => visible;
      set
      {
        visible = value;
        foreach (var child in Children)
        {
          if (!child.IgnoreParentVisibility) child.Visible = value;
        }
      }
    }

    public void Hide() { Visible = false; IgnoreEvents = true; }
    public void Reveal() { Visible = true; IgnoreEvents = false; }

    private bool revealed = true; public bool Revealed
    {
      get => revealed;
      set
      {
        revealed = value;
        if (revealed) Reveal(); else Hide();
      }
    }

    private CUIBool2 fillEmptySpace; public CUIBool2 FillEmptySpace
    {
      get => fillEmptySpace;
      set { fillEmptySpace = value; OnPropChanged(); }
    }

    private CUIBool2 fitContent; public CUIBool2 FitContent
    {
      get => fitContent;
      set { fitContent = value; OnPropChanged(); OnAbsolutePropChanged(); }
    }

    // Ugly, but otherwise it'll be undebugable
    #region Absolute Props
    private CUINullRect absolute; public CUINullRect Absolute
    {
      get => absolute;
      set => SetAbsolute(value);
    }
    internal void SetAbsolute(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absolute = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetAbsolute", memberName, "Absolute", Absolute.ToString());
      }
      OnPropChanged(); OnAbsolutePropChanged();
    }

    private CUINullRect absoluteMin; public CUINullRect AbsoluteMin
    {
      get => absoluteMin;
      set => SetAbsoluteMin(value);
    }
    internal void SetAbsoluteMin(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absoluteMin = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetAbsoluteMin", memberName, "AbsoluteMin", AbsoluteMin.ToString());
      }
      OnPropChanged(); OnAbsolutePropChanged();
    }
    private CUINullRect absoluteMax; public CUINullRect AbsoluteMax
    {
      get => absoluteMax;
      set => SetAbsoluteMax(value);
    }
    internal void SetAbsoluteMax(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absoluteMax = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetAbsoluteMax", memberName, "AbsoluteMax", AbsoluteMax.ToString());
      }
      OnPropChanged(); OnAbsolutePropChanged();
    }

    #endregion
    #region Relative Props

    //TODO make sure i don't call Relative setters directly
    private CUINullRect relative; public CUINullRect Relative
    {
      get => relative;
      set => SetRelative(value);
    }
    internal void SetRelative(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relative = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetRelative", memberName, "Relative", Relative.ToString());
      }
      OnPropChanged();
    }

    private CUINullRect relativeMin; public CUINullRect RelativeMin
    {
      get => relativeMin;
      set => SetRelativeMin(value);
    }
    internal void SetRelativeMin(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relativeMin = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetRelativeMin", memberName, "RelativeMin", RelativeMin.ToString());
      }
      OnPropChanged();
    }

    private CUINullRect relativeMax; public CUINullRect RelativeMax
    {
      get => relativeMax;
      set => SetRelativeMax(value);
    }
    internal void SetRelativeMax(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relativeMax = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetRelativeMax", memberName, "RelativeMax", RelativeMax.ToString());
      }
      OnPropChanged();
    }

    #endregion
    #region Graphic Props --------------------------------------------------------

    //HACK this is meant for buttons, but i want to access it on generic components in CUIMap
    public bool Disabled { get; set; }
    protected bool BackgroundVisible;
    private Color backgroundColor; public Color BackgroundColor
    {
      get => backgroundColor;
      set { backgroundColor = value; BackgroundVisible = backgroundColor != Color.Transparent; }
    }

    protected bool BorderVisible;
    private Color borderColor; public Color BorderColor
    {
      get => borderColor;
      set { borderColor = value; BorderVisible = borderColor != Color.Transparent; }
    }

    public float BorderThickness = 1f;
    private Vector2 padding = new Vector2(2, 2); public Vector2 Padding
    {
      get => padding;
      set { padding = value; OnDecorPropChanged(); }
    }
    #endregion
  }
}