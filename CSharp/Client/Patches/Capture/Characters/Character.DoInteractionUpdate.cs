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
      public static bool Character_DoInteractionUpdate_Replace(Character __instance, float deltaTime, Vector2 mouseSimPos)
      {
        if (Showperf == null || !Showperf.Revealed || !DoInteractionUpdateState.IsActive) return true;
        Capture.Update.EnsureCategory(DoInteractionUpdateState);

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        sw2.Restart();

        Character _ = __instance;

        if (_.IsAIControlled) { return false; }

        if (_.DisableInteract)
        {
          _.DisableInteract = false;
          return false;
        }

        if (!_.CanInteract)
        {
          _.SelectedItem = _.SelectedSecondaryItem = null;
          _.focusedItem = null;
          if (!_.AllowInput)
          {
            _.FocusedCharacter = null;
            if (_.SelectedCharacter != null) { _.DeselectCharacter(); }
            return false;
          }
        }

#if CLIENT
        if (_.IsLocalPlayer)
        {
          if (!Character.IsMouseOnUI && (_.ViewTarget == null || _.ViewTarget == _) && !_.DisableFocusingOnEntities)
          {
            if (_.findFocusedTimer <= 0.0f || Screen.Selected == GameMain.SubEditorScreen)
            {
              

              if (!PlayerInput.PrimaryMouseButtonHeld() || Barotrauma.Inventory.DraggingItemToWorld)
              {
                
                _.FocusedCharacter = _.CanInteract || _.CanEat ? _.FindCharacterAtPosition(mouseSimPos) : null;
                if (_.FocusedCharacter != null && !_.CanSeeTarget(_.FocusedCharacter)) { _.FocusedCharacter = null; }
                float aimAssist = GameSettings.CurrentConfig.AimAssistAmount * (_.AnimController.InWater ? 1.5f : 1.0f);
                if (_.HeldItems.Any(it => it?.GetComponent<Wire>()?.IsActive ?? false))
                {
                  //disable aim assist when rewiring to make it harder to accidentally select items when adding wire nodes
                  aimAssist = 0.0f;
                }
                sw.Restart();
                _.UpdateInteractablesInRange(); 
                sw.Stop();
                Capture.Update.AddTicks(sw.ElapsedTicks, DoInteractionUpdateState, "UpdateInteractablesInRange");

                if (!_.ShowInteractionLabels) // show labels handles setting the focused item in CharacterHUD, so we can click on them boxes
                {
                  _.focusedItem = _.CanInteract ? _.FindClosestItem(_.interactablesInRange, mouseSimPos, aimAssist) : null;
                }

                if (_.focusedItem != null)
                {
                  if (_.focusedItem.CampaignInteractionType != CampaignMode.InteractionType.None ||
                      /*pets' "play" interaction can interfere with interacting with items, so let's remove focus from the pet if the cursor is closer to a highlighted item*/
                      _.FocusedCharacter is { IsPet: true } && Vector2.DistanceSquared(_.focusedItem.SimPosition, mouseSimPos) < Vector2.DistanceSquared(_.FocusedCharacter.SimPosition, mouseSimPos))
                  {
                    _.FocusedCharacter = null;
                  }
                }
                _.findFocusedTimer = 0.05f;
                
              }
              else
              {
                if (_.focusedItem != null && !_.CanInteractWith(_.focusedItem)) { _.focusedItem = null; }
                if (_.FocusedCharacter != null && !_.CanInteractWith(_.FocusedCharacter)) { _.FocusedCharacter = null; }
              }
            }
          }
          else
          {
            _.FocusedCharacter = null;
            _.focusedItem = null;
          }
          _.findFocusedTimer -= deltaTime;
          _.DisableFocusingOnEntities = false;
        }
#endif
        var head = _.AnimController.GetLimb(LimbType.Head);
        bool headInWater = head == null ?
            _.AnimController.InWater :
            head.InWater;
        //climb ladders automatically when pressing up/down inside their trigger area
        Ladder currentLadder = _.SelectedSecondaryItem?.GetComponent<Ladder>();
        if ((_.SelectedSecondaryItem == null || currentLadder != null) &&
            !headInWater && Screen.Selected != GameMain.SubEditorScreen)
        {
          bool climbInput = _.IsKeyDown(InputType.Up) || _.IsKeyDown(InputType.Down);
          bool isControlled = Character.Controlled == _;

          Ladder nearbyLadder = null;
          if (isControlled || climbInput)
          {
            float minDist = float.PositiveInfinity;
            foreach (Ladder ladder in Ladder.List)
            {
              if (ladder == currentLadder)
              {
                continue;
              }
              else if (currentLadder != null)
              {
                //only switch from ladder to another if the ladders are above the current ladders and pressing up, or vice versa
                if (ladder.Item.WorldPosition.Y > currentLadder.Item.WorldPosition.Y != _.IsKeyDown(InputType.Up))
                {
                  continue;
                }
              }

              if (_.CanInteractWith(ladder.Item, out float dist, checkLinked: false) && dist < minDist)
              {
                minDist = dist;
                nearbyLadder = ladder;
                if (isControlled)
                {
                  ladder.Item.IsHighlighted = true;
                }
                break;
              }
            }
          }

          if (nearbyLadder != null && climbInput)
          {
            if (nearbyLadder.Select(_))
            {
              _.SelectedSecondaryItem = nearbyLadder.Item;
            }
          }
        }

        bool selectInputSameAsDeselect = false;
#if CLIENT
        selectInputSameAsDeselect = GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Select] == GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Deselect];
