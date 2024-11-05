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
      public static CaptureState ShowperfDrawHUD;
      public static CaptureState ShowperfDrawMap;

      public static CaptureState BackCharactersItems;
      public static CaptureState BackLevel;
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


      public static CaptureState UpdateCharacter;
      public static CaptureState UpdateGameSession;
      public static CaptureState UpdateLevel;
      public static CaptureState UpdateMapEntity;
      public static CaptureState UpdateParticles;
      public static CaptureState UpdatePhysics;
      public static CaptureState UpdatePower;
      public static CaptureState UpdateRagdolls;
      public static CaptureState UpdateStatusEffects;
      public static CaptureState UpdateSubmarine;


      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(GameScreen).GetMethod("Draw", AccessTools.all),
          prefix: new HarmonyMethod(typeof(GameScreenPatch).GetMethod("GameScreen_Draw_Replace"))
        );

        harmony.Patch(
          original: typeof(GameScreen).GetMethod("DrawMap", AccessTools.all),
          prefix: new HarmonyMethod(typeof(GameScreenPatch).GetMethod("GameScreen_DrawMap_Replace"))
        );

        harmony.Patch(
          original: typeof(GameScreen).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(GameScreenPatch).GetMethod("GameScreen_Update_Replace"))
        );

        ShowperfDrawHUD = Capture.Get("Showperf.Draw.HUD");
        ShowperfDrawMap = Capture.Get("Showperf.Draw.Map");

        BackCharactersItems = Capture.Get("Showperf.Draw.Map.BackCharactersItems");
        BackLevel = Capture.Get("Showperf.Draw.Map.BackLevel");
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

        UpdateCharacter = Capture.Get("Showperf.Update.Character");
        UpdateGameSession = Capture.Get("Showperf.Update.GameSession");
        UpdateLevel = Capture.Get("Showperf.Update.Level");
        UpdateMapEntity = Capture.Get("Showperf.Update.MapEntity");
        UpdateParticles = Capture.Get("Showperf.Update.Particles");
        UpdatePhysics = Capture.Get("Showperf.Update.Physics");
        UpdatePower = Capture.Get("Showperf.Update.Power");
        UpdateRagdolls = Capture.Get("Showperf.Update.Ragdolls");
        UpdateStatusEffects = Capture.Get("Showperf.Update.StatusEffects");
        UpdateSubmarine = Capture.Get("Showperf.Update.Submarine");
      }


      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaClient/ClientSource/Screens/GameScreen.cs#L98
      public static bool GameScreen_Draw_Replace(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch, GameScreen __instance)
      {
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

        Stopwatch sw = new Stopwatch();
        sw.Start();

        _.DrawMap(graphics, spriteBatch, deltaTime);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map", sw.ElapsedTicks);

        if (ShowperfDrawMap.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, ShowperfDrawMap, "Draw.Map"));
        }
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
        if (ShowperfDrawHUD.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, ShowperfDrawHUD, "Draw.HUD"));
        }
        sw.Restart();

        return false;
      }



      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaClient/ClientSource/Screens/GameScreen.cs#L268
      public static bool GameScreen_DrawMap_Replace(GraphicsDevice graphics, SpriteBatch spriteBatch, double deltaTime, GameScreen __instance)
      {
        GameScreen _ = __instance;

        foreach (Submarine sub in Submarine.Loaded)
        {
          sub.UpdateTransform();
        }

        GameMain.ParticleManager.UpdateTransforms();

        Stopwatch sw = new Stopwatch();
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
        if (LOS.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, LOS, "Draw.Map.LOS"));
        }
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
        Submarine.DrawBack(spriteBatch, false, e => e is Structure s && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && !IsFromOutpostDrawnBehindSubs(e));
        Submarine.DrawPaintedColors(spriteBatch, false);
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackStructures", sw.ElapsedTicks);
        if (BackStructures.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, BackStructures, "Draw.Map.BackStructures"));
        }
        sw.Restart();

        graphics.SetRenderTarget(null);
        GameMain.LightManager.RenderLightMap(graphics, spriteBatch, _.cam, _.renderTarget);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:Lighting", sw.ElapsedTicks);
        if (Lighting.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, Lighting, "Draw.Map.Lighting"));
        }
        sw.Restart();

        //------------------------------------------------------------------------
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

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
        Submarine.DrawBack(spriteBatch, false, e => e is Structure s && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && IsFromOutpostDrawnBehindSubs(e));
        spriteBatch.End();

        //draw alpha blended particles that are in water and behind subs
