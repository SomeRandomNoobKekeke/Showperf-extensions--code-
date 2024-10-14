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

      CUIVerticalList v = new CUIVerticalList()
      {
        Relative = new CUINullRect(0, 0, 1, 1),
        HideChildrenOutsideFrame = false,
        BackgroundColor = Color.Red,
      };
      f.Append(v);


      CUIHorizontalList h = new CUIHorizontalList()
      {
        Relative = new CUINullRect(0, 0, 1, null),
        FitContent = new CUIBool2(false, true),
        HideChildrenOutsideFrame = false,
        BackgroundColor = Color.Blue,
      };
      v.Append(h);

      h.Append(new CUIButton("123"));


      CUIDropDown dd = new CUIDropDown();

      dd.Add(321);
      dd.Select(321);

      h.Append(dd);


      h = new CUIHorizontalList()
      {
        Relative = new CUINullRect(0, 0, 1, null),
        FitContent = new CUIBool2(false, true),
        HideChildrenOutsideFrame = false,
        BackgroundColor = Color.Blue,
      };
      v.Append(h);

      h.Append(new CUIButton("123"));


      dd = new CUIDropDown();

      dd.Add(321);
      dd.Select(321);

      h.Append(dd);
    }

  }
}