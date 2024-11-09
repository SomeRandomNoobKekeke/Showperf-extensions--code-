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

using Barotrauma.Extensions;
using Barotrauma.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class GameScreenPatch
    {
      public static CaptureState Update;
      public static CaptureState Draw;

      public static CaptureState DrawHUD;
      public static CaptureState DrawMap;

      public static CaptureState BackCharactersItems;
      public static CaptureState BackCharactersItemsDrawCharacters;
      public static CaptureState BackCharactersItemsSubmarineDrawBack;
      public static CaptureState BackLevel;
      public static CaptureState BackLevelParticles;
      public static CaptureState BackLevelSubmarine;
      public static CaptureState BackStructures;
      public static CaptureState DeformableCharacters;
      public static CaptureState FrontDamageable;
      public static CaptureState FrontLevel;
      public static CaptureState FrontMisc;
      public static CaptureState FrontParticles;
      public static CaptureState FrontStructuresItems;
      public static CaptureState LOS;
      public static CaptureState Lighting;
      public static CaptureState PostProcess;


      public static CaptureState UpdateLightManager;
      public static CaptureState UpdatePhysicBodies;
      public static CaptureState UpdateItemsHUD;
      public static CaptureState UpdateCharacter;
      public static CaptureState UpdateGameSession;
      public static CaptureState UpdateLevel;
      public static CaptureState UpdateMapEntity;
      public static CaptureState UpdateParticles;
      public static CaptureState UpdatePhysics;
      public static CaptureState UpdateRagdolls;
      public static CaptureState UpdateAnimations;
      public static CaptureState UpdateStatusEffects;
      public static CaptureState UpdateCameraAndCursor;
      public static CaptureState UpdateSetPrevTransform;
      public static CaptureState UpdateSubmarine;



      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(GameScreen).GetMethod("Draw", AccessTools.all),
          prefix: ShowperfMethod(typeof(GameScreenPatch).GetMethod("GameScreen_Draw_Replace"))
        );

        harmony.Patch(
          original: typeof(GameScreen).GetMethod("DrawMap", AccessTools.all),
          prefix: ShowperfMethod(typeof(GameScreenPatch).GetMethod("GameScreen_DrawMap_Replace"))
        );

        harmony.Patch(
          original: typeof(GameScreen).GetMethod("Update", AccessTools.all),
          prefix: ShowperfMethod(typeof(GameScreenPatch).GetMethod("GameScreen_Update_Replace"))
        );

        Update = Capture.Get("Showperf.Update");
        Draw = Capture.Get("Showperf.Draw");

        DrawHUD = Capture.Get("Showperf.Draw.HUD");
        DrawMap = Capture.Get("Showperf.Draw.Map");

        BackCharactersItems = Capture.Get("Showperf.Draw.Map.BackCharactersItems");
        BackCharactersItemsDrawCharacters = Capture.Get("Showperf.Draw.Map.BackCharactersItems.DrawCharacters");
        BackCharactersItemsSubmarineDrawBack = Capture.Get("Showperf.Draw.Map.BackCharactersItems.SubmarineDrawBack");
        BackLevel = Capture.Get("Showperf.Draw.Map.BackLevel");
        BackLevelParticles = Capture.Get("Showperf.Draw.Map.BackLevel.Particles");
        BackLevelSubmarine = Capture.Get("Showperf.Draw.Map.BackLevel.SubmarineDrawBack");
        BackStructures = Capture.Get("Showperf.Draw.Map.BackStructures");
        DeformableCharacters = Capture.Get("Showperf.Draw.Map.DeformableCharacters");
        FrontDamageable = Capture.Get("Showperf.Draw.Map.FrontDamageable");
        FrontLevel = Capture.Get("Showperf.Draw.Map.FrontLevel");
        FrontMisc = Capture.Get("Showperf.Draw.Map.FrontMisc");
        FrontParticles = Capture.Get("Showperf.Draw.Map.FrontParticles");
        FrontStructuresItems = Capture.Get("Showperf.Draw.Map.FrontStructuresItems");
        LOS = Capture.Get("Showperf.Draw.Map.LOS");
        Lighting = Capture.Get("Showperf.Draw.Map.Lighting");
        PostProcess = Capture.Get("Showperf.Draw.Map.PostProcess");


        UpdateLightManager = Capture.Get("Showperf.Update.LightManager");
        UpdatePhysicBodies = Capture.Get("Showperf.Update.PhysicBodies");
        UpdateItemsHUD = Capture.Get("Showperf.Update.ItemsHUD");
        UpdateCharacter = Capture.Get("Showperf.Update.Character");
        UpdateGameSession = Capture.Get("Showperf.Update.GameSession");
        UpdateLevel = Capture.Get("Showperf.Update.Level");
        UpdateMapEntity = Capture.Get("Showperf.Update.MapEntity");
        UpdateParticles = Capture.Get("Showperf.Update.Particles");
        UpdatePhysics = Capture.Get("Showperf.Update.Physics");
        UpdateRagdolls = Capture.Get("Showperf.Update.Ragdolls");
        UpdateAnimations = Capture.Get("Showperf.Update.Animations");
        UpdateStatusEffects = Capture.Get("Showperf.Update.StatusEffects");
        UpdateCameraAndCursor = Capture.Get("Showperf.Update.CameraAndCursor");
        UpdateSetPrevTransform = Capture.Get("Showperf.Update.SetPrevTransform");
        UpdateSubmarine = Capture.Get("Showperf.Update.Submarine");


      }




      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaClient/ClientSource/Screens/GameScreen.cs#L98
      public static bool GameScreen_Draw_Replace(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch, GameScreen __instance)
      {
        if (!Showperf.Revealed) return true;

        Stopwatch sw = new Stopwatch();

        GameScreen _ = __instance;

        _.cam.UpdateTransform(true);
        Submarine.CullEntities(_.cam);

        foreach (Character c in Character.CharacterList)
        {
          c.AnimController.Limbs.ForEach(l => l.body.UpdateDrawPosition());
          bool wasVisible = c.IsVisible;
          c.DoVisibilityCheck(_.cam);
          if (c.IsVisible != wasVisible)
          {
            foreach (var limb in c.AnimController.Limbs)
            {
              if (limb.LightSource is LightSource light)
              {
                light.Enabled = c.IsVisible;
              }
            }
          }
        }


        sw.Start();

        _.DrawMap(graphics, spriteBatch, deltaTime);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "Draw.Map");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, Draw, "Draw.Map");
        sw.Restart();

        spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable);

        if (Character.Controlled != null && _.cam != null) { Character.Controlled.DrawHUD(spriteBatch, _.cam); }

        if (GameMain.GameSession != null) { GameMain.GameSession.Draw(spriteBatch); }

        if (Character.Controlled == null && !GUI.DisableHUD)
        {
          _.DrawPositionIndicators(spriteBatch);
        }

        if (!GUI.DisableHUD)
        {
          foreach (Character c in Character.CharacterList)
          {
            c.DrawGUIMessages(spriteBatch, _.cam);
          }
        }

        GUI.Draw(_.cam, spriteBatch);

        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:HUD", sw.ElapsedTicks);
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawHUD, "Draw.HUD");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, Draw, "Draw.HUD");
        sw.Restart();

        return false;
      }



      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaClient/ClientSource/Screens/GameScreen.cs#L268
      public static bool GameScreen_DrawMap_Replace(GraphicsDevice graphics, SpriteBatch spriteBatch, double deltaTime, GameScreen __instance)
      {
        if (!Showperf.Revealed) return true;

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        Stopwatch sw3 = new Stopwatch();

        GameScreen _ = __instance;

        foreach (Submarine sub in Submarine.Loaded)
        {
          sub.UpdateTransform();
        }

        GameMain.ParticleManager.UpdateTransforms();


        sw.Start();

        if (Character.Controlled != null &&
            (Character.Controlled.ViewTarget == Character.Controlled || Character.Controlled.ViewTarget == null))
        {
          GameMain.LightManager.ObstructVisionAmount = Character.Controlled.ObstructVisionAmount;
        }
        else
        {
          GameMain.LightManager.ObstructVisionAmount = 0.0f;
        }

        GameMain.LightManager.UpdateObstructVision(graphics, spriteBatch, _.cam, Character.Controlled?.CursorWorldPosition ?? Vector2.Zero);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:LOS", sw.ElapsedTicks);
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, LOS, "LOS");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "LOS");
        sw.Restart();


        static bool IsFromOutpostDrawnBehindSubs(Entity e)
            => e.Submarine is { Info.OutpostGenerationParams.DrawBehindSubs: true };

        //------------------------------------------------------------------------
        graphics.SetRenderTarget(_.renderTarget);
        graphics.Clear(Color.Transparent);
        //Draw background structures and wall background sprites 
        //(= the background texture that's revealed when a wall is destroyed) into the background render target
        //These will be visible through the LOS effect.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
        SubmarinePatch.DrawBack(BackStructures, spriteBatch, false, e => e is Structure s && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && !IsFromOutpostDrawnBehindSubs(e));

        Submarine.DrawPaintedColors(spriteBatch, false);
        spriteBatch.End();


        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackStructures", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, BackStructures, "BackStructures");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "BackStructures");
        sw.Restart();

        graphics.SetRenderTarget(null);
        GameMain.LightManager.RenderLightMap(graphics, spriteBatch, _.cam, _.renderTarget);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:Lighting", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, Lighting, "Lighting");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "Lighting");
        sw.Restart();

        //------------------------------------------------------------------------
        Capture.Draw.EnsureCategory(BackLevel);


        graphics.SetRenderTarget(_.renderTargetBackground);
        if (Level.Loaded == null)
        {
          graphics.Clear(new Color(11, 18, 26, 255));
        }
        else
        {
          //graphics.Clear(new Color(255, 255, 255, 255));
          Level.Loaded.DrawBack(graphics, spriteBatch, _.cam);
        }


        sw2.Restart();
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
        SubmarinePatch.DrawBack(BackLevelSubmarine, spriteBatch, false, e => e is Structure s && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && IsFromOutpostDrawnBehindSubs(e));
        spriteBatch.End();
        sw2.Stop();
        Capture.Draw.AddTicks(sw2.ElapsedTicks, BackLevel, "SubmarinePatch.DrawBack");
        sw2.Restart();
        //draw alpha blended particles that are in water and behind subs
