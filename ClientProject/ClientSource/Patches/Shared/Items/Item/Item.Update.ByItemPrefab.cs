using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;

namespace ShowperfPatches;


public static partial class Item_Update_Deep
{


  public static bool Item_Update_Replace_ByItemPrefab(Item __instance, float deltaTime, Camera cam)
  {
    Item _ = __instance;

    DisposableStopwatch watch = new DisposableStopwatch()
    {
      ShouldCapture = WhatToCapture == WhatToCaptureOptions.All
    };

    DisposableStopwatch icWatch = new DisposableStopwatch()
    {
      ShouldCapture = WhatToCapture == WhatToCaptureOptions.ItemComponents
    };


    string itemPrefabName = _.Prefab.Name.ToString();


    if (!_.IsActive || _.IsDisabled || _.IsInRemoveQueue)
    {
      return false;
    }

    if (_.impactQueue != null)
    {
      using (watch.Restart())
      {
        float impact;
        while (_.impactQueue.TryDequeue(out impact))
        {
          _.ReceiveImpact(impact, true);
        }

        watch.Stop().Capture($"{itemPrefabName}   -   Update Impacts");
      }
    }



    if (_.isDroppedStackOwner && _.body != null)
    {
      using (watch.Restart())
      {
        foreach (Item item in _.droppedStack)
        {
          if (item != _)
          {
            item.body.Enabled = false;
            item.body.SetTransformIgnoreContacts(_.SimPosition, _.body.Rotation, true);
          }
        }

        watch.Stop().Capture($"{itemPrefabName}   -   SetTransformIgnoreContacts on droppedStack");
      }
    }

    if (_.aiTarget != null && _.aiTarget.NeedsUpdate)
    {
      using (watch.Restart())
      {
        _.aiTarget.Update(deltaTime);

        watch.Stop().Capture($"{itemPrefabName}   -   aiTarget.Update");
      }
    }

    using (watch.Restart())
    {
      ActionType type = ActionType.Always;
      CharacterInventory characterInventory = _.parentInventory as CharacterInventory;
      _.ApplyStatusEffects(type, deltaTime, ((characterInventory != null) ? characterInventory.Owner : null) as Character, null, null, false, null);

      watch.Stop().Capture($"{itemPrefabName}   -   ApplyStatusEffects Always");
    }


    using (watch.Restart())
    {
      ActionType type2 = (_.parentInventory == null) ? ActionType.OnNotContained : ActionType.OnContained;
      CharacterInventory characterInventory2 = _.parentInventory as CharacterInventory;
      _.ApplyStatusEffects(type2, deltaTime, ((characterInventory2 != null) ? characterInventory2.Owner : null) as Character, null, null, false, null);

      watch.Stop().Capture($"{itemPrefabName}   -   ApplyStatusEffects OnNotContained / OnContained");
    }




    using (watch.Restart())
    {
      for (int i = 0; i < _.updateableComponents.Count; i++)
      {
        ItemComponent ic = _.updateableComponents[i];
        bool flag;
        if (ic.InheritParentIsActive)
        {
          ItemComponent parent = ic.Parent;
          flag = (parent != null && !parent.IsActive);
        }
        else
        {
          flag = false;
        }
        bool isParentInActive = flag;
        if (ic.IsActiveConditionals != null && !isParentInActive)
        {
          if (ic.IsActiveConditionalComparison == PropertyConditional.LogicalOperatorType.And)
          {
            bool shouldBeActive = true;
            foreach (PropertyConditional conditional in ic.IsActiveConditionals)
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
            bool shouldBeActive2 = false;
            foreach (PropertyConditional conditional2 in ic.IsActiveConditionals)
            {
              if (_.ConditionalMatches(conditional2))
              {
                shouldBeActive2 = true;
                break;
              }
            }
            ic.IsActive = shouldBeActive2;
          }
        }
        if (ic.HasSounds)
        {
          ic.PlaySound(ActionType.Always, null);
          ic.UpdateSounds();
          if (!ic.WasUsed)
          {
            ic.StopSounds(ActionType.OnUse);
          }
          if (!ic.WasSecondaryUsed)
          {
            ic.StopSounds(ActionType.OnSecondaryUse);
          }
        }
        ic.WasUsed = false;
        ic.WasSecondaryUsed = false;
        if (ic.IsActive || ic.UpdateWhenInactive)
        {
          if (!ic.UpdateWhenBroken && _.condition <= 0f)
          {
            ic.UpdateBroken(deltaTime, cam);
          }
          else
          {
            using (icWatch.Restart())
            {
              ic.Update(deltaTime, cam);
              icWatch.Stop().Capture($"{itemPrefabName}   -   {ic}");
            }

            if (ic.IsActive)
            {
              if (ic.IsActiveTimer > 0.02f)
              {
                ic.PlaySound(ActionType.OnActive, null);
              }
              ic.IsActiveTimer += deltaTime;
            }
          }
        }
      }

      watch.Stop().Capture($"{itemPrefabName}   -   Update ItemComponents");
    }




    if (_.Removed)
    {
      return false;
    }

    bool needsWaterCheck = _.hasInWaterStatusEffects || _.hasNotInWaterStatusEffects;

    using (watch.Restart())
    {
      if (_.body != null && _.body.Enabled)
      {
        if (Math.Abs(_.body.LinearVelocity.X) > 0.01f || Math.Abs(_.body.LinearVelocity.Y) > 0.01f || _.transformDirty)
        {
          if (_.body.CollisionCategories != Category.None)
          {
            _.UpdateTransform();
          }
          if (_.CurrentHull == null && Level.Loaded != null && _.body.SimPosition.Y < ConvertUnits.ToSimUnits(-1000000))
          {
            EntitySpawner spawner = Entity.Spawner;
            if (spawner == null)
            {
              return false;
            }
            spawner.AddItemToRemoveQueue(_, null);
            return false;
          }
        }
        needsWaterCheck = true;
        _.UpdateNetPosition(deltaTime);
        if (_.inWater)
        {
          _.ApplyWaterForces();
          Hull hull = _.CurrentHull;
          if (hull != null)
          {
            hull.ApplyFlowForces(deltaTime, _);
          }
        }
      }

      watch.Stop().Capture($"{itemPrefabName}   -   UpdateTransform");
    }


    using (watch.Restart())
    {
      if (needsWaterCheck)
      {
        bool wasInWater = _.inWater;
        _.inWater = (!_.inWaterProofContainer && _.IsInWater());
        if (_.inWater && !wasInWater && _.CurrentHull != null && _.body != null && _.body.LinearVelocity.Y < -1f)
        {
          _.Splash();
          Projectile component = _.GetComponent<Projectile>();
          if (component == null || !component.IsActive)
          {
            _.body.LinearVelocity *= 0.2f;
          }
        }
        if ((_.hasInWaterStatusEffects || _.hasNotInWaterStatusEffects) && _.condition > 0f)
        {
          _.ApplyStatusEffects(_.inWater ? ActionType.InWater : ActionType.NotInWater, deltaTime, _.GetRootInventoryOwner() as Character, null, null, false, null);
        }
        if (_.inWaterProofContainer && !_.hasNotInWaterStatusEffects)
        {
          needsWaterCheck = false;
        }
      }

      if (!needsWaterCheck && _.updateableComponents.Count == 0 && (_.aiTarget == null || !_.aiTarget.NeedsUpdate) && !_.HasStatusEffectsOfType(ActionType.Always) && !_.HasStatusEffectsOfType((_.parentInventory == null) ? ActionType.OnNotContained : ActionType.OnContained) && (_.body == null || !_.body.Enabled))
      {
        _.positionBuffer.Clear();
        _.IsActive = false;
      }

      watch.Stop().Capture($"{itemPrefabName}   -   Update water stuff");
    }

    return false;
  }
}