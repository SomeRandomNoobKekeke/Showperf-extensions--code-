using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Extensions;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Voronoi2;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class LevelRendererPatch
    {
      public static CaptureState LevelRenderer;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LevelRenderer).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("LevelRenderer_Update_Replace"))
        );

        LevelRenderer = Capture.Get("Showperf.Update.Level.LevelRenderer");
      }

      public static bool LevelRenderer_Update_Replace(float deltaTime, Camera cam, LevelRenderer __instance)
      {
        LevelRenderer _ = __instance;
        Stopwatch sw = new Stopwatch();

        sw.Restart();
        if (_.CollapseEffectStrength > 0.0f)
        {
          _.CollapseEffectStrength = Math.Max(0.0f, _.CollapseEffectStrength - deltaTime);
        }
        if (_.ChromaticAberrationStrength > 0.0f)
        {
          _.ChromaticAberrationStrength = Math.Max(0.0f, _.ChromaticAberrationStrength - deltaTime * 10.0f);
        }
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, LevelRenderer, "ChromaticAberrationStrength");

        sw.Restart();
        if (_.level.GenerationParams.FlashInterval.Y > 0)
        {
          _.flashCooldown -= deltaTime;
          if (_.flashCooldown <= 0.0f)
          {
            _.flashTimer = 1.0f;
            if (_.level.GenerationParams.FlashSound != null)
            {
              _.level.GenerationParams.FlashSound.Play(1.0f, "default");
            }
            _.flashCooldown = Rand.Range(_.level.GenerationParams.FlashInterval.X, _.level.GenerationParams.FlashInterval.Y, Rand.RandSync.Unsynced);
          }
          if (_.flashTimer > 0.0f)
          {
            float brightness = _.flashTimer * 1.1f - PerlinNoise.GetPerlin((float)Timing.TotalTime, (float)Timing.TotalTime * 0.66f) * 0.1f;
            _.FlashColor = _.level.GenerationParams.FlashColor.Multiply(MathHelper.Clamp(brightness, 0.0f, 1.0f));
            _.flashTimer -= deltaTime * 0.5f;
          }
          else
          {
            _.FlashColor = Color.TransparentBlack;
          }
        }
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, LevelRenderer, "Flashes");

        sw.Restart();
        //calculate the sum of the forces of nearby level triggers
        //and use it to move the water texture and water distortion effect
        Vector2 currentWaterParticleVel = _.level.GenerationParams.WaterParticleVelocity;
        foreach (LevelObject levelObject in _.level.LevelObjectManager.GetVisibleObjects())
        {
          if (levelObject.Triggers == null) { continue; }
          //use the largest water flow velocity of all the triggers
          Vector2 objectMaxFlow = Vector2.Zero;
          foreach (LevelTrigger trigger in levelObject.Triggers)
          {
            Vector2 vel = trigger.GetWaterFlowVelocity(cam.WorldViewCenter);
            if (vel.LengthSquared() > objectMaxFlow.LengthSquared())
            {
              objectMaxFlow = vel;
            }
          }
          currentWaterParticleVel += objectMaxFlow;
        }

        _.waterParticleVelocity = Vector2.Lerp(_.waterParticleVelocity, currentWaterParticleVel, deltaTime);

        WaterRenderer.Instance?.ScrollWater(_.waterParticleVelocity, deltaTime);

        if (_.level.GenerationParams.WaterParticles != null)
        {
          Vector2 waterTextureSize = _.level.GenerationParams.WaterParticles.size * _.level.GenerationParams.WaterParticleScale;
          _.waterParticleOffset += new Vector2(_.waterParticleVelocity.X, -_.waterParticleVelocity.Y) * _.level.GenerationParams.WaterParticleScale * deltaTime;
          while (_.waterParticleOffset.X <= -waterTextureSize.X) { _.waterParticleOffset.X += waterTextureSize.X; }
          while (_.waterParticleOffset.X >= waterTextureSize.X) { _.waterParticleOffset.X -= waterTextureSize.X; }
          while (_.waterParticleOffset.Y <= -waterTextureSize.Y) { _.waterParticleOffset.Y += waterTextureSize.Y; }
          while (_.waterParticleOffset.Y >= waterTextureSize.Y) { _.waterParticleOffset.Y -= waterTextureSize.Y; }
        }
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, LevelRenderer, "Water flow");

        return false;
      }

    }
  }
}