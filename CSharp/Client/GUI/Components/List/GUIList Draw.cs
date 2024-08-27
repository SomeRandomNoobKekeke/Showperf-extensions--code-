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
    public partial class GUIList
    {
      public void DrawFrame(SpriteBatch spriteBatch)
      {
        GUI.DrawRectangle(
          spriteBatch,
          HeaderDrawPosition,
          FullSize,
          backgroundColor, true
        );

        GUI.DrawRectangle(
          spriteBatch,
          new Vector2(DrawPosition.X, DrawPosition.Y - borderWidth),
          new Vector2(Size.X, borderWidth),
          BorderColor, true
        );

        GUI.DrawRectangle(
          spriteBatch,
          HeaderDrawPosition - Vector2.One,
          FullSizeWithBorders,
          BorderColor, false
        );
      }


      public void DrawHeader(SpriteBatch spriteBatch)
      {
        DrawString(
          spriteBatch,
          HeaderDrawPosition,
          Caption,
          Color.White
        );

        DrawString(
          spriteBatch,
          HeaderDrawPosition + new Vector2(0, symbolSize.Y * 1),
          $"By ID {CaptureById} | From: {String.Join(", ", CaptureFrom)}",
          Color.White
        );

        DrawString(
          spriteBatch,
          HeaderDrawPosition + new Vector2(0, symbolSize.Y * 2),
          $"Sum:{View.ConverToUnits(Sum)} {View.UnitsName} | Linearity:{String.Format("{0:0.000000}", Linearity)}",
          Color.White
        );
      }

      public void Draw(SpriteBatch spriteBatch)
      {

        DrawFrame(spriteBatch);
        DrawHeader(spriteBatch);

        float y = 0;
        foreach (var v in Values.Take(ShowedRange))
        {
          DrawString(
            spriteBatch,
            new Vector2(DrawPosition.X, DrawPosition.Y + y),
            v.ToString(),
            GetElementColor(v)
          );

          y += symbolSize.Y;
        }
      }

      public void DrawString(SpriteBatch spriteBatch, Vector2 pos, string text, Color color)
      {
        GUIStyle.MonospacedFont.DrawString(
            spriteBatch,
            text: text,
            position: pos,
            color: color,
            rotation: 0,
            origin: Vector2.Zero,
            scale: textScale,
            spriteEffects: SpriteEffects.None,
            layerDepth: 0.1f
        );
      }
    }
  }
}