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
    public class ParticleManagerPatch
    {
      public static CaptureState UpdateParticles;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(ParticleManager).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(ParticleManagerPatch).GetMethod("ParticleManager_Update_Replace"))
        );

        UpdateParticles = Capture.Get("Showperf.Update.Particles");
      }

      public static void Draw(CaptureState cs, SpriteBatch spriteBatch, bool inWater, bool? inSub, ParticleBlendState blendState, bool? background = null)
      {
        ParticleManager _ = GameMain.ParticleManager;
        if (_ == null) return;

        if (!cs.IsActive)
        {
          _.Draw(spriteBatch, inWater, inSub, blendState, background);
          return;
        }

        Capture.Draw.EnsureCategory(cs);
        Stopwatch sw = new Stopwatch();


        ParticlePrefab.DrawTargetType drawTarget = inWater ? ParticlePrefab.DrawTargetType.Water : ParticlePrefab.DrawTargetType.Air;

        foreach (var particle in _.particlesInCreationOrder)
        {
          sw.Restart();
          if (particle.BlendState != blendState) { continue; }
          //equivalent to !particles[i].DrawTarget.HasFlag(drawTarget) but garbage free and faster
          if ((particle.DrawTarget & drawTarget) == 0) { continue; }
          if (inSub.HasValue)
          {
            bool isOutside = particle.CurrentHull == null;
            if (particle.DrawOrder != ParticleDrawOrder.Foreground && isOutside == inSub.Value)
            {
              continue;
            }
          }
          if (background.HasValue)
          {
            bool isBackgroundParticle = particle.DrawOrder == ParticleDrawOrder.Background;
            if (background.Value != isBackgroundParticle) { continue; }
          }

          ParticlePatch.Draw(spriteBatch, particle);

          sw.Stop();
          Capture.Draw.AddTicks(sw.ElapsedTicks, cs, particle.ToString());
        }
      }

      public static bool ParticleManager_Update_Replace(float deltaTime, ParticleManager __instance)
      {
        ParticleManager _ = __instance;

        Stopwatch sw = new Stopwatch();

        Capture.Update.EnsureCategory(UpdateParticles);

        _.MaxParticles = GameSettings.CurrentConfig.Graphics.ParticleLimit;

        for (int i = 0; i < _.particleCount; i++)
        {
          bool remove;
          try
          {
            sw.Restart();
            remove = _.particles[i].Update(deltaTime) == Particle.UpdateResult.Delete;
            sw.Stop();

            Capture.Update.AddTicks(sw.ElapsedTicks, UpdateParticles, _.particles[i].ToString());
          }
          catch (Exception e)
          {
            DebugConsole.ThrowError("Particle update failed", e);
            remove = true;
          }
          sw.Restart();
          if (remove) { _.RemoveParticle(i); }
          sw.Stop();

          Capture.Update.AddTicks(sw.ElapsedTicks, UpdateParticles, "RemoveParticle");
        }

        return false;
      }


    }
  }
}