using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public static partial class CUITest
  {
    public static void CUIDropDown(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.4f, 0.6f);

      CUIDropDown dd = new CUIDropDown(0.2f, 0.2f, null, null);
      dd.Add("bebebe1");
      dd.Add("bebebe2");
      dd.Add("bebebe3");
      dd.Add("bebebe4");
      dd.Add("bebebe5");
      dd.Add("bebebe6");

      dd.Select("bebebe1");

      f.Append(dd);
      f.Debug = true;

      CUI.Append(f);
    }
  }
}