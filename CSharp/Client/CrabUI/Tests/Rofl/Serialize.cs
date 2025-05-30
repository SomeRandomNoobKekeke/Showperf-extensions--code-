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
    public static void Serialize(CUIMainComponent Main)
    {

      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);
      CUIComponent l = f.Append(new CUIVerticalList()
      {
        Relative = new CUINullRect(0, 0, 1, 1),
        Scrollable = true
      });


      foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      {
        CUIButton b;
        l[$"{sound.ToString()} Button"] = b = new CUIButton($"{sound}")
        {
          Relative = new CUINullRect(h: 0.1f),
          ClickSound = sound,
        };

        if (sound == GUISoundType.PickItem)
        {
          b.InactiveColor = Color.Yellow;
        }
      }
      foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      {
        CUIComponent b = l.Append(new CUITextBlock($"{sound}"));
      }

      f.SaveToFile(CUI.IgnoreDir + "/test.xml");
      CUIComponent c = CUIComponent.LoadFromFile(CUI.IgnoreDir + "/test.xml");

      Main.Append(c);
    }
  }
}