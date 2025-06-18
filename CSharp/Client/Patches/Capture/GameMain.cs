using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Collections.Immutable;

using HarmonyLib;

using Barotrauma;
using Barotrauma.IO;
using Barotrauma.Media;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Steam;
using Barotrauma.Transition;
using Barotrauma.Tutorials;
using Barotrauma.Extensions;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// [assembly: IgnoresAccessChecksTo("MonoGame.Framework.Windows.NetStandard")]
// [assembly: IgnoresAccessChecksTo("MonoGame.Framework.Linux.NetStandard")]

namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class GameMainPatch
    {
      public static CaptureState ShowperfDraw;
      public static CaptureState ShowperfUpdate;
      public static CaptureState UpdateMonoGame;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(GameMain).GetMethod("Draw", AccessTools.all),
          prefix: ShowperfMethod(typeof(GameMainPatch).GetMethod("GameMain_Draw_Replace"))
        );

        // harmony.Patch(
        //   original: typeof(GameMain).GetMethod("Update", AccessTools.all),
        //   prefix: ShowperfMethod(typeof(GameMainPatch).GetMethod("GameMain_Update_Replace"))
        // );

        // Harmony.ReversePatch(
        //   original: typeof(Game).GetMethod("Update", AccessTools.all),
        //   standin: new HarmonyMethod(typeof(GameMainPatch).GetMethod("Game_Update_ReversePatch"))
        // );

        ShowperfDraw = Capture.Get("Showperf.Draw");
        ShowperfUpdate = Capture.Get("Showperf.Update");
        UpdateMonoGame = Capture.Get("MonoGame");
      }


      public static void Game_Update_ReversePatch(object instance, GameTime gameTime)
      {
        log("guh...");
      }




      // https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.GraphicsMetrics.html
      public static void AddMonoGameMetrics()
      {
        if (!UpdateMonoGame.IsActive) return;
        Capture.MonoGame.EnsureCategory(UpdateMonoGame);

        GraphicsMetrics metrics = GameMain.Instance.GraphicsDevice.Metrics;

        Capture.MonoGame.AddTicks(metrics.ClearCount, UpdateMonoGame, "ClearCount");
        Capture.MonoGame.AddTicks(metrics.DrawCount, UpdateMonoGame, "DrawCount");
        Capture.MonoGame.AddTicks(metrics.PixelShaderCount, UpdateMonoGame, "PixelShaderCount");
        Capture.MonoGame.AddTicks(metrics.PrimitiveCount, UpdateMonoGame, "PrimitiveCount");
        Capture.MonoGame.AddTicks(metrics.SpriteCount, UpdateMonoGame, "SpriteCount");
        Capture.MonoGame.AddTicks(metrics.TargetCount, UpdateMonoGame, "TargetCount");
        Capture.MonoGame.AddTicks(metrics.TextureCount, UpdateMonoGame, "TextureCount");
        Capture.MonoGame.AddTicks(metrics.VertexShaderCount, UpdateMonoGame, "VertexShaderCount");
      }


      //https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaClient/ClientSource/GameMain.cs#L1057
      public static bool GameMain_Draw_Replace(GameTime gameTime, GameMain __instance)
      {
        if (Showperf == null || !Showperf.Revealed) return true;

        GameMain _ = __instance;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        _.FixRazerCortex();

        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

        if (Timing.FrameLimit > 0)
        {
          double step = 1.0 / Timing.FrameLimit;
          while (!GameSettings.CurrentConfig.Graphics.VSync && sw.Elapsed.TotalSeconds + deltaTime < step)
          {
            Thread.Sleep(1);
          }
        }

        GameMain.PerformanceCounter.Update(sw.Elapsed.TotalSeconds + deltaTime);

        if (_.loadingScreenOpen)
        {
          GameMain.TitleScreen.Draw(GameMain.spriteBatch, _.GraphicsDevice, (float)deltaTime);
        }
        else if (_.HasLoaded)
        {
          Screen.Selected.Draw(deltaTime, _.GraphicsDevice, GameMain.spriteBatch);
        }

        if (GameMain.DebugDraw && GUI.MouseOn != null)
        {
          GameMain.spriteBatch.Begin();
          if (PlayerInput.IsCtrlDown() && PlayerInput.KeyDown(Keys.G))
          {
            List<GUIComponent> hierarchy = new List<GUIComponent>();
            var currComponent = GUI.MouseOn;
            while (currComponent != null)
            {
              hierarchy.Add(currComponent);
              currComponent = currComponent.Parent;
            }

            Color[] colors = { Color.Lime, Color.Yellow, Color.Aqua, Color.Red };
            for (int index = 0; index < hierarchy.Count; index++)
            {
              var component = hierarchy[index];
              if (component is { MouseRect: var mouseRect, Rect: var rect })
              {
                if (mouseRect.IsEmpty) { mouseRect = rect; }
                mouseRect.Location += (index % 2, (index % 4) / 2);
                GUI.DrawRectangle(GameMain.spriteBatch, mouseRect, colors[index % 4]);
              }
            }
          }
          else
          {
            GUI.DrawRectangle(GameMain.spriteBatch, GUI.MouseOn.MouseRect, Color.Lime);
            GUI.DrawRectangle(GameMain.spriteBatch, GUI.MouseOn.Rect, Color.Cyan);
          }
          GameMain.spriteBatch.End();
        }

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Draw", sw.ElapsedTicks);
        GameMain.PerformanceCounter.DrawTimeGraph.Update(sw.ElapsedTicks * 1000.0f / (float)Stopwatch.Frequency);

        //Capture.Draw.AddTicksOnce(sw.ElapsedTicks, ShowperfDraw, "Draw");


        Capture.Draw.FirstSlice.Total = sw.ElapsedTicks;
        Capture.Draw.Update();

        AddMonoGameMetrics();
        Capture.MonoGame.Update();

        return false;
      }



    }
  }
}