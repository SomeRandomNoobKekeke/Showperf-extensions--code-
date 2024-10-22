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

    public virtual XElement ToXML()
    {
      Type type = GetType();

      XElement e = new XElement(type.Name);

      foreach (string key in CUITypeMetaData.Get(type).Serializable.Keys)
      {
        SetAttribute(key, e);
      }

      foreach (CUIComponent child in Children)
      {
        e.Add(child.ToXML());
      }

      return e;
    }

    public virtual void FromXML(XElement element)
    {
      Type type = GetType();

      CUITypeMetaData meta = CUITypeMetaData.Get(type);

      foreach (XAttribute attribute in element.Attributes())
      {
        if (!meta.Properties.ContainsKey(attribute.Name.ToString()))
        {
          CUIDebug.Err($"Can't parse prop {attribute.Name} in {type.Name} because type metadata doesn't contain that prop");
          continue;
        }

        PropertyInfo prop = meta.Properties[attribute.Name.ToString()];


        MethodInfo parse = prop.PropertyType.GetMethod(
          "Parse",
          BindingFlags.Public | BindingFlags.Static
        );

        if (parse == null)
        {
          CUIDebug.Err($"Can't parse prop {prop.Name} in {type.Name} because it's type {prop.PropertyType.Name} is missing Parse method");
          continue;
        }

        try
        {
          object result = parse.Invoke(null, new object[] { attribute.Value });

          prop.SetValue(this, result);
          CUIDebug.log($"{prop.GetValue(this)}");
        }
        catch (Exception e)
        {
          CUIDebug.Err($"Can't parse {attribute.Value} into {prop.PropertyType.Name}\n{e}");
        }


        // BaroDev (wide)
        // try
        // {
        //   object result = Activator.CreateInstance(prop.PropertyType.MakeByRefType());
        //   object[] args = new object[] { attribute.Value, result };
        //   object? ok = tryparse.Invoke(null, args);

        //   CUIDebug.log($"{ok} {attribute.Name} {result}");
        // }
        // catch (Exception e)
        // {
        //   CUIDebug.Err(e);
        // }
      }
    }


    public string Serialize()
    {
      try
      {
        XElement e = this.ToXML();
        return e.ToString();
      }
      catch (Exception e)
      {
        return e.Message;
      }
    }
    public static CUIComponent Deserialize(string raw)
    {
      return Deserialize(XElement.Parse(raw));
    }

    public static CUIComponent Deserialize(XElement e)
    {
      try
      {
        Type type = CUI.GetComponentTypeByName(e.Name.ToString());
        if (type == null) return null;

        CUIComponent c = (CUIComponent)Activator.CreateInstance(type);
        c.FromXML(e);

        return c;
      }
      catch (Exception ex)
      {
        CUIDebug.Err(ex);
        return null;
      }
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