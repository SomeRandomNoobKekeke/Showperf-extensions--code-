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
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class BebebAttribute : System.Attribute
    {
      // See the attribute guidelines at
      //  http://go.microsoft.com/fwlink/?LinkId=85236
      public string B;

      // This is a positional argument
      public BebebAttribute(string b)
      {
        this.B = b;
      }
    }

    [Bebeb("kokoko")]
    public static bool bebe;

    public static void Serialize(CUIMainComponent Main)
    {
      BebebAttribute? A = (BebebAttribute)Attribute.GetCustomAttribute(
        typeof(CUITest).GetField("bebe", BindingFlags.Static | BindingFlags.Public),
        typeof(BebebAttribute)
      );


      CUI.log(A.B);

      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);
      CUIComponent l = f.Append(new CUIVerticalList()
      {
        Relative = new CUINullRect(0, 0, 1, 1),
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

      Main.Append(f);

      CUI.log(f.Serialize());
    }
  }
}