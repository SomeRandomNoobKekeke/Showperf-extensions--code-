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

using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class MapEntityPatch
    {
      public static CaptureState Misc;
      public static CaptureState Items;
      public static CaptureState UpdateMapEntity;
      public static CaptureState UpdateHulls;
      public static CaptureState UpdateGaps;
      public static CaptureState UpdatePower;
      public static CaptureState UpdateStructures;
      public static CaptureState UpdateWholeSub;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(MapEntity).GetMethod("UpdateAll", AccessTools.all),
          prefix: ShowperfMethod(typeof(MapEntityPatch).GetMethod("MapEntity_UpdateAll_Replace"))
        );

        Misc = Capture.Get("Showperf.Update.MapEntity.Misc");
        Items = Capture.Get("Showperf.Update.MapEntity.Items");
        UpdateMapEntity = Capture.Get("Showperf.Update.MapEntity");

        UpdateHulls = Capture.Get("Showperf.Update.MapEntity.Misc.Hulls");
        UpdateGaps = Capture.Get("Showperf.Update.MapEntity.Misc.Gaps");
        UpdatePower = Capture.Get("Showperf.Update.MapEntity.Misc.UpdatePower");
        UpdateStructures = Capture.Get("Showperf.Update.MapEntity.Misc.Structures");
        UpdateWholeSub = Capture.Get("Showperf.Update.MapEntity.WholeSub");

      }

      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Map/MapEntity.cs#L616
      public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
      {
        if (!Showperf.Revealed) return true;

        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        Stopwatch sw3 = new Stopwatch();

        Capture.Update.EnsureCategory(UpdateMapEntity);
        Capture.Update.EnsureCategory(UpdateWholeSub);
        Capture.Update.EnsureCategory(Misc);

        MapEntity.mapEntityUpdateTick++;



        sw.Restart();

        if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
        {
          sw2.Restart();

          if (UpdateHulls.IsActive)
          {
            Capture.Update.EnsureCategory(UpdateHulls);

            foreach (Hull hull in Hull.HullList)
            {
              sw3.Restart();
              hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
              sw3.Stop();

              if (Capture.ShouldCapture(hull))
              {
                Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateHulls, hull.ToString());
              }

              Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateWholeSub, hull.Submarine?.ToString() ?? "Things in open water");
            }

          }
          else
          {
            foreach (Hull hull in Hull.HullList)
            {
              hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            }
          }


          Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);

          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, Misc, "Hulls");

          sw2.Restart();
          if (UpdateStructures.IsActive)
          {
            Capture.Update.EnsureCategory(UpdateStructures);

            foreach (Structure structure in Structure.WallList)
            {
              sw3.Restart();
              structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
              sw3.Stop();

              if (Capture.ShouldCapture(structure))
              {
                Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateStructures, structure.ToString());
              }

              Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateWholeSub, structure.Submarine?.ToString() ?? "Things in open water");
            }
          }
          else
          {
            foreach (Structure structure in Structure.WallList)
            {
              structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            }
          }
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, Misc, "Structures");
        }

        //update gaps in random order, because otherwise in rooms with multiple gaps
        //the water/air will always tend to flow through the first gap in the list,
        //which may lead to weird behavior like water draining down only through
        //one gap in a room even if there are several
        sw2.Restart();
        if (UpdateGaps.IsActive)
        {
          Capture.Update.EnsureCategory(UpdateGaps);

          foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
          {
            sw3.Restart();
            gap.Update(deltaTime, cam);
            sw3.Stop();

            if (Capture.ShouldCapture(gap))
            {
              Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateGaps, $"Gap ({gap.ID})");
            }

            Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateWholeSub, gap.Submarine?.ToString() ?? "Things in open water");
          }
        }
        else
        {
          foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
          {
            gap.Update(deltaTime, cam);
          }
        }
        sw2.Stop();
        Capture.Update.AddTicks(sw2.ElapsedTicks, Misc, "Gaps");

        sw2.Restart();
        if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
        {
          Powered.UpdatePower(deltaTime * MapEntity.PoweredUpdateInterval);
        }
        sw2.Stop();
        Capture.Update.AddTicks(sw2.ElapsedTicks, Misc, "Powered");
        Capture.Update.AddTicksOnce(sw2.ElapsedTicks, UpdatePower, "Powered");


        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateMapEntity, "Misc");
        sw.Restart();


        if (Items.IsActive)
        {
          Capture.Update.EnsureCategory(Items);

          Item.UpdatePendingConditionUpdates(deltaTime);
          if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
          {
            Item lastUpdatedItem = null;

            try
            {
              if (Items.ByID)
              {
                foreach (Item item in Item.ItemList)
                {
                  if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
                  lastUpdatedItem = item;
                  sw2.Restart();
                  item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
                  sw2.Stop();

                  if (Capture.ShouldCapture(item))
                  {
                    Capture.Update.AddTicks(sw2.ElapsedTicks, Items, item.ToString());
                  }
                  Capture.Update.AddTicks(sw2.ElapsedTicks, UpdateWholeSub, item.Submarine?.ToString() ?? "Things in open water");
                }
              }
              else
              {
                foreach (Item item in Item.ItemList)
                {
                  if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
                  lastUpdatedItem = item;
                  sw2.Restart();
                  item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
                  sw2.Stop();

                  if (Capture.ShouldCapture(item))
                  {
                    Capture.Update.AddTicks(sw2.ElapsedTicks, Items, item.Prefab.Identifier);
                  }
                  Capture.Update.AddTicks(sw2.ElapsedTicks, UpdateWholeSub, item.Submarine?.ToString() ?? "Things in open water");
                }
              }


            }
            catch (InvalidOperationException e)
            {
              GameAnalyticsManager.AddErrorEventOnce(
                  "MapEntity.UpdateAll:ItemUpdateInvalidOperation",
                  GameAnalyticsManager.ErrorSeverity.Critical,
                  $"Error while updating item {lastUpdatedItem?.Name ?? "null"}: {e.Message}");
              throw new InvalidOperationException($"Error while updating item {lastUpdatedItem?.Name ?? "null"}", innerException: e);
            }
          }

          if (Items.ByID)
          {
            foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
            {
              if (item.Removed) continue;

              sw2.Restart();
              item.Update(deltaTime, cam);
              sw2.Stop();

              if (Capture.ShouldCapture(item))
              {
                Capture.Update.AddTicks(sw2.ElapsedTicks, Items, item.ToString());
              }
              Capture.Update.AddTicks(sw2.ElapsedTicks, UpdateWholeSub, item.Submarine?.ToString() ?? "Things in open water");
            }
          }
          else
          {
            foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
            {
              if (item.Removed) continue;

              sw2.Restart();
              item.Update(deltaTime, cam);
              sw2.Stop();

              if (Capture.ShouldCapture(item))
              {
                Capture.Update.AddTicks(sw2.ElapsedTicks, Items, item.Prefab.Identifier);
              }
              Capture.Update.AddTicks(sw2.ElapsedTicks, UpdateWholeSub, item.Submarine?.ToString() ?? "Things in open water");
            }
          }


        }
        else
        {
          Item.UpdatePendingConditionUpdates(deltaTime);
          if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
          {
            Item lastUpdatedItem = null;

            try
            {
              foreach (Item item in Item.ItemList)
              {
                if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
                lastUpdatedItem = item;
                item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
              }
            }
            catch (InvalidOperationException e)
            {
              GameAnalyticsManager.AddErrorEventOnce(
                  "MapEntity.UpdateAll:ItemUpdateInvalidOperation",
                  GameAnalyticsManager.ErrorSeverity.Critical,
                  $"Error while updating item {lastUpdatedItem?.Name ?? "null"}: {e.Message}");
              throw new InvalidOperationException($"Error while updating item {lastUpdatedItem?.Name ?? "null"}", innerException: e);
            }
          }

          foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
          {
            if (item.Removed) continue;

            item.Update(deltaTime, cam);
          }
        }


        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateMapEntity, "Items");


        if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
        {
          sw.Restart();
          MapEntity.UpdateAllProjSpecific(deltaTime * MapEntity.MapEntityUpdateInterval);
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, UpdateMapEntity, "UpdateAllProjSpecific");

          sw.Restart();
          MapEntity.Spawner?.Update();
          sw.Stop();

          Capture.Update.AddTicks(sw.ElapsedTicks, UpdateMapEntity, "Spawner");
        }



        return false;
      }


    }
  }
}