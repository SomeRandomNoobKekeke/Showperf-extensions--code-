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
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Voronoi2;

using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class LevelPatch
    {
      public static CaptureState UpdateLevel;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Level).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(LevelPatch).GetMethod("Level_Update_Replace"))
        );

        // harmony.Patch(
        //   original: typeof(Level).GetMethod("DrawBack", AccessTools.all),
        //   prefix: new HarmonyMethod(typeof(LevelPatch).GetMethod("Level_DrawBack_Replace"))
        // );

        UpdateLevel = Capture.Get("Showperf.Update.Level");
      }

      public static bool Level_DrawBack_Replace(GraphicsDevice graphics, SpriteBatch spriteBatch, Camera cam, Level __instance)
      {
        Level _ = __instance;

        float brightness = MathHelper.Clamp(1.1f + (cam.Position.Y - _.Size.Y) / 100000.0f, 0.1f, 1.0f);
        var lightColorHLS = _.GenerationParams.AmbientLightColor.RgbToHLS();
        lightColorHLS.Y *= brightness;

        GameMain.LightManager.AmbientLight = ToolBox.HLSToRGB(lightColorHLS);

        graphics.Clear(_.BackgroundColor);

        if (_.renderer != null)
        {
          GameMain.LightManager.AmbientLight = GameMain.LightManager.AmbientLight.Add(_.renderer.FlashColor);
          _.renderer?.DrawBackground(spriteBatch, cam, _.LevelObjectManager, _.backgroundCreatureManager);
        }

        return false;
      }

      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Map/Levels/Level.cs#L3572
      public static bool Level_Update_Replace(float deltaTime, Camera cam, Level __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !UpdateLevel.IsActive) return true;

        Level _ = __instance;

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        sw.Restart();
        _.LevelObjectManager.Update(deltaTime, cam);
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateLevel, "Level Objects");

        sw.Restart();
        foreach (LevelWall wall in _.ExtraWalls) { wall.Update(deltaTime); }
        for (int i = _.UnsyncedExtraWalls.Count - 1; i >= 0; i--)
        {
          _.UnsyncedExtraWalls[i].Update(deltaTime);
        }
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateLevel, "Walls");


#if SERVER
      if (GameMain.NetworkMember is { IsServer: true })
      {
        foreach (LevelWall wall in _.ExtraWalls)
        {
          if (wall is DestructibleLevelWall { NetworkUpdatePending: true } destructibleWall)
          {
            GameMain.NetworkMember.CreateEntityEvent(_, new Level.SingleLevelWallEventData(destructibleWall));
            destructibleWall.NetworkUpdatePending = false;
          }
        }
        _.networkUpdateTimer += deltaTime;
        if (_.networkUpdateTimer > Level.NetworkUpdateInterval)
        {
          if (_.ExtraWalls.Any(w => w.Body.BodyType != BodyType.Static))
          {
            GameMain.NetworkMember.CreateEntityEvent(_, new Level.GlobalLevelWallEventData());
          }
          _.networkUpdateTimer = 0.0f;
        }
      }
#endif

#if CLIENT
      sw.Restart();
      _.backgroundCreatureManager.Update(deltaTime, cam);
      sw.Stop();
      Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateLevel, "Background Creatures");

      sw.Restart();
      WaterRenderer.Instance?.ScrollWater(Vector2.UnitY, (float)deltaTime);
      sw.Stop();
      Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateLevel, "Water Renderer");

      sw.Restart();
      _.renderer.Update(deltaTime, cam);
      sw.Stop();
      Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateLevel, "Level Renderer");
#endif

        return false;
      }

    }
  }
}