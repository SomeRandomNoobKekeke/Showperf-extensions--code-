using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

#if CLIENT
using Barotrauma.Particles;
#endif
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Voronoi2;
using Barotrauma.Extensions;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class LevelObjectManagerPatch
    {
      public static CaptureState LevelObjects;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LevelObjectManager).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(LevelObjectManagerPatch).GetMethod("LevelObjectManager_Update_Replace"))
        );

        LevelObjects = Capture.Get("Showperf.Update.Level.LevelObjectManager");
      }


      public static void DrawObjects(CaptureState cs, LevelObjectManager _, SpriteBatch spriteBatch, Camera cam, BackgroundCreatureManager backgroundCreatureManager, List<ILevelRenderableObject> objectList = null)
      {
        if (_ == null) return;

        if (!cs.IsActive)
        {
          _.DrawObjects(spriteBatch, cam, backgroundCreatureManager, objectList);
          return;
        }

        Capture.Draw.EnsureCategory(cs);
        LevelObjectManager_DrawObjects_Alt(cs, _, spriteBatch, cam, backgroundCreatureManager, objectList);
      }

      public static bool LevelObjectManager_DrawObjects_Alt(CaptureState cs, LevelObjectManager __instance, SpriteBatch spriteBatch, Camera cam, BackgroundCreatureManager backgroundCreatureManager, List<ILevelRenderableObject> objectList)
      {
        LevelObjectManager _ = __instance;

        Stopwatch sw = new Stopwatch();
        Color HighlightColor = Color.Yellow;

        sw.Restart();

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
          _.RefreshVisibleObjects(indices, backgroundCreatureManager, cam.Zoom);
          _.ForceRefreshVisibleObjects = false;
          if (cam.Zoom < 0.1f)
          {
            //when zoomed very far out, refresh a little less often
            _.NextRefreshTime = Timing.TotalTime + MathHelper.Lerp(1.0f, 0.0f, cam.Zoom * 10.0f);
          }
        }

        sw.Stop();
        Capture.Draw.AddTicks(sw.ElapsedTicks, cs, "RefreshVisibleObjects");

        bool prevObjectHasDeformableSprite = false;
        foreach (ILevelRenderableObject obj2 in objectList)
        {
          sw.Restart();
          Vector2 camDiff = new Vector2(obj2.Position.X, obj2.Position.Y) - cam.WorldViewCenter;
          camDiff.Y = -camDiff.Y;

          bool hasDeformableSprite = false;
          if (obj2 is LevelObject levelObject)
          {
            hasDeformableSprite = levelObject.ActivePrefab.DeformableSprite != null;
            if (hasDeformableSprite != prevObjectHasDeformableSprite)
            {
              spriteBatch.End();
              spriteBatch.Begin(SpriteSortMode.Deferred,
                  BlendState.NonPremultiplied,
                  SamplerState.LinearWrap, DepthStencilState.DepthRead,
                  transformMatrix: cam.Transform);
            }

            Sprite activeSprite = levelObject.Sprite;
            activeSprite?.Draw(
                spriteBatch,
                new Vector2(levelObject.Position.X, -levelObject.Position.Y) - camDiff * levelObject.Position.Z * LevelObjectManager.ParallaxStrength,
                Color.Lerp(levelObject.Prefab.SpriteColor, levelObject.Prefab.SpriteColor.Multiply(Level.Loaded.BackgroundTextureColor), levelObject.Position.Z / levelObject.Prefab.FadeOutDepth),
                activeSprite.Origin,
                levelObject.CurrentRotation,
                levelObject.CurrentScale,
                SpriteEffects.None,
                z);

            if (hasDeformableSprite)
            {
              if (levelObject.CurrentSpriteDeformation != null)
              {
                levelObject.ActivePrefab.DeformableSprite.Deform(levelObject.CurrentSpriteDeformation);
              }
              else
              {
                levelObject.ActivePrefab.DeformableSprite.Reset();
              }
              levelObject.ActivePrefab.DeformableSprite?.Draw(cam,
                  new Vector3(new Vector2(levelObject.Position.X, levelObject.Position.Y) - camDiff * levelObject.Position.Z * LevelObjectManager.ParallaxStrength, z * 10.0f),
                  levelObject.ActivePrefab.DeformableSprite.Origin,
                  levelObject.CurrentRotation,
                  levelObject.CurrentScale,
                  Color.Lerp(levelObject.Prefab.SpriteColor, levelObject.Prefab.SpriteColor.Multiply(Level.Loaded.BackgroundTextureColor), levelObject.Position.Z / 5000.0f));
            }
            prevObjectHasDeformableSprite = hasDeformableSprite;

            if (GameMain.DebugDraw)
            {
              GUI.DrawRectangle(spriteBatch, new Vector2(levelObject.Position.X, -levelObject.Position.Y), new Vector2(10.0f, 10.0f), GUIStyle.Red, true);

              if (levelObject.Triggers == null) { continue; }
              foreach (LevelTrigger trigger in levelObject.Triggers)
              {
                if (trigger.PhysicsBody == null) continue;
                GUI.DrawLine(spriteBatch, new Vector2(levelObject.Position.X, -levelObject.Position.Y), new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y), Color.Cyan, 0, 3);

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

            sw.Stop();
            Capture.Draw.AddTicks(sw.ElapsedTicks, cs, levelObject.ToString());
          }
          else if (obj2 is BackgroundCreature backgroundCreature && cam.Zoom > 0.05f)
          {
            hasDeformableSprite = backgroundCreature.Prefab.DeformableSprite != null;
            if (hasDeformableSprite != prevObjectHasDeformableSprite)
            {
              spriteBatch.End();
              spriteBatch.Begin(SpriteSortMode.Deferred,
                  BlendState.NonPremultiplied,
                  SamplerState.LinearWrap, DepthStencilState.DepthRead,
                  transformMatrix: cam.Transform);
            }

            backgroundCreature.Draw(spriteBatch, cam);

            sw.Stop();
            Capture.Draw.AddTicks(sw.ElapsedTicks, cs, backgroundCreature.Prefab.Name);
          }
          prevObjectHasDeformableSprite = hasDeformableSprite;


          z += 0.0001f;
        }

        return false;
      }


      public static bool LevelObjectManager_Update_Replace(LevelObjectManager __instance, float deltaTime, Camera cam)
      {
        if (Showperf == null || !Showperf.Revealed || !LevelObjects.IsActive) return true;

        LevelObjectManager _ = __instance;

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        Capture.Update.EnsureCategory(LevelObjects);

        _.GlobalForceDecreaseTimer += deltaTime;
        if (_.GlobalForceDecreaseTimer > 1000000.0f)
        {
          _.GlobalForceDecreaseTimer = 0.0f;
        }

        if (_.updateableObjects is not null)
        {
          foreach (LevelObject obj in _.updateableObjects)
          {
            if (GameMain.NetworkMember is { IsServer: true })
            {
              obj.NetworkUpdateTimer -= deltaTime;
              if (obj.NeedsNetworkSyncing && obj.NetworkUpdateTimer <= 0.0f)
              {
                GameMain.NetworkMember.CreateEntityEvent(_, new LevelObjectManager.EventData(obj));
                obj.NeedsNetworkSyncing = false;
                obj.NetworkUpdateTimer = NetConfig.LevelObjectUpdateInterval;
              }
            }
            if (obj.Prefab.HideWhenBroken && obj.Health <= 0.0f) { continue; }

            long tiggersUpdateTicks = 0;
            sw.Restart();
            if (obj.Triggers != null)
            {
              obj.ActivePrefab = obj.Prefab;
              for (int i = 0; i < obj.Triggers.Count; i++)
              {
                sw2.Restart();
                obj.Triggers[i].Update(deltaTime);
                sw2.Stop();
                tiggersUpdateTicks += sw2.ElapsedTicks;

                if (obj.Triggers[i].IsTriggered && obj.Prefab.OverrideProperties[i] != null)
                {
                  obj.ActivePrefab = obj.Prefab.OverrideProperties[i];
                }
              }
            }
            sw.Stop();
            if (LevelObjects.ByID)
            {
              Capture.Update.AddTicks(tiggersUpdateTicks, LevelObjects, $"{obj}.Triggers");
              Capture.Update.AddTicks(sw.ElapsedTicks - tiggersUpdateTicks, LevelObjects, $"{obj}.Triggers prefab juggling");
            }
            else
            {
              Capture.Update.AddTicks(tiggersUpdateTicks, LevelObjects, $"Triggers");
              Capture.Update.AddTicks(sw.ElapsedTicks - tiggersUpdateTicks, LevelObjects, $"Triggers prefab juggling");
            }

            if (obj.PhysicsBody != null)
            {
              if (obj.Prefab.PhysicsBodyTriggerIndex > -1) { obj.PhysicsBody.Enabled = obj.Triggers[obj.Prefab.PhysicsBodyTriggerIndex].IsTriggered; }
              /*obj.Position = new Vector3(obj.PhysicsBody.Position, obj.Position.Z);
              obj.Rotation = -obj.PhysicsBody.Rotation;*/
            }
          }
        }

#if CLIENT
        _.UpdateProjSpecific(deltaTime,cam);
#endif

        return false;
      }
    }
  }
}