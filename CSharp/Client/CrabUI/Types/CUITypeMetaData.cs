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

    public object Default;
    public List<string> Props = new List<string>();

    public CUITypeMetaData(Type type)
    {
      CUIDebug.log($"{type} ----------------------", Color.Lime);
      Default = Activator.CreateInstance(type);

      foreach (MemberInfo mi in type.GetMembers(AccessTools.all))
      {
        if (Attribute.IsDefined(mi, typeof(CUISerializableAttribute)))
        {
          CUIDebug.log(mi.Name, Color.Lime);
          Props.Add(mi.Name);
        }
      }
    }
  }

}