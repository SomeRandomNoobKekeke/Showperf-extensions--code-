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
using FarseerPhysics;
using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class SubmarinePatch
    {
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Submarine).GetMethod("DrawBack", AccessTools.all),
          prefix: new HarmonyMethod(typeof(SubmarinePatch).GetMethod("Submarine_DrawBack_Replace"))
        );

        DrawBack = Capture.Get("MapEntityDrawing");
      }
      public static CaptureState DrawBack;


      public static bool Submarine_DrawBack_Replace(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
      {
        if (!DrawBack.IsActive) return true;
        Window.EnsureCategory(DrawBack.ID.HashCode);

        var entitiesToRender = !editing && Submarine.visibleEntities != null ? Submarine.visibleEntities : MapEntity.MapEntityList;

        Stopwatch sw = new Stopwatch();

        foreach (MapEntity e in entitiesToRender)
        {
          if (!e.DrawBelowWater) continue;

          if (predicate != null)
          {
            if (!predicate(e)) continue;
          }

          sw.Restart();
          e.Draw(spriteBatch, editing, true);
          sw.Stop();

          if (Window.ShouldCapture(e))
          {
            if (DrawBack.ByID || e.Prefab == null)
            {
              Window.AddTicks(new UpdateTicks(sw.ElapsedTicks, DrawBack, $"{e.Name} (ID: {e.ID})"));
            }
            else
            {
              Window.AddTicks(new UpdateTicks(sw.ElapsedTicks, DrawBack, e.Prefab.Identifier));
            }
          }
        }

        sw.Stop();

        return false;
      }

      // public static bool Submarine_DrawFront_Replace(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
      // {
      //   if (!Capture.MapEntityDrawing.IsActive) return true;
      //   Window.EnsureCategory(Capture.MapEntityDrawing.Category);

      //   var entitiesToRender = !editing && Submarine.visibleEntities != null ? Submarine.visibleEntities : MapEntity.MapEntityList;

      //   var sw = new System.Diagnostics.Stopwatch();
      //   long ticks;

      //   foreach (MapEntity e in entitiesToRender)
      //   {
      //     if (!e.DrawOverWater) { continue; }

      //     if (predicate != null)
      //     {
      //       if (!predicate(e)) { continue; }
      //     }

      //     sw.Restart();
      //     e.Draw(spriteBatch, editing, false);

      //     ticks = sw.ElapsedTicks;
      //     if (Window.ShouldCapture(e))
      //     {
      //       if (Capture.MapEntityDrawing.ByID || e.Prefab == null)
      //       {
      //         Window.AddTicks(new UpdateTicks(ticks, CName.MapEntityDrawing, $"{e.Name} (ID: {e.ID})"));
      //       }
      //       else
      //       {
      //         Window.AddTicks(new UpdateTicks(ticks, CName.MapEntityDrawing, e.Prefab.Identifier));
      //       }
      //     }
      //   }

      //   sw.Stop();

      //   if (GameMain.DebugDraw)
      //   {
      //     foreach (Submarine sub in Submarine.Loaded)
      //     {
      //       Rectangle worldBorders = sub.Borders;
      //       worldBorders.Location += sub.WorldPosition.ToPoint();
      //       worldBorders.Y = -worldBorders.Y;

      //       GUI.DrawRectangle(spriteBatch, worldBorders, Color.White, false, 0, 5);

      //       if (sub.SubBody == null || sub.subBody.PositionBuffer.Count < 2) continue;

      //       Vector2 prevPos = ConvertUnits.ToDisplayUnits(sub.subBody.PositionBuffer[0].Position);
      //       prevPos.Y = -prevPos.Y;

      //       for (int i = 1; i < sub.subBody.PositionBuffer.Count; i++)
      //       {
      //         Vector2 currPos = ConvertUnits.ToDisplayUnits(sub.subBody.PositionBuffer[i].Position);
      //         currPos.Y = -currPos.Y;

      //         GUI.DrawRectangle(spriteBatch, new Rectangle((int)currPos.X - 10, (int)currPos.Y - 10, 20, 20), Color.Blue * 0.6f, true, 0.01f);
      //         GUI.DrawLine(spriteBatch, prevPos, currPos, Color.Cyan * 0.5f, 0, 5);

      //         prevPos = currPos;
      //       }
      //     }
      //   }

      //   return false;
      // }


    }
  }
}