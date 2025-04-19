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
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class MotionSensorPatch
    {
      public static CaptureState CaptureMotionSensor;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(MotionSensor).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(MotionSensorPatch).GetMethod("MotionSensor_Update_Replace"))
        );

        CaptureMotionSensor = Capture.Get("Showperf.Update.MapEntity.Items.Components.MotionSensor");
      }

      public static bool MotionSensor_Update_Replace(float deltaTime, Camera cam, MotionSensor __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !CaptureMotionSensor.IsActive) return true;
        Capture.Update.EnsureCategory(CaptureMotionSensor);

        Stopwatch sw = new Stopwatch();

        MotionSensor _ = __instance;

        sw.Restart();
        string signalOut = _.MotionDetected ? _.Output : _.FalseOutput;

        if (!string.IsNullOrEmpty(signalOut)) { _.item.SendSignal(new Signal(signalOut, 1), "state_out"); }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "SendSignal");

        sw.Restart();
        if (_.MotionDetected)
        {
          _.ApplyStatusEffects(ActionType.OnUse, deltaTime);
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "ApplyStatusEffects(ActionType.OnUse");

        _.updateTimer -= deltaTime;
        if (_.updateTimer > 0.0f) { return false; }

        _.MotionDetected = false;
        _.updateTimer = _.UpdateInterval;

        sw.Restart();
        if (_.item.body != null && _.item.body.Enabled && _.DetectOwnMotion)
        {
          if (Math.Abs(_.item.body.LinearVelocity.X) > _.MinimumVelocity || Math.Abs(_.item.body.LinearVelocity.Y) > _.MinimumVelocity)
          {
            _.MotionDetected = true;
            sw.Stop();
            Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "DetectOwnMotion");
            return false;
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "DetectOwnMotion");

        Vector2 detectPos = _.item.WorldPosition + _.TransformedDetectOffset;
        Rectangle detectRect = new Rectangle((int)(detectPos.X - _.rangeX), (int)(detectPos.Y - _.rangeY), (int)(_.rangeX * 2), (int)(_.rangeY * 2));
        float broadRangeX = Math.Max(_.rangeX * 2, 500);
        float broadRangeY = Math.Max(_.rangeY * 2, 500);


        if (_.item.CurrentHull == null && _.item.Submarine != null && _.Target.HasFlag(MotionSensor.TargetType.Wall))
        {

          sw.Restart();
          if (Level.Loaded != null && (Math.Abs(_.item.Submarine.Velocity.X) > _.MinimumVelocity || Math.Abs(_.item.Submarine.Velocity.Y) > _.MinimumVelocity))
          {
            var cells = Level.Loaded.GetCells(_.item.WorldPosition, 1);
            foreach (var cell in cells)
            {
              if (cell.IsPointInside(_.item.WorldPosition))
              {
                _.MotionDetected = true;

                sw.Stop();
                Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect level cells");
                return false;
              }
              foreach (var edge in cell.Edges)
              {
                Vector2 e1 = edge.Point1 + cell.Translation;
                Vector2 e2 = edge.Point2 + cell.Translation;
                if (MathUtils.LineSegmentsIntersect(e1, e2, new Vector2(detectRect.X, detectRect.Y), new Vector2(detectRect.Right, detectRect.Y)) ||
                    MathUtils.LineSegmentsIntersect(e1, e2, new Vector2(detectRect.X, detectRect.Bottom), new Vector2(detectRect.Right, detectRect.Bottom)) ||
                    MathUtils.LineSegmentsIntersect(e1, e2, new Vector2(detectRect.X, detectRect.Y), new Vector2(detectRect.X, detectRect.Bottom)) ||
                    MathUtils.LineSegmentsIntersect(e1, e2, new Vector2(detectRect.Right, detectRect.Y), new Vector2(detectRect.Right, detectRect.Bottom)))
                {
                  _.MotionDetected = true;

                  sw.Stop();
                  Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect level cells");
                  return false;
                }
              }
            }
          }
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect level cells");

          sw.Restart();
          foreach (Submarine sub in Submarine.Loaded)
          {
            if (sub == _.item.Submarine) { continue; }

            Vector2 relativeVelocity = _.item.Submarine.Velocity - sub.Velocity;
            if (Math.Abs(relativeVelocity.X) < _.MinimumVelocity && Math.Abs(relativeVelocity.Y) < _.MinimumVelocity) { continue; }

            Rectangle worldBorders = new Rectangle(
                sub.Borders.X + (int)sub.WorldPosition.X,
                sub.Borders.Y + (int)sub.WorldPosition.Y - sub.Borders.Height,
                sub.Borders.Width,
                sub.Borders.Height);

            if (worldBorders.Intersects(detectRect))
            {
              foreach (Structure wall in Structure.WallList)
              {
                if (wall.Submarine == sub &&
                    wall.WorldRect.Intersects(detectRect))
                {
                  _.MotionDetected = true;
                  sw.Stop();
                  Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect sub walls");
                  return false;
                }
              }
            }
          }
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect sub walls");
        }


        bool hasTriggers = _.triggerFromHumans || _.triggerFromPets || _.triggerFromMonsters;
        if (!hasTriggers) { return false; }

        sw.Restart();
        foreach (Character character in Character.CharacterList)
        {
          //ignore characters that have spawned a second or less ago
          //makes it possible to detect when a spawned character moves without triggering the detector immediately as the ragdoll spawns and drops to the ground
          if (character.SpawnTime > Timing.TotalTime - 1.0) { continue; }

          if (!_.TriggersOn(character)) { continue; }

          //do a rough check based on the position of the character's collider first
          //before the more accurate limb-based check
          if (Math.Abs(character.WorldPosition.X - detectPos.X) > broadRangeX || Math.Abs(character.WorldPosition.Y - detectPos.Y) > broadRangeY)
          {
            continue;
          }

          foreach (Limb limb in character.AnimController.Limbs)
          {
            if (limb.IsSevered) { continue; }
            if (limb.LinearVelocity.LengthSquared() < _.MinimumVelocity * _.MinimumVelocity) { continue; }
            if (MathUtils.CircleIntersectsRectangle(limb.WorldPosition, ConvertUnits.ToDisplayUnits(limb.body.GetMaxExtent()), detectRect))
            {
              _.MotionDetected = true;
              sw.Stop();
              Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect Characters");
              return false;
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, CaptureMotionSensor, "Detect Characters");

        return false;
      }


    }
  }
}