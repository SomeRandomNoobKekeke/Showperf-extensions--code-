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
using HarmonyLib;

namespace CrabUI
{
  public partial class CUIComponent
  {
    #region State --------------------------------------------------------

    public Dictionary<string, CUIComponent> States = new Dictionary<string, CUIComponent>();
    public CUIComponent Clone()
    {
      CUIComponent clone = new CUIComponent();
      clone.ApplyState(this);
      return clone;
    }
    public virtual partial void ApplyState(CUIComponent state)
    {
      if (state == null) return;

      ShouldPassPropsToChildren = state.ShouldPassPropsToChildren;
      zIndex = state.ZIndex; // TODO think how to uncurse this
      //IgnoreEvents = state.IgnoreEvents;  // TODO think how to uncurse this
      //Visible = state.Visible;  // TODO think how to uncurse this
      ChildrenOffset = state.ChildrenOffset;
      Draggable = state.Draggable;
      LeftResizeHandle.Visible = state.LeftResizeHandle.Visible;
      RightResizeHandle.Visible = state.RightResizeHandle.Visible;
      Swipeable = state.Swipeable;
      Anchor = state.Anchor;
      Absolute = state.Absolute;
      AbsoluteMax = state.AbsoluteMax;
      AbsoluteMin = state.AbsoluteMin;
      Relative = state.Relative;
      RelativeMax = state.RelativeMax;
      RelativeMin = state.RelativeMin;
      FillEmptySpace = state.FillEmptySpace;
      FitContent = state.FitContent;
      HideChildrenOutsideFrame = state.HideChildrenOutsideFrame;
      BackgroundColor = state.BackgroundColor;
      BorderColor = state.BorderColor;
      BorderThickness = state.BorderThickness;
      Padding = state.Padding;
    }

    #endregion
    #region XML --------------------------------------------------------
    public virtual void FromXML(XElement element)
    {

    }

    public virtual void ToXML(XElement e)
    {
      try
      {
        SetAttribute("HideChildrenOutsideFrame", e);
        SetAttribute("UnCullable", e);
        SetAttribute("IgnoreParentVisibility", e);
        SetAttribute("IgnoreParentEventIgnorance", e);
        SetAttribute("IgnoreParentZIndex", e);
        SetAttribute("Fixed", e);
        SetAttribute("Anchor.Type", e);
        SetAttribute("ZIndex", e);
        SetAttribute("Visible", e);

        SetAttribute("FillEmptySpace", e);
        SetAttribute("FitContent", e);
        SetAttribute("Absolute", e);
        SetAttribute("AbsoluteMin", e);
        SetAttribute("AbsoluteMax", e);
        SetAttribute("Relative", e);
        SetAttribute("RelativeMin", e);
        SetAttribute("RelativeMax", e);

        SetAttribute("Disabled", e);
        SetAttribute("BackgroundColor", e);
        SetAttribute("BorderColor", e);
        SetAttribute("Padding", e);
      }
      catch (Exception ex)
      {
        Info(ex);
      }
    }


    //TODO think, what would happen if value is null and != def?
    public void SetAttribute(string name, XElement e)
    {
      object def = CUI.GetDefault(this);
      object value = CUI.GetNestedValue(this, name);

      if (def == null)
      {
        e?.SetAttributeValue(name, value);
        return;
      }

      object defValue = CUI.GetNestedValue(def, name);

      e?.SetAttributeValue(name, Object.Equals(value, defValue) ? null : value);
    }

    private XElement ToXMLRec()
    {
      XElement e = new XElement(GetType().Name);
      ToXML(e);

      foreach (CUIComponent child in Children)
      {
        e.Add(child.ToXMLRec());
      }

      return e;
    }

    public string Serialize()
    {
      XElement e = this.ToXMLRec();
      return e.ToString();
    }
    public static CUIComponent Deserialize(string raw)
    {
      return null;
    }

    #endregion
  }
}