#endif



        if (_.SelectedCharacter != null && (_.IsKeyHit(InputType.Grab) || _.IsKeyHit(InputType.Health))) //Let people use ladders and buttons and stuff when dragging chars
        {
          _.DeselectCharacter();
        }
        else if (_.FocusedCharacter != null && _.IsKeyHit(InputType.Grab) && _.FocusedCharacter.CanBeDraggedBy(_) && (_.CanInteract || _.FocusedCharacter.IsDead && _.CanEat))
        {
          _.SelectCharacter(_.FocusedCharacter);
        }
        else if (_.FocusedCharacter is { IsIncapacitated: false } && _.IsKeyHit(InputType.Use) && _.FocusedCharacter.IsPet && _.CanInteract)
        {
          (_.FocusedCharacter.AIController as EnemyAIController).PetBehavior.Play(_);
        }
        else if (_.FocusedCharacter != null && _.IsKeyHit(InputType.Health) && _.FocusedCharacter.CanBeHealedBy(_))
        {
          if (_.FocusedCharacter == _.SelectedCharacter)
          {
            _.DeselectCharacter();
#if CLIENT
            if (Character.Controlled == _)
            {
              CharacterHealth.OpenHealthWindow = null;
            }
#endif
          }
          else
          {
            _.SelectCharacter(_.FocusedCharacter);
#if CLIENT
            if (Character.Controlled == _)
            {
              HealingCooldown.PutOnCooldown();
              CharacterHealth.OpenHealthWindow = _.FocusedCharacter.CharacterHealth;
            }
#elif SERVER
            if (GameMain.Server?.ConnectedClients is { } clients)
            {
              foreach (Client c in clients)
              {
                if (c.Character != _) { continue; }

                HealingCooldown.SetCooldown(c);
                break;
              }
            }
#endif
          }
        }
        else if (_.FocusedCharacter != null && _.IsKeyHit(InputType.Use) && _.FocusedCharacter.onCustomInteract != null && _.FocusedCharacter.AllowCustomInteract)
        {
          _.FocusedCharacter.onCustomInteract(_.FocusedCharacter, _);
        }
        else if (_.IsKeyHit(InputType.Deselect) && _.SelectedItem != null &&
            (_.focusedItem == null || _.focusedItem == _.SelectedItem || !selectInputSameAsDeselect))
        {
          _.SelectedItem = null;
#if CLIENT
          CharacterHealth.OpenHealthWindow = null;
#endif
        }
        else if (_.IsKeyHit(InputType.Deselect) && _.SelectedSecondaryItem != null && _.SelectedSecondaryItem.GetComponent<Ladder>() == null &&
            (_.focusedItem == null || _.focusedItem == _.SelectedSecondaryItem || !selectInputSameAsDeselect))
        {
          _.ReleaseSecondaryItem();
#if CLIENT
          CharacterHealth.OpenHealthWindow = null;
#endif
        }
        else if (_.IsKeyHit(InputType.Health) && _.SelectedItem != null)
        {
          _.SelectedItem = null;
        }
        else if (_.focusedItem != null)
        {
#if CLIENT
          if (CharacterInventory.DraggingItemToWorld) { return false; }
          if (selectInputSameAsDeselect)
          {
            _.keys[(int)InputType.Deselect].Reset();
          }
#endif
          bool canInteract = _.focusedItem.TryInteract(_);
#if CLIENT
          if (Character.Controlled == _)
          {
            _.focusedItem.IsHighlighted = true;
            if (canInteract)
            {
              CharacterHealth.OpenHealthWindow = null;
            }
          }
#endif
        }

        sw2.Stop();
        Capture.Update.AddTicks(sw2.ElapsedTicks - sw.ElapsedTicks, DoInteractionUpdateState, "Rest");

        return false;
      }


    }
  }
}