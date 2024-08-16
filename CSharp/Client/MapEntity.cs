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

using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {

    public static void tryAddTicks(Item item, long ticks)
    {
      Dictionary<int, ItemUpdateTicks> dict = item.Submarine == Submarine.MainSub ? window.firstSlice.mainSub : window.firstSlice.otherSubs;

      if (dict.ContainsKey(item.Prefab.Identifier.HashCode))
      {
        dict[item.Prefab.Identifier.HashCode] += ticks;
      }
      else
      {
        dict[item.Prefab.Identifier.HashCode] = new ItemUpdateTicks(
          item.Prefab.Identifier.Value,
          ticks
        );
      }
    }

    /// <summary>
    /// Call Update() on every object in Entity.list
    /// </summary>
    public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
    {
      if (!DrawItemUpdateTimes) return true;

      MapEntity.mapEntityUpdateTick++;

      //#if CLIENT
      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      //#endif
      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {

        foreach (Hull hull in Hull.HullList)
        {
          hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
        }
        //#if CLIENT
        Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
        //#endif

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

      //#if CLIENT
      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
      sw.Restart();
      //#endif

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

            sw.Restart();
            item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);

            tryAddTicks(item, sw.ElapsedTicks);
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


        sw.Restart();

        item.Update(deltaTime, cam);

        tryAddTicks(item, sw.ElapsedTicks);
      }

      //#if CLIENT
      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
      sw.Restart();
      //#endif

      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {
        MapEntity.UpdateAllProjSpecific(deltaTime * MapEntity.MapEntityUpdateInterval);

        MapEntity.Spawner?.Update();
      }

      return false;
    }
  }
}