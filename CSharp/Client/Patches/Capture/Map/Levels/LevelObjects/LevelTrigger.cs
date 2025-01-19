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
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class LevelTriggerPatch
    {
      public static CaptureState LevelObjectTriggers;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LevelTrigger).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(LevelTriggerPatch).GetMethod("LevelTrigger_Update_Replace"))
        );

        LevelObjectTriggers = Capture.Get("Showperf.Update.Level.LevelObjectManager.Triggers");
      }

      public static void CaptureTrigger(long ticks, LevelTrigger _, Entity triggerer, string name)
      {
        if (LevelObjectTriggers.ByID)
        {
          ParentInfo p = LevelTrigger_Parent.GetValueOrDefault(_);
          Capture.Update.AddTicks(ticks, LevelObjectTriggers, $"{p.Name} - {triggerer} {name}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, LevelObjectTriggers, name);
        }
      }

      public static void CaptureTrigger(long ticks, LevelTrigger _, string name)
      {
        if (LevelObjectTriggers.ByID)
        {
          ParentInfo p = LevelTrigger_Parent.GetValueOrDefault(_);
          Capture.Update.AddTicks(ticks, LevelObjectTriggers, $"{p.Name}  {name}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, LevelObjectTriggers, name);
        }
      }

      public static bool LevelTrigger_Update_Replace(float deltaTime, LevelTrigger __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !LevelObjectTriggers.IsActive) return true;

        Capture.Update.EnsureCategory(LevelObjectTriggers);

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        LevelTrigger _ = __instance;

        if (_.ParentTrigger != null && !_.ParentTrigger.IsTriggered) { return false; }


        bool isNotClient = true;
#if CLIENT
        isNotClient = GameMain.Client == null;
#endif

        sw.Restart();
        if (!_.UseNetworkSyncing || isNotClient)
        {
          if (_.GlobalForceDecreaseInterval > 0.0f && Level.Loaded?.LevelObjectManager != null &&
              Level.Loaded.LevelObjectManager.GlobalForceDecreaseTimer % (_.GlobalForceDecreaseInterval * 2) < _.GlobalForceDecreaseInterval)
          {
            _.NeedsNetworkSyncing |= _.currentForceFluctuation > 0.0f;
            _.currentForceFluctuation = 0.0f;
          }
          else if (_.ForceFluctuationStrength > 0.0f)
          {
            //no need for force fluctuation (or network updates) if the trigger limits velocity and there are no triggerers
            if (_.forceMode != LevelTrigger.TriggerForceMode.LimitVelocity || _.triggerers.Any())
            {
              _.forceFluctuationTimer += deltaTime;
              if (_.forceFluctuationTimer > _.ForceFluctuationInterval)
              {
                _.NeedsNetworkSyncing = true;
                _.currentForceFluctuation = Rand.Range(1.0f - _.ForceFluctuationStrength, 1.0f);
                _.forceFluctuationTimer = 0.0f;
              }
            }
          }

          if (_.randomTriggerProbability > 0.0f)
          {
            _.randomTriggerTimer += deltaTime;
            if (_.randomTriggerTimer > _.randomTriggerInterval)
            {
              if (Rand.Range(0.0f, 1.0f) < _.randomTriggerProbability)
              {
                _.NeedsNetworkSyncing = true;
                _.triggeredTimer = _.stayTriggeredDelay;
              }
              _.randomTriggerTimer = 0.0f;
            }
          }
        }
        sw.Stop();
        CaptureTrigger(sw.ElapsedTicks, _, "ForceFluctuation");

        sw.Restart();
        LevelTrigger.RemoveInActiveTriggerers(_.PhysicsBody, _.triggerers);
        sw.Stop();
        CaptureTrigger(sw.ElapsedTicks, _, "RemoveInActiveTriggerers");

        sw.Restart();
        if (_.stayTriggeredDelay > 0.0f)
        {
          if (_.triggerers.Count == 0)
          {
            _.triggeredTimer -= deltaTime;
          }
          else
          {
            _.triggeredTimer = _.stayTriggeredDelay;
          }
        }
        sw.Stop();
        CaptureTrigger(sw.ElapsedTicks, _, "stayTriggered");

        if (_.triggerOnce && _.triggeredOnce)
        {
          return false;
        }

        if (_.PhysicsBody != null)
        {
          if (_.currentForceFluctuation <= 0.0f && _.statusEffects.None() && _.attacks.None())
          {
            //no force atm, and no status effects or attacks the trigger could apply
            //    -> we can disable the collider and get a minor physics performance improvement
            _.PhysicsBody.Enabled = false;
            return false;
          }
          else
          {
            _.PhysicsBody.Enabled = true;
          }
        }


        long realStuff = 0;
        sw2.Restart();
        foreach (Entity triggerer in _.triggerers)
        {
          if (triggerer.Removed) { continue; }

          sw.Restart();
          LevelTrigger.ApplyStatusEffects(_.statusEffects, _.worldPosition, triggerer, deltaTime, _.targets);
          sw.Stop();
          realStuff += sw.ElapsedTicks;
          CaptureTrigger(sw.ElapsedTicks, _, triggerer, "ApplyStatusEffects");

          if (triggerer is IDamageable damageable)
          {
            sw.Restart();
            LevelTrigger.ApplyAttacks(_.attacks, damageable, _.worldPosition, deltaTime);
            sw.Stop();
            realStuff += sw.ElapsedTicks;
            CaptureTrigger(sw.ElapsedTicks, _, triggerer, "ApplyAttacks damageable");
          }
          else if (triggerer is Submarine submarine)
          {
            sw.Restart();
            LevelTrigger.ApplyAttacks(_.attacks, _.worldPosition, deltaTime);
            if (!_.InfectIdentifier.IsEmpty)
            {
              submarine.AttemptBallastFloraInfection(_.InfectIdentifier, deltaTime, _.InfectionChance);
            }
            sw.Stop();
            realStuff += sw.ElapsedTicks;
            CaptureTrigger(sw.ElapsedTicks, _, triggerer, "ApplyAttacks submarine");
          }

          if (_.Force.LengthSquared() > 0.01f)
          {
            if (triggerer is Character character)
            {
              sw.Restart();
              _.ApplyForce(character.AnimController.Collider);
              foreach (Limb limb in character.AnimController.Limbs)
              {
                if (limb.IsSevered) { continue; }
                _.ApplyForce(limb.body);
              }
              sw.Stop();
              realStuff += sw.ElapsedTicks;
              CaptureTrigger(sw.ElapsedTicks, _, triggerer, "ApplyForce character");
            }
            else if (triggerer is Submarine submarine)
            {
              sw.Restart();
              _.ApplyForce(submarine.SubBody.Body);
              sw.Stop();
              realStuff += sw.ElapsedTicks;
              CaptureTrigger(sw.ElapsedTicks, _, triggerer, "ApplyForce submarine");
            }
          }

          sw.Restart();
          if (triggerer == Character.Controlled || triggerer == Character.Controlled?.Submarine)
          {
            GameMain.GameScreen.Cam.Shake = Math.Max(GameMain.GameScreen.Cam.Shake, _.cameraShake);
          }
          sw.Stop();
          realStuff += sw.ElapsedTicks;
          CaptureTrigger(sw.ElapsedTicks, _, triggerer, "Cam.Shake");
        }
        sw2.Stop();
        CaptureTrigger(sw2.ElapsedTicks - realStuff, _, "iterate removed triggerers");



        if (_.triggerOnce && _.triggerers.Count > 0)
        {
          _.PhysicsBody.Enabled = false;
          _.triggeredOnce = true;
        }

        return false;
      }


    }
  }
}