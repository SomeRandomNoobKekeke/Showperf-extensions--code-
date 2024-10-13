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
    public static Type GetComponentTypeByName(string name)
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
  }
}