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

          if (remove) { _.RemoveParticle(i); }
        }

        return false;
      }


    }
  }
}