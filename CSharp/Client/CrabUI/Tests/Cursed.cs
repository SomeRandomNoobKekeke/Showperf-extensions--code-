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
    public static void Cursed(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);

      CUI.Append(f);
    }
  }
}