#if LINUX || OSX
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
#else
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
#endif
        ParticleManagerPatch.Draw(BackLevelParticles, spriteBatch, true, false, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
        spriteBatch.End();

        //draw additive particles that are in water and behind subs
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, _.cam.Transform);
        ParticleManagerPatch.Draw(BackLevelParticles, spriteBatch, true, false, Barotrauma.Particles.ParticleBlendState.Additive);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None);
        spriteBatch.Draw(_.renderTarget, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
        spriteBatch.End();

        sw2.Stop();
        Capture.Draw.AddTicks(sw2.ElapsedTicks, BackLevel, "Particles");

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackLevel", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, BackLevel, "BackLevel");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "BackLevel");
        sw.Restart();

        //----------------------------------------------------------------------------

        //Start drawing to the normal render target (stuff that can't be seen through the LOS effect)
        graphics.SetRenderTarget(_.renderTarget);

        graphics.BlendState = BlendState.NonPremultiplied;
        graphics.SamplerStates[0] = SamplerState.LinearWrap;
        GraphicsQuad.UseBasicEffect(_.renderTargetBackground);
        GraphicsQuad.Render();

        //Draw the rest of the structures, characters and front structures
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);

        sw2.Restart();
        SubmarinePatch.DrawBack(BackCharactersItemsSubmarineDrawBack, spriteBatch, false, e => !(e is Structure) || e.SpriteDepth < 0.9f);
        sw2.Stop();
        Capture.Draw.AddTicksOnce(sw2.ElapsedTicks, BackCharactersItems, "Submarine.DrawBack");

        sw2.Restart();
        DrawDrawCharactersProxy(BackCharactersItemsDrawCharacters, deformed: false, firstPass: true);

        sw2.Stop();
        Capture.Draw.AddTicksOnce(sw2.ElapsedTicks, BackCharactersItems, "DrawCharacters(deformed: false, firstPass: true)");

        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackCharactersItems", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, BackCharactersItems, "BackCharactersItems");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "BackCharactersItems");
        sw.Restart();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);

        if (DeformableCharacters.IsActive)
        {
          Capture.Draw.EnsureCategory(DeformableCharacters);

          sw2.Restart();
          DrawCharactersAlt(DeformableCharacters, deformed: true, firstPass: true);
          sw2.Stop();
          Capture.Draw.AddTicks(sw2.ElapsedTicks, DeformableCharacters, "deformed: true, firstPass: true");
          sw2.Restart();
          DrawCharactersAlt(DeformableCharacters, deformed: true, firstPass: false);

          sw2.Stop();
          Capture.Draw.AddTicks(sw2.ElapsedTicks, DeformableCharacters, "deformed: true, firstPass: false");
          sw2.Restart();

          DrawCharactersAlt(DeformableCharacters, deformed: false, firstPass: false);
          sw2.Stop();
          Capture.Draw.AddTicks(sw2.ElapsedTicks, DeformableCharacters, "deformed: false, firstPass: false");
        }
        else
        {
          DrawCharacters(deformed: true, firstPass: true);
          DrawCharacters(deformed: true, firstPass: false);
          DrawCharacters(deformed: false, firstPass: false);
        }

        spriteBatch.End();

        void DrawDrawCharactersProxy(CaptureState cs, bool deformed, bool firstPass)
        {
          if (!cs.IsActive)
          {
            DrawCharacters(deformed, firstPass);
            return;
          }

          Capture.Draw.EnsureCategory(cs);
          DrawCharactersAlt(cs, deformed, firstPass);
        }

        void DrawCharacters(bool deformed, bool firstPass)
        {
          //backwards order to render the most recently spawned characters in front (characters spawned later have a larger sprite depth)
          for (int i = Character.CharacterList.Count - 1; i >= 0; i--)
          {
            Character c = Character.CharacterList[i];
            if (!c.IsVisible) { continue; }
            if (c.Params.DrawLast == firstPass) { continue; }
            if (deformed)
            {
              if (c.AnimController.Limbs.All(l => l.DeformSprite == null)) { continue; }
            }
            else
            {
              if (c.AnimController.Limbs.Any(l => l.DeformSprite != null)) { continue; }
            }
            c.Draw(spriteBatch, _.Cam);
          }
        }

        void DrawCharactersAlt(CaptureState cs, bool deformed, bool firstPass)
        {
          Stopwatch sw = new Stopwatch();

          //backwards order to render the most recently spawned characters in front (characters spawned later have a larger sprite depth)
          for (int i = Character.CharacterList.Count - 1; i >= 0; i--)
          {
            sw.Restart();
            Character c = Character.CharacterList[i];
            if (!c.IsVisible) { continue; }
            if (c.Params.DrawLast == firstPass) { continue; }
            if (deformed)
            {
              if (c.AnimController.Limbs.All(l => l.DeformSprite == null)) { continue; }
            }
            else
            {
              if (c.AnimController.Limbs.Any(l => l.DeformSprite != null)) { continue; }
            }
            c.Draw(spriteBatch, _.Cam);
            sw.Stop();

            if (cs.ByID)
            {
              string info = c.Info?.DisplayName ?? c.ToString();
              Capture.Draw.AddTicks(sw.ElapsedTicks, cs, info);
            }
            else
            {
              Capture.Draw.AddTicks(sw.ElapsedTicks, cs, c.ToString());
            }
          }
        }

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:DeformableCharacters", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DeformableCharacters, "DeformableCharacters");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "DeformableCharacters");
        sw.Restart();

        Level.Loaded?.DrawFront(spriteBatch, _.cam);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontLevel", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, FrontLevel, "FrontLevel");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "FrontLevel");


        //draw the rendertarget and particles that are only supposed to be drawn in water into renderTargetWater
        graphics.SetRenderTarget(_.renderTargetWater);

        graphics.BlendState = BlendState.Opaque;
        graphics.SamplerStates[0] = SamplerState.LinearWrap;
        GraphicsQuad.UseBasicEffect(_.renderTarget);
        GraphicsQuad.Render();

        //draw alpha blended particles that are inside a sub
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, _.cam.Transform);
        ParticleManagerPatch.Draw(FrontParticles, spriteBatch, true, true, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
        spriteBatch.End();

        graphics.SetRenderTarget(_.renderTarget);

        //draw alpha blended particles that are not in water
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, _.cam.Transform);
        ParticleManagerPatch.Draw(FrontParticles, spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
        spriteBatch.End();

        //draw additive particles that are not in water
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, _.cam.Transform);
        ParticleManagerPatch.Draw(FrontParticles, spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.Additive);
        spriteBatch.End();

        graphics.DepthStencilState = DepthStencilState.DepthRead;
        graphics.SetRenderTarget(_.renderTargetFinal);

        sw.Restart();
        WaterRenderer.Instance.ResetBuffers();
        Hull.UpdateVertices(_.cam, WaterRenderer.Instance);
        WaterRenderer.Instance.RenderWater(spriteBatch, _.renderTargetWater, _.cam);
        WaterRenderer.Instance.RenderAir(graphics, _.cam, _.renderTarget, _.Cam.ShaderTransform);
        graphics.DepthStencilState = DepthStencilState.None;

        sw.Stop();
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, FrontParticles, "RenderWater");

        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontParticles", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, FrontParticles, "FrontParticles");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "FrontParticles");
        sw.Restart();

        _.DamageEffect.CurrentTechnique = _.DamageEffect.Techniques["StencilShader"];
        spriteBatch.Begin(SpriteSortMode.Immediate,
            BlendState.NonPremultiplied, SamplerState.LinearWrap,
            null, null,
            _.DamageEffect,
            _.cam.Transform);

        if (FrontDamageable.IsActive)
        {
          SubmarinePatch.DrawDamageable(FrontDamageable, spriteBatch, _.DamageEffect, false);
        }
        else
        {
          Submarine.DrawDamageable(spriteBatch, _.DamageEffect, false);
        }

        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontDamageable", sw.ElapsedTicks);
        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, FrontDamageable, "FrontDamageable");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "FrontDamageable");
        sw.Restart();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
        Submarine.DrawFront(spriteBatch, false, null);
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontStructuresItems", sw.ElapsedTicks);
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, FrontStructuresItems, "FrontStructuresItems");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "FrontStructuresItems");


        //draw additive particles that are inside a sub
        Capture.Draw.EnsureCategory(FrontMisc);


        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.Default, null, null, _.cam.Transform);

        ParticleManagerPatch.Draw(FrontMisc, spriteBatch, true, true, Barotrauma.Particles.ParticleBlendState.Additive);
        sw.Restart();
        foreach (var discharger in Barotrauma.Items.Components.ElectricalDischarger.List)
        {
          discharger.DrawElectricity(spriteBatch);
        }
        sw.Stop();
        Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, "DrawElectricity");
        sw.Restart();

        spriteBatch.End();
        if (GameMain.LightManager.LightingEnabled)
        {
          graphics.DepthStencilState = DepthStencilState.None;
          graphics.SamplerStates[0] = SamplerState.LinearWrap;
          graphics.BlendState = CustomBlendStates.Multiplicative;
          GraphicsQuad.UseBasicEffect(GameMain.LightManager.LightMap);
          GraphicsQuad.Render();
        }

        sw.Stop();
        Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, "GraphicsQuad.Render LightManager");


        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, null, null, _.cam.Transform);
        foreach (Character c in Character.CharacterList)
        {
          sw.Restart();
          c.DrawFront(spriteBatch, _.cam);
          sw.Stop();
          Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, $"{c} DrawFront");
        }


        Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, "Character.DrawFront");
        sw.Restart();

        GameMain.LightManager.DebugDrawVertices(spriteBatch);

        sw.Stop();
        Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, "LightManager.DebugDrawVertices");
        sw.Restart();

        Level.Loaded?.DrawDebugOverlay(spriteBatch, _.cam);
        if (GameMain.DebugDraw)
        {
          MapEntity.MapEntityList.ForEach(me => me.AiTarget?.Draw(spriteBatch));
          Character.CharacterList.ForEach(c => c.AiTarget?.Draw(spriteBatch));
          if (GameMain.GameSession?.EventManager != null)
          {
            GameMain.GameSession.EventManager.DebugDraw(spriteBatch);
          }
        }

        sw.Stop();
        Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, "Level.DrawDebugOverlay");

        spriteBatch.End();


        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontMisc", sw.ElapsedTicks);
        //Capture.Draw.AddTicks(sw.ElapsedTicks, FrontMisc, "FrontMisc");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "FrontMisc");
        sw.Restart();

        if (GameMain.LightManager.LosEnabled && GameMain.LightManager.LosMode != LosMode.None && Barotrauma.Lights.LightManager.ViewTarget != null)
        {
          GameMain.LightManager.LosEffect.CurrentTechnique = GameMain.LightManager.LosEffect.Techniques["LosShader"];

          GameMain.LightManager.LosEffect.Parameters["blurDistance"].SetValue(0.005f);
          GameMain.LightManager.LosEffect.Parameters["xTexture"].SetValue(_.renderTargetBackground);
          GameMain.LightManager.LosEffect.Parameters["xLosTexture"].SetValue(GameMain.LightManager.LosTexture);
          GameMain.LightManager.LosEffect.Parameters["xLosAlpha"].SetValue(GameMain.LightManager.LosAlpha);

          Color losColor;
          if (GameMain.LightManager.LosMode == LosMode.Transparent)
          {
            //convert the los color to HLS and make sure the luminance of the color is always the same
            //as the luminance of the ambient light color
            float r = Character.Controlled?.CharacterHealth == null ?
                0.0f : Math.Min(Character.Controlled.CharacterHealth.DamageOverlayTimer * 0.5f, 0.5f);
            Vector3 ambientLightHls = GameMain.LightManager.AmbientLight.RgbToHLS();
            Vector3 losColorHls = Color.Lerp(GameMain.LightManager.AmbientLight, Color.Red, r).RgbToHLS();
            losColorHls.Y = ambientLightHls.Y;
            losColor = ToolBox.HLSToRGB(losColorHls);
          }
          else
          {
            losColor = Color.Black;
          }

          GameMain.LightManager.LosEffect.Parameters["xColor"].SetValue(losColor.ToVector4());

          graphics.BlendState = BlendState.NonPremultiplied;
          graphics.SamplerStates[0] = SamplerState.PointClamp;
          graphics.SamplerStates[1] = SamplerState.PointClamp;
          GameMain.LightManager.LosEffect.CurrentTechnique.Passes[0].Apply();
          GraphicsQuad.Render();
          graphics.SamplerStates[0] = SamplerState.LinearWrap;
          graphics.SamplerStates[1] = SamplerState.LinearWrap;
        }

        if (Character.Controlled is { } character)
        {
          float grainStrength = character.GrainStrength;
          Rectangle screenRect = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
          spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, effect: _.GrainEffect);
          GUI.DrawRectangle(spriteBatch, screenRect, Color.White, isFilled: true);
          _.GrainEffect.Parameters["seed"].SetValue(Rand.Range(0f, 1f, Rand.RandSync.Unsynced));
          _.GrainEffect.Parameters["intensity"].SetValue(grainStrength);
          _.GrainEffect.Parameters["grainColor"].SetValue(character.GrainColor.ToVector4());
          spriteBatch.End();
        }

        graphics.SetRenderTarget(null);

        float BlurStrength = 0.0f;
        float DistortStrength = 0.0f;
        Vector3 chromaticAberrationStrength = GameSettings.CurrentConfig.Graphics.ChromaticAberration ?
            new Vector3(-0.02f, -0.01f, 0.0f) : Vector3.Zero;

        if (Level.Loaded?.Renderer != null)
        {
          chromaticAberrationStrength += new Vector3(-0.03f, -0.015f, 0.0f) * Level.Loaded.Renderer.ChromaticAberrationStrength;
        }

        if (Character.Controlled != null)
        {
          BlurStrength = Character.Controlled.BlurStrength * 0.005f;
          DistortStrength = Character.Controlled.DistortStrength;
          if (GameSettings.CurrentConfig.Graphics.RadialDistortion)
          {
            chromaticAberrationStrength -= Vector3.One * Character.Controlled.RadialDistortStrength;
          }
          chromaticAberrationStrength += new Vector3(-0.03f, -0.015f, 0.0f) * Character.Controlled.ChromaticAberrationStrength;
        }
        else
        {
          BlurStrength = 0.0f;
          DistortStrength = 0.0f;
        }

        string postProcessTechnique = "";
        if (BlurStrength > 0.0f)
        {
          postProcessTechnique += "Blur";
          _.PostProcessEffect.Parameters["blurDistance"].SetValue(BlurStrength);
        }
        if (chromaticAberrationStrength != Vector3.Zero)
        {
          postProcessTechnique += "ChromaticAberration";
          _.PostProcessEffect.Parameters["chromaticAberrationStrength"].SetValue(chromaticAberrationStrength);
        }
        if (DistortStrength > 0.0f)
        {
          postProcessTechnique += "Distort";
          _.PostProcessEffect.Parameters["distortScale"].SetValue(Vector2.One * DistortStrength);
          _.PostProcessEffect.Parameters["distortUvOffset"].SetValue(WaterRenderer.Instance.WavePos * 0.001f);
        }

        graphics.BlendState = BlendState.Opaque;
        graphics.SamplerStates[0] = SamplerState.LinearClamp;
        graphics.DepthStencilState = DepthStencilState.None;
        if (string.IsNullOrEmpty(postProcessTechnique))
        {
          GraphicsQuad.UseBasicEffect(_.renderTargetFinal);
        }
        else
        {
          _.PostProcessEffect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
          _.PostProcessEffect.Parameters["xTexture"].SetValue(_.renderTargetFinal);
          _.PostProcessEffect.CurrentTechnique = _.PostProcessEffect.Techniques[postProcessTechnique];
          _.PostProcessEffect.CurrentTechnique.Passes[0].Apply();
        }
        GraphicsQuad.Render();

        Character.DrawSpeechBubbles(spriteBatch, _.cam);

        if (_.fadeToBlackState > 0.0f)
        {
          spriteBatch.Begin(SpriteSortMode.Deferred);
          GUI.DrawRectangle(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.Lerp(Color.TransparentBlack, Color.Black, _.fadeToBlackState), isFilled: true);
          spriteBatch.End();
        }

        if (GameMain.LightManager.DebugLos)
        {
          GameMain.LightManager.DebugDrawLos(spriteBatch, _.cam);
        }

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:PostProcess", sw.ElapsedTicks);
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, PostProcess, "PostProcess");
        Capture.Draw.AddTicksOnce(sw.ElapsedTicks, DrawMap, "PostProcess");
        sw.Restart();

        return false;
      }







      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Screens/GameScreen.cs#L99
      public static bool GameScreen_Update_Replace(double deltaTime, GameScreen __instance)
      {
        if (!Showperf.Revealed) return true;
        Capture.Update.EnsureCategory(Update);

        GameScreen _ = __instance;

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

#if RUN_PHYSICS_IN_SEPARATE_THREAD
        physicsTime += deltaTime;
        lock (updateLock)
        {
#endif


#if DEBUG && CLIENT
          if (GameMain.GameSession != null && !DebugConsole.IsOpen && GUI.KeyboardDispatcher.Subscriber == null)
          {
            if (GameMain.GameSession.Level != null && GameMain.GameSession.Submarine != null)
            {
              Submarine closestSub = Submarine.FindClosest(cam.WorldViewCenter) ?? GameMain.GameSession.Submarine;

              Vector2 targetMovement = Vector2.Zero;
              if (PlayerInput.KeyDown(Keys.I)) { targetMovement.Y += 1.0f; }
              if (PlayerInput.KeyDown(Keys.K)) { targetMovement.Y -= 1.0f; }
              if (PlayerInput.KeyDown(Keys.J)) { targetMovement.X -= 1.0f; }
              if (PlayerInput.KeyDown(Keys.L)) { targetMovement.X += 1.0f; }

              if (targetMovement != Vector2.Zero)
              {
                closestSub.ApplyForce(targetMovement * closestSub.SubBody.Body.Mass * 100.0f);
              }
            }
          }
#endif

#if CLIENT
        sw.Start();
        GameMain.LightManager?.Update((float)deltaTime);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateLightManager, "Update.LightManager");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.LightManager");
        sw.Stop();
#endif

        _.GameTime += deltaTime;

        sw.Restart();

        // Note: it too heavy, it impacts update time and messes up timestamps
        // if (UpdatePhysicBodies.IsActive)
        // {
        //   Capture.Update.EnsureCategory(UpdatePhysicBodies);
        //   foreach (PhysicsBody body in PhysicsBody.List)
        //   {
        //     sw2.Restart();
        //     if (body.Enabled && body.BodyType != FarseerPhysics.BodyType.Static) { body.Update(); }
        //     sw2.Stop();

        //     if (body.UserData is Character c)
        //     {
        //       if (UpdatePhysicBodies.ByID)
        //       {
        //         string info = c.Info == null ? "" : $":{c.Info.DisplayName}";
        //         Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, $"{c.ID}|{c}{info}");
        //         continue;
        //       }
        //       else
        //       {
        //         Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, c.ToString());
        //         continue;
        //       }

        //     }

        //     if (body.UserData is Limb l)
        //     {
        //       if (UpdatePhysicBodies.ByID)
        //       {
        //         string info = l.character.Info == null ? "" : $":{l.character.Info.DisplayName}";
        //         Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, $"{l.character.ID}|{l.character}{info}.{l.type}");
        //         continue;
        //       }
        //       else
        //       {
        //         Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, $"{l.character}.{l.type}");
        //         continue;
        //       }
        //     }

        //     if (body.UserData is Item i)
        //     {
        //       if (UpdatePhysicBodies.ByID)
        //       {
        //         Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, i.ToString());
        //         continue;
        //       }
        //       else
        //       {
        //         Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, i.Prefab.Identifier);
        //         continue;
        //       }
        //     }

        //     Capture.Update.AddTicks(sw2.ElapsedTicks, UpdatePhysicBodies, body.UserData.ToString());
        //   }
        // }
        // else
        // {
        //   foreach (PhysicsBody body in PhysicsBody.List)
        //   {
        //     if (body.Enabled && body.BodyType != FarseerPhysics.BodyType.Static) { body.Update(); }
        //   }
        // }

        foreach (PhysicsBody body in PhysicsBody.List)
        {
          if (body.Enabled && body.BodyType != FarseerPhysics.BodyType.Static) { body.Update(); }
        }

        MapEntity.ClearHighlightedEntities();

        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdatePhysicBodies, "Interpolate draw position");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Interpolate draw position");
        sw.Restart();


        GameMain.GameSession?.Update((float)deltaTime);

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:GameSession", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateGameSession, "Update.GameSession");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.GameSession");
        sw.Restart();

        GameMain.ParticleManager.Update((float)deltaTime);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Particles", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Particles");
        sw.Restart();

        if (Level.Loaded != null) Level.Loaded.Update((float)deltaTime, _.cam);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Level", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Level");

        Capture.Update.EnsureCategory(UpdateItemsHUD);
        if (Character.Controlled is { } controlled)
        {
          if (controlled.SelectedItem != null && controlled.CanInteractWith(controlled.SelectedItem))
          {
            sw.Restart();
            controlled.SelectedItem.UpdateHUD(_.cam, controlled, (float)deltaTime);
            sw.Stop();
            Capture.Update.AddTicks(sw.ElapsedTicks, UpdateItemsHUD, controlled.SelectedItem.ToString());
            Capture.Update.AddTicks(sw.ElapsedTicks, Update, controlled.SelectedItem.ToString());
          }
          if (controlled.Inventory != null)
          {
            foreach (Item item in controlled.Inventory.AllItems)
            {
              if (controlled.HasEquippedItem(item))
              {
                sw.Restart();
                item.UpdateHUD(_.cam, controlled, (float)deltaTime);
                sw.Stop();
                Capture.Update.AddTicks(sw.ElapsedTicks, UpdateItemsHUD, item.ToString());
                Capture.Update.AddTicks(sw.ElapsedTicks, Update, item.ToString());
              }
            }
          }
        }

        sw.Restart();

        Character.UpdateAll((float)deltaTime, _.cam);
