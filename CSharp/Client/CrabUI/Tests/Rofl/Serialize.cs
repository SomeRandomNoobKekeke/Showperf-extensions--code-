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
      // CUIComponent l = f.Append(new CUIVerticalList()
      // {
      //   Relative = new CUINullRect(0, 0, 1, 1),
      //   Scrollable = true
      // });


      // foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      // {
      //   l[$"{sound.ToString()} Button"] = new CUIButton($"{sound}")
      //   {
      //     Relative = new CUINullRect(h: 0.1f),
      //     ClickSound = sound,
      //   };
      // }
      // foreach (var sound in Enum.GetValues(typeof(GUISoundType)).Cast<GUISoundType>())
      // {
      //   CUIComponent b = l.Append(new CUITextBlock($"{sound}"));
      // }

      string s = f.Serialize();
      CUIDebug.log(s);

      CUIComponent c = CUIComponent.Deserialize(s);
      CUIDebug.log("------------------------------");
      CUIDebug.log(c.Serialize());

      Main.Append(c);
    }
  }
}