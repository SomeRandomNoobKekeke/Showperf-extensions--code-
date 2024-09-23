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
    public static void Anchors(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUIFrame(0.2f, 0.2f, 0.6f, 0.6f));
      f.HideChildrenOutsideFrame = false;

      CUIComponent c = new CUIComponent();

      foreach (CUIAnchorType type in Enum.GetValues(typeof(CUIAnchorType)))
      {
        c = f.Append(new CUIComponent()
        {
          Absolute = new CUINullRect(null, null, 30, 30),
          Anchor = new CUIAnchor(type),
          BackgroundColor = Color.Yellow,
        });
      }
    }
  }
}