using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public static float DefaultStringWidth = 260f;
    public static float DefaultStringHeight = GUI.AdjustForTextScale(12);
    public static float MonospacedFontRealSize = 0.8f;

    public static Color ShowperfGradient(double f) => ShowperfGradient((float)f);

    public static Color ShowperfGradient(float f)
    {
      return ToolBox.GradientLerp(f,
            Color.MediumSpringGreen,
            Color.Yellow,
            Color.Orange,
            Color.Red,
            Color.Magenta,
            Color.Magenta
      );
    }

    public static void DrawStringWithScale(SpriteBatch spriteBatch, GUIFont font, string text, Vector2 position, Color color, Vector2 scale)
    {
      Vector2 textSize = font.MeasureString(text);
      GUI.DrawRectangle(spriteBatch, position, textSize, Color.Black * 0.8f, true);

      font.DrawString(spriteBatch, text, position, color, rotation: 0, origin: new Vector2(0, 0), scale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);
    }
  }
}