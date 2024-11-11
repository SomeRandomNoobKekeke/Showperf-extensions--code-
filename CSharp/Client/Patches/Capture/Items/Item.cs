#define CLIENT
using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using MoonSharp.Interpreter;
using System.Collections.Immutable;
using Barotrauma.Abilities;

#if CLIENT
using Microsoft.Xna.Framework.Graphics;
#endif


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class ItemPatch
    {
      public static Stopwatch sw = new Stopwatch();
      public static Stopwatch sw2 = new Stopwatch();
      public static Stopwatch sw3 = new Stopwatch();

      public static CaptureState Components;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Item).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(ItemPatch).GetMethod("Item_Update_Replace"))
        );

        Components = Capture.Get("Showperf.Update.MapEntity.Items.Components");
      }

      public static bool Item_Update_Replace(float deltaTime, Camera cam, Item __instance)
      {
        if (!Showperf.Revealed || !Components.IsActive) return true;

        Capture.Update.EnsureCategory(Components);

        Item _ = __instance;

        if (!_.isActive || _.IsLayerHidden) { return false; }


        sw.Restart();
        if (_.impactQueue != null)
        {
          while (_.impactQueue.TryDequeue(out float impact))
          {
            _.HandleCollision(impact);
          }
        }
        if (_.isDroppedStackOwner && _.body != null)
        {
          foreach (var item in _.droppedStack)
          {
            if (item != _)
            {
              item.body.Enabled = false;
              item.body.SetTransformIgnoreContacts(_.SimPosition, _.body.Rotation);
            }
          }
        }
        sw.Stop();
        if (Components.ByID)
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, $"{_.Prefab.Name}.HandleCollision");
        }
        else
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, "HandleCollision");
        }


        sw.Restart();
        if (_.aiTarget != null && _.aiTarget.NeedsUpdate)
        {
          _.aiTarget.Update(deltaTime);
        }
        sw.Stop();
        if (Components.ByID)
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, $"{_.Prefab.Name}.AITargets");
        }
        else
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, "AITargets");
        }

        var containedEffectType = _.parentInventory == null ? ActionType.OnNotContained : ActionType.OnContained;

        sw.Restart();
        _.ApplyStatusEffects(ActionType.Always, deltaTime, character: (_.parentInventory as CharacterInventory)?.Owner as Character);
        sw.Stop();
        if (Components.ByID)
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, $"{_.Prefab.Name}.ApplyStatusEffects.ActionType.Always");
        }
        else
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, "ApplyStatusEffects.ActionType.Always");
        }

        sw.Restart();
        _.ApplyStatusEffects(containedEffectType, deltaTime, character: (_.parentInventory as CharacterInventory)?.Owner as Character);
        sw.Stop();
        if (Components.ByID)
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, $"{_.Prefab.Name}.ApplyStatusEffects.containedEffectType");
        }
        else
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, "ApplyStatusEffects.containedEffectType");
        }


        for (int i = 0; i < _.updateableComponents.Count; i++)
        {
          sw.Restart();

          ItemComponent ic = _.updateableComponents[i];

          bool isParentInActive = ic.InheritParentIsActive && ic.Parent is { IsActive: false };

          if (ic.IsActiveConditionals != null && !isParentInActive)
          {
            if (ic.IsActiveConditionalComparison == PropertyConditional.LogicalOperatorType.And)
            {
              bool shouldBeActive = true;
              foreach (var conditional in ic.IsActiveConditionals)
              {
                if (!_.ConditionalMatches(conditional))
                {
                  shouldBeActive = false;
                  break;
                }
              }
              ic.IsActive = shouldBeActive;
            }
            else
            {
              bool shouldBeActive = false;
              foreach (var conditional in ic.IsActiveConditionals)
              {
                if (_.ConditionalMatches(conditional))
                {
                  shouldBeActive = true;
                  break;
                }
              }
              ic.IsActive = shouldBeActive;
            }
          }
#if CLIENT
          if (ic.HasSounds)
          {
            ic.PlaySound(ActionType.Always);
            ic.UpdateSounds();
            if (!ic.WasUsed) { ic.StopSounds(ActionType.OnUse); }
            if (!ic.WasSecondaryUsed) { ic.StopSounds(ActionType.OnSecondaryUse); }
          }
#endif
          ic.WasUsed = false;
          ic.WasSecondaryUsed = false;

          if (ic.IsActive || ic.UpdateWhenInactive)
          {
            if (_.condition <= 0.0f)
            {
              ic.UpdateBroken(deltaTime, cam);
            }
            else
            {
              ic.Update(deltaTime, cam);
#if CLIENT
              if (ic.IsActive)
              {
                if (ic.IsActiveTimer > 0.02f)
                {
                  ic.PlaySound(ActionType.OnActive);
                }
                ic.IsActiveTimer += deltaTime;
              }
#endif
            }
          }


          sw.Stop();
          if (Components.ByID)
          {
            if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, $"{_.Prefab.Name}.{ic.name}");
          }
          else
          {
            if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, ic.name);
          }
        }

        if (_.Removed) { return false; }

        sw.Restart();
        bool needsWaterCheck = _.hasInWaterStatusEffects || _.hasNotInWaterStatusEffects;
        if (_.body != null && _.body.Enabled)
        {
          System.Diagnostics.Debug.Assert(_.body.FarseerBody.FixtureList != null);

          if (Math.Abs(_.body.LinearVelocity.X) > 0.01f || Math.Abs(_.body.LinearVelocity.Y) > 0.01f || _.transformDirty)
          {
            if (_.body.CollisionCategories != Category.None)
            {
              _.UpdateTransform();
            }
            if (_.CurrentHull == null && Level.Loaded != null && _.body.SimPosition.Y < ConvertUnits.ToSimUnits(Level.MaxEntityDepth))
            {
              Item.Spawner?.AddItemToRemoveQueue(_);
              return false;
            }
          }
          needsWaterCheck = true;
          _.UpdateNetPosition(deltaTime);
          if (_.inWater)
          {
            _.ApplyWaterForces();
            _.CurrentHull?.ApplyFlowForces(deltaTime, _);
          }
        }

        if (needsWaterCheck)
        {
          bool wasInWater = _.inWater;
          _.inWater = !_.inWaterProofContainer && _.IsInWater();
          if (_.inWater)
          {
            //the item has gone through the surface of the water
            if (!wasInWater && _.CurrentHull != null && _.body != null && _.body.LinearVelocity.Y < -1.0f)
            {

              // Note: #if CLIENT is needed because on server side Splash isn't compiled 
#if CLIENT
              _.Splash();
#endif

              if (_.GetComponent<Projectile>() is not { IsActive: true })
              {
                //slow the item down (not physically accurate, but looks good enough)
                _.body.LinearVelocity *= 0.2f;
              }
            }
          }
          if ((_.hasInWaterStatusEffects || _.hasNotInWaterStatusEffects) && _.condition > 0.0f)
          {
            _.ApplyStatusEffects(_.inWater ? ActionType.InWater : ActionType.NotInWater, deltaTime);
          }
          if (_.inWaterProofContainer && !_.hasNotInWaterStatusEffects)
          {
            needsWaterCheck = false;
          }
        }

        if (!needsWaterCheck &&
            _.updateableComponents.Count == 0 &&
            (_.aiTarget == null || !_.aiTarget.NeedsUpdate) &&
            !_.hasStatusEffectsOfType[(int)ActionType.Always] &&
            !_.hasStatusEffectsOfType[(int)containedEffectType] &&
            (_.body == null || !_.body.Enabled))
        {
#if CLIENT
          _.positionBuffer.Clear();
#endif
          _.isActive = false;
        }

        sw.Stop();
        if (Components.ByID)
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, $"{_.Prefab.Name}.CheckInWater");
        }
        else
        {
          if (Capture.ShouldCapture(_)) Capture.Update.AddTicks(sw.ElapsedTicks, Components, "CheckInWater");
        }

        return false;
      }


    }
  }
}