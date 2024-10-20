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
  public class CUISerializableAttribute : System.Attribute
  {
    public CUISerializableAttribute() { }
  }


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
    public void FromXML(XElement element)
    {

    }

    public XElement ToXML()
    {
      Type type = GetType();

      XElement e = new XElement(type.Name);

      foreach (string prop in CUITypeMetaData.Get(type).Props)
      {
        SetAttribute(prop, e);
      }

      foreach (CUIComponent child in Children)
      {
        e.Add(child.ToXML());
      }

      return e;
    }


    public string Serialize()
    {
      XElement e = this.ToXML();
      return e.ToString();
    }
    public static CUIComponent Deserialize(string raw)
    {
      return null;
    }

    //TODO think, what would happen if value is null and != def?
    public void SetAttribute(string name, XElement e)
    {
      Type type = GetType();
      object def = CUITypeMetaData.Get(type).Default;
      object value = CUI.GetNestedValue(this, name);

      if (def == null)
      {
        e?.SetAttributeValue(name, value);
      }
      else
      {
        object defValue = CUI.GetNestedValue(def, name);
        e?.SetAttributeValue(name, Object.Equals(value, defValue) ? null : value);
      }
    }

    #endregion
  }
}