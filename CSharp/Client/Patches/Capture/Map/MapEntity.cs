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
        UpdatePower = Capture.Get("Showperf.Update.MapEntity.Misc.Power");
        UpdateStructures = Capture.Get("Showperf.Update.MapEntity.Misc.Structures");
      }

      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Map/MapEntity.cs#L616
      public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
      {
        if (!Showperf.Revealed) return true;
        if (!UpdateMapEntity.IsActive && !Items.IsActive && !Misc.IsActive &&
        !UpdateHulls.IsActive && !UpdateGaps.IsActive &&
        !UpdatePower.IsActive && !UpdateStructures.IsActive) return true;

        MapEntity.mapEntityUpdateTick++;

#if CLIENT
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        Stopwatch sw3 = new Stopwatch();
        sw.Start();
#endif

        if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
        {
          sw2.Restart();
          Capture.Update.EnsureCategory(UpdateHulls);
          foreach (Hull hull in Hull.HullList)
          {
            sw3.Restart();
            hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            sw3.Stop();
            Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateHulls, $"{hull}");
          }
#if CLIENT
          Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
#endif
          sw2.Stop();
          Capture.Update.AddTicksOnce(sw2.ElapsedTicks, Misc, "Hulls");
          sw2.Restart();

          Capture.Update.EnsureCategory(UpdateStructures);
          foreach (Structure structure in Structure.WallList)
          {
            sw3.Restart();
            structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            sw3.Stop();
            Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateStructures, $"{structure}");
          }
          sw2.Stop();
          Capture.Update.AddTicksOnce(sw2.ElapsedTicks, Misc, "Structures");
          sw2.Restart();
        }


        //update gaps in random order, because otherwise in rooms with multiple gaps
        //the water/air will always tend to flow through the first gap in the list,
        //which may lead to weird behavior like water draining down only through
        //one gap in a room even if there are several
        Capture.Update.EnsureCategory(UpdateGaps);
        foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
        {
          sw3.Restart();
          gap.Update(deltaTime, cam);
          sw3.Stop();
          Capture.Update.AddTicks(sw3.ElapsedTicks, UpdateGaps, $"{gap.Submarine?.Info?.Name}({gap.Submarine?.IdOffset}).Gap({gap.ID})");
        }
        sw2.Stop();
        Capture.Update.AddTicksOnce(sw2.ElapsedTicks, Misc, "Gaps");
        sw2.Restart();

        if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
        {
          Powered.UpdatePower(deltaTime * MapEntity.PoweredUpdateInterval);
        }
        sw2.Stop();
        Capture.Update.AddTicksOnce(sw2.ElapsedTicks, Misc, "UpdatePower");


#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateMapEntity, "MapEntity.Misc");
        sw.Restart();
#endif

        Capture.Update.EnsureCategory(Items);
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
              sw2.Restart();
              item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
              sw2.Stop();
              if (Items.ByID)
              {
                Capture.Update.AddTicks(sw2.ElapsedTicks, Items, $"{item}");
              }
              else
              {
                Capture.Update.AddTicks(sw2.ElapsedTicks, Items, item.Prefab.Identifier);
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

        foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
        {
          if (item.Removed) continue;

          sw2.Restart();
          item.Update(deltaTime, cam);
          sw2.Stop();
          if (Items.ByID)
          {
            Capture.Update.AddTicks(sw2.ElapsedTicks, Items, $"{item}");
          }
          else
          {
            Capture.Update.AddTicks(sw2.ElapsedTicks, Items, item.Prefab.Identifier);
          }
        }

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateMapEntity, "MapEntity.Items");
        sw.Restart();
#endif

        if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
        {
          MapEntity.UpdateAllProjSpecific(deltaTime * MapEntity.MapEntityUpdateInterval);

          MapEntity.Spawner?.Update();
        }
        sw.Stop();
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateMapEntity, "MapEntity.Spawner");

        return false;
      }


    }
  }
}