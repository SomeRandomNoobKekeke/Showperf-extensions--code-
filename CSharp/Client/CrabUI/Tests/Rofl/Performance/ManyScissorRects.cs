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
    private class CUITestLooseFrame : CUIFrame
    {
      internal override CUIBoundaries ChildrenBoundaries => new CUIBoundaries(
        minX: -Real.Width * 0.1f,
        maxX: +Real.Width * 1.1f,
        minY: -Real.Height * 0.1f,
        maxY: +Real.Height * 1.1f
      );

      public CUITestLooseFrame(float? x = null, float? y = null, float? w = null, float? h = null) : base(x, y, w, h)
      {
        BackgroundColor = Color.Black * 0.2f;
        // HideChildrenOutsideFrame = false;
      }
    }

    public static void ManyScissorRects(CUIMainComponent CUI)
    {
      CUIComponent f = CUI.Append(new CUITestLooseFrame(0.2f, 0.2f, 0.6f, 0.6f));

      float fpos = 0.02f;
      float fsize = 1f - fpos * 2;

      for (int i = 0; i < 20; i++)
      {
        f = f.Append(new CUITestLooseFrame(fpos, fpos, fsize, fsize));
      }

      int count = 10;
      float size = 1.0f / count;

      for (int x = 0; x < count; x++)
      {
        for (int y = 0; y < count; y++)
        {
          CUIComponent c = f.Append(new CUIComponent(x * size, y * size, size, size));

          c.BackgroundColor = ToolBox.GradientLerp((float)(x + y) / (2 * count),
            Color.MediumSpringGreen,
            Color.Yellow,
            Color.Orange,
            Color.Red,
            Color.Magenta,
            Color.Magenta
          );

          c.BorderColor = ToolBox.GradientLerp((float)(2 * count - x - y) / (2 * count),
            Color.MediumSpringGreen,
            Color.Yellow,
            Color.Orange,
            Color.Red,
            Color.Magenta,
            Color.Magenta
          );

          //c.BackgroundColor = Color.Transparent;
          //c.BorderColor = Color.Transparent;
        }
      }
    }
  }
}