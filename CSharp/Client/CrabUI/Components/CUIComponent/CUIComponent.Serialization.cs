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

      PackProps(e);

      foreach (CUIComponent child in Children)
      {
        e.Add(child.ToXML());
      }

      return e;
    }


    public virtual void FromXML(XElement element)
    {
      ExtractProps(element);

      foreach (XElement childElement in element.Elements())
      {
        Type childType = CUI.GetComponentTypeByName(childElement.Name.ToString());
        if (childType == null) continue;

        CUIComponent child = (CUIComponent)Activator.CreateInstance(childType);
        child.FromXML(childElement);

        this.Append(child);
      }
    }

    protected void ExtractProps(XElement element)
    {
      Type type = GetType();

      CUITypeMetaData meta = CUITypeMetaData.Get(type);

      foreach (XAttribute attribute in element.Attributes())
      {
        if (!meta.Properties.ContainsKey(attribute.Name.ToString()))
        {
          CUIDebug.Error($"Can't parse prop {attribute.Name} in {type.Name} because type metadata doesn't contain that prop (is it a property? fields aren't supported yet)");
          continue;
        }

        PropertyInfo prop = meta.Properties[attribute.Name.ToString()];

        MethodInfo parse = null;
        if (CUIExtensions.Parse.ContainsKey(prop.PropertyType))
        {
          parse = CUIExtensions.Parse[prop.PropertyType];
        }

        parse ??= prop.PropertyType.GetMethod(
          "Parse",
          BindingFlags.Public | BindingFlags.Static,
          new Type[] { typeof(string) }
        );


        if (parse == null)
        {
          CUIDebug.Error($"Can't parse prop {prop.Name} in {type.Name} because it's type {prop.PropertyType.Name} is missing Parse method");
          continue;
        }

        try
        {
          object result = parse.Invoke(null, new object[] { attribute.Value });
          prop.SetValue(this, result);
        }
        catch (Exception e)
        {
          CUIDebug.Error($"Can't parse {attribute.Value} into {prop.PropertyType.Name}\n{e}");
        }
      }
    }

    protected void PackProps(XElement element)
    {
      Type type = GetType();
      CUITypeMetaData meta = CUITypeMetaData.Get(type);

      foreach (string key in meta.Serializable.Keys)
      {
        object value = CUI.GetNestedValue(this, key);

        if (meta.Default != null && Object.Equals(value, CUI.GetNestedValue(meta.Default, key)))
        {
          value = null;
        }

        //TODO rethink, what if value should be null?
        if (value == null) continue;

        MethodInfo customToString = null;
        if (CUIExtensions.CustomToString.ContainsKey(value.GetType()))
        {
          customToString = CUIExtensions.CustomToString[value.GetType()];
        }

        if (customToString != null)
        {
          element?.SetAttributeValue(key, customToString.Invoke(value, new object[] { }));
        }
        else
        {
          element?.SetAttributeValue(key, value);
        }
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
        CUIDebug.Error(ex);
        return null;
      }
    }

    public static CUIComponent LoadFromFile(string path)
    {
      XDocument xdoc = XDocument.Load(path);
      return Deserialize(xdoc.Root);
    }
    public void SaveToFile(string path)
    {
      XDocument xdoc = new XDocument();
      xdoc.Add(this.ToXML());
      xdoc.Save(path);
    }

    #endregion
  }
}