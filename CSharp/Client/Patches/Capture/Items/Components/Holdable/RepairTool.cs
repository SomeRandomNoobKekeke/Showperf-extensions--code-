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
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class RepairToolPatch
    {
      public static CaptureState RepairToolState;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(RepairTool).GetMethod("Use", AccessTools.all),
          prefix: new HarmonyMethod(typeof(RepairToolPatch).GetMethod("RepairTool_Use_Replace"))
        );

        RepairToolState = Capture.Get("Showperf.Update.MapEntity.Items.Use.RepairToolUse");
      }

      public static bool RepairTool_Use_Replace(RepairTool __instance, ref bool __result, float deltaTime, Character character = null)
      {
        if (!RepairToolState.IsActive || !Showperf.Revealed) return true;
        Capture.Update.EnsureCategory(RepairToolState);
        Stopwatch sw = new Stopwatch();

        RepairTool _ = __instance;

        if (character != null)
        {
          if (_.item.RequireAimToUse && !character.IsKeyDown(InputType.Aim)) { __result = false; return false; }
        }

        sw.Restart();
        float degreeOfSuccess = character == null ? 0.5f : _.DegreeOfSuccess(character);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "DegreeOfSuccess");

        sw.Restart();
        bool failed = false;
        if (Rand.Range(0.0f, 0.5f) > degreeOfSuccess)
        {
          _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
          failed = true;
        }
        if (_.UsableIn == RepairTool.UseEnvironment.None)
        {
          _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
          failed = true;
        }
        if (_.item.InWater)
        {
          if (_.UsableIn == RepairTool.UseEnvironment.Air)
          {
            _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
            failed = true;
          }
        }
        else
        {
          if (_.UsableIn == RepairTool.UseEnvironment.Water)
          {
            _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
            failed = true;
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "ApplyStatusEffects(ActionType.OnFailure");


        if (failed)
        {
          // Always apply ActionType.OnUse. If doesn't fail, the effect is called later.
          sw.Restart();
          _.ApplyStatusEffects(ActionType.OnUse, deltaTime, character);
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "ApplyStatusEffects(ActionType.OnUse");
          __result = false; return false;
        }

        Vector2 rayStart;
        Vector2 rayStartWorld;
        Vector2 sourcePos = character?.AnimController == null ? _.item.SimPosition : character.AnimController.AimSourceSimPos;
        Vector2 barrelPos = _.item.SimPosition + ConvertUnits.ToSimUnits(_.TransformedBarrelPos);
        //make sure there's no obstacles between the base of the item (or the shoulder of the character) and the end of the barrel

        sw.Restart();
        if (Submarine.PickBody(sourcePos, barrelPos, collisionCategory: Physics.CollisionWall | Physics.CollisionLevel | Physics.CollisionItemBlocking) == null)
        {
          //no obstacles -> we start the raycast at the end of the barrel
          rayStart = ConvertUnits.ToSimUnits(_.item.Position + _.TransformedBarrelPos);
          rayStartWorld = ConvertUnits.ToSimUnits(_.item.WorldPosition + _.TransformedBarrelPos);
        }
        else
        {
          rayStart = rayStartWorld = Submarine.LastPickedPosition + Submarine.LastPickedNormal * 0.1f;
          if (_.item.Submarine != null) { rayStartWorld += _.item.Submarine.SimPosition; }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "Submarine.PickBody");

        //if the calculated barrel pos is in another hull, use the origin of the item to make sure the particles don't end up in an incorrect hull
        sw.Restart();
        if (_.item.CurrentHull != null)
        {
          var barrelHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(rayStartWorld), _.item.CurrentHull, useWorldCoordinates: true);
          if (barrelHull != null && barrelHull != _.item.CurrentHull)
          {
            if (MathUtils.GetLineRectangleIntersection(ConvertUnits.ToDisplayUnits(sourcePos), ConvertUnits.ToDisplayUnits(rayStart), _.item.CurrentHull.Rect, out Vector2 hullIntersection))
            {
              if (!_.item.CurrentHull.ConnectedGaps.Any(g => g.Open > 0.0f && Submarine.RectContains(g.Rect, hullIntersection)))
              {
                Vector2 rayDir = rayStart.NearlyEquals(sourcePos) ? Vector2.Zero : Vector2.Normalize(rayStart - sourcePos);
                rayStartWorld = ConvertUnits.ToSimUnits(hullIntersection - rayDir * 5.0f);
                if (_.item.Submarine != null) { rayStartWorld += _.item.Submarine.SimPosition; }
              }
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "FindHull");

        float spread = MathHelper.ToRadians(MathHelper.Lerp(_.UnskilledSpread, _.Spread, degreeOfSuccess));

        float angle = MathHelper.ToRadians(_.BarrelRotation) + spread * Rand.Range(-0.5f, 0.5f);
        float dir = 1;
        if (_.item.body != null)
        {
          angle += _.item.body.Rotation;
          dir = _.item.body.Dir;
        }
        Vector2 rayEnd = rayStartWorld + ConvertUnits.ToSimUnits(new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * _.Range * dir);

        sw.Restart();
        _.ignoredBodies.Clear();
        if (character != null)
        {
          foreach (Limb limb in character.AnimController.Limbs)
          {
            if (Rand.Range(0.0f, 0.5f) > degreeOfSuccess) continue;
            _.ignoredBodies.Add(limb.body.FarseerBody);
          }
          _.ignoredBodies.Add(character.AnimController.Collider.FarseerBody);
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "Find ignoredBodies");

        _.IsActive = true;
        _.activeTimer = 0.1f;

        _.debugRayStartPos = ConvertUnits.ToDisplayUnits(rayStartWorld);
        _.debugRayEndPos = ConvertUnits.ToDisplayUnits(rayEnd);

        sw.Restart();
        Submarine parentSub = character?.Submarine ?? _.item.Submarine;
        if (parentSub == null)
        {
          foreach (Submarine sub in Submarine.Loaded)
          {
            Rectangle subBorders = sub.Borders;
            subBorders.Location += new Point((int)sub.WorldPosition.X, (int)sub.WorldPosition.Y - sub.Borders.Height);
            if (!MathUtils.CircleIntersectsRectangle(_.item.WorldPosition, _.Range * 5.0f, subBorders))
            {
              continue;
            }
            _.Repair(rayStartWorld - sub.SimPosition, rayEnd - sub.SimPosition, deltaTime, character, degreeOfSuccess, _.ignoredBodies);
          }
          _.Repair(rayStartWorld, rayEnd, deltaTime, character, degreeOfSuccess, _.ignoredBodies);
        }
        else
        {
          _.Repair(rayStartWorld - parentSub.SimPosition, rayEnd - parentSub.SimPosition, deltaTime, character, degreeOfSuccess, _.ignoredBodies);
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "Repair");

        //TODO test in multiplayer, this is probably not compiled on server side
        sw.Restart();
        _.UseProjSpecific(deltaTime, rayStartWorld);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairToolState, "UseProjSpecific");

        __result = true; return false;
      }


    }
  }
}