#if LINUX || OSX
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
#else
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
#endif
        GameMain.ParticleManager.Draw(spriteBatch, true, false, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
        spriteBatch.End();

        //draw additive particles that are in water and behind subs
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, _.cam.Transform);
        GameMain.ParticleManager.Draw(spriteBatch, true, false, Barotrauma.Particles.ParticleBlendState.Additive);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None);
        spriteBatch.Draw(_.renderTarget, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackLevel", sw.ElapsedTicks);
        if (BackLevel.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, BackLevel, "Draw.Map.BackLevel"));
        }
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
        Submarine.DrawBack(spriteBatch, false, e => !(e is Structure) || e.SpriteDepth < 0.9f);
        DrawCharacters(deformed: false, firstPass: true);
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackCharactersItems", sw.ElapsedTicks);
        if (BackCharactersItems.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, BackCharactersItems, "Draw.Map.BackCharactersItems"));
        }
        sw.Restart();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
        DrawCharacters(deformed: true, firstPass: true);
        DrawCharacters(deformed: true, firstPass: false);
        DrawCharacters(deformed: false, firstPass: false);
        spriteBatch.End();

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

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:DeformableCharacters", sw.ElapsedTicks);
        if (DeformableCharacters.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, DeformableCharacters, "Draw.Map.DeformableCharacters"));
        }
        sw.Restart();

        Level.Loaded?.DrawFront(spriteBatch, _.cam);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontLevel", sw.ElapsedTicks);
        if (FrontLevel.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, FrontLevel, "Draw.Map.FrontLevel"));
        }
        sw.Restart();

        //draw the rendertarget and particles that are only supposed to be drawn in water into renderTargetWater
        graphics.SetRenderTarget(_.renderTargetWater);

        graphics.BlendState = BlendState.Opaque;
        graphics.SamplerStates[0] = SamplerState.LinearWrap;
        GraphicsQuad.UseBasicEffect(_.renderTarget);
        GraphicsQuad.Render();

        //draw alpha blended particles that are inside a sub
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, _.cam.Transform);
        GameMain.ParticleManager.Draw(spriteBatch, true, true, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
        spriteBatch.End();

        graphics.SetRenderTarget(_.renderTarget);

        //draw alpha blended particles that are not in water
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, _.cam.Transform);
        GameMain.ParticleManager.Draw(spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
        spriteBatch.End();

        //draw additive particles that are not in water
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, _.cam.Transform);
        GameMain.ParticleManager.Draw(spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.Additive);
        spriteBatch.End();

        graphics.DepthStencilState = DepthStencilState.DepthRead;
        graphics.SetRenderTarget(_.renderTargetFinal);

        WaterRenderer.Instance.ResetBuffers();
        Hull.UpdateVertices(_.cam, WaterRenderer.Instance);
        WaterRenderer.Instance.RenderWater(spriteBatch, _.renderTargetWater, _.cam);
        WaterRenderer.Instance.RenderAir(graphics, _.cam, _.renderTarget, _.Cam.ShaderTransform);
        graphics.DepthStencilState = DepthStencilState.None;

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontParticles", sw.ElapsedTicks);
        if (FrontParticles.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, FrontParticles, "Draw.Map.FrontParticles"));
        }
        sw.Restart();

        _.DamageEffect.CurrentTechnique = _.DamageEffect.Techniques["StencilShader"];
        spriteBatch.Begin(SpriteSortMode.Immediate,
            BlendState.NonPremultiplied, SamplerState.LinearWrap,
            null, null,
            _.DamageEffect,
            _.cam.Transform);
        Submarine.DrawDamageable(spriteBatch, _.DamageEffect, false);
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontDamageable", sw.ElapsedTicks);
        if (FrontDamageable.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, FrontDamageable, "Draw.Map.FrontDamageable"));
        }
        sw.Restart();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, _.cam.Transform);
        Submarine.DrawFront(spriteBatch, false, null);
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontStructuresItems", sw.ElapsedTicks);
        if (FrontStructuresItems.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, FrontStructuresItems, "Draw.Map.FrontStructuresItems"));
        }
        sw.Restart();

        //draw additive particles that are inside a sub
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.Default, null, null, _.cam.Transform);
        GameMain.ParticleManager.Draw(spriteBatch, true, true, Barotrauma.Particles.ParticleBlendState.Additive);
        foreach (var discharger in Barotrauma.Items.Components.ElectricalDischarger.List)
        {
          discharger.DrawElectricity(spriteBatch);
        }
        spriteBatch.End();
        if (GameMain.LightManager.LightingEnabled)
        {
          graphics.DepthStencilState = DepthStencilState.None;
          graphics.SamplerStates[0] = SamplerState.LinearWrap;
          graphics.BlendState = CustomBlendStates.Multiplicative;
          GraphicsQuad.UseBasicEffect(GameMain.LightManager.LightMap);
          GraphicsQuad.Render();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, null, null, _.cam.Transform);
        foreach (Character c in Character.CharacterList)
        {
          c.DrawFront(spriteBatch, _.cam);
        }

        GameMain.LightManager.DebugDrawVertices(spriteBatch);

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
        spriteBatch.End();

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontMisc", sw.ElapsedTicks);
        if (FrontMisc.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, FrontMisc, "Draw.Map.FrontMisc"));
        }
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
        if (PostProcess.IsActive)
        {
          Capture.Draw.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, PostProcess, "Draw.Map.PostProcess"));
        }
        sw.Restart();

        return false;
      }







      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Screens/GameScreen.cs#L99
      public static bool GameScreen_Update_Replace(double deltaTime, GameScreen __instance)
      {
        GameScreen _ = __instance;

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
        GameMain.LightManager?.Update((float)deltaTime);
#endif

        _.GameTime += deltaTime;

        foreach (PhysicsBody body in PhysicsBody.List)
        {
          if (body.Enabled && body.BodyType != FarseerPhysics.BodyType.Static) { body.Update(); }
        }
        MapEntity.ClearHighlightedEntities();

#if CLIENT
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
#endif

        GameMain.GameSession?.Update((float)deltaTime);

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:GameSession", sw.ElapsedTicks);
        if (UpdateGameSession.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateGameSession, "Update.GameSession"));
        }
        sw.Restart();

        GameMain.ParticleManager.Update((float)deltaTime);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Particles", sw.ElapsedTicks);
        if (UpdateParticles.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateParticles, "Update.Particles"));
        }
        sw.Restart();

        if (Level.Loaded != null) Level.Loaded.Update((float)deltaTime, _.cam);

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Level", sw.ElapsedTicks);
        if (UpdateLevel.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateLevel, "Update.Level"));
        }

        if (Character.Controlled is { } controlled)
        {
          if (controlled.SelectedItem != null && controlled.CanInteractWith(controlled.SelectedItem))
          {
            controlled.SelectedItem.UpdateHUD(_.cam, controlled, (float)deltaTime);
          }
          if (controlled.Inventory != null)
          {
            foreach (Item item in controlled.Inventory.AllItems)
            {
              if (controlled.HasEquippedItem(item))
              {
                item.UpdateHUD(_.cam, controlled, (float)deltaTime);
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
        if (UpdateCharacter.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateCharacter, "Update.Character"));
        }
        sw.Restart();
#endif

        StatusEffect.UpdateAll((float)deltaTime);

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:StatusEffects", sw.ElapsedTicks);
        if (UpdateStatusEffects.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateStatusEffects, "Update.StatusEffects"));
        }
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

