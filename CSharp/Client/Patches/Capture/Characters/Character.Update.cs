
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
      public static void CaptureCharacter2(long ticks, Character character, string text)
      {
        if (UpdateState.ByID)
        {
          Capture.Update.AddTicks(ticks, UpdateState, $"{character.Info?.DisplayName ?? character.ToString()}.{text}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, UpdateState, text);
        }
      }


      public static bool Character_Update_Replace(float deltaTime, Camera cam, Character __instance)
      {
        if (Showperf == null || !Showperf.Revealed || (!UpdateState.IsActive && !TalentsState.IsActive)) return true;
        Capture.Update.EnsureCategory(UpdateState);

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        Character _ = __instance;

        // Note: #if CLIENT is needed because on server side UpdateProjSpecific isn't compiled 
#if CLIENT
        sw.Restart();
        _.UpdateProjSpecific(deltaTime, cam);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateProjSpecific");
#endif

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

        _.KnockbackCooldownTimer -= deltaTime;

        if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && _ == Character.Controlled && !_.isSynced) { return false; }

        sw.Restart();
        _.UpdateDespawn(deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateDespawn");

        if (!_.Enabled) { return false; }

        if (Level.Loaded != null)
        {
          if (_.WorldPosition.Y < Level.MaxEntityDepth ||
              (_.Submarine != null && _.Submarine.WorldPosition.Y < Level.MaxEntityDepth))
          {
            _.Enabled = false;
            _.Kill(CauseOfDeathType.Pressure, null);
            return false;
          }
        }

        sw.Restart();
        _.ApplyStatusEffects(ActionType.Always, deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "ApplyStatusEffects Always");

        sw.Restart();
        _.PreviousHull = _.CurrentHull;
        _.CurrentHull = Hull.FindHull(_.WorldPosition, _.CurrentHull, useWorldCoordinates: true);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "FindHull");


        _.obstructVisionAmount = Math.Max(_.obstructVisionAmount - deltaTime, 0.0f);

        sw.Restart();
        if (_.Inventory != null)
        {
          //do not check for duplicates: _ is code is called very frequently, and duplicates don't matter here,
          //so it's better just to avoid the relatively expensive duplicate check
          foreach (Item item in _.Inventory.GetAllItems(checkForDuplicates: false))
          {
            if (item.body == null || item.body.Enabled) { continue; }
            item.SetTransform(_.SimPosition, 0.0f);
            item.Submarine = _.Submarine;
          }
        }
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "item.SetTransform");

        _.HideFace = false;
        _.IgnoreMeleeWeapons = false;

        sw.Restart();
        _.UpdateSightRange(deltaTime);
        _.UpdateSoundRange(deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateSightRange");

        sw.Restart();
        _.UpdateAttackers(deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateAttackers");

        sw.Restart();
        if (TalentsState.IsActive)
        {

          Capture.Update.EnsureCategory(TalentsState);

          if (TalentsState.ByID)
          {
            foreach (var characterTalent in _.characterTalents)
            {
              sw2.Restart();
              characterTalent.UpdateTalent(deltaTime);
              sw2.Stop();
              Capture.Update.AddTicks(sw2.ElapsedTicks, TalentsState, $"{_.Info?.DisplayName ?? _.ToString()} - {characterTalent.Prefab.OriginalName}");
            }
          }
          else
          {
            foreach (var characterTalent in _.characterTalents)
            {
              sw2.Restart();
              characterTalent.UpdateTalent(deltaTime);
              sw2.Stop();
              Capture.Update.AddTicks(sw2.ElapsedTicks, TalentsState, characterTalent.Prefab.OriginalName);
            }
          }
        }
        else
        {
          foreach (var characterTalent in _.characterTalents)
          {
            characterTalent.UpdateTalent(deltaTime);
          }
        }


        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateTalents");

        if (_.IsDead) { return false; }


        if (GameMain.NetworkMember != null)
        {
          sw.Restart();
          _.UpdateNetInput();
          sw.Stop();
          CaptureCharacter2(sw.ElapsedTicks, _, "UpdateNetInput");
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

        sw.Restart();
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
                  sw.Stop();
                  CaptureCharacter2(sw.ElapsedTicks, _, "NeedsAir");
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
                sw.Stop();
                CaptureCharacter2(sw.ElapsedTicks, _, "NeedsAir");
                return false;
              }
            }
          }
        }

        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "NeedsAir");

        sw.Restart();
        _.ApplyStatusEffects(_.AnimController.InWater ? ActionType.InWater : ActionType.NotInWater, deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "ApplyStatusEffects InWater");


        sw.Restart();
        _.ApplyStatusEffects(ActionType.OnActive, deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "ApplyStatusEffects OnActive");

        //wait 0.1 seconds so status effects that continuously set InDetectable to true can keep the character InDetectable
        if (_.aiTarget != null && Timing.TotalTime > _.aiTarget.InDetectableSetTime + 0.1f)
        {
          _.aiTarget.InDetectable = false;
        }

        // Note: #if CLIENT is needed because on server side UpdateControlled isn't compiled 
