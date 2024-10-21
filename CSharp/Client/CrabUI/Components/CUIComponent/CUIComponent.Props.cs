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
    #region Declaration
    //TODO This is potentially cursed
    public object Data;
    [CUISerializable] public bool HideChildrenOutsideFrame;
    [CUISerializable] public bool ShouldPassPropsToChildren = true;
    [CUISerializable] public bool UnCullable;
    [CUISerializable] public bool IgnoreParentVisibility;
    [CUISerializable] public bool IgnoreParentEventIgnorance;
    [CUISerializable] public bool IgnoreParentZIndex;
    [CUISerializable] public bool Fixed;
    [CUISerializable] public Vector2 Anchor;

    [CUISerializable]
    public int? ZIndex { get => zIndex; set => SetZIndex(value); }
    [CUISerializable]
    public bool IgnoreEvents { get => ignoreEvents; set => SetIgnoreEvents(value); }
    [CUISerializable]
    public bool Visible { get => visible; set => SetVisible(value); }

    public bool Revealed { get => revealed; set => SetRevealed(value); }
    //HACK this is meant for buttons, but i want to access it on generic components in CUIMap
    [CUISerializable]
    public bool Disabled { get; set; }
    [CUISerializable]
    public float BorderThickness = 1f;
    [CUISerializable]
    public Vector2 Padding { get => padding; set => SetPadding(value); }
    [CUISerializable]
    public Color BorderColor { get => borderColor; set => SetBorderColor(value); }
    [CUISerializable]
    public Color BackgroundColor { get => backgroundColor; set => SetBackgroundColor(value); }

    [CUISerializable]
    public CUIBool2 FillEmptySpace { get => fillEmptySpace; set => SetFillEmptySpace(value); }
    [CUISerializable]
    public CUIBool2 FitContent { get => fitContent; set => SetFitContent(value); }
    [CUISerializable]
    public CUINullRect Absolute { get => absolute; set => SetAbsolute(value); }
    [CUISerializable]
    public CUINullRect AbsoluteMin { get => absoluteMin; set => SetAbsoluteMin(value); }
    [CUISerializable]
    public CUINullRect AbsoluteMax { get => absoluteMax; set => SetAbsoluteMax(value); }
    [CUISerializable]//TODO make sure i don't call Relative setters directly
    public CUINullRect Relative { get => relative; set => SetRelative(value); }
    [CUISerializable]
    public CUINullRect RelativeMin { get => relativeMin; set => SetRelativeMin(value); }
    [CUISerializable]
    public CUINullRect RelativeMax { get => relativeMax; set => SetRelativeMax(value); }

    #endregion
    //Note those should be wrapped in objects, but i don't know how to unwrap closure to CUIComponent
    #region implementation


    private int? zIndex; internal void SetZIndex(int? value)
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

    private bool ignoreEvents; internal void SetIgnoreEvents(bool value)
    {
      ignoreEvents = value;
      foreach (var child in Children)
      {
        if (!child.IgnoreParentEventIgnorance) child.IgnoreEvents = value;
      }
    }

    private bool visible = true; internal void SetVisible(bool value)
    {
      visible = value;
      foreach (var child in Children)
      {
        if (!child.IgnoreParentVisibility) child.Visible = value;
      }
    }

    private bool revealed = true; internal void SetRevealed(bool value)
    {
      revealed = value;
      if (revealed) { Visible = true; IgnoreEvents = false; }
      else { Visible = false; IgnoreEvents = true; }
    }

    private CUIBool2 fillEmptySpace; internal void SetFillEmptySpace(CUIBool2 value)
    {
      fillEmptySpace = value; OnPropChanged();
    }

    private CUIBool2 fitContent; internal void SetFitContent(CUIBool2 value)
    {
      fitContent = value; OnPropChanged(); OnAbsolutePropChanged();
    }

    #region Absolute Props 
    #endregion

    private CUINullRect absolute; internal void SetAbsolute(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absolute = value;
      CUIDebug.Capture(null, this, "SetAbsolute", memberName, "Absolute", Absolute.ToString());
      OnPropChanged(); OnAbsolutePropChanged();
    }

    private CUINullRect absoluteMin; internal void SetAbsoluteMin(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absoluteMin = value;
      CUIDebug.Capture(null, this, "SetAbsoluteMin", memberName, "AbsoluteMin", AbsoluteMin.ToString());
      OnPropChanged(); OnAbsolutePropChanged();
    }

    private CUINullRect absoluteMax; internal void SetAbsoluteMax(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absoluteMax = value;
      CUIDebug.Capture(null, this, "SetAbsoluteMax", memberName, "AbsoluteMax", AbsoluteMax.ToString());
      OnPropChanged(); OnAbsolutePropChanged();
    }


    #region Relative Props
    #endregion
    private CUINullRect relative; internal void SetRelative(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relative = value;
      CUIDebug.Capture(null, this, "SetRelative", memberName, "Relative", Relative.ToString());
      OnPropChanged();
    }

    private CUINullRect relativeMin; internal void SetRelativeMin(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relativeMin = value;
      CUIDebug.Capture(null, this, "SetRelativeMin", memberName, "RelativeMin", RelativeMin.ToString());
      OnPropChanged();
    }

    private CUINullRect relativeMax; internal void SetRelativeMax(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relativeMax = value;
      CUIDebug.Capture(null, this, "SetRelativeMax", memberName, "RelativeMax", RelativeMax.ToString());
      OnPropChanged();
    }

    #region Graphic Props --------------------------------------------------------
    #endregion

    protected bool BackgroundVisible;
    private Color backgroundColor; internal void SetBackgroundColor(Color value)
    {
      backgroundColor = value; BackgroundVisible = backgroundColor != Color.Transparent;
    }

    protected bool BorderVisible;
    private Color borderColor; internal void SetBorderColor(Color value)
    {
      borderColor = value; BorderVisible = borderColor != Color.Transparent;
    }

    private Vector2 padding = new Vector2(2, 2); internal void SetPadding(Vector2 value)
    {
      padding = value; OnDecorPropChanged();
    }



    #endregion
  }
}