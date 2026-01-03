
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
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
#if SERVER
using System.Text;
#endif


namespace ShowPerfExtensions
{
  public partial class Plugin
  {

    public partial class CharacterPatch
    {
      public static void CaptureCharacter4(long ticks, Character character, string text)
      {
        if (SEState.ByID)
        {
          Capture.Update.AddTicks(ticks, SEState, $"{character.Info?.DisplayName ?? character.ToString()}.{text}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, SEState, text);
        }
      }

      public static bool Character_ApplyStatusEffects_Replace(Character __instance, ActionType actionType, float deltaTime)
      {
        if (Showperf == null || !Showperf.Revealed || !SEState.IsActive) return true;
        Capture.Update.EnsureCategory(SEState);

        Stopwatch sw = new Stopwatch();
        Character _ = __instance;


        if (actionType == ActionType.OnEating)
        {
          sw.Restart();

          float eatingRegen = _.Params.Health.HealthRegenerationWhenEating;
          if (eatingRegen > 0)
          {
            _.CharacterHealth.ReduceAfflictionOnAllLimbs(AfflictionPrefab.DamageType, eatingRegen * deltaTime);
          }

          sw.Stop();
          CaptureCharacter4(sw.ElapsedTicks, _, "OnEating");
        }



        if (_.statusEffects.TryGetValue(actionType, out var statusEffectList))
        {

          foreach (StatusEffect statusEffect in statusEffectList)
          {
            if (statusEffect.type == ActionType.OnDamaged)
            {
              if (!statusEffect.HasRequiredAfflictions(_.LastDamage)) { continue; }
              if (statusEffect.OnlyWhenDamagedByPlayer)
              {
                if (_.LastAttacker == null || !_.LastAttacker.IsPlayer)
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
              statusEffect.Apply(actionType, deltaTime, _, _.targets);

              sw.Stop();

              if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems))
              {
                CaptureCharacter4(sw.ElapsedTicks, _, $"type:[{actionType}] target:[NearbyItems]");
              }
              if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
              {
                CaptureCharacter4(sw.ElapsedTicks, _, $"type:[{actionType}] target:[NearbyCharacters]");
              }
            }
            else if (statusEffect.targetLimbs != null)
            {
              foreach (var limbType in statusEffect.targetLimbs)
              {
                if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
                {
                  sw.Restart();

                  // Target all matching limbs
                  foreach (var limb in _.AnimController.Limbs)
                  {
                    if (limb.IsSevered) { continue; }
                    if (limb.type == limbType)
                    {
                      ApplyToLimb(actionType, deltaTime, statusEffect, _, limb);
                    }
                  }

                  sw.Stop();
                  CaptureCharacter4(sw.ElapsedTicks, _, $"type:[{actionType}] target:[AllLimbs]");
                }
                else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
                {
                  sw.Restart();

                  // Target just the first matching limb
                  Limb limb = _.AnimController.GetLimb(limbType);
                  if (limb != null)
                  {
                    ApplyToLimb(actionType, deltaTime, statusEffect, _, limb);
                  }

                  sw.Stop();
                  CaptureCharacter4(sw.ElapsedTicks, _, $"type:[{actionType}] target:[Limb]");
                }
                else if (statusEffect.HasTargetType(StatusEffect.TargetType.LastLimb))
                {
                  sw.Restart();
                  // Target just the last matching limb
                  Limb limb = _.AnimController.Limbs.LastOrDefault(l => l.type == limbType && !l.IsSevered && !l.Hidden);
                  if (limb != null)
                  {
                    ApplyToLimb(actionType, deltaTime, statusEffect, _, limb);
                  }

                  sw.Stop();
                  CaptureCharacter4(sw.ElapsedTicks, _, $"type:[{actionType}] target:[LastLimb]");
                }
              }
            }
            else if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
            {
              sw.Restart();

              // Target all limbs
              foreach (var limb in _.AnimController.Limbs)
              {
                if (limb.IsSevered) { continue; }
                ApplyToLimb(actionType, deltaTime, statusEffect, character: _, limb);
              }

              sw.Stop();
              CaptureCharacter4(sw.ElapsedTicks, _, $"type:[{actionType}] target:[AllLimbs]");
            }
            if (statusEffect.HasTargetType(StatusEffect.TargetType.This) || statusEffect.HasTargetType(StatusEffect.TargetType.Character))
            {
              statusEffect.Apply(actionType, deltaTime, _, _);
            }
            if (statusEffect.HasTargetType(StatusEffect.TargetType.Hull) && _.CurrentHull != null)
            {
              statusEffect.Apply(actionType, deltaTime, _, _.CurrentHull);
            }
          }


          if (actionType != ActionType.OnDamaged && actionType != ActionType.OnSevered)
          {
            sw.Restart();

            // OnDamaged is called only for the limb that is hit.
            foreach (Limb limb in _.AnimController.Limbs)
            {
              limb.ApplyStatusEffects(actionType, deltaTime);
            }
            sw.Stop();
            CaptureCharacter4(sw.ElapsedTicks, _, $" Limb.{actionType}");
          }

        }


        //OnActive effects are handled by the afflictions themselves
        if (actionType != ActionType.OnActive)
        {
          sw.Restart();
          _.CharacterHealth.ApplyAfflictionStatusEffects(actionType);
          sw.Stop();
          CaptureCharacter4(sw.ElapsedTicks, _, $" CharacterHealth.{actionType}");
        }


        static void ApplyToLimb(ActionType actionType, float deltaTime, StatusEffect statusEffect, Character character, Limb limb)
        {
          statusEffect.sourceBody = limb.body;
          statusEffect.Apply(actionType, deltaTime, entity: character, target: limb);
        }



        return false;
      }

    }
  }
}