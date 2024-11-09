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
      public static bool shouldDrawBack1;

      public static void Initialize()
      {
        // harmony.Patch(
        //   original: typeof(Submarine).GetMethod("DrawBack", AccessTools.all),
        //   prefix: ShowperfMethod(typeof(SubmarinePatch).GetMethod("Submarine_DrawBack_Replace"))
        // );

        harmony.Patch(
          original: typeof(Submarine).GetMethod("DrawPaintedColors", AccessTools.all),
          prefix: new HarmonyMethod(typeof(SubmarinePatch).GetMethod("Submarine_DrawPaintedColors_Replace"))
        );
      }


      //static bool IsFromOutpostDrawnBehindSubs(Entity e) => e.Submarine is { Info.OutpostGenerationParams.DrawBehindSubs: true };


      public static void DrawBack(CaptureState cs, SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
      {
        if (!cs.IsActive)
        {
          Submarine.DrawBack(spriteBatch, editing, predicate);
          return;
        }

        Capture.Draw.EnsureCategory(cs);
        Submarine_DrawBack_Alt(cs, spriteBatch, editing, predicate);
      }

      public static void DrawDamageable(CaptureState cs, SpriteBatch spriteBatch, Effect damageEffect, bool editing = false)
      {
        if (!cs.IsActive)
        {
          Submarine.DrawDamageable(spriteBatch, damageEffect, editing);
          return;
        }

        Capture.Draw.EnsureCategory(cs);
        Submarine_DrawDamageable_Alt(cs, spriteBatch, damageEffect, editing);
      }

      public static bool Submarine_DrawBack_Alt(CaptureState cs, SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
      {
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

          if (Capture.ShouldCapture(e))
          {
            if (cs.ByID || e.Prefab == null)
            {
              Capture.Draw.AddTicks(sw.ElapsedTicks, cs, $"{e.Name} (ID: {e.ID})");
            }
            else
            {
              Capture.Draw.AddTicks(sw.ElapsedTicks, cs, e.Prefab.Identifier);
            }
          }
        }

        return false;
      }


      public static bool Submarine_DrawDamageable_Alt(CaptureState cs, SpriteBatch spriteBatch, Effect damageEffect, bool editing = false, Predicate<MapEntity> predicate = null)
      {
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        var entitiesToRender = !editing && Submarine.visibleEntities != null ? Submarine.visibleEntities : MapEntity.MapEntityList;

        sw.Restart();

        Submarine.depthSortedDamageable.Clear();

        //insertion sort according to draw depth
        foreach (MapEntity e in entitiesToRender)
        {
          if (e is Structure structure && structure.DrawDamageEffect)
          {
            if (predicate != null)
            {
              if (!predicate(e)) { continue; }
            }
            float drawDepth = structure.GetDrawDepth();
            int i = 0;
            while (i < Submarine.depthSortedDamageable.Count)
            {
              float otherDrawDepth = Submarine.depthSortedDamageable[i].GetDrawDepth();
              if (otherDrawDepth < drawDepth) { break; }
              i++;
            }
            Submarine.depthSortedDamageable.Insert(i, structure);
          }
        }

        sw.Stop();
        Capture.Draw.AddTicks(sw.ElapsedTicks, cs, "insertion sort according to draw depth");
        sw.Restart();

        foreach (Structure s in Submarine.depthSortedDamageable)
        {
          sw2.Restart();
          s.DrawDamage(spriteBatch, damageEffect, editing);
          sw2.Stop();
          if (Capture.ShouldCapture(s))
          {
            if (cs.ByID)
            {
              Capture.Draw.AddTicks(sw2.ElapsedTicks, cs, $"{s} ({s.ID})");
            }
            else
            {
              Capture.Draw.AddTicks(sw2.ElapsedTicks, cs, s.ToString());
            }
          }
        }
        if (damageEffect != null)
        {
          damageEffect.Parameters["aCutoff"].SetValue(0.0f);
          damageEffect.Parameters["cCutoff"].SetValue(0.0f);
          Submarine.DamageEffectCutoff = 0.0f;
        }
        sw.Stop();
        //Capture.Draw.AddTicks(sw.ElapsedTicks, cs, "DrawDamage");

        return false;
      }


      public static bool Submarine_DrawPaintedColors_Replace(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
      {
        if (!GameScreenPatch.BackStructures.IsActive) return true;

        Stopwatch sw = new Stopwatch();
        Capture.Draw.EnsureCategory(GameScreenPatch.BackStructures);

        var entitiesToRender = !editing && Submarine.visibleEntities != null ? Submarine.visibleEntities : MapEntity.MapEntityList;

        foreach (MapEntity e in entitiesToRender)
        {
          if (e is Hull hull)
          {
            sw.Restart();
            if (hull.SupportsPaintedColors)
            {
              if (predicate != null)
              {
                if (!predicate(e)) { continue; }
              }
              hull.DrawSectionColors(spriteBatch);
            }
            sw.Stop();
            if (Capture.ShouldCapture(hull))
            {
              Capture.Draw.AddTicks(sw.ElapsedTicks, GameScreenPatch.BackStructures, "DrawPaintedColors");
            }
          }
        }

        return false;
      }



      // public static bool Submarine_DrawBack_Replace(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
      // {
      //   if (!DrawBack.IsActive) return true;
      //   Capture.Update.EnsureCategory(DrawBack);

      //   var entitiesToRender = !editing && Submarine.visibleEntities != null ? Submarine.visibleEntities : MapEntity.MapEntityList;

      //   Stopwatch sw = new Stopwatch();

      //   foreach (MapEntity e in entitiesToRender)
      //   {
      //     if (!e.DrawBelowWater) continue;

      //     if (predicate != null)
      //     {
      //       if (!predicate(e)) continue;
      //     }

      //     sw.Restart();
      //     e.Draw(spriteBatch, editing, true);
      //     sw.Stop();

      //     if (Capture.ShouldCapture(e))
      //     {
      //       if (DrawBack.ByID || e.Prefab == null)
      //       {
      //         Capture.Update.AddTicks(sw.ElapsedTicks, DrawBack, $"{e.Name} (ID: {e.ID})");
      //       }
      //       else
      //       {
      //         Capture.Update.AddTicks(sw.ElapsedTicks, DrawBack, e.Prefab.Identifier);
      //       }
      //     }
      //   }

      //   sw.Stop();

      //   return false;
      // }

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