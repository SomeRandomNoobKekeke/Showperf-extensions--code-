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
    public static void UpdatePropagation(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.6f, 0.2f, 0.3f, 0.6f));

      f["list"] = new CUIVerticalList(0, 0, 1, 1)
      {
        Scrollable = true,
        //HideChildrenOutsideFrame = false,
      };

      for (int i = 0; i < 21; i++)
      {
        CUIComponent l = f["list"].Append(new CUIFrame(0, 0, 1, null)
        {
          Absolute = new CUINullRect(h: 30),
          BackgroundColor = Color.Blue,
        });

        l["h"] = new CUIHorizontalList(0, 0, 1, 1);
        l["h"]["Left"] = new CUIButton($"{i} Left")
        {
          Relative = new CUINullRect(w: 0.5f),
        };
        l["h"]["Right"] = new CUIButton($"{i} Right")
        {
          Relative = new CUINullRect(w: 0.5f),
        };
      }

      CUI.Append(f);
    }
  }
}