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

    /// <summary>
    /// Call Update() on every object in Entity.list
    /// </summary>
    public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
    {
      if (
        ActiveCategory != ShowperfCategory.ItemUpdate &&
        ActiveCategory != ShowperfCategory.HullUpdate &&
        ActiveCategory != ShowperfCategory.StructureUpdate &&
        ActiveCategory != ShowperfCategory.GapUpdate
      ) return true;

      var sw2 = new System.Diagnostics.Stopwatch();
      long ticks;


      MapEntity.mapEntityUpdateTick++;

      //#if CLIENT
      var sw = new System.Diagnostics.Stopwatch();


      sw.Start();
      //#endif
      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {

        if (ActiveCategory == ShowperfCategory.HullUpdate)
        {
          Window.ensureCategory(CaptureCategory.HullUpdate);
          foreach (Hull hull in Hull.HullList)
          {
            sw2.Restart();
            hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            ticks = sw2.ElapsedTicks;
            Window.tryAddTicks($"{hull}", CaptureCategory.HullUpdate, ticks);
          }
        }
        else
        {
          foreach (Hull hull in Hull.HullList)
          {
            hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          }
        }


        //#if CLIENT
        Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
        //#endif

        if (ActiveCategory == ShowperfCategory.StructureUpdate)
        {
          Window.ensureCategory(CaptureCategory.StructureUpdate);

          foreach (Structure structure in Structure.WallList)
          {
            sw2.Restart();
            structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            ticks = sw2.ElapsedTicks;
            if (CaptureFrom.Count == 0 || (structure.Submarine != null && CaptureFrom.Contains(structure.Submarine.Info.Type)))
            {
              if (CaptureById)
              {
                Window.tryAddTicks($"{structure}({structure.ID})", CaptureCategory.StructureUpdate, ticks);
              }
              else
              {
                Window.tryAddTicks($"{structure}", CaptureCategory.StructureUpdate, ticks);
              }
            }


          }
        }
        else
        {
          foreach (Structure structure in Structure.WallList)
          {
            structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          }
        }
      }

      //update gaps in random order, because otherwise in rooms with multiple gaps
      //the water/air will always tend to flow through the first gap in the list,
      //which may lead to weird behavior like water draining down only through
      //one gap in a room even if there are several
      if (ActiveCategory == ShowperfCategory.GapUpdate)
      {
        Window.ensureCategory(CaptureCategory.GapUpdate);
        foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
        {
          sw2.Restart();
          gap.Update(deltaTime, cam);
          ticks = sw2.ElapsedTicks;

          if (CaptureFrom.Count == 0 || (gap.Submarine != null && CaptureFrom.Contains(gap.Submarine.Info.Type)))
          {
            Window.tryAddTicks($"Gap({gap.ID})", CaptureCategory.GapUpdate, ticks);
          }
        }
      }
      else
      {
        foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
        {
          gap.Update(deltaTime, cam);
        }
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
          if (ActiveCategory == ShowperfCategory.ItemUpdate)
          {
            Window.ensureCategory(CaptureCategory.ItemUpdate);
            foreach (Item item in Item.ItemList)
            {
              if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
              lastUpdatedItem = item;

              sw2.Restart();
              item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);


              ticks = sw2.ElapsedTicks;
              if (CaptureFrom.Count == 0 || (item.Submarine != null && CaptureFrom.Contains(item.Submarine.Info.Type)))
              {
                if (CaptureById)
                {
                  Window.tryAddTicks($"{item}", CaptureCategory.ItemUpdate, ticks);
                }
                else
                {
                  Window.tryAddTicks(item.Prefab.Identifier, CaptureCategory.ItemUpdate, ticks);
                }
              }
            }
          }
          else
          {
            foreach (Item item in Item.ItemList)
            {
              if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
              lastUpdatedItem = item;

              item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
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



      if (ActiveCategory == ShowperfCategory.ItemUpdate)
      {
        Window.ensureCategory(CaptureCategory.ItemUpdate);
        foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
        {
          if (item.Removed) continue;

          sw2.Restart();

          item.Update(deltaTime, cam);

          ticks = sw2.ElapsedTicks;
          if (CaptureFrom.Count == 0 || (item.Submarine != null && CaptureFrom.Contains(item.Submarine.Info.Type)))
          {
            if (CaptureById)
            {
              Window.tryAddTicks($"{item}", CaptureCategory.ItemUpdate, ticks);
            }
            else
            {
              Window.tryAddTicks(item.Prefab.Identifier, CaptureCategory.ItemUpdate, ticks);
            }
          }
        }
      }
      else
      {
        foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
        {
          if (item.Removed) continue;
          item.Update(deltaTime, cam);
        }
      }
      sw2.Stop();

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