#if CLIENT
        sw.Restart();
        _.UpdateControlled(deltaTime, cam);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateControlled");
#endif

        sw.Restart();
        //Health effects
        if (_.NeedsOxygen)
        {
          _.UpdateOxygen(deltaTime);
        }
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateOxygen");

        sw.Restart();
        _.CalculateHealthMultiplier();
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "CalculateHealthMultiplier");

        sw.Restart();
        _.CharacterHealth.Update(deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "CharacterHealth.Update");



        if (_.IsIncapacitated)
        {
          sw.Restart();
          _.Stun = Math.Max(5.0f, _.Stun);
          _.AnimController.ResetPullJoints();
          _.SelectedItem = _.SelectedSecondaryItem = null;
          sw.Stop();
          CaptureCharacter2(sw.ElapsedTicks, _, "ResetPullJoints");
          return false;
        }


        sw.Restart();
        _.UpdateAIChatMessages(deltaTime);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "UpdateAIChatMessages");


        sw.Restart();
        bool wasRagdolled = _.IsRagdolled;
        if (_.IsForceRagdolled)
        {
          _.IsRagdolled = _.IsForceRagdolled;
        }
        else if (_ != Character.Controlled)
        {
          wasRagdolled = _.IsRagdolled;
          _.IsRagdolled = _.IsKeyDown(InputType.Ragdoll);
          if (_.IsRagdolled && _.IsBot && GameMain.NetworkMember is not { IsClient: true })
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
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "Ragdoll Input");


        _.lowPassMultiplier = MathHelper.Lerp(_.lowPassMultiplier, 1.0f, 0.1f);

        if (_.IsRagdolled || !_.CanMove)
        {
          sw.Restart();
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
          _.SelectedItem = _.SelectedSecondaryItem = null;

          sw.Stop();
          CaptureCharacter2(sw.ElapsedTicks, _, "ResetPullJoints");
          return false;
        }

        //AI and control stuff
        sw.Restart();
        _.Control(deltaTime, cam);
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "Control");


        sw.Restart();
        if (_.IsRemotePlayer)
        {
          Vector2 mouseSimPos = ConvertUnits.ToSimUnits(_.cursorPosition);
          _.DoInteractionUpdate(deltaTime, mouseSimPos);
        }
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "DoInteractionUpdate");

        sw.Restart();
        if (MustDeselect(_.SelectedItem))
        {
          _.SelectedItem = null;
        }

        if (MustDeselect(_.SelectedSecondaryItem))
        {
          _.ReleaseSecondaryItem();
        }
        sw.Stop();
        CaptureCharacter2(sw.ElapsedTicks, _, "MustDeselect");


        if (!_.IsDead) { _.LockHands = false; }

        bool MustDeselect(Item item)
        {
          if (item == null) { return false; }
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



        return false;
      }
    }
  }
}