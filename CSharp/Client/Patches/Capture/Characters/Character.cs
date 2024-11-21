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
    [ShowperfPatch]
    public class CharacterPatch
    {
      public static CaptureState CaptureUpdateAll;
      public static CaptureState CaptureUpdate;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Character).GetMethod("UpdateAll", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterPatch).GetMethod("Character_UpdateAll_Replace"))
        );

        harmony.Patch(
          original: typeof(Character).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterPatch).GetMethod("Character_Update_Replace"))
        );

        CaptureUpdateAll = Capture.Get("Showperf.Update.Character");
        CaptureUpdate = Capture.Get("Showperf.Update.Character.Update");
      }

      public static void CaptureCharacter(long ticks, Character character)
      {
        if (CaptureUpdateAll.ByID)
        {
          string info = character.Info?.DisplayName ?? "";
          string enabled = character.Enabled ? "Enabled" : "Disabled";
          string alive = character.IsDead ? "Dead" : "Alive";
          string simplified = character.AnimController.SimplePhysicsEnabled ? "Simple" : "Complex";

          Capture.Update.AddTicks(ticks, CaptureUpdateAll, $"{character.ID}|{character}{info} - {alive}:{enabled}:{simplified}", (int)character.ID);
        }
        else
        {
          Capture.Update.AddTicks(ticks, CaptureUpdateAll, character.ToString());
        }
      }

      public static void CaptureCharacter2(long ticks, Character character, string text)
      {
        if (CaptureUpdate.ByID)
        {
          Capture.Update.AddTicks(ticks, CaptureUpdate, $"{character.Info?.DisplayName ?? character.ToString()}.{text}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, CaptureUpdate, text);
        }
      }

      public static bool Character_UpdateAll_Replace(float deltaTime, Camera cam)
      {
        if (!CaptureUpdateAll.IsActive || !Showperf.Revealed) return true;
        Capture.Update.EnsureCategory(CaptureUpdateAll);

        Stopwatch sw = new Stopwatch();

        sw.Restart();
        if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
        {
          foreach (Character c in Character.CharacterList)
          {
            if (c is not AICharacter && !c.IsRemotePlayer) { continue; }

            if (c.IsPlayer || (c.IsBot && !c.IsDead))
            {
              c.Enabled = true;
            }
            else if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
            {
              //disable AI characters that are far away from all clients and the host's character and not controlled by anyone
              float closestPlayerDist = c.GetDistanceToClosestPlayer();
              if (closestPlayerDist > c.Params.DisableDistance)
              {
                c.Enabled = false;
                if (c.IsDead && c.AIController is EnemyAIController)
                {
                  Character.Spawner?.AddEntityToRemoveQueue(c);
                }
              }
              else if (closestPlayerDist < c.Params.DisableDistance * 0.9f)
              {
                c.Enabled = true;
              }
            }
            else if (Submarine.MainSub != null)
            {
              //disable AI characters that are far away from the sub and the controlled character
              float distSqr = Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, c.WorldPosition);
              if (Character.Controlled != null)
              {
                distSqr = Math.Min(distSqr, Vector2.DistanceSquared(Character.Controlled.WorldPosition, c.WorldPosition));
              }
              else
              {
                distSqr = Math.Min(distSqr, Vector2.DistanceSquared(GameMain.GameScreen.Cam.GetPosition(), c.WorldPosition));
              }

              if (distSqr > MathUtils.Pow2(c.Params.DisableDistance))
              {
                c.Enabled = false;
                if (c.IsDead && c.AIController is EnemyAIController)
                {
                  Entity.Spawner?.AddEntityToRemoveQueue(c);
                }
              }
              else if (distSqr < MathUtils.Pow2(c.Params.DisableDistance * 0.9f))
              {
                c.Enabled = true;
              }
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, CaptureUpdateAll, "Disable far away characters");

        Character.characterUpdateTick++;



        if (Character.characterUpdateTick % Character.CharacterUpdateInterval == 0)
        {
          for (int i = 0; i < Character.CharacterList.Count; i++)
          {
            if (GameMain.LuaCs.Game.UpdatePriorityCharacters.Contains(Character.CharacterList[i])) continue;

            sw.Restart();
            Character.CharacterList[i].Update(deltaTime * Character.CharacterUpdateInterval, cam);
            sw.Stop();
            CaptureCharacter(sw.ElapsedTicks, Character.CharacterList[i]);
          }
        }

        foreach (Character character in GameMain.LuaCs.Game.UpdatePriorityCharacters)
        {
          if (character.Removed) { continue; }

          sw.Restart();
          character.Update(deltaTime, cam);
          sw.Stop();
          CaptureCharacter(sw.ElapsedTicks, character);
        }

#if CLIENT
        sw.Restart();
        Character.UpdateSpeechBubbles(deltaTime);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, CaptureUpdateAll, "UpdateSpeechBubbles");
#endif

        return false;
      }


      public static bool Character_Update_Replace(float deltaTime, Camera cam, Character __instance)
      {
        if (!CaptureUpdate.IsActive || !Showperf.Revealed) return true;
        Capture.Update.EnsureCategory(CaptureUpdate);

        Stopwatch sw = new Stopwatch();

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
        foreach (var characterTalent in _.characterTalents)
        {
          characterTalent.UpdateTalent(deltaTime);
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
          if (_.IsRagdolled) { _.AnimController.IgnorePlatforms = true; }
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
        bool isNotControlled = Character.Controlled != _;

        if (isNotControlled && (!(_ is AICharacter) || _.IsRemotePlayer))
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