using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace CrabUI
{
  public class CUITypeMetaData
  {
    public static Dictionary<Type, CUITypeMetaData> TypeMetaData = new Dictionary<Type, CUITypeMetaData>();

    public static CUITypeMetaData Get(Type type)
    {
      if (!TypeMetaData.ContainsKey(type)) TypeMetaData[type] = new CUITypeMetaData(type);
      return TypeMetaData[type];
    }

    public void SetProp(string name, object target, object value)
    {
      if (Fields.ContainsKey(name))
      {
        Fields[name].SetValue(target, value);
        return;
      }

      if (Properties.ContainsKey(name))
      {
        Properties[name].SetValue(target, value);
        return;
      }
    }

    public object Default;

    public Dictionary<string, MemberInfo> Serializable = new Dictionary<string, MemberInfo>();
    public Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
    public Dictionary<string, PropertyInfo> Properties = new Dictionary<string, PropertyInfo>();


    public CUITypeMetaData(Type type)
    {
      Default = Activator.CreateInstance(type);

      foreach (MemberInfo mi in type.GetMembers(AccessTools.all))
      {
        if (Attribute.IsDefined(mi, typeof(CUISerializableAttribute)))
        {
          Serializable[mi.Name] = mi;
        }
      }

      foreach (PropertyInfo pi in type.GetProperties(AccessTools.all))
      {
        if (Attribute.IsDefined(pi, typeof(CUISerializableAttribute)))
        {
          Properties[pi.Name] = pi;
        }
      }

      foreach (FieldInfo fi in type.GetFields(AccessTools.all))
      {
        if (Attribute.IsDefined(fi, typeof(CUISerializableAttribute)))
        {
          Fields[fi.Name] = fi;
        }
      }
    }
  }

}