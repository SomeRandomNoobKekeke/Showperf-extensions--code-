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
    public static void ManyRects(CUIMainComponent CUI)
    {
      CUIComponent f = new CUIFrame(0.6f, 0.2f, 0.2f, 0.6f);

      int count = 100;
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

      CUI.Append(f);
    }
  }
}