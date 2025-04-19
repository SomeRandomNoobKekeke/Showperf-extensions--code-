using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using Barotrauma.SpriteDeformations;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class LevelObjectPatch
    {
      public static CaptureState LevelObjects;
      public static CaptureState LevelObjectSounds;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LevelObject).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(LevelObjectPatch).GetMethod("LevelObject_Update_Replace"))
        );

        LevelObjects = Capture.Get("Showperf.Update.Level.LevelObjectManager");
        LevelObjectSounds = Capture.Get("Showperf.Update.Level.LevelObjectManager.Sounds");
      }

      public static bool LevelObject_Update_Replace(LevelObject __instance, float deltaTime, Camera cam)
      {
        if (Showperf == null || !Showperf.Revealed || !LevelObjects.IsActive && !LevelObjectSounds.IsActive) return true;

        LevelObject _ = __instance;

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        sw.Restart();
        _.CurrentRotation = _.Rotation;
        if (_.ActivePrefab.SwingFrequency > 0.0f)
        {
          _.SwingTimer += deltaTime * _.ActivePrefab.SwingFrequency;
          _.SwingTimer = _.SwingTimer % MathHelper.TwoPi;
          //lerp the swing amount to the correct value to prevent it from abruptly changing to a different value
          //when a trigger changes the swing amoung
          _.CurrentSwingAmount = MathHelper.Lerp(_.CurrentSwingAmount, _.ActivePrefab.SwingAmountRad, deltaTime * 10.0f);

          if (_.ActivePrefab.SwingAmountRad > 0.0f)
          {
            _.CurrentRotation += (float)Math.Sin(_.SwingTimer) * _.CurrentSwingAmount;
          }
        }

        _.CurrentScale = Vector2.One * _.Scale;
        if (_.ActivePrefab.ScaleOscillationFrequency > 0.0f)
        {
          _.ScaleOscillateTimer += deltaTime * _.ActivePrefab.ScaleOscillationFrequency;
          _.ScaleOscillateTimer = _.ScaleOscillateTimer % MathHelper.TwoPi;
          _.CurrentScaleOscillation = Vector2.Lerp(_.CurrentScaleOscillation, _.ActivePrefab.ScaleOscillation, deltaTime * 10.0f);

          float sin = (float)Math.Sin(_.ScaleOscillateTimer);
          _.CurrentScale *= new Vector2(
              1.0f + sin * _.CurrentScaleOscillation.X,
              1.0f + sin * _.CurrentScaleOscillation.Y);
        }
        sw.Stop();
        if (LevelObjects.ByID)
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"{_}.Math");
        }
        else
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"Math");
        }


        sw.Restart();
        if (_.LightSources != null)
        {
          Vector2 position2D = new Vector2(_.Position.X, _.Position.Y);
          Vector2 camDiff = position2D - cam.WorldViewCenter;
          for (int i = 0; i < _.LightSources.Length; i++)
          {
            if (_.LightSourceTriggers[i] != null)
            {
              _.LightSources[i].Enabled = _.LightSourceTriggers[i].IsTriggered;
            }
            _.LightSources[i].Rotation = -_.CurrentRotation;
            _.LightSources[i].SpriteScale = _.CurrentScale;
            _.LightSources[i].Position = position2D - camDiff * _.Position.Z * LevelObjectManager.ParallaxStrength;
          }
        }
        sw.Stop();
        if (LevelObjects.ByID)
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"{_}.LightSources");
        }
        else
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"LightSources");
        }

        sw.Restart();
        if (_.spriteDeformations.Count > 0)
        {
          _.UpdateDeformations(deltaTime);
        }
        sw.Stop();
        if (LevelObjects.ByID)
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"{_}.Deformations");
        }
        else
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"Deformations");
        }

        sw.Restart();
        if (_.ParticleEmitters != null)
        {
          for (int i = 0; i < _.ParticleEmitters.Length; i++)
          {
            if (_.ParticleEmitterTriggers[i] != null && !_.ParticleEmitterTriggers[i].IsTriggered) { continue; }
            Vector2 emitterPos = _.LocalToWorld(_.Prefab.EmitterPositions[i]);
            _.ParticleEmitters[i].Emit(deltaTime, emitterPos, hullGuess: null,
                angle: _.ParticleEmitters[i].Prefab.Properties.CopyEntityAngle ? -_.CurrentRotation + MathHelper.Pi : 0.0f);
          }
        }
        sw.Stop();
        if (LevelObjects.ByID)
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"{_}.ParticleEmitters");
        }
        else
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"ParticleEmitters");
        }


        if (LevelObjectSounds.IsActive) Capture.Update.EnsureCategory(LevelObjectSounds);

        long roundSoundSoundPlay = 0;



        sw.Restart();
        for (int i = 0; i < _.Sounds.Length; i++)
        {
          if (_.Sounds[i] == null) { continue; }
          if (_.SoundTriggers[i] == null || _.SoundTriggers[i].IsTriggered)
          {
            RoundSound roundSound = _.Sounds[i];
            Vector2 soundPos = _.LocalToWorld(new Vector2(_.Prefab.Sounds[i].Position.X, _.Prefab.Sounds[i].Position.Y));
            if (Vector2.DistanceSquared(new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y), soundPos) <
                roundSound.Range * roundSound.Range)
            {
              if (_.SoundChannels[i] == null || !_.SoundChannels[i].IsPlaying)
              {
                sw2.Restart();
                _.SoundChannels[i] = roundSound.Sound.Play(roundSound.Volume, roundSound.Range, roundSound.GetRandomFrequencyMultiplier(), soundPos);
                sw2.Stop();
                roundSoundSoundPlay += sw2.ElapsedTicks;
                Capture.Update.AddTicks(sw2.ElapsedTicks, LevelObjectSounds, $"{_} roundSound.Sound.Play");

                // if (LevelObjects.ByID)
                // {
                //   Capture.RawCount.AddTicksOnce(1, LevelObjects, $"{_} roundSound.Sound.Play calls");
                // }
                // else
                // {
                //   Capture.RawCount.AddTicksOnce(1, LevelObjects, $"roundSound.Sound.Play calls");
                // }

              }
              if (_.SoundChannels[i] != null)
              {
                _.SoundChannels[i].Position = new Vector3(soundPos.X, soundPos.Y, 0.0f);
              }
            }
          }
          else if (_.SoundChannels[i] != null && _.SoundChannels[i].IsPlaying)
          {
            _.SoundChannels[i].FadeOutAndDispose();
            _.SoundChannels[i] = null;
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks - roundSoundSoundPlay, LevelObjectSounds, $"{_} rest");

        if (LevelObjects.ByID)
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"{_}.Sounds");
        }
        else
        {
          Capture.Update.AddTicks(sw.ElapsedTicks, LevelObjects, $"Sounds");
        }

        // if (LevelObjects.ByID)
        // {
        //   Capture.RawCount.AddTicksOnce(_.Sounds.Length, LevelObjects, $"{_}.Sounds.Length");
        // }
        // else
        // {
        //   Capture.RawCount.AddTicksOnce(_.Sounds.Length, LevelObjects, $"Sounds.Length");
        // }

        return false;
      }
    }
  }
}