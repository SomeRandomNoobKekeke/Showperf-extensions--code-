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
    public static void ClickSounds(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);
      CUIComponent l = f.Append(new CUIVerticalList(0f, 0f, 1f, 1f)
      {
        Scrollable = true
      });


      foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      {
        CUIComponent b = l.Append(new CUIButton($"{sound}") { ClickSound = sound });
        b.Relative = new CUINullRect(null, null, null, 0.1f);
      }
      foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      {
        CUIComponent b = l.Append(new CUITextBlock($"{sound}"));
      }

      CUI.Append(f);
    }

    public static void FrameInFrame(CUIMainComponent CUI)
    {
      CUIFrame outer = new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f);
      CUIFrame inner = new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f);
      outer.Append(inner);

      inner.BackgroundColor = Color.Yellow * 0.5f;
      inner.AbsoluteMin = new CUINullRect(100f, null, null, null);
      //inner.RelativeMax.Width = 1f;

      CUI.Append(outer);
    }

    public static void ButtonsOnSimpleLayout(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);

      CUIComponent b1 = f.Append(new CUIButton("bebebe"));
      CUIComponent b2 = f.Append(new CUIButton("bebebe"));
      CUIComponent b3 = f.Append(new CUIButton("bebebe"));

      b1.Absolute = new CUINullRect(30, 0, null, null);
      b2.Absolute = new CUINullRect(0, 20, null, null);
      b3.Absolute = new CUINullRect(30, 40, null, null);

      CUIComponent t1 = f.Append(new CUITextBlock("kokoko") { BorderColor = Color.White });
      CUIComponent t2 = f.Append(new CUITextBlock("kokoko") { BorderColor = Color.White });
      CUIComponent t3 = f.Append(new CUITextBlock("kokoko") { BorderColor = Color.White });

      t1.Absolute = new CUINullRect(100, 0, null, null);
      t2.Absolute = new CUINullRect(80, 20, null, null);
      t3.Absolute = new CUINullRect(100, 40, null, null);

      CUI.Append(f);
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
      CUIComponent b = l.Append(new CUIButton($"bebebe"));
      b.Relative = new CUINullRect(null, null, null, 0.1f);

      l.Append(new CUITextBlock("be be be be be be be be be be be be be be be be be be")
      {
        TextScale = 1.5f,
        TextAling = new CUIAnchor(CUIAnchorType.CenterCenter),
        BackgroundColor = Color.Red * 0.25f,
        FillEmptySpace = true
      });

      l.Append(new CUITextBlock("be be be be be be be be be be be be be be be be be be")
      {
        TextScale = 1.5f,
        TextAling = new CUIAnchor(CUIAnchorType.CenterCenter),
        BackgroundColor = Color.Red * 0.25f,
        FillEmptySpace = true
      });

      CUI.Append(f);
    }
  }
}