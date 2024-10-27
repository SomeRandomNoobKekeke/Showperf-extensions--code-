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
    public static void NestedScale(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.4f, 0.2f, 0.4f, 0.6f));

      CUIMap m = new CUIMap()
      {
        Relative = new CUINullRect(0, 0, 1, 1),
      };

      m.Add(new CUIButton($"just button")
      {
        Absolute = new CUINullRect(x: 50, y: 100),
        ConsumeSwipe = false,
      });


      CUIComponent c = m.Add(new CUIComponent()
      {
        Absolute = new CUINullRect(50, 300, 200, 200),
      });

      c["1"] = new CUIComponent()
      {
        Relative = new CUINullRect(0.4f, 0.2f, 0.4f, 0.6f),
      };

      c["1"]["2"] = new CUIComponent()
      {
        Relative = new CUINullRect(0.4f, 0.2f, 0.4f, 0.6f),
        BackgroundColor = Color.Yellow,
      };

      m.Add(new CUIButton($"just button")
      {
        Absolute = new CUINullRect(x: 50, y: 100),
        ConsumeSwipe = false,
      });


      CUIComponent v = m.Add(new CUIVerticalList()
      {
        FitContent = new CUIBool2(true, true),
        Absolute = new CUINullRect(x: 150, y: 100),
      });

      for (int i = 0; i < 5; i++)
      {
        v.Append(new CUIButton($"button {i}") { ConsumeSwipe = false, });
      }



      CUIComponent v2 = m.Add(new CUIVerticalList()
      {
        FitContent = new CUIBool2(true, false),
        Absolute = new CUINullRect(x: 300, y: 100, w: null, h: 200),
        BackgroundColor = Color.DarkBlue
      });

      for (int i = 0; i < 5; i++)
      {
        v2.Append(new CUIButton($"button {i}") { ConsumeSwipe = false, });
      }

      v2["Wrapper"] = new CUIComponent()
      {
        ConsumeSwipe = false,
        FillEmptySpace = new CUIBool2(false, true),
      };
      v2["Wrapper"].Append(new CUIButton("button")
      {
        Relative = new CUINullRect(0, 0, 1, 1)
      });




      f.Append(m);

    }
  }
}