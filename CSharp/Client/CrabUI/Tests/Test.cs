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
  public static class CUITest
  {
    public static void ClickSounds(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);
      CUIComponent l = f.Append(new CUIVerticalList(0f, 0f, 1f, 1f));
      foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      {
        CUIComponent b = l.Append(new CUIButton($"{sound}") { ClickSound = sound });
        b.Relative.Height = 0.1f;
      }
      foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      {
        CUIComponent b = l.Append(new CUITextBlock($"{sound}"));
        b.Relative.Height = 0.1f;
      }

      CUI.Append(f);
    }

    public static void FrameInFrame(CUIMainComponent CUI)
    {
      CUIFrame outer = new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f);
      CUIFrame inner = new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f);
      outer.Append(inner);

      inner.BackgroundColor = Color.Yellow * 0.5f;
      inner.AbsoluteMin.Left = 100f;

      CUI.Append(outer);
    }

    public static void FillEmptySpace(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);

      CUIComponent l = f.Append(new CUIVerticalList(0f, 0f, 1f, 0.9f));
      l.BackgroundColor = Color.Blue * 0.25f;


      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe"));
      l.Append(new CUIButton($"bebebe")).Relative.Height = 0.1f;


      l.Append(new CUITextBlock("be be be be be be be be be be be be be be be be be be")
      {
        TextScale = 1.5f,
        TextAling = CUITextAling.Center,
        BackgroundColor = Color.Red * 0.25f,
        FillEmptySpace = true
      });

      l.Append(new CUITextBlock("be be be be be be be be be be be be be be be be be be")
      {
        TextScale = 1.5f,
        TextAling = CUITextAling.Center,
        BackgroundColor = Color.Red * 0.25f,
        FillEmptySpace = true
      });

      CUI.Append(f);
    }
  }
}