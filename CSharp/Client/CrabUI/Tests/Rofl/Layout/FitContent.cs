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
    public static void FitContent(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.6f, 0.2f, 0.3f, 0.6f));

      CUIVerticalList l = new CUIVerticalList()
      {
        Relative = new CUINullRect(0.2f, 0.2f, null, 0.4f),
        FitContent = new CUIBool2(true, false),
        // Absolute = new CUINullRect(0, 0, 100, 100),
        BackgroundColor = Color.Green,
      };

      l.Append(new CUIButton("1"));
      l.Append(new CUIButton("bebebeb"));
      l.Append(new CUIButton("ewrqwrqwerwqer"));
      l.Append(new CUIButton("be"));
      l.Append(new CUIButton("bebebeb"));
      l.Append(new CUIButton("ewrqwrqwerwqer"));
      l.Append(new CUIButton("ewrqwrqwerwqe"));
      l.Append(new CUIButton("ewrqwrqwerwq"));

      f.Append(l);

    }
  }
}