
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
      public static void CaptureCharacter3(long ticks, Character character, string text)
      {
        if (ControlState.ByID)
        {
          Capture.Update.AddTicks(ticks, ControlState, $"{character.Info?.DisplayName ?? character.ToString()}.{text}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, ControlState, text);
        }
      }



      public static bool Character_Control_Replace(float deltaTime, Camera cam, Character __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !ControlState.IsActive) return true;
        Capture.Update.EnsureCategory(ControlState);
        Stopwatch sw = new Stopwatch();

        Character _ = __instance;

        _.ViewTarget = null;
        if (!_.AllowInput) { return false; }


        sw.Restart();
        if (Character.Controlled == _ || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer))
        {
          _.SmoothedCursorPosition = _.cursorPosition;
        }
        else
        {
          //apply some smoothing to the cursor positions of remote players when playing as a client
          //to make aiming look a little less choppy
          Vector2 smoothedCursorDiff = _.cursorPosition - _.SmoothedCursorPosition;
          smoothedCursorDiff = NetConfig.InterpolateCursorPositionError(smoothedCursorDiff);
          _.SmoothedCursorPosition = _.cursorPosition - smoothedCursorDiff;
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "SmoothedCursorPosition");

        sw.Restart();
        bool aiControlled = _ is AICharacter && Character.Controlled != _ && !_.IsRemotePlayer;
        bool controlledByServer = GameMain.NetworkMember is { IsClient: true } && _.IsRemotelyControlled;
        if (!aiControlled && !controlledByServer)
        {
          Vector2 targetMovement = _.GetTargetMovement();
          _.AnimController.TargetMovement = targetMovement;
          _.AnimController.IgnorePlatforms = _.AnimController.TargetMovement.Y < -0.1f;
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "GetTargetMovement");

        sw.Restart();
        if (_.AnimController is HumanoidAnimController humanAnimController)
        {
          humanAnimController.Crouching =
              humanAnimController.ForceSelectAnimationType == AnimationType.Crouch ||
              _.IsKeyDown(InputType.Crouch);
          if (Screen.Selected is not { IsEditor: true })
          {
            humanAnimController.ForceSelectAnimationType = AnimationType.NotDefined;
          }
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "Crouching");

        sw.Restart();
        if (!aiControlled &&
            !_.AnimController.IsUsingItem &&
            _.AnimController.Anim != AnimController.Animation.CPR &&
            (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient || Character.Controlled == _) &&
            ((!_.IsClimbing && _.AnimController.OnGround) || (_.IsClimbing && _.IsKeyDown(InputType.Aim))) &&
            !_.AnimController.InWater)
        {
          if (!_.FollowCursor)
          {
            _.AnimController.TargetDir = Direction.Right;
          }
          //only humanoids' flipping is controlled by the cursor, monster flipping is driven by their movement in FishAnimController
          else if (_.AnimController is HumanoidAnimController)
          {
            if (_.CursorPosition.X < _.AnimController.Collider.Position.X - Character.cursorFollowMargin)
            {
              _.AnimController.TargetDir = Direction.Left;
            }
            else if (_.CursorPosition.X > _.AnimController.Collider.Position.X + Character.cursorFollowMargin)
            {
              _.AnimController.TargetDir = Direction.Right;
            }
          }
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "FollowCursor");

        sw.Restart();
        if (GameMain.NetworkMember != null)
        {
          if (GameMain.NetworkMember.IsServer)
          {
            if (!aiControlled)
            {
              if (_.dequeuedInput.HasFlag(Character.InputNetFlags.FacingLeft))
              {
                _.AnimController.TargetDir = Direction.Left;
              }
              else
              {
                _.AnimController.TargetDir = Direction.Right;
              }
            }
          }
          else if (GameMain.NetworkMember.IsClient && Character.Controlled != _)
          {
            if (_.memState.Count > 0)
            {
              _.AnimController.TargetDir = _.memState[0].Direction;
            }
          }
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "dequeuedInput");

#if DEBUG && CLIENT
        if (PlayerInput.KeyHit(Microsoft.Xna.Framework.Input.Keys.F))
        {
            _.AnimController.ReleaseStuckLimbs();
            if (_.AIController != null && _.AIController is EnemyAIController enemyAI)
            {
                enemyAI.LatchOntoAI?.DeattachFromBody(reset: true);
            }
        }
