using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Barotrauma.Items.Components;
using Barotrauma.Extensions;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public static bool LevelObjectManager_DrawObjects_Replace(SpriteBatch spriteBatch, Camera cam, List<LevelObject> objectList, LevelObjectManager __instance)
    {
      if (ActiveCategory != ShowperfCategory.LevelObjectsDrawing) return true;
      Window.ensureCategory(CaptureCategory.LevelObjectsDrawing);

      LevelObjectManager _ = __instance;



      Rectangle indices = Rectangle.Empty;
      indices.X = (int)Math.Floor(cam.WorldView.X / (float)LevelObjectManager.GridSize);
      if (indices.X >= _.objectGrid.GetLength(0)) { return false; }
      indices.Y = (int)Math.Floor((cam.WorldView.Y - cam.WorldView.Height - Level.Loaded.BottomPos) / (float)LevelObjectManager.GridSize);
      if (indices.Y >= _.objectGrid.GetLength(1)) { return false; }

      indices.Width = (int)Math.Floor(cam.WorldView.Right / (float)LevelObjectManager.GridSize) + 1;
      if (indices.Width < 0) { return false; }
      indices.Height = (int)Math.Floor((cam.WorldView.Y - Level.Loaded.BottomPos) / (float)LevelObjectManager.GridSize) + 1;
      if (indices.Height < 0) { return false; }

      indices.X = Math.Max(indices.X, 0);
      indices.Y = Math.Max(indices.Y, 0);
      indices.Width = Math.Min(indices.Width, _.objectGrid.GetLength(0) - 1);
      indices.Height = Math.Min(indices.Height, _.objectGrid.GetLength(1) - 1);

      float z = 0.0f;
      if (_.ForceRefreshVisibleObjects || (_.currentGridIndices != indices && Timing.TotalTime > _.NextRefreshTime))
      {
        _.RefreshVisibleObjects(indices, cam.Zoom);
        _.ForceRefreshVisibleObjects = false;
        if (cam.Zoom < 0.1f)
        {
          //when zoomed very far out, refresh a little less often
          _.NextRefreshTime = Timing.TotalTime + MathHelper.Lerp(1.0f, 0.0f, cam.Zoom * 10.0f);
        }
      }

      var sw = new System.Diagnostics.Stopwatch();

      foreach (LevelObject obj in objectList)
      {
        sw.Restart();

        Vector2 camDiff = new Vector2(obj.Position.X, obj.Position.Y) - cam.WorldViewCenter;
        camDiff.Y = -camDiff.Y;

        Sprite activeSprite = obj.Sprite;
        activeSprite?.Draw(
            spriteBatch,
            new Vector2(obj.Position.X, -obj.Position.Y) - camDiff * obj.Position.Z * LevelObjectManager.ParallaxStrength,
            Color.Lerp(obj.Prefab.SpriteColor, obj.Prefab.SpriteColor.Multiply(Level.Loaded.BackgroundTextureColor), obj.Position.Z / 3000.0f),
            activeSprite.Origin,
            obj.CurrentRotation,
            obj.CurrentScale,
            SpriteEffects.None,
            z);

        if (obj.ActivePrefab.DeformableSprite != null)
        {
          if (obj.CurrentSpriteDeformation != null)
          {
            obj.ActivePrefab.DeformableSprite.Deform(obj.CurrentSpriteDeformation);
          }
          else
          {
            obj.ActivePrefab.DeformableSprite.Reset();
          }
          obj.ActivePrefab.DeformableSprite?.Draw(cam,
              new Vector3(new Vector2(obj.Position.X, obj.Position.Y) - camDiff * obj.Position.Z * LevelObjectManager.ParallaxStrength, z * 10.0f),
              obj.ActivePrefab.DeformableSprite.Origin,
              obj.CurrentRotation,
              obj.CurrentScale,
              Color.Lerp(obj.Prefab.SpriteColor, obj.Prefab.SpriteColor.Multiply(Level.Loaded.BackgroundTextureColor), obj.Position.Z / 5000.0f));
        }

        // idk if levelobjects without prefab actually exist
        if (obj.Prefab != null)
        {
          Window.tryAddTicks(obj.Prefab.Identifier, CaptureCategory.LevelObjectsDrawing, sw.ElapsedTicks);
        }
        else
        {
          string s = $"{obj}";
          Window.tryAddTicks(s, CaptureCategory.LevelObjectsDrawing, sw.ElapsedTicks);
        }

        if (GameMain.DebugDraw)
        {
          GUI.DrawRectangle(spriteBatch, new Vector2(obj.Position.X, -obj.Position.Y), new Vector2(10.0f, 10.0f), GUIStyle.Red, true);

          if (obj.Triggers == null) { continue; }
          foreach (LevelTrigger trigger in obj.Triggers)
          {
            if (trigger.PhysicsBody == null) continue;
            GUI.DrawLine(spriteBatch, new Vector2(obj.Position.X, -obj.Position.Y), new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y), Color.Cyan, 0, 3);

            Vector2 flowForce = trigger.GetWaterFlowVelocity();
            if (flowForce.LengthSquared() > 1)
            {
              flowForce.Y = -flowForce.Y;
              GUI.DrawLine(spriteBatch, new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y), new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y) + flowForce * 10, GUIStyle.Orange, 0, 5);
            }
            trigger.PhysicsBody.UpdateDrawPosition();
            trigger.PhysicsBody.DebugDraw(spriteBatch, trigger.IsTriggered ? Color.Cyan : Color.DarkCyan);
          }
        }

        z += 0.0001f;


      }

      sw.Stop();

      return false;
    }
  }
}