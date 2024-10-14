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
    public static void Culling(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f)
      {
        //HideChildrenOutsideFrame = false,
      });

      f["list"] = new CUIVerticalList()
      {
        Relative = new CUINullRect(0, 0, 1, 1),
        Scrollable = true,

        //HideChildrenOutsideFrame = false,
      };

      for (int i = 0; i < 2000; i++)
      {
        f["list"].Append(new CUIButton(i.ToString()));
      }

      CUI.Append(f);
    }
  }
}