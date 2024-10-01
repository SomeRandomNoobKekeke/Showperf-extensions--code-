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
    public static void CursedWrappers(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.4f, 0.2f, 0.4f, 0.6f)
      {
        HideChildrenOutsideFrame = false,
      });


      CUIVerticalList v = new CUIVerticalList(0, 0, 1, 1)
      {
        HideChildrenOutsideFrame = false,
        BackgroundColor = Color.Red,
      };

      f.Append(v);

      CUIHorizontalList h = new CUIHorizontalList()
      {
        FitContent = new CUIBool2(false, true),
        HideChildrenOutsideFrame = false,
      };
      h.BackgroundColor = Color.Blue;
      v.Append(h);

      h.Append(new CUIButton("123"));

      CUIComponent wrapper = new CUIComponent()
      {
        FitContent = new CUIBool2(true, true)
      };

      wrapper.Append(new CUIButton("123"));
      h.Append(wrapper);
    }

  }
}