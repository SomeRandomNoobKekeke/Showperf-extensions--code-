using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class ParticlePatch
    {
      public static void Initialize()
      {

      }

      public static bool Draw(SpriteBatch spriteBatch, Particle __instance)
      {
        Particle _ = __instance;

        Color HighlightColor = Color.Yellow;

        if (_.startDelay > 0.0f) { return false; }

        Vector2 drawSize = _.size;
        if (_.prefab.GrowTime > 0.0f && _.totalLifeTime - _.lifeTime < _.prefab.GrowTime)
        {
          drawSize *= MathUtils.SmoothStep((_.totalLifeTime - _.lifeTime) / _.prefab.GrowTime);
        }

        Color currColor = new Color(_.color.ToVector4() * _.ColorMultiplier);

        Vector2 drawPos = new Vector2(_.drawPosition.X, -_.drawPosition.Y);
        if (_.prefab.Sprites[_.spriteIndex] is SpriteSheet sheet)
        {
          sheet.Draw(
              spriteBatch, _.animFrame,
              drawPos,
              HighlightColor,
              //currColor * (currColor.A / 255.0f),
              _.prefab.Sprites[_.spriteIndex].Origin, _.drawRotation,
              drawSize, SpriteEffects.None, _.prefab.Sprites[_.spriteIndex].Depth);
        }
        else
        {
          _.prefab.Sprites[_.spriteIndex].Draw(spriteBatch,
              drawPos,
              HighlightColor,
              //currColor * (currColor.A / 255.0f),
              _.prefab.Sprites[_.spriteIndex].Origin, _.drawRotation,
              drawSize, SpriteEffects.None, _.prefab.Sprites[_.spriteIndex].Depth);
        }

        /*
        if (GameMain.DebugDraw && _.prefab.UseCollision)
        {
          GUI.DrawLine(spriteBatch,
              drawPos - Vector2.UnitX * _.colliderRadius.X,
              drawPos + Vector2.UnitX * _.colliderRadius.X,
              Color.Gray);
          GUI.DrawLine(spriteBatch,
              drawPos - Vector2.UnitY * _.colliderRadius.Y,
              drawPos + Vector2.UnitY * _.colliderRadius.Y,
              Color.Gray);
        }
        */

        return false;
      }


    }
  }
}