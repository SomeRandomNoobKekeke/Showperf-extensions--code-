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
    public static CUIComponent ClickSounds()
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

      return f;
    }

    public static CUIComponent FrameInFrame()
    {
      CUIFrame outer = new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f);
      CUIFrame inner = new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f);
      outer.Append(inner);

      inner.BackgroundColor = Color.Yellow * 0.5f;
      inner.AbsoluteMin.Left = 100f;

      return outer;
    }
  }
}