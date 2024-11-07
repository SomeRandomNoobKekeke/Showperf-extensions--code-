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
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(MapEntity).GetMethod("UpdateAll", AccessTools.all),
          prefix: ShowperfMethod(typeof(MapEntityPatch).GetMethod("MapEntity_UpdateAll_Replace"))
        );

        Misc = Capture.Get("Showperf.Update.MapEntity.Misc");
        Items = Capture.Get("Showperf.Update.MapEntity.Items");
      }

      // https://github.com/evilfactory/LuaCsForBarotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Map/MapEntity.cs#L616
      public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
      {
        if (!Showperf.Revealed) return true;

        MapEntity.mapEntityUpdateTick++;

#if CLIENT
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
#endif
        if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
        {

          foreach (Hull hull in Hull.HullList)
          {
            hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          }
#if CLIENT
          Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
#endif

          foreach (Structure structure in Structure.WallList)
          {
            structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          }
        }

        //update gaps in random order, because otherwise in rooms with multiple gaps
        //the water/air will always tend to flow through the first gap in the list,
        //which may lead to weird behavior like water draining down only through
        //one gap in a room even if there are several
        foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
        {
          gap.Update(deltaTime, cam);
        }

        if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
        {
          Powered.UpdatePower(deltaTime * MapEntity.PoweredUpdateInterval);
        }

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Misc, "Update.MapEntity.Misc");
        sw.Restart();
#endif

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

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, Items, "Update.MapEntity.Items");
        sw.Restart();
#endif

        if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
        {
          MapEntity.UpdateAllProjSpecific(deltaTime * MapEntity.MapEntityUpdateInterval);

          MapEntity.Spawner?.Update();
        }

        return false;
      }


    }
  }
}