#elif SERVER
        if (Level.Loaded != null) Level.Loaded.Update((float)deltaTime, Camera.Instance);
        Character.UpdateAll((float)deltaTime, Camera.Instance);
#endif


#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Character", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Character");
        sw.Restart();
#endif

        StatusEffect.UpdateAll((float)deltaTime);

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:StatusEffects", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateStatusEffects, "Update.StatusEffects");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.StatusEffects");
        sw.Restart();

        if (Character.Controlled != null &&
            Barotrauma.Lights.LightManager.ViewTarget != null)
        {
          Vector2 targetPos = Barotrauma.Lights.LightManager.ViewTarget.WorldPosition;
          if (Barotrauma.Lights.LightManager.ViewTarget == Character.Controlled &&
              (CharacterHealth.OpenHealthWindow != null || CrewManager.IsCommandInterfaceOpen || ConversationAction.IsDialogOpen))
          {
            Vector2 screenTargetPos = new Vector2(GameMain.GraphicsWidth, GameMain.GraphicsHeight) * 0.5f;
            if (CharacterHealth.OpenHealthWindow != null)
            {
              screenTargetPos.X = GameMain.GraphicsWidth * (CharacterHealth.OpenHealthWindow.Alignment == Alignment.Left ? 0.6f : 0.4f);
            }
            else if (ConversationAction.IsDialogOpen)
            {
              screenTargetPos.Y = GameMain.GraphicsHeight * 0.4f;
            }
            Vector2 screenOffset = screenTargetPos - new Vector2(GameMain.GraphicsWidth / 2, GameMain.GraphicsHeight / 2);
            screenOffset.Y = -screenOffset.Y;
            targetPos -= screenOffset / _.cam.Zoom;
          }
          _.cam.TargetPos = targetPos;
        }

        _.cam.MoveCamera((float)deltaTime, allowZoom: GUI.MouseOn == null && !Inventory.IsMouseOnInventory);

        Character.Controlled?.UpdateLocalCursor(_.cam);
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateCameraAndCursor, "Camera and cursor");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Camera and cursor");
        sw.Restart();
