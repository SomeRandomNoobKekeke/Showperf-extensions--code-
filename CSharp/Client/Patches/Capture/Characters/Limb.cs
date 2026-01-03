
using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using System.Globalization;
using MoonSharp.Interpreter;
using Barotrauma.Abilities;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public partial class LimbPatch
    {
      public static CaptureState SEState;

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Limb).GetMethod("ApplyStatusEffects", AccessTools.all),
          prefix: new HarmonyMethod(typeof(LimbPatch).GetMethod("Limb_ApplyStatusEffects_Replace"))
        );

        SEState = Capture.Get("Showperf.Update.Character.Update.StatusEffects.Limb");
      }

      public static void CaptureLimb(long ticks, ActionType type, Limb _, string text)
      {
        if (SEState.ByID)
        {
          Capture.Update.AddTicks(ticks, SEState, $"{_.character.Info?.DisplayName ?? _.character.ToString()}.{_.Name}.{type} {text}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, SEState, $"{_.character.ToString()}.{_.Name}.{type} {text}");
        }
      }

      public static bool Limb_ApplyStatusEffects_Replace(Limb __instance, ActionType actionType, float deltaTime)
      {
        if (Showperf == null || !Showperf.Revealed || !SEState.IsActive) return true;
        Capture.Update.EnsureCategory(SEState);

        Stopwatch sw = new Stopwatch();
        Stopwatch sw1 = new Stopwatch();

        Limb _ = __instance;

        sw1.Restart();

        if (!_.statusEffects.TryGetValue(actionType, out var statusEffectList))
        {
          sw1.Stop();
          CaptureLimb(sw1.ElapsedTicks, actionType, _, $"TryGetValue statusEffectList");
          return false;
        }
        sw1.Stop();
        CaptureLimb(sw1.ElapsedTicks, actionType, _, $"TryGetValue statusEffectList");

        foreach (StatusEffect statusEffect in statusEffectList)
        {
          if (statusEffect.ShouldWaitForInterval(_.character, deltaTime))
          {
            return false;
          }

          statusEffect.sourceBody = _.body;
          if (statusEffect.type == ActionType.OnDamaged)
          {
            if (!statusEffect.HasRequiredAfflictions(_.character.LastDamage)) { continue; }
            if (statusEffect.OnlyWhenDamagedByPlayer)
            {
              if (_.character.LastAttacker == null || !_.character.LastAttacker.IsPlayer)
              {
                continue;
              }
            }
          }

          if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems) ||
              statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
          {
            sw.Restart();
            _.targets.Clear();
            statusEffect.AddNearbyTargets(_.WorldPosition, _.targets);
            statusEffect.Apply(actionType, deltaTime, _.character, _.targets);
            sw.Stop();
            CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [Nearby stuff]");
          }
          else if (statusEffect.targetLimbs != null)
          {
            foreach (var limbType in statusEffect.targetLimbs)
            {
              if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
              {
                sw.Restart();
                // Target all matching limbs
                foreach (var limb in _.ragdoll.Limbs)
                {
                  if (limb.IsSevered) { continue; }
                  if (limb.type == limbType)
                  {
                    ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb);
                  }
                }
                sw.Stop();
                CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [AllLimbs]");
              }
              else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
              {
                sw.Restart();
                // Target just the first matching limb
                Limb limb = _.ragdoll.GetLimb(limbType);
                if (limb != null)
                {
                  ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb);
                }
                sw.Stop();
                CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [Limb] ({limbType})");
              }
              else if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
              {
                sw.Restart();
                // Target just the first matching limb
                Limb limb = _.ragdoll.GetLimb(limbType);
                if (limb != null)
                {
                  ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb);
                }
                sw.Stop();
                CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [Character]");
              }
              else if (statusEffect.HasTargetType(StatusEffect.TargetType.This))
              {
                sw.Restart();
                // Target just the first matching limb
                Limb limb = _.ragdoll.GetLimb(limbType);
                if (limb != null)
                {
                  ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb);
                }
                sw.Stop();
                CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [This]");
              }
              else if (statusEffect.HasTargetType(StatusEffect.TargetType.LastLimb))
              {
                sw.Restart();

                // Target just the last matching limb
                Limb limb = _.ragdoll.Limbs.LastOrDefault(l => l.type == limbType && !l.IsSevered && !l.Hidden);
                if (limb != null)
                {
                  ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb);
                }

                sw.Stop();
                CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [LastLimb] ({limbType})");
              }
            }
          }
          else if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
          {
            sw.Restart();
            // Target all limbs
            foreach (var limb in _.ragdoll.Limbs)
            {
              if (limb.IsSevered) { continue; }
              ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb);
            }
            sw.Stop();
            CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [AllLimbs]");
          }
          else if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
          {
            sw.Restart();
            statusEffect.Apply(actionType, deltaTime, _.character, _.character, _.WorldPosition);
            sw.Stop();
            CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [Character]");
          }
          else if (statusEffect.HasTargetType(StatusEffect.TargetType.This))
          {
            sw.Restart();
            ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb: _);
            sw.Stop();
            CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [This]");
          }
          else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
          {
            sw.Restart();
            ApplyToLimb(actionType, deltaTime, statusEffect, _.character, limb: _);
            sw.Stop();
            CaptureLimb(sw.ElapsedTicks, actionType, _, $"Target: [Limb]");
          }
        }


        static void ApplyToLimb(ActionType actionType, float deltaTime, StatusEffect statusEffect, Character character, Limb limb)
        {
          statusEffect.sourceBody = limb.body;
          statusEffect.Apply(actionType, deltaTime, entity: character, target: limb);
        }

        sw1.Stop();
        Capture.Update.AddTicks(sw1.ElapsedTicks, SEState, $"the whole thing {actionType}");

        return false;
      }
    }
  }
}