#endif

        sw.Restart();
        if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && Character.Controlled != _ && _.IsKeyDown(InputType.Aim))
        {
          if (_.currentAttackTarget.AttackLimb?.attack is Attack { Ranged: true } attack && _.AIController is EnemyAIController enemyAi)
          {
            enemyAi.AimRangedAttack(attack, _.currentAttackTarget.DamageTarget as Entity);
          }
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "AimRangedAttack");

        sw.Restart();
        if (_.attackCoolDown > 0.0f)
        {
          _.attackCoolDown -= deltaTime;
        }
        else if (_.IsKeyDown(InputType.Attack))
        {
          //normally the attack target, where to aim the attack and such is handled by EnemyAIController,
          //but in the case of player-controlled monsters, we handle it here
          if (_.IsPlayer)
          {
            float dist = -1;
            Vector2 attackPos = _.SimPosition + ConvertUnits.ToSimUnits(_.cursorPosition - _.Position);
            List<Body> ignoredBodies = _.AnimController.Limbs.Select(l => l.body.FarseerBody).ToList();
            ignoredBodies.Add(_.AnimController.Collider.FarseerBody);

            var body = Submarine.PickBody(
                _.SimPosition,
                attackPos,
                ignoredBodies,
                Physics.CollisionCharacter | Physics.CollisionWall);

            IDamageable attackTarget = null;
            if (body != null)
            {
              attackPos = Submarine.LastPickedPosition;

              if (body.UserData is Submarine sub)
              {
                body = Submarine.PickBody(
                    _.SimPosition - ((Submarine)body.UserData).SimPosition,
                    attackPos - ((Submarine)body.UserData).SimPosition,
                    ignoredBodies,
                    Physics.CollisionWall);

                if (body != null)
                {
                  attackPos = Submarine.LastPickedPosition + sub.SimPosition;
                  attackTarget = body.UserData as IDamageable;
                }
              }
              else
              {
                if (body.UserData is IDamageable damageable)
                {
                  attackTarget = damageable;
                }
                else if (body.UserData is Limb limb)
                {
                  attackTarget = limb.character;
                }
              }
            }
            var currentContexts = _.GetAttackContexts();
            var attackLimbs = _.AnimController.Limbs.Where(static l => l.attack != null);
            bool hasAttacksWithoutRootForce = attackLimbs.Any(static l => !l.attack.HasRootForce);
            var validLimbs = attackLimbs.Where(l =>
            {
              if (l.IsSevered || l.IsStuck) { return false; }
              if (l.Disabled) { return false; }
              var attack = l.attack;
              if (attack.CoolDownTimer > 0) { return false; }
              //disallow attacks with root force if there's any other attacks available
              if (hasAttacksWithoutRootForce && attack.HasRootForce) { return false; }
              if (!attack.IsValidContext(currentContexts)) { return false; }
              if (attackTarget != null)
              {
                if (!attack.IsValidTarget(attackTarget as Entity)) { return false; }
                if (attackTarget is ISerializableEntity se and Character)
                {
                  if (attack.Conditionals.Any(c => !c.TargetSelf && !c.Matches(se))) { return false; }
                }
              }
              if (attack.Conditionals.Any(c => c.TargetSelf && !c.Matches(_))) { return false; }
              return true;
            });
            var sortedLimbs = validLimbs.OrderBy(l => Vector2.DistanceSquared(ConvertUnits.ToDisplayUnits(l.SimPosition), _.cursorPosition));
            // Select closest
            var attackLimb = sortedLimbs.FirstOrDefault();
            if (attackLimb != null)
            {
              if (attackTarget is Character targetCharacter)
              {
                dist = ConvertUnits.ToDisplayUnits(Vector2.Distance(Submarine.LastPickedPosition, attackLimb.SimPosition));
                foreach (Limb limb in targetCharacter.AnimController.Limbs)
                {
                  if (limb.IsSevered || limb.Removed) { continue; }
                  float tempDist = ConvertUnits.ToDisplayUnits(Vector2.Distance(limb.SimPosition, attackLimb.SimPosition));
                  if (tempDist < dist)
                  {
                    dist = tempDist;
                  }
                }
              }
              attackLimb.UpdateAttack(deltaTime, attackPos, attackTarget, out AttackResult attackResult, dist);
              if (!attackLimb.attack.IsRunning)
              {
                _.attackCoolDown = 1.0f;
              }
            }
          }
          else if (GameMain.NetworkMember is { IsClient: true } && Character.Controlled != _)
          {
            if (_.currentAttackTarget.DamageTarget is Entity { Removed: true })
            {
              _.currentAttackTarget = default;
            }

            AttackResult attackResult;
            _.currentAttackTarget.AttackLimb?.UpdateAttack(deltaTime, _.currentAttackTarget.AttackPos, _.currentAttackTarget.DamageTarget, out attackResult);
          }
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "UpdateAttack");

        if (_.Inventory != null)
        {
          //this doesn't need to be run by the server, clients sync the contents of their inventory with the server instead of the inputs used to manipulate the inventory
#if CLIENT
          sw.Restart();
          if (_.IsKeyHit(InputType.DropItem) && Screen.Selected is { IsEditor: false }&&  CharacterHUD.ShouldDrawInventory(_))
          {
            foreach (Item item in _.HeldItems)
            {
              if (!_.CanInteractWith(item)) { continue; }

              if (_.SelectedItem?.OwnInventory != null && !_.SelectedItem.OwnInventory.Locked && _.SelectedItem.OwnInventory.CanBePut(item))
              {
                _.SelectedItem.OwnInventory.TryPutItem(item, _);
              }
              else
              {
                item.Drop(_);
              }
              //only drop one held item per key hit
              break;
            }
          }
          sw.Stop();
          CaptureCharacter3(sw.ElapsedTicks, _, "Drop item");
#endif

          sw.Restart();
          bool CanUseItemsWhenSelected(Item item) => item == null || !item.Prefab.DisableItemUsageWhenSelected;
          if (CanUseItemsWhenSelected(_.SelectedItem) && CanUseItemsWhenSelected(_.SelectedSecondaryItem))
          {
            sw.Restart();
            foreach (Item item in _.HeldItems)
            {
              tryUseItem(item, deltaTime);
            }
            sw.Stop();
            CaptureCharacter3(sw.ElapsedTicks, _, "tryUseItem HeldItems");

            sw.Restart();
            foreach (Item item in _.Inventory.AllItems)
            {
              if (item.GetComponent<Wearable>() is { AllowUseWhenWorn: true } && _.HasEquippedItem(item))
              {
                tryUseItem(item, deltaTime);
              }
            }
            sw.Stop();
            CaptureCharacter3(sw.ElapsedTicks, _, "tryUseItem AllItems");
          }

        }

        void tryUseItem(Item item, float deltaTime)
        {
          if (_.IsKeyDown(InputType.Aim) || !item.RequireAimToSecondaryUse)
          {
            item.SecondaryUse(deltaTime, _);
          }
          if (_.IsKeyDown(InputType.Use) && !item.IsShootable)
          {
            if (!item.RequireAimToUse || _.IsKeyDown(InputType.Aim))
            {
              item.Use(deltaTime, user: _);
            }
          }
          if (_.IsKeyDown(InputType.Shoot) && item.IsShootable)
          {
            if (!item.RequireAimToUse || _.IsKeyDown(InputType.Aim))
            {
              item.Use(deltaTime, user: _);
            }
#if CLIENT
            else if (item.RequireAimToUse && !_.IsKeyDown(InputType.Aim))
            {
                HintManager.OnShootWithoutAiming(_, item);
            }
#endif
          }
        }

        sw.Restart();
        if (_.SelectedItem != null)
        {
          tryUseItem(_.SelectedItem, deltaTime);
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "tryUseItem SelectedItem");

        sw.Restart();
        if (_.SelectedCharacter != null)
        {
          if (!_.SelectedCharacter.CanBeSelected ||
              (Vector2.DistanceSquared(_.SelectedCharacter.WorldPosition, _.WorldPosition) > Character.MaxDragDistance * Character.MaxDragDistance &&
              _.SelectedCharacter.GetDistanceToClosestLimb(_.GetRelativeSimPosition(_.selectedCharacter, _.WorldPosition)) > ConvertUnits.ToSimUnits(Character.MaxDragDistance)))
          {
            _.DeselectCharacter();
          }
        }
        sw.Stop();
        CaptureCharacter3(sw.ElapsedTicks, _, "DeselectCharacter");

        if (_.IsRemotelyControlled && _.keys != null)
        {
          foreach (Key key in _.keys)
          {
            key.ResetHit();
          }
        }

        return false;
      }


    }
  }
}