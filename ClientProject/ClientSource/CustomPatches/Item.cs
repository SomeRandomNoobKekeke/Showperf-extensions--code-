using Barotrauma;
using CursedUI;
using CUILibs;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using System.Diagnostics;

namespace ShowperfExtensions;

public class Item_Patch
{
  public static bool Item_Update_Replace(Item __instance, float deltaTime, Camera cam)
  {
    Item _ = __instance;

    Stopwatch sw = new();

    if (!_.IsActive || _.IsLayerHidden || _.IsInRemoveQueue) { return false; }

    if (_.impactQueue != null)
    {
      while (_.impactQueue.TryDequeue(out float impact))
      {
        _.ReceiveImpact(impact);
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

    if (_.aiTarget != null && _.aiTarget.NeedsUpdate)
    {
      _.aiTarget.Update(deltaTime);
    }

    var containedEffectType = _.parentInventory == null ? ActionType.OnNotContained : ActionType.OnContained;

    _.ApplyStatusEffects(ActionType.Always, deltaTime, character: (_.parentInventory as CharacterInventory)?.Owner as Character);
    _.ApplyStatusEffects(containedEffectType, deltaTime, character: (_.parentInventory as CharacterInventory)?.Owner as Character);


    sw.Restart();
    for (int i = 0; i < _.updateableComponents.Count; i++)
    {
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
        if (!ic.UpdateWhenBroken && _.condition <= 0.0f)
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
    }
    sw.Stop();
    Plugin.Capture.Update.AddTicks($"{_}.ItemComponents", sw.ElapsedTicks);




    if (_.Removed) { return false; }

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
          _.Splash();
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
      _.IsActive = false;
    }

    return false;
  }
}