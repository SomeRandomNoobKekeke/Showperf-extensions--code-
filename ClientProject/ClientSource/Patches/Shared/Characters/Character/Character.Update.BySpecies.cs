using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfPatches;


public static partial class Character_UpdateAll_Deep
{

  public static bool Character_Update_Replace_BySpecies(Character __instance, float deltaTime, Camera cam)
  {
    string GetSpeciesName(Character c) => c.SpeciesName.Value;


    Character _ = __instance;

    DisposableStopwatch watch = new();

    using (watch.Restart())
    {
      _.UpdateProjSpecific(deltaTime, cam);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  UpdateProjSpecific");

    if (_.TextChatVolume > 0)
    {
      _.TextChatVolume -= 0.2f * deltaTime;
    }

    if (_.InvisibleTimer > 0.0f)
    {
      if (Character.Controlled == null || Character.Controlled == _ || (Character.Controlled.CharacterHealth.GetAffliction("psychosis")?.Strength ?? 0.0f) <= 0.0f)
      {
        _.InvisibleTimer = Math.Min(_.InvisibleTimer, 1.0f);
      }
      _.InvisibleTimer -= deltaTime;
    }

    if (_.ForceDisableTargetingTimer > 0f)
    {
      _.ForceDisableTargetingTimer -= deltaTime;
    }

    _.KnockbackCooldownTimer -= deltaTime;

    if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && _ == Character.Controlled && !_.isSynced) { return false; }

    using (watch.Restart())
    {
      _.UpdateDespawn(deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  UpdateDespawn");


    if (!_.Enabled) { return false; }

    if (Level.Loaded != null)
    {
      if (_.WorldPosition.Y < Level.MaxEntityDepth ||
          (_.Submarine != null && _.Submarine.WorldPosition.Y < Level.MaxEntityDepth))
      {
        _.Enabled = false;

        using (watch.Restart())
        {
          _.Kill(CauseOfDeathType.Pressure, null);
        }
        watch.Capture($"{GetSpeciesName(_)}  -  Kill");

        return false;
      }
    }

    using (watch.Restart())
    {
      _.ApplyStatusEffects(ActionType.Always, deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  ApplyStatusEffects Always");


    using (watch.Restart())
    {
      _.PreviousHull = _.CurrentHull;
      _.CurrentHull = Hull.FindHull(_.WorldPosition, _.CurrentHull, useWorldCoordinates: true);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  FindHull");

    _.obstructVisionAmount = Math.Max(_.obstructVisionAmount - deltaTime, 0.0f);

    using (watch.Restart())
    {
      if (_.Inventory != null && Vector2.DistanceSquared(_.lastInventoryItemSetTransformPosition, _.Position) > 0.1f)
      {
        //do not check for duplicates: _ is code is called very frequently, and duplicates don't matter here,
        //so it's better just to avoid the relatively expensive duplicate check
        foreach (Item item in _.Inventory.GetAllItems(checkForDuplicates: false))
        {
          if (item.body == null || item.body.Enabled) { continue; }
          item.SetTransform(_.SimPosition, 0.0f, forceSubmarine: _.Submarine);
        }
        _.lastInventoryItemSetTransformPosition = _.Position;
      }
    }
    watch.Capture($"{GetSpeciesName(_)}  -  Inventory.GetAllItems.SetTransform");



    _.HideFace = false;
    _.IgnoreMeleeWeapons = false;


    using (watch.Restart())
    {
      _.UpdateSightRange(deltaTime);
      _.UpdateSoundRange(deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  Update Sight/Sound Ranges");

    using (watch.Restart())
    {
      _.UpdateAttackers(deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  UpdateAttackers");


    using (watch.Restart())
    {
      //use a for loop instead of foreach because talents can unlock other talents via StatusEffectAction (see #17328)
      //_ way we'll just add them to the end of the list without causing a collection was modified exception
      for (int i = 0; i < _.characterTalents.Count; i++)
      {
        _.characterTalents[i].UpdateTalent(deltaTime);
      }
    }
    watch.Capture($"{GetSpeciesName(_)}  -  UpdateTalent");



    if (_.IsDead) { return false; }



    if (GameMain.NetworkMember != null)
    {
      using (watch.Restart())
      {
        _.UpdateNetInput();
      }
      watch.Capture($"{GetSpeciesName(_)}  -  UpdateNetInput");
    }
    else
    {
      _.AnimController.Frozen = false;
    }

    _.DisableImpactDamageTimer -= deltaTime;

    if (!_.speechImpedimentSet)
    {
      //if no statuseffect or anything else has set a speech impediment, allow speaking normally
      _.speechImpediment = 0.0f;
    }
    _.speechImpedimentSet = false;

    using (watch.Restart())
    {
      if (_.NeedsAir)
      {
        //implode if not protected from pressure, and either outside or in a high-pressure hull
        if (!_.IsProtectedFromPressure && (_.AnimController.CurrentHull == null || _.AnimController.CurrentHull.LethalPressure >= 80.0f))
        {
          if (_.PressureTimer > _.CharacterHealth.PressureKillDelay * 0.1f)
          {
            //after a brief delay, start doing increasing amounts of organ damage
            _.CharacterHealth.ApplyAffliction(
                targetLimb: _.AnimController.MainLimb,
                new Affliction(AfflictionPrefab.OrganDamage, _.PressureTimer / 10.0f * deltaTime));
          }

          if (_.CharacterHealth.PressureKillDelay <= 0.0f)
          {
            _.PressureTimer = 100.0f;
          }
          else
          {
            _.PressureTimer += ((_.AnimController.CurrentHull == null) ?
                100.0f : _.AnimController.CurrentHull.LethalPressure) / _.CharacterHealth.PressureKillDelay * deltaTime;
          }

          if (_.PressureTimer >= 100.0f)
          {
            if (Character.Controlled == _) { cam.Zoom = 5.0f; }
            if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
            {
              _.Implode();
              if (_.IsDead)
              {
                watch.Stop().Capture("Implode");
                return false;
              }
            }
          }
        }
        else
        {
          _.PressureTimer = 0.0f;
        }
      }
      else if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && !_.IsProtectedFromPressure)
      {
        float realWorldDepth = Level.Loaded?.GetRealWorldDepth(_.WorldPosition.Y) ?? 0.0f;
        if (_.PressureProtection < realWorldDepth && realWorldDepth > _.CharacterHealth.CrushDepth)
        {
          //implode if below crush depth, and either outside or in a high-pressure hull                
          if (_.AnimController.CurrentHull == null || _.AnimController.CurrentHull.LethalPressure >= 80.0f)
          {
            _.Implode();
            if (_.IsDead)
            {
              watch.Stop().Capture("Implode");
              return false;
            }
          }
        }
      }
    }
    watch.Capture($"{GetSpeciesName(_)}  -  Implode");



    using (watch.Restart())
    {
      _.ApplyStatusEffects(_.AnimController.InWater ? ActionType.InWater : ActionType.NotInWater, deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  ApplyStatusEffects InWater");

    using (watch.Restart())
    {
      _.ApplyStatusEffects(ActionType.OnActive, deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  ApplyStatusEffects OnActive");


    //wait 0.1 seconds so status effects that continuously set InDetectable to true can keep the character InDetectable
    if (_.aiTarget != null && Timing.TotalTime > _.aiTarget.InDetectableSetTime + 0.1f)
    {
      _.aiTarget.InDetectable = false;
    }

    using (watch.Restart())
    {
      _.UpdateControlled(deltaTime, cam);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  UpdateControlled");



    //Health effects
    if (_.NeedsOxygen)
    {
      using (watch.Restart())
      {
        _.UpdateOxygen(deltaTime);
      }
      watch.Capture($"{GetSpeciesName(_)}  -  UpdateOxygen");
    }

    using (watch.Restart())
    {
      _.CalculateHealthMultiplier();
      _.CharacterHealth.Update(deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  CharacterHealth.Update");



    if (_.IsIncapacitated)
    {
      _.Stun = Math.Max(5.0f, _.Stun);
      _.AnimController.ResetPullJoints();
      _.SelectedItem = _.SelectedSecondaryItem = null;
      return false;
    }


    using (watch.Restart())
    {
      _.UpdateAIChatMessages(deltaTime);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  UpdateAIChatMessages");


    using (watch.Restart())
    {
      bool wasRagdolled = _.IsRagdolled;
      if (_.IsForceRagdolled)
      {
        _.IsRagdolled = _.IsForceRagdolled;
      }
      else if (_ != Character.Controlled)
      {
        wasRagdolled = _.IsRagdolled;
        _.IsRagdolled = _.IsKeyDown(InputType.Ragdoll);
        if (_.IsRagdolled && !_.IsPlayer && GameMain.NetworkMember is not { IsClient: true })
        {
          _.ClearInput(InputType.Ragdoll);
        }
      }
      else
      {
        bool tooFastToUnragdoll = bodyMovingTooFast(_.AnimController.Collider) || bodyMovingTooFast(_.AnimController.MainLimb.body);
        bool bodyMovingTooFast(PhysicsBody body)
        {
          return
              body.LinearVelocity.LengthSquared() > 8.0f * 8.0f ||
              //falling down counts as going too fast
              (!_.InWater && body.LinearVelocity.Y < -5.0f);
        }
        if (_.ragdollingLockTimer > 0.0f)
        {
          _.ragdollingLockTimer -= deltaTime;
        }
        else if (!tooFastToUnragdoll)
        {
          _.IsRagdolled = _.IsKeyDown(InputType.Ragdoll); //Handle _ here instead of Control because we can stop being ragdolled ourselves
          if (wasRagdolled != _.IsRagdolled && !_.AnimController.IsHangingWithRope)
          {
            _.ragdollingLockTimer = 0.2f;
          }
        }
        _.SetInput(InputType.Ragdoll, false, _.IsRagdolled);
      }
      if (!wasRagdolled && _.IsRagdolled && !_.AnimController.IsHangingWithRope)
      {
        _.CheckTalents(AbilityEffectType.OnRagdoll);
      }



      _.lowPassMultiplier = MathHelper.Lerp(_.lowPassMultiplier, 1.0f, 0.1f);

      if (_.IsRagdolled || !_.CanMove)
      {
        if (_.AnimController is HumanoidAnimController humanAnimController)
        {
          humanAnimController.Crouching = false;
        }
        //ragdolling manually makes the character go through platforms
        //EXCEPT if the character is controlled by the server (i.e. remote player or bot),
        //in that case the server decides whether platforms should be ignored or not
        bool isControlledByRemotelyByServer = GameMain.NetworkMember is { IsClient: true } && _.IsRemotelyControlled;
        if (_.IsRagdolled &&
            !isControlledByRemotelyByServer)
        {
          _.AnimController.IgnorePlatforms = true;
        }
        _.AnimController.ResetPullJoints();

        // Prevent us from detaching from the controller if we are attached to it OR detach if we
        // manually ragdoll, in _ case it should be similar to us deselecting the controller
        if (!_.IsAttachedToController() ||
            (_.IsKeyDown(InputType.Ragdoll)
            // Let only the server do _ check since the Ragdoll input for other clients is set to be held
            // for stunned characters even if a character isn't manually ragdolling
            && (GameMain.NetworkMember == null || GameMain.NetworkMember is { IsServer: true })))
        {
          _.SelectedItem = null;
        }

        _.SelectedSecondaryItem = null;
        _.SelectedCharacter = null;

        watch.Stop().Capture($"{GetSpeciesName(_)}  -  Ragdoll stuff");
        return false;
      }

    }
    watch.Capture($"{GetSpeciesName(_)}  -  Ragdoll stuff");






    //AI and control stuff

    using (watch.Restart())
    {
      _.Control(deltaTime, cam);
    }
    watch.Capture($"{GetSpeciesName(_)}  -  Control");

    if (_.IsRemotePlayer)
    {
      using (watch.Restart())
      {
        Vector2 mouseSimPos = ConvertUnits.ToSimUnits(_.cursorPosition);
        _.DoInteractionUpdate(deltaTime, mouseSimPos);
      }
      watch.Capture($"{GetSpeciesName(_)}  -  DoInteractionUpdate");
    }


    using (watch.Restart())
    {
      if (MustDeselect(_.SelectedItem))
      {
        _.SelectedItem = null;
      }
      if (MustDeselect(_.SelectedSecondaryItem))
      {
        _.ReleaseSecondaryItem();
      }

      if (!_.IsDead) { _.LockHands = false; }

      bool MustDeselect(Item item)
      {
        if (item == null) { return false; }

        // Prevent creatures from deselecting the controller if they are attached to it
        if (_.IsAIControlled && !_.CanInteract && _.IsAttachedToController())
        {
          return false;
        }

        if (!_.CanInteractWith(item)) { return true; }
        bool hasSelectableComponent = false;
        foreach (var component in item.Components)
        {
          //the "selectability" of an item can change e.g. if the player unequips another item that's required to access it
          if (component.CanBeSelected && component.HasRequiredItems(_, addMessage: false))
          {
            hasSelectableComponent = true;
            break;
          }
        }
        return !hasSelectableComponent;
      }
    }
    watch.Capture($"{GetSpeciesName(_)}  -  i have no hands but MustDeselect");

    return false;
  }


}