#if CLIENT
        MapEntity.UpdateAll((float)deltaTime, _.cam);
#elif SERVER
        MapEntity.UpdateAll((float)deltaTime, Camera.Instance);
#endif

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity", sw.ElapsedTicks);
        if (UpdateMapEntity.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateMapEntity, "Update.MapEntity"));
        }
        sw.Restart();
#endif
        Character.UpdateAnimAll((float)deltaTime);

#if CLIENT
        Ragdoll.UpdateAll((float)deltaTime, _.cam);
#elif SERVER
        Ragdoll.UpdateAll((float)deltaTime, Camera.Instance);
#endif

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Ragdolls", sw.ElapsedTicks);
        if (UpdateRagdolls.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateRagdolls, "Update.Ragdolls"));
        }
        sw.Restart();
#endif

        foreach (Submarine sub in Submarine.Loaded)
        {
          sub.Update((float)deltaTime);
        }

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:Submarine", sw.ElapsedTicks);
        if (UpdateSubmarine.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdateSubmarine, "Update.Submarine"));
        }
        sw.Restart();
#endif

#if !RUN_PHYSICS_IN_SEPARATE_THREAD
        try
        {
          GameMain.World.Step((float)Timing.Step);
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
        if (UpdatePhysics.IsActive)
        {
          Capture.Update.AddTicksOnce(new UpdateTicks(sw.ElapsedTicks, UpdatePhysics, "Update.Physics"));
        }
        _.UpdateProjSpecific(deltaTime);
#endif
        // it seems that on server side this method is not even compiled because it's empty
        // _.UpdateProjSpecific(deltaTime);

#if RUN_PHYSICS_IN_SEPARATE_THREAD
        }
#endif

        return false;
      }

    }
  }
}