#endif

        foreach (Submarine sub in Submarine.Loaded)
        {
          sub.SetPrevTransform(sub.Position);
        }

        foreach (PhysicsBody body in PhysicsBody.List)
        {
          if (body.Enabled && body.BodyType != FarseerPhysics.BodyType.Static)
          {
            body.SetPrevTransform(body.SimPosition, body.Rotation);
          }
        }

        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateSetPrevTransform, "SetPrevTransform");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "SetPrevTransform");
        sw.Restart();

#if CLIENT
        MapEntity.UpdateAll((float)deltaTime, _.cam);
#elif SERVER
        MapEntity.UpdateAll((float)deltaTime, Camera.Instance);
#endif

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.MapEntity");
        sw.Restart();
#endif
        Character.UpdateAnimAll((float)deltaTime);
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateAnimations, "Update.Animations");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Animations");
        sw.Restart();

#if CLIENT
        Ragdoll.UpdateAll((float)deltaTime, _.cam);
#elif SERVER
        Ragdoll.UpdateAll((float)deltaTime, Camera.Instance);
#endif

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Ragdolls", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Ragdolls");
        sw.Restart();
#endif

        Capture.Update.EnsureCategory(UpdateSubmarine);
        foreach (Submarine sub in Submarine.Loaded)
        {
          sw2.Restart();
          sub.Update((float)deltaTime);
          sw2.Stop();
          Capture.Update.AddTicksOnce(sw2.ElapsedTicks, UpdateSubmarine, $"{sub}");
        }

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Submarine", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Submarine");
        sw.Restart();
