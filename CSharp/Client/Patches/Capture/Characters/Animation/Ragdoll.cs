using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using LimbParams = Barotrauma.RagdollParams.LimbParams;
using JointParams = Barotrauma.RagdollParams.JointParams;
using MoonSharp.Interpreter;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class RagdollPatch
    {
      public static CaptureState UpdateRagdoll;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Ragdoll).GetMethod("UpdateRagdoll", AccessTools.all),
          prefix: new HarmonyMethod(typeof(RagdollPatch).GetMethod("Ragdoll_UpdateRagdoll_Replace"))
        );

        UpdateRagdoll = Capture.Get("Showperf.Update.Ragdolls");
      }

      public static void CaptureRagdoll(Ragdoll ragdoll, long ticks, string name)
      {
        string info = ragdoll.character?.Info?.DisplayName ?? ragdoll.character.ToString();
        if (UpdateRagdoll.ByID)
        {
          Capture.Update.AddTicks(ticks, UpdateRagdoll, $"{info} {name}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, UpdateRagdoll, name);
        }
      }

      public static bool Ragdoll_UpdateRagdoll_Replace(float deltaTime, Camera cam, Ragdoll __instance)
      {
        if (!UpdateRagdoll.IsActive || !Showperf.Revealed) return true;
        Capture.Update.EnsureCategory(UpdateRagdoll);

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        Ragdoll _ = __instance;

        if (!_.character.Enabled || _.character.Removed || _.Frozen || _.Invalid || _.Collider == null || _.Collider.Removed) { return false; }

        sw.Restart();
        while (_.impactQueue.Count > 0)
        {
          var impact = _.impactQueue.Dequeue();
          _.ApplyImpact(impact.F1, impact.F2, impact.LocalNormal, impact.ImpactPos, impact.Velocity);
        }
        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "Apply Impacts");
        sw.Restart();

        sw2.Restart();

        _.CheckValidity();
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "CheckValidity");
        sw2.Restart();

        _.UpdateNetPlayerPosition(deltaTime);
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "UpdateNetPlayerPosition");
        sw2.Restart();

        _.CheckDistFromCollider();
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "CheckDistFromCollider");
        sw2.Restart();

        _.UpdateCollisionCategories();
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "UpdateCollisionCategories");
        sw2.Restart();

        _.FindHull();
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "FindHull");
        sw2.Restart();

        _.PreventOutsideCollision();
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "PreventOutsideCollision");
        sw2.Restart();

        _.CheckBodyInRest(deltaTime);
        sw2.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "CheckBodyInRest");

        _.splashSoundTimer -= deltaTime;

        sw.Stop();
        //Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateRagdoll, "Checks");
        sw.Restart();

        if (_.character.Submarine == null && Level.Loaded != null)
        {
          if (_.Collider.SimPosition.Y > Level.Loaded.TopBarrier.Position.Y)
          {
            _.Collider.LinearVelocity = new Vector2(_.Collider.LinearVelocity.X, Math.Min(_.Collider.LinearVelocity.Y, -1));
          }
          else if (_.Collider.SimPosition.Y < Level.Loaded.BottomBarrier.Position.Y)
          {
            _.Collider.LinearVelocity = new Vector2(_.Collider.LinearVelocity.X,
                MathHelper.Clamp(_.Collider.LinearVelocity.Y, Level.Loaded.BottomBarrier.Position.Y - _.Collider.SimPosition.Y, 10.0f));
          }
          foreach (Limb limb in _.Limbs)
          {
            if (limb.SimPosition.Y > Level.Loaded.TopBarrier.Position.Y)
            {
              limb.body.LinearVelocity = new Vector2(limb.LinearVelocity.X, Math.Min(limb.LinearVelocity.Y, -1));
            }
            else if (limb.SimPosition.Y < Level.Loaded.BottomBarrier.Position.Y)
            {
              limb.body.LinearVelocity = new Vector2(
                  limb.LinearVelocity.X,
                  MathHelper.Clamp(limb.LinearVelocity.Y, Level.Loaded.BottomBarrier.Position.Y - limb.SimPosition.Y, 10.0f));
            }
          }
        }

        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "Check Out ouf bounds");
        sw.Restart();

        if (_.forceStanding)
        {
          _.inWater = false;
          _.headInWater = false;
          _.RefreshFloorY(deltaTime, ignoreStairs: _.Stairs == null);
        }
        //ragdoll isn't in any room -> it's in the water
        else if (_.currentHull == null)
        {
          _.inWater = true;
          _.headInWater = true;
        }
        else
        {
          _.headInWater = false;
          _.inWater = false;
          _.RefreshFloorY(deltaTime, ignoreStairs: _.Stairs == null);
          if (_.currentHull.WaterPercentage > 0.001f)
          {
            (float waterSurfaceDisplayUnits, float ceilingDisplayUnits) = _.GetWaterSurfaceAndCeilingY();
            float waterSurfaceY = ConvertUnits.ToSimUnits(waterSurfaceDisplayUnits);
            float ceilingY = ConvertUnits.ToSimUnits(ceilingDisplayUnits);
            if (_.targetMovement.Y < 0.0f)
            {
              Vector2 colliderBottom = _.GetColliderBottom();
              _.floorY = Math.Min(colliderBottom.Y, _.floorY);
              //check if the bottom of the collider is below the current hull
              if (_.floorY < ConvertUnits.ToSimUnits(_.currentHull.Rect.Y - _.currentHull.Rect.Height))
              {
                //set floorY to the position of the floor in the hull below the character
                var lowerHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(colliderBottom), useWorldCoordinates: false);
                if (lowerHull != null)
                {
                  _.floorY = ConvertUnits.ToSimUnits(lowerHull.Rect.Y - lowerHull.Rect.Height);
                }
              }
            }
            float standHeight = _.HeadPosition ?? _.TorsoPosition ?? _.Collider.GetMaxExtent() * 0.5f;
            if (_.Collider.SimPosition.Y < waterSurfaceY)
            {
              //too deep to stand up, or not enough room to stand up
              if (waterSurfaceY - _.floorY > standHeight * 0.8f ||
                  ceilingY - _.floorY < standHeight * 0.8f)
              {
                _.inWater = true;
              }
            }
          }
        }

        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "Update inWater");
        sw.Restart();

        _.UpdateHullFlowForces(deltaTime);

        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "UpdateHullFlowForces");
        sw.Restart();

        if (_.currentHull == null ||
            _.currentHull.WaterVolume > _.currentHull.Volume * 0.95f ||
            ConvertUnits.ToSimUnits(_.currentHull.Surface) > _.Collider.SimPosition.Y)
        {
          _.Collider.ApplyWaterForces();
        }

        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "ApplyWaterForces");
        sw.Restart();

        foreach (Limb limb in _.Limbs)
        {
          //find the room which the limb is in
          //the room where the ragdoll is in is used as the "guess", meaning that it's checked first                
          Hull newHull = _.currentHull == null ? null : Hull.FindHull(limb.WorldPosition, _.currentHull);

          bool prevInWater = limb.InWater;
          limb.InWater = false;

          if (_.forceStanding)
          {
            limb.InWater = false;
          }
          else if (newHull == null)
          {
            //limb isn't in any room -> it's in the water
            limb.InWater = true;
            if (limb.type == LimbType.Head) { _.headInWater = true; }
          }
          else if (newHull.WaterVolume > 0.0f && Submarine.RectContains(newHull.Rect, limb.Position))
          {
            if (limb.Position.Y < newHull.Surface)
            {
              limb.InWater = true;
              _.surfaceY = newHull.Surface;
              if (limb.type == LimbType.Head)
              {
                _.headInWater = true;
              }
            }
            //the limb has gone through the surface of the water
            if (Math.Abs(limb.LinearVelocity.Y) > 5.0f && limb.InWater != prevInWater && newHull == limb.Hull)
            {
#if CLIENT
              _.Splash(limb, newHull);
#endif

              //if the Character dropped into water, create a wave
              if (limb.LinearVelocity.Y < 0.0f)
              {
                Vector2 impulse = limb.LinearVelocity * limb.Mass;
                int n = (int)((limb.Position.X - newHull.Rect.X) / Hull.WaveWidth);
                newHull.WaveVel[n] += MathHelper.Clamp(impulse.Y, -5.0f, 5.0f);
              }
            }
          }
          limb.Hull = newHull;
          limb.Update(deltaTime);
        }

        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "limb.Update");
        sw.Restart();

        bool isAttachedToController =
            _.character.SelectedItem?.GetComponent<Barotrauma.Items.Components.Controller>() is { } controller &&
            controller.User == _.character &&
            controller.IsAttachedUser(controller.User);

        if (!_.inWater && _.character.AllowInput && _.levitatingCollider && !isAttachedToController)
        {
          if (_.onGround && _.Collider.LinearVelocity.Y > -_.ImpactTolerance)
          {
            float targetY = _.standOnFloorY + ((float)Math.Abs(Math.Cos(_.Collider.Rotation)) * _.Collider.Height * 0.5f) + _.Collider.Radius + _.ColliderHeightFromFloor;

            const float LevitationSpeedMultiplier = 5f;

            // If the character is walking down a slope, target a position that moves along it
            float slopePull = 0f;
            if (_.floorNormal.Y is > 0f and < 1f
                && Math.Sign(_.movement.X) == Math.Sign(_.floorNormal.X))
            {
              float steepness = Math.Abs(_.floorNormal.X);
              slopePull = Math.Abs(_.movement.X * steepness) / LevitationSpeedMultiplier;
            }

            if (Math.Abs(_.Collider.SimPosition.Y - targetY - slopePull) > 0.01f)
            {
              float yVelocity = (targetY - _.Collider.SimPosition.Y) * LevitationSpeedMultiplier;
              if (_.Stairs != null && targetY < _.Collider.SimPosition.Y)
              {
                yVelocity = Math.Sign(yVelocity);
              }

              yVelocity -= slopePull * LevitationSpeedMultiplier;

              _.Collider.LinearVelocity = new Vector2(_.Collider.LinearVelocity.X, yVelocity);
            }
          }
          else
          {
            // Falling -> ragdoll briefly if we are not moving at all, because we are probably stuck.
            if (_.Collider.LinearVelocity == Vector2.Zero)
            {
              _.character.IsRagdolled = true;
              if (_.character.IsBot)
              {
                // Seems to work without this on player controlled characters -> not sure if we should call it always or just for the bots.
                _.character.SetInput(InputType.Ragdoll, hit: false, held: true);
              }
            }
          }
        }

        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "levitatingCollider");
        sw.Restart();

#if CLIENT
        _.UpdateProjSpecific(deltaTime, cam);
#endif
        sw.Stop();
        CaptureRagdoll(_, sw.ElapsedTicks, "Update sprite deformations");
        sw.Restart();

        _.forceNotStanding = false;

        return false;
      }
    }
  }
}