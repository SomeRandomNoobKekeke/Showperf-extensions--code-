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
    public static void FillEmptySpace(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);

      CUIComponent l = f.Append(new CUIVerticalList(0f, 0f, 1f, 0.9f));
      l.BackgroundColor = Color.Blue * 0.25f;


      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe"));
      CUIComponent b = l.Append(new CUIButton($"bebebe"));
      b.Relative = new CUINullRect(null, null, null, 0.1f);

      l.Append(new CUITextBlock("be be be be be be be be be be be be be be be be be be")
      {
        TextScale = 1.5f,
        TextAling = new CUIAnchor(CUIAnchorType.CenterCenter),
        BackgroundColor = Color.Red * 0.25f,
        FillEmptySpace = new CUIBool2(true, true),
      });

      l.Append(new CUITextBlock("be be be be be be be be be be be be be be be be be be")
      {
        TextScale = 1.5f,
        TextAling = new CUIAnchor(CUIAnchorType.CenterCenter),
        BackgroundColor = Color.Red * 0.25f,
        FillEmptySpace = new CUIBool2(true, true),
      });

      CUI.Append(f);
    }
  }
}