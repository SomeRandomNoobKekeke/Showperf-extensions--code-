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
using HarmonyLib;

namespace CrabUI
{
  public static partial class CUI
  {

    public static Dictionary<string, Type> CUITypes = new Dictionary<string, Type>();

    public static Type GetComponentTypeByName(string name)
    {
      if (!CUITypes.ContainsKey(name))
      {
        CUITypes[name] = getComponentTypeByName(name);
      }

      return CUITypes[name];
    }
    private static Type getComponentTypeByName(string name)
    {
      Assembly CUIAssembly = Assembly.GetAssembly(typeof(CUI));
      Assembly CallingAssembly = Assembly.GetCallingAssembly();

      if (name.Equals("CUIComponent", StringComparison.OrdinalIgnoreCase)) return typeof(CUIComponent);

      foreach (Type t in CallingAssembly.GetTypes())
      {
        if (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && t.IsSubclassOf(typeof(CUIComponent)))
        {
          return t;
        }
      }

      foreach (Type t in CUIAssembly.GetTypes())
      {
        if (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && t.IsSubclassOf(typeof(CUIComponent)))
        {
          return t;
        }
      }

      return null;
    }

    public static object GetDefault(object obj)
    {
      FieldInfo defField = obj.GetType().GetField("Default", BindingFlags.Static | BindingFlags.Public);
      if (defField == null) return null;
      return defField.GetValue(null);
    }

    public static object GetNestedValue(object obj, string nestedName)
    {
      string[] names = nestedName.Split('.');

      foreach (string name in names)
      {
        FieldInfo fi = obj.GetType().GetField(name, AccessTools.all);
        PropertyInfo pi = obj.GetType().GetProperty(name, AccessTools.all);

        if (fi != null)
        {
          obj = fi.GetValue(obj);
          continue;
        }

        if (pi != null)
        {
          obj = pi.GetValue(obj);
          continue;
        }

        return null;
      }

      return obj;
    }
  }
}