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
    public static void Swipe(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f));
      f.Layout = new CUILayoutVerticalList();

      f.Append(new CUIComponent()
      {
        Absolute = new CUINullRect(null, null, null, 30)
      });

      CUIComponent l = f.Append(new CUIVerticalList()
      {
        Scrollable = true,
        Swipeable = true,
        ConsumeDragAndDrop = true,
        HideChildrenOutsideFrame = true,
        FillEmptySpace = new CUIBool2(false, true),
      });

      for (int i = 0; i < 100; i++)
      {
        l.Append(new CUITextBlock($"Button {i}"));
      }
    }
  }
}