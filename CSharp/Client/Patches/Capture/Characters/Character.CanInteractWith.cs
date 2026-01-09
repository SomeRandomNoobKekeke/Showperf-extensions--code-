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

      public static bool Character_CanInteractWith_Item_Replace(Character __instance, ref bool __result, Item item, out float distanceToItem, bool checkLinked)
      {
        distanceToItem = -1.0f;

        if (Showperf == null || !Showperf.Revealed || !CanInteractWithState.IsActive) return true;
        Capture.Update.EnsureCategory(CanInteractWithState);

        Stopwatch sw = new Stopwatch();

        Character _ = __instance;

        string section = "none";
        void StopStopwatch()
        {
          sw.Stop();
          if (CanInteractWithState.ByID)
          {
            Capture.Update.AddTicks(sw.ElapsedTicks, CanInteractWithState, $"CanInteractWith {item} {section}");
          }
          else
          {
            Capture.Update.AddTicks(sw.ElapsedTicks, CanInteractWithState, $"CanInteractWith {section}");
          }

        }


        sw.Restart();

        bool hidden = item.IsHidden;
#if CLIENT
        if (Screen.Selected == GameMain.SubEditorScreen) { hidden = false; }
#endif
        if (!_.CanInteract || hidden || !item.IsInteractable(_)) { StopStopwatch(); __result = false; return false; }

        section = "controller";
        Controller controller = item.GetComponent<Controller>();
        if (controller != null && _.IsAnySelectedItem(item) && controller.IsAttachedUser(_))
        {
          StopStopwatch(); __result = true; return false;
        }

        section = "from inventory";
        if (item.ParentInventory != null)
        {
          StopStopwatch(); __result = _.CanAccessInventory(item.ParentInventory); return false;
        }

        section = "wires";
        Wire wire = item.GetComponent<Wire>();
        if (wire != null && item.GetComponent<ConnectionPanel>() == null)
        {
          //locked wires are never interactable
          if (wire.Locked) { StopStopwatch(); __result = false; return false; }
          if (wire.HiddenInGame && Screen.Selected == GameMain.GameScreen) { StopStopwatch(); __result = false; return false; }

          //wires are interactable if the character has selected an item the wire is connected to,
          //and it's disconnected from the other end
          if (wire.Connections[0]?.Item != null && _.SelectedItem == wire.Connections[0].Item)
          {
            StopStopwatch(); __result = wire.Connections[1] == null; return false;
          }
          if (wire.Connections[1]?.Item != null && _.SelectedItem == wire.Connections[1].Item)
          {
            StopStopwatch(); __result = wire.Connections[0] == null; return false;
          }
          if (_.SelectedItem?.GetComponent<ConnectionPanel>()?.DisconnectedWires.Contains(wire) ?? false)
          {
            StopStopwatch(); __result = wire.Connections[0] == null && wire.Connections[1] == null; return false;
          }
        }

        section = "linked items";
        if (checkLinked && item.DisplaySideBySideWhenLinked)
        {
          foreach (MapEntity linked in item.linkedTo)
          {
            if (linked is Item linkedItem &&
                //if the linked item is inside this container (a modder or sub builder doing smth really weird?)
                //don't check it here because it'd lead to an infinite loop
                linkedItem.ParentInventory?.Owner != item)
            {
              if (_.CanInteractWith(linkedItem, out float distToLinked, checkLinked: false))
              {
                distanceToItem = distToLinked;
                StopStopwatch(); __result = true; return false;
              }
            }
          }
        }


        if (item.InteractDistance == 0.0f && !item.Prefab.Triggers.Any()) { StopStopwatch(); __result = false; return false; }

        section = "Pickec by someone else";
        Pickable pickableComponent = item.GetComponent<Pickable>();
        if (pickableComponent != null && pickableComponent.Picker != _ && pickableComponent.Picker != null && !pickableComponent.Picker.IsDead) { StopStopwatch(); __result = false; return false; }


        section = "RemoteController";
        if (_.SelectedItem?.GetComponent<RemoteController>()?.TargetItem == item) { StopStopwatch(); __result = true; return false; }
        //optimization: don't use HeldItems because it allocates memory and this method is executed very frequently
        var heldItem1 = _.Inventory?.GetItemInLimbSlot(InvSlotType.RightHand);
        if (heldItem1?.GetComponent<RemoteController>()?.TargetItem == item) { StopStopwatch(); __result = true; return false; }
        var heldItem2 = _.Inventory?.GetItemInLimbSlot(InvSlotType.LeftHand);
        if (heldItem2?.GetComponent<RemoteController>()?.TargetItem == item) { StopStopwatch(); __result = true; return false; }

        Vector2 characterDirection = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(_.AnimController.Collider.Rotation));

        Vector2 upperBodyPosition = _.Position + (characterDirection * 20.0f);
        Vector2 lowerBodyPosition = _.Position - (characterDirection * 60.0f);

        if (_.Submarine != null)
        {
          upperBodyPosition += _.Submarine.Position;
          lowerBodyPosition += _.Submarine.Position;
        }

        section = "!insideTrigger";
        bool insideTrigger = item.IsInsideTrigger(upperBodyPosition) || item.IsInsideTrigger(lowerBodyPosition);
        if (item.Prefab.Triggers.Length > 0 && !insideTrigger && item.Prefab.RequireBodyInsideTrigger) { StopStopwatch(); __result = false; return false; }

        Rectangle itemDisplayRect = new Rectangle(item.InteractionRect.X, item.InteractionRect.Y - item.InteractionRect.Height, item.InteractionRect.Width, item.InteractionRect.Height);

        // Get the point along the line between lowerBodyPosition and upperBodyPosition which is closest to the center of itemDisplayRect
        Vector2 playerDistanceCheckPosition =
            lowerBodyPosition.Y < upperBodyPosition.Y ?
            Vector2.Clamp(itemDisplayRect.Center.ToVector2(), lowerBodyPosition, upperBodyPosition) :
            Vector2.Clamp(itemDisplayRect.Center.ToVector2(), upperBodyPosition, lowerBodyPosition);

        // If playerDistanceCheckPosition is inside the itemDisplayRect then we consider the character to within 0 distance of the item
        if (itemDisplayRect.Contains(playerDistanceCheckPosition))
        {
          distanceToItem = 0.0f;
        }
        else
        {
          // Here we get the point on the itemDisplayRect which is closest to playerDistanceCheckPosition
          Vector2 rectIntersectionPoint = new Vector2(
              MathHelper.Clamp(playerDistanceCheckPosition.X, itemDisplayRect.X, itemDisplayRect.Right),
              MathHelper.Clamp(playerDistanceCheckPosition.Y, itemDisplayRect.Y, itemDisplayRect.Bottom));
          distanceToItem = Vector2.Distance(rectIntersectionPoint, playerDistanceCheckPosition);
        }

        float interactDistance = item.InteractDistance;
        if ((_.SelectedSecondaryItem != null || item.IsSecondaryItem) && _.AnimController is HumanoidAnimController c)
        {
          // Use a distance slightly shorter than the arms length to keep the character in a comfortable pose
          float armLength = 0.75f * ConvertUnits.ToDisplayUnits(c.ArmLength);
          interactDistance = Math.Min(interactDistance, armLength);
        }

        section = "distanceToItem > interactDistance";
        if (distanceToItem > interactDistance && item.InteractDistance > 0.0f) { StopStopwatch(); __result = false; return false; }

        Vector2 itemPosition = GetPosition(_.Submarine, item, item.SimPosition);

        section = "_.SelectedSecondaryItem != null";
        if (_.SelectedSecondaryItem != null && !item.IsSecondaryItem)
        {
          //don't allow selecting another Controller if it'd try to turn the character in the opposite direction
          //(e.g. periscope that's facing the wrong way while sitting in a chair)
          if (controller != null && controller.Direction != 0 && controller.Direction != _.AnimController.Direction) { StopStopwatch(); __result = false; return false; }

          //if a Controller that controls the character's pose is selected, 
          //don't allow selecting items that are behind the character's back
          if (_.SelectedSecondaryItem.GetComponent<Controller>() is { ControlCharacterPose: true } selectedController)
          {
            float threshold = ConvertUnits.ToSimUnits(Character.cursorFollowMargin);
            if (_.AnimController.Direction == Direction.Left && _.SimPosition.X + threshold < itemPosition.X) { StopStopwatch(); __result = false; return false; }
            if (_.AnimController.Direction == Direction.Right && _.SimPosition.X - threshold > itemPosition.X) { StopStopwatch(); __result = false; return false; }
          }
        }

        section = "Submarine.CheckVisibility";
        if (!item.Prefab.InteractThroughWalls && Screen.Selected != GameMain.SubEditorScreen && !insideTrigger)
        {
          var body = Submarine.CheckVisibility(_.SimPosition, itemPosition, ignoreLevel: true);
          bool itemCenterVisible = CheckBody(body, item);

          if (!itemCenterVisible && item.Prefab.RequireCursorInsideTrigger)
          {
            foreach (Rectangle trigger in item.Prefab.Triggers)
            {
              Rectangle transformTrigger = item.TransformTrigger(trigger, world: false);

              RectangleF simRect = new RectangleF(
                  x: ConvertUnits.ToSimUnits(transformTrigger.X),
                  y: ConvertUnits.ToSimUnits(transformTrigger.Y - transformTrigger.Height),
                  width: ConvertUnits.ToSimUnits(transformTrigger.Width),
                  height: ConvertUnits.ToSimUnits(transformTrigger.Height));

              simRect.Location = GetPosition(_.Submarine, item, simRect.Location);

              Vector2 closest = ToolBox.GetClosestPointOnRectangle(simRect, _.SimPosition);
              var triggerBody = Submarine.CheckVisibility(_.SimPosition, closest, ignoreLevel: true);

              if (CheckBody(triggerBody, item)) { StopStopwatch(); __result = true; return false; }
            }
          }
          else
          {
            StopStopwatch(); __result = itemCenterVisible; return false;
          }

        }



        StopStopwatch(); __result = true; return false;






        static bool CheckBody(Body body, Item item)
        {
          if (body is null) { return true; }
          var otherItem = body.UserData as Item ?? (body.UserData as ItemComponent)?.Item;
          if (otherItem != item &&
              (body.UserData as ItemComponent)?.Item != item &&
              /*allow interacting through open doors (e.g. duct blocks' colliders stay active despite being open)*/
              otherItem?.GetComponent<Door>() is not { IsOpen: true } &&
              Submarine.LastPickedFixture?.UserData as Item != item)
          {
            return false;
          }

          return true;
        }

        static Vector2 GetPosition(Submarine submarine, Item item, Vector2 simPosition)
        {
          Vector2 position = simPosition;

          Vector2 itemSubPos = item.Submarine?.SimPosition ?? Vector2.Zero;
          Vector2 subPos = submarine?.SimPosition ?? Vector2.Zero;

          if (submarine == null && item.Submarine != null)
          {
            //character is outside, item inside
            position += itemSubPos;
          }
          else if (submarine != null && item.Submarine == null)
          {
            //character is inside, item outside
            position -= subPos;
          }
          else if (submarine != item.Submarine && submarine != null)
          {
            //character and the item are inside different subs
            position += itemSubPos;
            position -= subPos;
          }

          return position;
        }


      }

    }
  }
}