#endif

#if !RUN_PHYSICS_IN_SEPARATE_THREAD
        try
        {
          GameMain.World.Step((float)Timing.Step);

          Capture.Farseer.EnsureCategory(UpdatePhysics);
          Capture.Farseer.AddTicks(GameMain.World.ContinuousPhysicsTime.TotalMilliseconds, UpdatePhysics, "ContinuousPhysicsTime");
          Capture.Farseer.AddTicks(GameMain.World.ControllersUpdateTime.TotalMilliseconds, UpdatePhysics, "ControllersUpdateTime");
          Capture.Farseer.AddTicks(GameMain.World.AddRemoveTime.TotalMilliseconds, UpdatePhysics, "AddRemoveTime");
          Capture.Farseer.AddTicks(GameMain.World.NewContactsTime.TotalMilliseconds, UpdatePhysics, "NewContactsTime");
          Capture.Farseer.AddTicks(GameMain.World.ContactsUpdateTime.TotalMilliseconds, UpdatePhysics, "ContactsUpdateTime");
          Capture.Farseer.AddTicks(GameMain.World.SolveUpdateTime.TotalMilliseconds, UpdatePhysics, "SolveUpdateTime");
          Capture.Farseer.AddTicks(GameMain.World.ProxyCount, UpdatePhysics, "ProxyCount");
          Capture.Farseer.AddTicks(GameMain.World.ContactCount, UpdatePhysics, "ContactCount");
        }
        catch (WorldLockedException e)
        {
          string errorMsg = "Attempted to modify the state of the physics simulation while a time step was running.";
          DebugConsole.ThrowError(errorMsg, e);
          GameAnalyticsManager.AddErrorEventOnce("GameScreen.Update:WorldLockedException" + e.Message, GameAnalyticsManager.ErrorSeverity.Critical, errorMsg);
        }
#endif


#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Physics", sw.ElapsedTicks);
        //Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdatePhysics, "Update.Physics");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Update, "Update.Physics");
        _.UpdateProjSpecific(deltaTime);
#endif
        // it seems that on server side this method is not even compiled because it's empty
        // _.UpdateProjSpecific(deltaTime);

#if RUN_PHYSICS_IN_SEPARATE_THREAD
        }
#endif

        Capture.Farseer.Update();

        return false;
      }

    }
  }
}