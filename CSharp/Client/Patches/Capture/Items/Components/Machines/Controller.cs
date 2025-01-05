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

using FarseerPhysics;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class ControllerPatch
    {
      public static CaptureState ControllerState;
      public static CaptureState ControllerUseState;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Controller).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(ControllerPatch).GetMethod("Controller_Update_Replace"))
        );

        harmony.Patch(
          original: typeof(Controller).GetMethod("Use", AccessTools.all),
          prefix: new HarmonyMethod(typeof(ControllerPatch).GetMethod("Controller_Use_Replace"))
        );

        ControllerState = Capture.Get("Showperf.Update.MapEntity.Items.Components.Controller");
        ControllerUseState = Capture.Get("Showperf.Update.MapEntity.Items.Use.Controller");
      }

      public static void CaptureController(long ticks, Controller _, string name)
      {
        if (ControllerState.ByID)
        {
          Capture.Update.AddTicks(ticks, ControllerState, $"{_.Item} {name}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, ControllerState, name);
        }
      }

      public static void CaptureController2(long ticks, Controller _, string name)
      {
        if (ControllerUseState.ByID)
        {
          Capture.Update.AddTicks(ticks, ControllerUseState, $"{_.Item} {name}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, ControllerUseState, name);
        }
      }

      public static bool Controller_Update_Replace(float deltaTime, Camera cam, Controller __instance)
      {
        if (!ControllerState.IsActive || !Showperf.Revealed) return true;
        Capture.Update.EnsureCategory(ControllerState);

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();


        Controller _ = __instance;

        sw.Restart();

        _.cam = cam;
        if (!_.ForceUserToStayAttached) { _.UserInCorrectPosition = false; }

        string signal = _.IsToggle && _.State ? _.output : _.falseOutput;
        if (_.item.Connections != null && _.IsToggle && !string.IsNullOrEmpty(signal) && !_.IsOutOfPower())
        {
          _.item.SendSignal(signal, "signal_out");
          _.item.SendSignal(signal, "trigger_out");
        }

        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "SendSignal");


        if (_.forceSelectNextFrame && _.user != null)
        {
          _.user.SelectedItem = _.item;
        }
        _.forceSelectNextFrame = false;
        _.userCanInteractCheckTimer -= deltaTime;


        bool condition1 = false;
        bool condition2 = false;
        bool condition3 = false;
        bool condition4 = false;
        bool condition5 = false;
        bool condition6 = false;
        bool condition7 = false;

        bool shouldCancelUsing = false;

        try
        {
          sw2.Restart();
          shouldCancelUsing |= _.user == null;
          sw2.Stop();
          CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition1");

          if (!shouldCancelUsing)
          {
            sw2.Restart();
            shouldCancelUsing |= _.user.Removed;
            sw2.Stop();
            CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition2");
          }

          if (!shouldCancelUsing)
          {
            sw2.Restart();
            shouldCancelUsing |= !_.user.IsAnySelectedItem(_.item);
            sw2.Stop();
            CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition3");
          }

          if (!shouldCancelUsing)
          {
            sw2.Restart();
            shouldCancelUsing |= (_.item.ParentInventory != null && !_.IsAttachedUser(_.user));
            sw2.Stop();
            CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition4");
          }

          if (!shouldCancelUsing)
          {
            sw2.Restart();
            shouldCancelUsing |= (_.UsableIn == Controller.UseEnvironment.Water && !_.user.AnimController.InWater);
            sw2.Stop();
            CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition5");
          }

          if (!shouldCancelUsing)
          {
            sw2.Restart();
            shouldCancelUsing |= (_.UsableIn == Controller.UseEnvironment.Air && _.user.AnimController.InWater);
            sw2.Stop();
            CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition6");
          }

          if (!shouldCancelUsing)
          {
            sw2.Restart();
            shouldCancelUsing |= !_.CheckUserCanInteract();
            sw2.Stop();
            CaptureController(sw2.ElapsedTicks, _, "CancelUsing condition7");
          }
        }
        catch (Exception e) { error(e); }


        sw.Restart();
        if (shouldCancelUsing)
        {
          if (_.user != null)
          {
            _.CancelUsing(_.user);
            _.user = null;
          }

          if (_.item.Connections == null || !_.IsToggle || string.IsNullOrEmpty(signal)) { _.IsActive = false; }

          sw.Stop();
          CaptureController(sw.ElapsedTicks, _, "CancelUsing");
          return false;
        }
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "CancelUsing");

        sw.Restart();
        if (_.ForceUserToStayAttached && Vector2.DistanceSquared(_.item.WorldPosition, _.user.WorldPosition) > 0.1f)
        {
          _.user.TeleportTo(_.item.WorldPosition);
          _.user.AnimController.Collider.ResetDynamics();
          foreach (var limb in _.user.AnimController.Limbs)
          {
            if (limb.Removed || limb.IsSevered) { continue; }
            limb.body?.ResetDynamics();
          }
        }
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "limb.body?.ResetDynamics");

        sw.Restart();
        _.user.AnimController.StartUsingItem();
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "limb.body?.ResetDynamics");

        sw.Restart();
        if (_.userPos != Vector2.Zero)
        {
          Vector2 diff = (_.item.WorldPosition + _.userPos) - _.user.WorldPosition;

          if (_.user.AnimController.InWater)
          {
            if (diff.LengthSquared() > 30.0f * 30.0f)
            {
              _.user.AnimController.TargetMovement = Vector2.Clamp(diff * 0.01f, -Vector2.One, Vector2.One);
              _.user.AnimController.TargetDir = diff.X > 0.0f ? Direction.Right : Direction.Left;
            }
            else
            {
              _.user.AnimController.TargetMovement = Vector2.Zero;
              _.UserInCorrectPosition = true;
            }
          }
          else
          {
            // Secondary items (like ladders or chairs) will control the character position over primary items
            // Only control the character position if the character doesn't have another secondary item already controlling it
            if (!_.user.HasSelectedAnotherSecondaryItem(_.Item))
            {
              diff.Y = 0.0f;
              if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && _.user != Character.Controlled)
              {
                if (Math.Abs(diff.X) > 20.0f)
                {
                  //wait for the character to walk to the correct position
                  return false;
                }
                else if (Math.Abs(diff.X) > 0.1f)
                {
                  //aim to keep the collider at the correct position once close enough
                  _.user.AnimController.Collider.LinearVelocity = new Vector2(
                      diff.X * 0.1f,
                      _.user.AnimController.Collider.LinearVelocity.Y);
                }
              }
              else if (Math.Abs(diff.X) > 10.0f)
              {
                _.user.AnimController.TargetMovement = Vector2.Normalize(diff);
                _.user.AnimController.TargetDir = diff.X > 0.0f ? Direction.Right : Direction.Left;
                return false;
              }
              _.user.AnimController.TargetMovement = Vector2.Zero;
            }
            _.UserInCorrectPosition = true;
          }
        }
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "AnimController.InWater");


        sw.Restart();
        _.ApplyStatusEffects(ActionType.OnActive, deltaTime, _.user);
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "ApplyStatusEffects OnActive");

        if (_.limbPositions.Count == 0) { return false; }

        sw.Restart();
        _.user.AnimController.StartUsingItem();
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "StartUsingItem");

        sw.Restart();
        if (_.user.SelectedItem != null)
        {
          _.user.AnimController.ResetPullJoints(l => l.IsLowerBody);
        }
        else
        {
          _.user.AnimController.ResetPullJoints();
        }
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "ResetPullJoints");


        sw.Restart();
        if (_.dir != 0) { _.user.AnimController.TargetDir = _.dir; }

        foreach (LimbPos lb in _.limbPositions)
        {
          Limb limb = _.user.AnimController.GetLimb(lb.LimbType);
          if (limb == null || !limb.body.Enabled) { continue; }
          // Don't move lower body limbs if there's another selected secondary item that should control them
          if (limb.IsLowerBody && _.user.HasSelectedAnotherSecondaryItem(_.Item)) { continue; }
          // Don't move hands if there's a selected primary item that should control them
          if (!limb.IsLowerBody && _.Item == _.user.SelectedSecondaryItem && _.user.SelectedItem != null) { continue; }
          if (lb.AllowUsingLimb)
          {
            switch (lb.LimbType)
            {
              case LimbType.RightHand:
              case LimbType.RightForearm:
              case LimbType.RightArm:
                if (_.user.Inventory.GetItemInLimbSlot(InvSlotType.RightHand) != null) { continue; }
                break;
              case LimbType.LeftHand:
              case LimbType.LeftForearm:
              case LimbType.LeftArm:
                if (_.user.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand) != null) { continue; }
                break;
            }
          }
          limb.Disabled = true;
          Vector2 worldPosition = new Vector2(_.item.WorldRect.X, _.item.WorldRect.Y) + lb.Position * _.item.Scale;
          Vector2 diff = worldPosition - limb.WorldPosition;
          limb.PullJointEnabled = true;
          limb.PullJointWorldAnchorB = limb.SimPosition + ConvertUnits.ToSimUnits(diff);
        }
        sw.Stop();
        CaptureController(sw.ElapsedTicks, _, "set limb anchors");

        return false;
      }


      public static bool Controller_Use_Replace(Controller __instance, ref bool __result, float deltaTime, Character activator = null)
      {
        Controller _ = __instance;
        try
        {
          if (!Showperf.Revealed || !ControllerUseState.IsActive) return Controller_Use_FullVanillaWithTryCatch(__instance, deltaTime, activator);
          Capture.Update.EnsureCategory(ControllerUseState);
          Stopwatch sw = new Stopwatch();

          if (activator != _.user)
          {
            __result = false; return false;
          }

          sw.Restart();
          if (_.user == null || _.user.Removed || !_.user.IsAnySelectedItem(_.item) || !_.user.CanInteractWith(_.item))
          {
            sw.Stop();
            CaptureController2(sw.ElapsedTicks, _, "CanInteractWith");
            _.user = null;
            __result = false; return false;
          }
          sw.Stop();
          CaptureController2(sw.ElapsedTicks, _, "CanInteractWith");

          sw.Restart();
          if (_.IsOutOfPower())
          {
            sw.Stop();
            CaptureController2(sw.ElapsedTicks, _, "IsOutOfPower");
            __result = false; return false;
          }
          sw.Stop();
          CaptureController2(sw.ElapsedTicks, _, "IsOutOfPower");

          sw.Restart();
          if (_.IsToggle && (activator == null || _.lastUsed < Timing.TotalTime - 0.1))
          {
            if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
            {
              _.State = !_.State;
#if SERVER
              _.item.CreateServerEvent(_);
#endif
            }
          }
          else if (!string.IsNullOrEmpty(_.output))
          {
            _.item.SendSignal(new Signal(_.output, sender: _.user), "trigger_out");
          }
          sw.Stop();
          CaptureController2(sw.ElapsedTicks, _, "SendSignal");

          sw.Restart();
          _.lastUsed = Timing.TotalTime;
          _.ApplyStatusEffects(ActionType.OnUse, 1.0f, activator);
          sw.Stop();
          CaptureController2(sw.ElapsedTicks, _, "ApplyStatusEffects(ActionType.OnUse");

          __result = true; return false;
        }
        catch (Exception e)
        {
          DebugConsole.IsOpen = true;
          error($"Mysterious {e.Message} in Controller_Use_Replace just happened");
          log($"Controller item is {_.item}", Color.Orange);
          log($"Controller user is {_.user}[{_.user?.ID}] {_.user?.Info?.DisplayName}", Color.Orange);
          log($"activator is {activator}[{activator?.ID}] {activator?.Info?.DisplayName}", Color.Orange);
          log($"Controller.output is {_.output}", Color.Orange);
          log($"Controller.State is {_.State}", Color.Orange);
          log($"Controller.IsOutOfPower is {_.IsOutOfPower()}", Color.Orange);
          log($"Report this to Showperf Extensions steam page or github", Color.Yellow);

          __result = false; return false;
        }
      }



      public static bool Controller_Use_FullVanillaWithTryCatch(Controller __instance, float deltaTime, Character activator = null)
      {
        Controller _ = __instance;
        try
        {
          if (activator != _.user)
          {
            return false;
          }
          if (_.user == null || _.user.Removed || !_.user.IsAnySelectedItem(_.item) || !_.user.CanInteractWith(_.item))
          {
            _.user = null;
            return false;
          }

          if (_.IsOutOfPower()) { return false; }

          if (_.IsToggle && (activator == null || _.lastUsed < Timing.TotalTime - 0.1))
          {
            if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
            {
              _.State = !_.State;
#if SERVER
              _.item.CreateServerEvent(_);
#endif
            }
          }
          else if (!string.IsNullOrEmpty(_.output))
          {
            _.item.SendSignal(new Signal(_.output, sender: _.user), "trigger_out");
          }

          _.lastUsed = Timing.TotalTime;
          _.ApplyStatusEffects(ActionType.OnUse, 1.0f, activator);

          return true;
        }
        catch (Exception e)
        {
          DebugConsole.IsOpen = true;
          error($"Mysterious {e.Message} in Controller_Use_Replace just happened");
          log($"Controller item is {_.item}", Color.Orange);
          log($"Controller user is {_.user}[{_.user?.ID}] {_.user?.Info?.DisplayName}", Color.Orange);
          log($"activator is {activator}[{activator?.ID}] {activator?.Info?.DisplayName}", Color.Orange);
          log($"Controller.output is {_.output}", Color.Orange);
          log($"Controller.State is {_.State}", Color.Orange);
          log($"Controller.IsOutOfPower is {_.IsOutOfPower()}", Color.Orange);
          log($"Report this to Showperf Extensions steam page or github", Color.Yellow);

          return false;
        }
      }
    }
  }
}